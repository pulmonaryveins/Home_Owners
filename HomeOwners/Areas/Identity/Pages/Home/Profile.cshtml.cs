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
        private readonly UserManager<ApplicationUser> _userManager; // Change this
        private readonly SignInManager<ApplicationUser> _signInManager; // Change this
        // Add your user preferences service if you have one
        // private readonly IUserPreferencesService _preferencesService;

        public ProfileModel(
            UserManager<ApplicationUser> userManager, // Change this
            SignInManager<ApplicationUser> signInManager) // Change this
                                                          
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
            
            // Get custom user properties if you have them
            // You might need to fetch from another table or use claims
            // Example using a custom user class:
            // if (user is ApplicationUser appUser)
            // {
            //     FullName = appUser.FullName;
            // }
            
            // For preferences, you might have a separate service
            // var preferences = await _preferencesService.GetUserPreferencesAsync(user.Id);
            // EmailNotifications = preferences.EmailNotifications;
            // SmsNotifications = preferences.SmsNotifications;
            // MarketingEmails = preferences.MarketingEmails;

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
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
                    return Page();
                }
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
                    return Page();
                }
            }
            
            // If you have custom user properties like FullName, update them here
            // Example:
            // if (user is ApplicationUser appUser)
            // {
            //     appUser.FullName = FullName;
            //     await _userManager.UpdateAsync(appUser);
            // }

            // Force a refresh sign-in to update any claim-based values
            await _signInManager.RefreshSignInAsync(user);

            TempData["StatusMessage"] = "Your profile has been updated successfully.";
            TempData["StatusType"] = "Success";
            
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostUpdatePasswordAsync()
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
