using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using HomeOwners.Areas.Identity.Data;
using HomeOwners.Models.Users;
using Microsoft.AspNetCore.Identity.UI.Services;
using HomeOwners.Services;
using HomeOwners.Infrastructure;
using Microsoft.AspNetCore.Authentication.Cookies;

var builder = WebApplication.CreateBuilder(args);
var connectionString = builder.Configuration.GetConnectionString("HomeDbContextConnection") ?? throw new InvalidOperationException("Connection string 'HomeDbContextConnection' not found.");

builder.Services.AddDbContext<HomeDbContext>(options => options.UseSqlServer(connectionString));

// Identity configuration with multi-user types
builder.Services.AddIdentity<IdentityUser, IdentityRole>(options =>
{
    options.SignIn.RequireConfirmedAccount = false;
    options.SignIn.RequireConfirmedEmail = false;
    options.SignIn.RequireConfirmedPhoneNumber = false;
})
.AddEntityFrameworkStores<HomeDbContext>();

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();

// Register IEmailSender
builder.Services.AddTransient<IEmailSender, HomeOwners.Services.NoOpEmailSender>();
builder.Services.AddScoped<HomeOwners.Services.UserService>();
builder.Services.AddScoped<IUserPreferencesService, UserPreferencesService>();
builder.Services.AddScoped<AnnouncementService>();
builder.Services.AddScoped<EventService>();
builder.Services.AddScoped<FacilityService>();
builder.Services.AddScoped<BookingService>();

builder.Services.AddControllers(options =>
{
    options.ModelBinderProviders.Insert(0, new TimeSpanModelBinderProvider());
});

// Add authorization policies
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("RequireAdminRole", policy => policy.RequireRole("Admin"));
    options.AddPolicy("RequireStaffRole", policy => policy.RequireRole("Staff"));
    options.AddPolicy("RequireHomeOwnerRole", policy => policy.RequireRole("HomeOwner"));
});

// Configure authentication
builder.Services.Configure<CookieAuthenticationOptions>(
    IdentityConstants.ApplicationScheme, 
    options =>
    {
        options.LoginPath = "/Identity/Account/Login";
        options.AccessDeniedPath = "/Identity/Account/AccessDenied";
    });

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.MapAreaControllerRoute(
    name: "areas",
    areaName: "Admin",
    pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}");

app.MapAreaControllerRoute(
    name: "staffArea",
    areaName: "Staff",
    pattern: "Staff/{controller=Dashboard}/{action=Index}/{id?}");

app.MapRazorPages();

// Create roles and seed initial users
using (var scope = app.Services.CreateScope())
{
    var serviceProvider = scope.ServiceProvider;
    var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
    var userManager = serviceProvider.GetRequiredService<UserManager<IdentityUser>>();
    var dbContext = serviceProvider.GetRequiredService<HomeDbContext>();

    // Create roles if they don't exist
    string[] roles = { "Admin", "Staff", "HomeOwner" };
    foreach (var role in roles)
    {
        if (!await roleManager.RoleExistsAsync(role))
        {
            await roleManager.CreateAsync(new IdentityRole(role));
        }
    }

    // Create Admin user
    string adminEmail = "admin@example.com";
    if (await userManager.FindByEmailAsync(adminEmail) == null)
    {
        var adminUser = new AdminUser
        {
            UserName = "admin",
            Email = adminEmail,
            EmailConfirmed = true,
            FullName = "System Administrator",
            AdminLevel = "SuperAdmin"
        };

        var result = await userManager.CreateAsync(adminUser, "Admin@123456");
        if (result.Succeeded)
        {
            await userManager.AddToRoleAsync(adminUser, "Admin");
        }
    }

    // Create Staff user
    string staffEmail = "staff@example.com";
    if (await userManager.FindByEmailAsync(staffEmail) == null)
    {
        var staffUser = new StaffUser
        {
            UserName = "staff",
            Email = staffEmail,
            EmailConfirmed = true,
            FullName = "Staff Member",
            Department = "Maintenance",
            Position = "Manager"
        };

        var result = await userManager.CreateAsync(staffUser, "Staff@123456");
        if (result.Succeeded)
        {
            await userManager.AddToRoleAsync(staffUser, "Staff");
        }
    }

}

app.Run();
