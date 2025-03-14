using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using HomeOwners.Data;
using HomeOwners.Areas.Identity.Data;
var builder = WebApplication.CreateBuilder(args);
var connectionString = builder.Configuration.GetConnectionString("HomeDbContextConnection") ?? throw new InvalidOperationException("Connection string 'HomeDbContextConnection' not found.");

builder.Services.AddDbContext<HomeDbContext>(options => options.UseSqlServer(connectionString));

builder.Services.AddDefaultIdentity<ApplicationUser>(options =>
{
    options.SignIn.RequireConfirmedAccount = false;
    options.SignIn.RequireConfirmedEmail = false;
    options.SignIn.RequireConfirmedPhoneNumber = false;
})
.AddRoles<IdentityRole>() // Add role management
.AddEntityFrameworkStores<HomeDbContext>();

// Add services to the container.
builder.Services.AddControllersWithViews();

builder.Services.AddRazorPages();

// Add authorization policies
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("RequireAdminRole", policy => policy.RequireRole("Admin"));
    options.AddPolicy("RequireStaffRole", policy => policy.RequireRole("Staff"));
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication(); // Make sure this is before UseAuthorization
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

// Add this new line for area support
app.MapAreaControllerRoute(
    name: "areas",
    areaName: "Admin",
    pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}");

// Make sure this is added to enable Staff area routing
app.MapAreaControllerRoute(
    name: "staffArea",
    areaName: "Staff",
    pattern: "Staff/{controller=Dashboard}/{action=Index}/{id?}");

app.MapRazorPages();

// Create roles and admin user if they don't exist
using (var scope = app.Services.CreateScope())
{
    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

    // Create Admin role if it doesn't exist
    if (!await roleManager.RoleExistsAsync("Admin"))
    {
        await roleManager.CreateAsync(new IdentityRole("Admin"));
    }

    // Create Staff role if it doesn't exist
    if (!await roleManager.RoleExistsAsync("Staff"))
    {
        await roleManager.CreateAsync(new IdentityRole("Staff"));
    }

    // Create Admin user if it doesn't exist
    string adminEmail = "admin@example.com";
    if (await userManager.FindByEmailAsync(adminEmail) == null)
    {
        var adminUser = new ApplicationUser
        {
            UserName = "admin",
            Email = adminEmail,
            EmailConfirmed = true
        };

        var result = await userManager.CreateAsync(adminUser, "Admin@123456"); // Change this to a secure password

        if (result.Succeeded)
        {
            await userManager.AddToRoleAsync(adminUser, "Admin");
        }
    }

    // Create Staff user if it doesn't exist - MOVED INSIDE THE USING BLOCK
    string staffEmail = "staff@example.com";
    if (await userManager.FindByEmailAsync(staffEmail) == null)
    {
        var staffUser = new ApplicationUser
        {
            UserName = "staff",
            Email = staffEmail,
            EmailConfirmed = true
        };

        var result = await userManager.CreateAsync(staffUser, "Staff@123456"); // Change this to a secure password

        if (result.Succeeded)
        {
            await userManager.AddToRoleAsync(staffUser, "Staff");
        }
    }
}

app.Run();
