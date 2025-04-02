using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using HomeOwners.Areas.Identity.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using HomeOwners.Models.Users;

namespace HomeOwners.Areas.Admin.Pages
{
    [Authorize(Policy = "RequireAdminRole")]
    public class EditModel : PageModel
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly ILogger<EditModel> _logger;

        public EditModel(
            UserManager<IdentityUser> userManager,
            RoleManager<IdentityRole> roleManager,
            ILogger<EditModel> logger)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _logger = logger;
        }

        [BindProperty]
        public InputModel Input { get; set; }

        public List<string> AvailableRoles { get; set; }
        public string UserId { get; set; }
        public string UserType { get; set; }
        public bool IsHomeOwner { get; set; }

        public class InputModel
        {
            [Required]
            [Display(Name = "Username")]
            [StringLength(100, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 3)]
            public string Username { get; set; }

            [Required]
            [EmailAddress]
            [Display(Name = "Email")]
            public string Email { get; set; }

            [StringLength(100, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 6)]
            [DataType(DataType.Password)]
            [Display(Name = "New password (leave blank to keep current)")]
            public string Password { get; set; }

            [DataType(DataType.Password)]
            [Display(Name = "Confirm new password")]
            [Compare("Password", ErrorMessage = "The new password and confirmation password do not match.")]
            public string ConfirmPassword { get; set; }

            public List<string> SelectedRoles { get; set; } = new List<string>();

            // HomeOwner specific properties
            [Display(Name = "Full Name")]
            public string FullName { get; set; }

            [Display(Name = "Phone Number")]
            [Phone]
            public string PhoneNumber { get; set; }

            [Display(Name = "House Number")]
            public string HouseNumber { get; set; }
        }

        private async Task<List<string>> GetAvailableRoles()
        {
            return await Task.FromResult(_roleManager.Roles.Select(r => r.Name).OrderBy(n => n).ToList());
        }

        public async Task<IActionResult> OnGetAsync(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            UserId = user.Id;

            // Set user type based on actual type
            if (user is AdminUser)
            {
                UserType = "Admin";
                IsHomeOwner = false;
            }
            else if (user is StaffUser)
            {
                UserType = "Staff";
                IsHomeOwner = false;
            }
            else if (user is HomeOwnerUser homeOwner)
            {
                UserType = "HomeOwner";
                IsHomeOwner = true;

                Input = new InputModel
                {
                    Username = user.UserName,
                    Email = user.Email,
                    SelectedRoles = (await _userManager.GetRolesAsync(user)).ToList(),
                    FullName = homeOwner.FullName,
                    PhoneNumber = homeOwner.PhoneNumber,
                    HouseNumber = homeOwner.HouseNumber
                };
            }
            else
            {
                UserType = "Standard";
                IsHomeOwner = false;

                Input = new InputModel
                {
                    Username = user.UserName,
                    Email = user.Email,
                    SelectedRoles = (await _userManager.GetRolesAsync(user)).ToList(),
                    PhoneNumber = user.PhoneNumber
                };
            }

            // If Input is not set yet (for non-HomeOwner users)
            if (Input == null)
            {
                Input = new InputModel
                {
                    Username = user.UserName,
                    Email = user.Email,
                    SelectedRoles = (await _userManager.GetRolesAsync(user)).ToList(),
                    PhoneNumber = user.PhoneNumber
                };
            }

            AvailableRoles = await GetAvailableRoles();
            return Page();
        }

        public async Task<IActionResult> OnPostAsync(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            UserId = user.Id;
            AvailableRoles = await GetAvailableRoles();

            if (!ModelState.IsValid)
            {
                return Page();
            }

            // Check if another user already has this email
            var existingUser = await _userManager.FindByEmailAsync(Input.Email);
            if (existingUser != null && existingUser.Id != id)
            {
                ModelState.AddModelError(string.Empty, "Email is already taken by another user.");
                return Page();
            }

            // Update user properties
            user.UserName = Input.Username;
            user.Email = Input.Email;
            user.PhoneNumber = Input.PhoneNumber;

            // Update HomeOwner specific properties if applicable
            if (user is HomeOwnerUser homeOwner)
            {
                homeOwner.FullName = Input.FullName;
                homeOwner.HouseNumber = Input.HouseNumber;
                IsHomeOwner = true;
            }

            // Update the user
            var updateResult = await _userManager.UpdateAsync(user);
            if (!updateResult.Succeeded)
            {
                foreach (var error in updateResult.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
                return Page();
            }

            // Update password if provided
            if (!string.IsNullOrEmpty(Input.Password))
            {
                // Remove the token-based approach that's causing issues
                var removePasswordResult = await _userManager.RemovePasswordAsync(user);
                if (!removePasswordResult.Succeeded)
                {
                    foreach (var error in removePasswordResult.Errors)
                    {
                        ModelState.AddModelError(string.Empty, error.Description);
                    }
                    return Page();
                }

                var addPasswordResult = await _userManager.AddPasswordAsync(user, Input.Password);
                if (!addPasswordResult.Succeeded)
                {
                    foreach (var error in addPasswordResult.Errors)
                    {
                        ModelState.AddModelError(string.Empty, error.Description);
                    }
                    return Page();
                }

                _logger.LogInformation("Password updated for user {Username}.", user.UserName);
            }

            // Update roles
            var currentRoles = await _userManager.GetRolesAsync(user);
            var rolesToRemove = currentRoles.Except(Input.SelectedRoles ?? new List<string>());
            var rolesToAdd = Input.SelectedRoles?.Except(currentRoles) ?? new List<string>();

            if (rolesToRemove.Any())
            {
                var removeResult = await _userManager.RemoveFromRolesAsync(user, rolesToRemove);
                if (!removeResult.Succeeded)
                {
                    foreach (var error in removeResult.Errors)
                    {
                        ModelState.AddModelError(string.Empty, error.Description);
                    }
                    return Page();
                }
            }

            if (rolesToAdd.Any())
            {
                var addResult = await _userManager.AddToRolesAsync(user, rolesToAdd);
                if (!addResult.Succeeded)
                {
                    foreach (var error in addResult.Errors)
                    {
                        ModelState.AddModelError(string.Empty, error.Description);
                    }
                    return Page();
                }
            }

            _logger.LogInformation("Admin updated user account for {UserName}.", user.UserName);
            TempData["StatusMessage"] = "User updated successfully.";
            TempData["StatusType"] = "Success";

            return RedirectToPage("./Users");
        }
    }
}
