// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
#nullable disable

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Logging;
using HomeOwners.Models.Users;

namespace HomeOwners.Areas.Identity.Pages.Account
{
    public class RegisterModel : PageModel
    {
        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly ILogger<RegisterModel> _logger;
        private readonly IEmailSender _emailSender;

        public RegisterModel(
            UserManager<IdentityUser> userManager,
            SignInManager<IdentityUser> signInManager,
            ILogger<RegisterModel> logger,
            IEmailSender emailSender)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _logger = logger;
            _emailSender = emailSender;
        }

        [BindProperty]
        public InputModel Input { get; set; }

        public string ReturnUrl { get; set; }

        public IList<AuthenticationScheme> ExternalLogins { get; set; }

        [TempData]
        public string StatusMessage { get; set; }

        public class InputModel
        {
            [Required]
            [Display(Name = "Username")]
            [StringLength(100, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 3)]
            public string Username { get; set; }

            [Required]
            [Display(Name = "Full Name")]
            public string FullName { get; set; }

            [Required]
            [EmailAddress]
            [Display(Name = "Email")]
            public string Email { get; set; }

            [Required]
            [StringLength(100, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 6)]
            [DataType(DataType.Password)]
            [Display(Name = "Password")]
            public string Password { get; set; }

            [DataType(DataType.Password)]
            [Display(Name = "Confirm password")]
            [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
            public string ConfirmPassword { get; set; }

            [Required]
            [Phone]
            [Display(Name = "Phone Number")]
            public string PhoneNumber { get; set; }

            [Required]
            [Display(Name = "House Number")]
            public string HouseNumber { get; set; }
        }

        public async Task<IActionResult> OnPostAsync(string returnUrl = null)
        {
            returnUrl ??= Url.Content("~/");
            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();

            if (ModelState.IsValid)
            {
                var user = new HomeOwnerUser
                {
                    UserName = Input.Username,
                    Email = Input.Email,
                    EmailConfirmed = true,
                    PhoneNumber = Input.PhoneNumber,
                    FullName = Input.FullName ?? string.Empty,
                    HouseNumber = Input.HouseNumber,
                    AccountStatus = "pending" // Set initial status as pending
                };

                var result = await _userManager.CreateAsync(user, Input.Password);

                if (result.Succeeded)
                {
                    _logger.LogInformation("User created a new account with password.");

                    // Add user to HomeOwner role
                    await _userManager.AddToRoleAsync(user, "HomeOwner");

                    // Send an email notification to the user
                    if (_emailSender != null)
                    {
                        var emailSubject = "HomeOwners Registration - Account Pending Approval";
                        var emailMessage = $"Dear {user.FullName},<br><br>" +
                                          "Thank you for registering with our HomeOwners Association. " +
                                          "Your account has been created and is pending approval by an administrator. " +
                                          "You will receive a notification once your account has been reviewed.<br><br>" +
                                          "Regards,<br>The HomeOwners Association Team";

                        await _emailSender.SendEmailAsync(user.Email, emailSubject, emailMessage);
                    }

                    // Do not sign in the user automatically - important!
                    // await _signInManager.SignInAsync(user, isPersistent: false);

                    // Set a success message
                    StatusMessage = "Your account has been created and is pending approval by an administrator. You will be notified once your account has been approved.";

                    // Redirect to login page with status message
                    return RedirectToPage("./Login", new { pendingApproval = true });
                }

                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }

            // If we got this far, something failed, redisplay form
            return Page();
        }
    }
}