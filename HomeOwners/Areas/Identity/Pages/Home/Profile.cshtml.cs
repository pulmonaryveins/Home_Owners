using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using HomeOwners.Areas.Identity.Data;

namespace HomeOwners.Areas.Identity.Pages.Home
{
    public class ProfileModel : PageModel
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        // Add your user preferences service if you have one
        // private readonly IUserPreferencesService _preferencesService;

        public ProfileModel(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager)
                                                          
        {
            _userManager = userManager;
            _signInManager = signInManager;
            // _preferencesService = preferencesService;
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
            FullName = user.FullName;

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

            if (user.FullName != FullName)
            {
                hasChanges = true;
                user.FullName = FullName;
                var updateResult = await _userManager.UpdateAsync(user);
                if (!updateResult.Succeeded)
                {
                    foreach (var error in updateResult.Errors)
                    {
                        ModelState.AddModelError(string.Empty, error.Description);
                    }
                    TempData["StatusMessage"] = "Error: Full name could not be updated.";
                    TempData["StatusType"] = "Error";
                    return Page();
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

            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostUpdatePasswordAsync()
        {
            if (string.IsNullOrEmpty(CurrentPassword) || string.IsNullOrEmpty(NewPassword) || string.IsNullOrEmpty(ConfirmPassword))
            {
                ModelState.AddModelError(string.Empty, "All password fields are required.");
                TempData["StatusMessage"] = "Error: All password fields are required.";
                TempData["StatusType"] = "Error";
                return Page();
            }

            if (NewPassword != ConfirmPassword)
            {
                ModelState.AddModelError(string.Empty, "The new password and confirmation password do not match.");
                TempData["StatusMessage"] = "Error: The new password and confirmation password do not match.";
                TempData["StatusType"] = "Error";
                return Page();
            }

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                TempData["StatusMessage"] = "Error: User not found.";
                TempData["StatusType"] = "Error";
                return Page();
            }

            var changePasswordResult = await _userManager.ChangePasswordAsync(user, CurrentPassword, NewPassword);
            if (!changePasswordResult.Succeeded)
            {
                foreach (var error in changePasswordResult.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
                
                TempData["StatusMessage"] = "Error: Password could not be changed.";
                TempData["StatusType"] = "Error";
                return Page();
            }

            await _signInManager.RefreshSignInAsync(user);
            
            TempData["StatusMessage"] = "Your password has been changed successfully.";
            TempData["StatusType"] = "Success";
            
            // Clear password fields
            CurrentPassword = string.Empty;
            NewPassword = string.Empty;
            ConfirmPassword = string.Empty;
            
            return RedirectToPage();
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

            // Save preferences to your database
            // You'll need to implement this based on your data model
            // Example:
            // await _preferencesService.SaveUserPreferencesAsync(user.Id, new UserPreferences
            // {
            //     EmailNotifications = EmailNotifications,
            //     SmsNotifications = SmsNotifications,
            //     MarketingEmails = MarketingEmails
            // });

            TempData["StatusMessage"] = "Your preferences have been saved successfully.";
            TempData["StatusType"] = "Success";
            
            return RedirectToPage();
        }
    }
}
