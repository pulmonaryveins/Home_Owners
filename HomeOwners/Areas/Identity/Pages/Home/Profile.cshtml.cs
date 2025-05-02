using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using HomeOwners.Areas.Identity.Data;
using HomeOwners.Models;
using HomeOwners.Models.Users;
using HomeOwners.Services;
using Microsoft.EntityFrameworkCore;

namespace HomeOwners.Areas.Identity.Pages.Home
{
    public class ProfileModel : PageModel
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly HomeDbContext _context;
        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly IUserPreferencesService _preferencesService;

        public ProfileModel(
            UserManager<IdentityUser> userManager,
            SignInManager<IdentityUser> signInManager,
            IUserPreferencesService preferencesService,
            HomeDbContext context)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _preferencesService = preferencesService;
            _context = context;
        }

        [BindProperty]
        [Display(Name = "Username")]
        public string Username { get; set; }

        [BindProperty]
        [EmailAddress]
        [Display(Name = "Email")]
        public string Email { get; set; }

        [BindProperty]
        [Display(Name = "Full Name")]
        public string FullName { get; set; }

        [BindProperty]
        [Phone]
        [Display(Name = "Phone Number")]
        public string PhoneNumber { get; set; }

        [BindProperty]
        [Display(Name = "House Number")]
        public string HouseNumber { get; set; }

        [Display(Name = "Account Status")]
        public string AccountStatus { get; set; }

        [BindProperty]
        [Display(Name = "Current Password")]
        [DataType(DataType.Password)]
        public string CurrentPassword { get; set; }

        [BindProperty]
        [Display(Name = "New Password")]
        [DataType(DataType.Password)]
        [StringLength(100, ErrorMessage = "The {0} must be at least {2} characters long.", MinimumLength = 8)]
        public string NewPassword { get; set; }

        [BindProperty]
        [Display(Name = "Confirm New Password")]
        [DataType(DataType.Password)]
        [Compare("NewPassword", ErrorMessage = "The new password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; }

        [Display(Name = "Email Notifications")]
        [BindProperty]
        public bool EmailNotifications { get; set; } = true;

        [Display(Name = "SMS Notifications")]
        [BindProperty]
        public bool SmsNotifications { get; set; }

        [Display(Name = "Marketing Emails")]
        [BindProperty]
        public bool MarketingEmails { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            // Clear existing entity tracking
            _context.ChangeTracker.Clear();

            Username = user.UserName;
            Email = user.Email;
            PhoneNumber = user.PhoneNumber;

            // Instead of direct casting, query specific user types from the database
            var userId = user.Id;

            // Try to get HomeOwnerUser with AsNoTracking to ensure fresh data
            var homeOwner = await _context.HomeOwnerUsers
                .AsNoTracking()
                .FirstOrDefaultAsync(h => h.Id == userId);

            if (homeOwner != null)
            {
                FullName = homeOwner.FullName;
                HouseNumber = homeOwner.HouseNumber;
                AccountStatus = homeOwner.AccountStatus;

                // Load preferences
                var prefs = await _preferencesService.GetUserPreferencesAsync(userId);
                EmailNotifications = prefs.EmailNotifications;
                SmsNotifications = prefs.SmsNotifications;
                MarketingEmails = prefs.MarketingEmails;
                return Page();
            }

            // Check for other user types using fully qualified name
            var appUser = await _context.Set<HomeOwners.Models.Users.ApplicationUser>().FirstOrDefaultAsync(a => a.Id == userId);
            if (appUser != null)
            {
                FullName = appUser.FullName;
                return Page();
            }

            var adminUser = await _context.AdminUsers.FirstOrDefaultAsync(a => a.Id == userId);
            if (adminUser != null)
            {
                FullName = adminUser.FullName;
                return Page();
            }

            var staffUser = await _context.StaffUsers.FirstOrDefaultAsync(s => s.Id == userId);
            if (staffUser != null)
            {
                FullName = staffUser.FullName;
                return Page();
            }

            return Page();
        }

        public async Task<IActionResult> OnPostUpdatePersonalAsync()
        {
            if (!ModelState.IsValid)
            {
                await LoadUserDataAsync(); // Reload data before returning the page
                return Page();
            }

            // Capture original values for logging
            string originalUsername = null;
            string originalEmail = null;
            string originalPhone = null;
            string originalFullName = null;
            string originalHouseNumber = null;

            try
            {
                // Get a fresh reference to the user to avoid stale data
                var userId = _userManager.GetUserId(User);
                var user = await _userManager.FindByIdAsync(userId);

                if (user == null)
                {
                    TempData["StatusMessage"] = "Error: User not found.";
                    TempData["StatusType"] = "Error";
                    return Page();
                }

                // Store original values for logging
                originalUsername = user.UserName;
                originalEmail = user.Email;
                originalPhone = user.PhoneNumber;

                bool hasChanges = false;
                bool identityChanges = false;

                // Use separate try-catch blocks for Identity updates vs. HomeOwnerUser updates
                try
                {
                    // Process identity-related changes
                    if (user.UserName != Username)
                    {
                        var setUsernameResult = await _userManager.SetUserNameAsync(user, Username);
                        if (!setUsernameResult.Succeeded)
                        {
                            foreach (var error in setUsernameResult.Errors)
                            {
                                ModelState.AddModelError(string.Empty, error.Description);
                            }
                            TempData["StatusMessage"] = "Error: Username could not be updated.";
                            TempData["StatusType"] = "Error";
                            await LoadUserDataAsync();
                            return Page();
                        }
                        identityChanges = true;
                    }

                    if (user.Email != Email)
                    {
                        var setEmailResult = await _userManager.SetEmailAsync(user, Email);
                        if (!setEmailResult.Succeeded)
                        {
                            foreach (var error in setEmailResult.Errors)
                            {
                                ModelState.AddModelError(string.Empty, error.Description);
                            }
                            TempData["StatusMessage"] = "Error: Email could not be updated.";
                            TempData["StatusType"] = "Error";
                            await LoadUserDataAsync();
                            return Page();
                        }
                        identityChanges = true;
                    }

                    if (user.PhoneNumber != PhoneNumber)
                    {
                        var setPhoneResult = await _userManager.SetPhoneNumberAsync(user, PhoneNumber);
                        if (!setPhoneResult.Succeeded)
                        {
                            foreach (var error in setPhoneResult.Errors)
                            {
                                ModelState.AddModelError(string.Empty, error.Description);
                            }
                            TempData["StatusMessage"] = "Error: Phone number could not be updated.";
                            TempData["StatusType"] = "Error";
                            await LoadUserDataAsync();
                            return Page();
                        }
                        identityChanges = true;
                    }

                    if (identityChanges)
                    {
                        hasChanges = true;
                        // Update identity in DbContext
                        _context.Update(user);
                        await _context.SaveChangesAsync();
                    }
                }
                catch (Exception ex)
                {
                    TempData["StatusMessage"] = $"Error updating identity data: {ex.Message}";
                    TempData["StatusType"] = "Error";
                    await LoadUserDataAsync();
                    return Page();
                }

                // Process HomeOwnerUser changes in a separate try-catch block
                try
                {
                    // Create a new database context to ensure a fresh connection for the HomeOwnerUser update
                    using (var scope = new Microsoft.Extensions.DependencyInjection.ServiceCollection()
                        .AddDbContext<HomeDbContext>(options =>
                            options.UseSqlServer(_context.Database.GetConnectionString()))
                        .BuildServiceProvider()
                        .CreateScope())
                    {
                        var freshDbContext = scope.ServiceProvider.GetRequiredService<HomeDbContext>();

                        // Get a fresh reference to the HomeOwnerUser
                        var homeOwner = await freshDbContext.HomeOwnerUsers
                            .FirstOrDefaultAsync(h => h.Id == userId);

                        if (homeOwner != null)
                        {
                            // Store original values
                            originalFullName = homeOwner.FullName;
                            originalHouseNumber = homeOwner.HouseNumber;

                            bool homeOwnerChanged = false;

                            if (homeOwner.FullName != FullName)
                            {
                                homeOwner.FullName = FullName;
                                homeOwnerChanged = true;
                            }

                            if (homeOwner.HouseNumber != HouseNumber)
                            {
                                homeOwner.HouseNumber = HouseNumber;
                                homeOwnerChanged = true;
                            }

                            if (homeOwnerChanged)
                            {
                                hasChanges = true;

                                // Explicitly mark entity as modified
                                freshDbContext.Entry(homeOwner).State = EntityState.Modified;

                                // Use a dedicated transaction with higher isolation level
                                using (var transaction = await freshDbContext.Database.BeginTransactionAsync(System.Data.IsolationLevel.ReadCommitted))
                                {
                                    try
                                    {
                                        // Force immediate execution
                                        int rowsAffected = await freshDbContext.SaveChangesAsync();

                                        // Verify changes were actually saved
                                        if (rowsAffected == 0)
                                        {
                                            throw new Exception("No records were updated in the database.");
                                        }

                                        await transaction.CommitAsync();

                                        // Log successful update
                                        System.Diagnostics.Debug.WriteLine($"Successfully updated HomeOwnerUser: {userId}");
                                        System.Diagnostics.Debug.WriteLine($"Changed FullName from '{originalFullName}' to '{FullName}'");
                                        System.Diagnostics.Debug.WriteLine($"Changed HouseNumber from '{originalHouseNumber}' to '{HouseNumber}'");
                                    }
                                    catch (Exception ex)
                                    {
                                        await transaction.RollbackAsync();
                                        throw new Exception($"Error saving HomeOwnerUser changes: {ex.Message}", ex);
                                    }
                                }
                            }
                        }
                        else
                        {
                            // Check for other user types
                            var appUser = await freshDbContext.Set<HomeOwners.Models.Users.ApplicationUser>()
                                .FirstOrDefaultAsync(a => a.Id == userId);

                            if (appUser != null && appUser.FullName != FullName)
                            {
                                appUser.FullName = FullName;
                                hasChanges = true;
                                freshDbContext.Entry(appUser).State = EntityState.Modified;
                                await freshDbContext.SaveChangesAsync();
                            }
                            else
                            {
                                // Similar code for AdminUser and StaffUser
                                var adminUser = await freshDbContext.AdminUsers.FirstOrDefaultAsync(a => a.Id == userId);
                                if (adminUser != null && adminUser.FullName != FullName)
                                {
                                    adminUser.FullName = FullName;
                                    hasChanges = true;
                                    freshDbContext.Entry(adminUser).State = EntityState.Modified;
                                    await freshDbContext.SaveChangesAsync();
                                }
                                else
                                {
                                    var staffUser = await freshDbContext.StaffUsers.FirstOrDefaultAsync(s => s.Id == userId);
                                    if (staffUser != null && staffUser.FullName != FullName)
                                    {
                                        staffUser.FullName = FullName;
                                        hasChanges = true;
                                        freshDbContext.Entry(staffUser).State = EntityState.Modified;
                                        await freshDbContext.SaveChangesAsync();
                                    }
                                }
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    TempData["StatusMessage"] = $"Error updating user profile: {ex.Message}";
                    TempData["StatusType"] = "Error";
                    await LoadUserDataAsync();
                    return Page();
                }

                if (hasChanges)
                {
                    // Final step - refresh the sign-in cookie with updated user information
                    await _signInManager.RefreshSignInAsync(await _userManager.GetUserAsync(User));

                    TempData["StatusMessage"] = "Your profile has been updated successfully.";
                    TempData["StatusType"] = "Success";

                    // Add detailed logging
                    System.Diagnostics.Debug.WriteLine($"Profile updated for user {userId}");
                    if (originalUsername != Username)
                        System.Diagnostics.Debug.WriteLine($"Changed username from '{originalUsername}' to '{Username}'");
                    if (originalEmail != Email)
                        System.Diagnostics.Debug.WriteLine($"Changed email from '{originalEmail}' to '{Email}'");
                    if (originalPhone != PhoneNumber)
                        System.Diagnostics.Debug.WriteLine($"Changed phone from '{originalPhone}' to '{PhoneNumber}'");
                }
                else
                {
                    TempData["StatusMessage"] = "No changes detected.";
                    TempData["StatusType"] = "Info";
                }

                // Use PRG pattern to avoid duplicate form submission
                return RedirectToPage(new { activeTab = "personal" });
            }
            catch (Exception ex)
            {
                // Global exception handler
                TempData["StatusMessage"] = $"An error occurred: {ex.Message}";
                TempData["StatusType"] = "Error";

                // Log the error
                System.Diagnostics.Debug.WriteLine($"ERROR in OnPostUpdatePersonalAsync: {ex.ToString()}");

                await LoadUserDataAsync();
                return Page();
            }
        }

        public async Task<IActionResult> OnPostUpdatePasswordAsync()
        {
            // Only validate the password fields
            if (string.IsNullOrEmpty(CurrentPassword) || string.IsNullOrEmpty(NewPassword) || string.IsNullOrEmpty(ConfirmPassword))
            {
                ModelState.AddModelError(string.Empty, "All password fields are required.");
                TempData["StatusMessage"] = "Error: All password fields are required.";
                TempData["StatusType"] = "Error";

                // Load user data to correctly display the page when returned
                await LoadUserDataAsync();
                return Page();
            }

            if (NewPassword != ConfirmPassword)
            {
                ModelState.AddModelError(string.Empty, "The new password and confirmation password do not match.");
                TempData["StatusMessage"] = "Error: The new password and confirmation password do not match.";
                TempData["StatusType"] = "Error";

                // Load user data to correctly display the page when returned
                await LoadUserDataAsync();
                return Page();
            }

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                TempData["StatusMessage"] = "Error: User not found.";
                TempData["StatusType"] = "Error";
                return Page();
            }

            try
            {
                // Use Identity's password change functionality
                var changePasswordResult = await _userManager.ChangePasswordAsync(user, CurrentPassword, NewPassword);
                if (!changePasswordResult.Succeeded)
                {
                    foreach (var error in changePasswordResult.Errors)
                    {
                        ModelState.AddModelError(string.Empty, error.Description);
                    }

                    TempData["StatusMessage"] = "Error: Password could not be changed.";
                    TempData["StatusType"] = "Error";

                    // Load user data to correctly display the page when returned
                    await LoadUserDataAsync();
                    return Page();
                }

                await _signInManager.RefreshSignInAsync(user);

                TempData["StatusMessage"] = "Your password has been changed successfully.";
                TempData["StatusType"] = "Success";

                // Clear password fields
                CurrentPassword = string.Empty;
                NewPassword = string.Empty;
                ConfirmPassword = string.Empty;

                return RedirectToPage(new { activeTab = "security" });
            }
            catch (Exception ex)
            {
                TempData["StatusMessage"] = $"Error changing password: {ex.Message}";
                TempData["StatusType"] = "Error";
                await LoadUserDataAsync();
                return Page();
            }
        }

        private async Task LoadUserDataAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user != null)
            {
                // Ensure we're getting fresh data from the database
                Username = user.UserName;
                Email = user.Email;
                PhoneNumber = user.PhoneNumber;

                var userId = user.Id;

                // Clear existing DbContext tracking to prevent stale data
                _context.ChangeTracker.Clear();

                // Try to get HomeOwnerUser with AsNoTracking to get fresh data
                var homeOwner = await _context.HomeOwnerUsers
                    .AsNoTracking()
                    .FirstOrDefaultAsync(h => h.Id == userId);

                if (homeOwner != null)
                {
                    FullName = homeOwner.FullName;
                    HouseNumber = homeOwner.HouseNumber;
                    AccountStatus = homeOwner.AccountStatus;
                    return;
                }

                // Check for other user types as before
                var appUser = await _context.Set<HomeOwners.Models.Users.ApplicationUser>()
                    .AsNoTracking()
                    .FirstOrDefaultAsync(a => a.Id == userId);

                if (appUser != null)
                {
                    FullName = appUser.FullName;
                    return;
                }

                var adminUser = await _context.AdminUsers
                    .AsNoTracking()
                    .FirstOrDefaultAsync(a => a.Id == userId);

                if (adminUser != null)
                {
                    FullName = adminUser.FullName;
                    return;
                }

                var staffUser = await _context.StaffUsers
                    .AsNoTracking()
                    .FirstOrDefaultAsync(s => s.Id == userId);

                if (staffUser != null)
                {
                    FullName = staffUser.FullName;
                }
            }
        }

        public async Task<IActionResult> OnPostUpdatePreferencesAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                TempData["StatusMessage"] = "Error: User not found.";
                TempData["StatusType"] = "Error";
                return Page();
            }

            try
            {
                // Save preferences using the service
                await _preferencesService.SaveUserPreferencesAsync(
                    user.Id,
                    EmailNotifications,
                    SmsNotifications,
                    MarketingEmails
                );

                TempData["StatusMessage"] = "Your preferences have been saved successfully.";
                TempData["StatusType"] = "Success";
            }
            catch (Exception ex)
            {
                TempData["StatusMessage"] = $"Error: {ex.Message}";
                TempData["StatusType"] = "Error";
            }

            return RedirectToPage(new { activeTab = "preferences" });
        }
    }
}