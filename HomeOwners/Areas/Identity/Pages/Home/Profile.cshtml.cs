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

            Username = user.UserName;
            Email = user.Email;
            PhoneNumber = user.PhoneNumber;

            // Instead of direct casting, query specific user types from the database
            var userId = user.Id;

            // Try to get HomeOwnerUser
            var homeOwner = await _context.HomeOwnerUsers.FirstOrDefaultAsync(h => h.Id == userId);
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
                return Page();
            }

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                TempData["StatusMessage"] = "Error: User not found.";
                TempData["StatusType"] = "Error";
                return Page();
            }

            bool hasChanges = false;

            if (user.UserName != Username)
            {
                hasChanges = true;
                var setUsernameResult = await _userManager.SetUserNameAsync(user, Username);
                if (!setUsernameResult.Succeeded)
                {
                    foreach (var error in setUsernameResult.Errors)
                    {
                        ModelState.AddModelError(string.Empty, error.Description);
                    }
                    TempData["StatusMessage"] = "Error: Username could not be updated.";
                    TempData["StatusType"] = "Error";
                    return Page();
                }
            }

            if (user.Email != Email)
            {
                hasChanges = true;
                var setEmailResult = await _userManager.SetEmailAsync(user, Email);
                if (!setEmailResult.Succeeded)
                {
                    foreach (var error in setEmailResult.Errors)
                    {
                        ModelState.AddModelError(string.Empty, error.Description);
                    }
                    TempData["StatusMessage"] = "Error: Email could not be updated.";
                    TempData["StatusType"] = "Error";
                    return Page();
                }
            }

            if (user.PhoneNumber != PhoneNumber)
            {
                hasChanges = true;
                var setPhoneResult = await _userManager.SetPhoneNumberAsync(user, PhoneNumber);
                if (!setPhoneResult.Succeeded)
                {
                    foreach (var error in setPhoneResult.Errors)
                    {
                        ModelState.AddModelError(string.Empty, error.Description);
                    }
                    TempData["StatusMessage"] = "Error: Phone number could not be updated.";
                    TempData["StatusType"] = "Error";
                    return Page();
                }
            }

            // Update user type-specific properties
            var userId = user.Id;

            // Try to get HomeOwnerUser
            var homeOwner = await _context.HomeOwnerUsers.FirstOrDefaultAsync(h => h.Id == userId);
            if (homeOwner != null)
            {
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
                    _context.HomeOwnerUsers.Update(homeOwner);
                    await _context.SaveChangesAsync();
                }
            }
            else
            {
                // Check for ApplicationUser
                var appUser = await _context.Set<HomeOwners.Models.Users.ApplicationUser>().FirstOrDefaultAsync(a => a.Id == userId);
                if (appUser != null && appUser.FullName != FullName)
                {
                    appUser.FullName = FullName;
                    hasChanges = true;
                    _context.Set<HomeOwners.Models.Users.ApplicationUser>().Update(appUser);
                    await _context.SaveChangesAsync();
                }
                else
                {
                    // Check for AdminUser
                    var adminUser = await _context.AdminUsers.FirstOrDefaultAsync(a => a.Id == userId);
                    if (adminUser != null && adminUser.FullName != FullName)
                    {
                        adminUser.FullName = FullName;
                        hasChanges = true;
                        _context.AdminUsers.Update(adminUser);
                        await _context.SaveChangesAsync();
                    }
                    else
                    {
                        // Check for StaffUser
                        var staffUser = await _context.StaffUsers.FirstOrDefaultAsync(s => s.Id == userId);
                        if (staffUser != null && staffUser.FullName != FullName)
                        {
                            staffUser.FullName = FullName;
                            hasChanges = true;
                            _context.StaffUsers.Update(staffUser);
                            await _context.SaveChangesAsync();
                        }
                    }
                }
            }

            if (hasChanges)
            {
                await _signInManager.RefreshSignInAsync(user);
                TempData["StatusMessage"] = "Your profile has been updated successfully.";
                TempData["StatusType"] = "Success";
            }
            else
            {
                TempData["StatusMessage"] = "No changes detected.";
                TempData["StatusType"] = "Info";
            }

            return RedirectToPage(new { activeTab = "personal" });
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
                Username = user.UserName;
                Email = user.Email;
                PhoneNumber = user.PhoneNumber;

                var userId = user.Id;

                // Try to get HomeOwnerUser
                var homeOwner = await _context.HomeOwnerUsers.FirstOrDefaultAsync(h => h.Id == userId);
                if (homeOwner != null)
                {
                    FullName = homeOwner.FullName;
                    HouseNumber = homeOwner.HouseNumber;
                    AccountStatus = homeOwner.AccountStatus;
                    return;
                }

                // Check for ApplicationUser using fully qualified name
                var appUser = await _context.Set<HomeOwners.Models.Users.ApplicationUser>().FirstOrDefaultAsync(a => a.Id == userId);
                if (appUser != null)
                {
                    FullName = appUser.FullName;
                    return;
                }

                // Check for AdminUser
                var adminUser = await _context.AdminUsers.FirstOrDefaultAsync(a => a.Id == userId);
                if (adminUser != null)
                {
                    FullName = adminUser.FullName;
                    return;
                }

                // Check for StaffUser
                var staffUser = await _context.StaffUsers.FirstOrDefaultAsync(s => s.Id == userId);
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