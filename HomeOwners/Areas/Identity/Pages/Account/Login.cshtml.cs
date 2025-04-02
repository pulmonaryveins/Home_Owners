// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
#nullable disable

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using HomeOwners.Models.Users;

namespace HomeOwners.Areas.Identity.Pages.Account
{
    public class LoginModel : PageModel
    {
        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly ILogger<LoginModel> _logger;

        public LoginModel(SignInManager<IdentityUser> signInManager,
                 UserManager<IdentityUser> userManager,
                 ILogger<LoginModel> logger)
        {
            _signInManager = signInManager;
            _userManager = userManager;
            _logger = logger;
        }

        [BindProperty]
        public InputModel Input { get; set; }

        public IList<AuthenticationScheme> ExternalLogins { get; set; }

        public string ReturnUrl { get; set; }

        [TempData]
        public string ErrorMessage { get; set; }

        public bool PendingApproval { get; set; }

        public class InputModel
        {
            [Required]
            [Display(Name = "Username")]
            public string Username { get; set; }

            [Required]
            [DataType(DataType.Password)]
            public string Password { get; set; }

            [Display(Name = "Remember me?")]
            public bool RememberMe { get; set; }
        }

        public async Task OnGetAsync(string returnUrl = null, bool pendingApproval = false)
        {
            if (!string.IsNullOrEmpty(ErrorMessage))
            {
                ModelState.AddModelError(string.Empty, ErrorMessage);
            }

            PendingApproval = pendingApproval;

            returnUrl ??= Url.Content("~/");

            // Clear the existing external cookie to ensure a clean login process
            await HttpContext.SignOutAsync(IdentityConstants.ExternalScheme);

            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();

            ReturnUrl = returnUrl;
        }

        public async Task<IActionResult> OnPostAsync(string returnUrl = null)
        {
            returnUrl ??= Url.Content("~/");

            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();

            if (ModelState.IsValid)
            {
                // First find the user by username
                var user = await _userManager.FindByNameAsync(Input.Username);

                // Check if user exists and is a HomeOwner with pending status
                if (user != null && await _userManager.IsInRoleAsync(user, "HomeOwner"))
                {
                    if (user is HomeOwnerUser homeOwnerUser && homeOwnerUser.AccountStatus == "pending")
                    {
                        ModelState.AddModelError(string.Empty, "Your account is pending admin approval. Please wait for verification.");
                        return Page();
                    }
                    else if (user is HomeOwnerUser rejectedUser && rejectedUser.AccountStatus == "rejected")
                    {
                        string rejectionMessage = string.IsNullOrEmpty(rejectedUser.RejectionReason)
                            ? "Your account has been rejected by the administrator."
                            : $"Your account has been rejected: {rejectedUser.RejectionReason}";

                        ModelState.AddModelError(string.Empty, rejectionMessage);
                        return Page();
                    }
                }

                var result = await _signInManager.PasswordSignInAsync(Input.Username, Input.Password, Input.RememberMe, lockoutOnFailure: false);
                if (result.Succeeded)
                {
                    _logger.LogInformation("User logged in.");

                    // Check if the user is an admin and redirect accordingly
                    if (user != null)
                    {
                        // Check user roles and redirect accordingly
                        if (await _userManager.IsInRoleAsync(user, "Admin"))
                        {
                            return LocalRedirect("/Admin/Dashboard");
                        }
                        else if (await _userManager.IsInRoleAsync(user, "Staff"))
                        {
                            return LocalRedirect("/Staff/Dashboard");
                        }
                    }

                    return LocalRedirect(returnUrl);
                }
                if (result.RequiresTwoFactor)
                {
                    return RedirectToPage("./LoginWith2fa", new { ReturnUrl = returnUrl, RememberMe = Input.RememberMe });
                }
                if (result.IsLockedOut)
                {
                    _logger.LogWarning("User account locked out.");
                    return RedirectToPage("./Lockout");
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "Invalid login attempt.");
                    return Page();
                }
            }

            // If we got this far, something failed, redisplay form
            return Page();
        }
    }
}