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
using HomeOwners.Models.Users; // Add this import

namespace HomeOwners.Areas.Admin.Pages
{
    [Authorize(Policy = "RequireAdminRole")]
    public class CreateModel : PageModel
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly ILogger<CreateModel> _logger;

        public CreateModel(
            UserManager<IdentityUser> userManager,
            RoleManager<IdentityRole> roleManager,
            ILogger<CreateModel> logger)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _logger = logger;
        }

        [BindProperty]
        public InputModel Input { get; set; }

        public List<string> AvailableRoles { get; set; }

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

            [Required]
            [StringLength(100, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 6)]
            [DataType(DataType.Password)]
            [Display(Name = "Password")]
            public string Password { get; set; }

            [DataType(DataType.Password)]
            [Display(Name = "Confirm password")]
            [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
            public string ConfirmPassword { get; set; }

            public List<string> SelectedRoles { get; set; } = new List<string>();

            [Display(Name = "User Type")]
            public string UserType { get; set; } = "Standard";
        }

        private async Task<List<string>> GetAvailableRoles()
        {
            return await Task.FromResult(_roleManager.Roles.Select(r => r.Name).OrderBy(n => n).ToList());
        }

        public async Task<IActionResult> OnGetAsync()
        {
            AvailableRoles = await GetAvailableRoles();
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            AvailableRoles = await GetAvailableRoles();

            if (ModelState.IsValid)
            {
                // Check if user with same email already exists
                var existingUserByEmail = await _userManager.FindByEmailAsync(Input.Email);
                if (existingUserByEmail != null)
                {
                    ModelState.AddModelError(string.Empty, "Email is already taken.");
                    return Page();
                }

                // Check if user with same username already exists
                var existingUserByName = await _userManager.FindByNameAsync(Input.Username);
                if (existingUserByName != null)
                {
                    ModelState.AddModelError(string.Empty, "Username is already taken.");
                    return Page();
                }

                // Create appropriate user type based on the selected roles
                IdentityUser user;

                if (Input.SelectedRoles.Contains("Admin"))
                {
                    user = new AdminUser
                    {
                        UserName = Input.Username,
                        Email = Input.Email,
                        EmailConfirmed = true,
                        CreatedDate = DateTime.Now,
                        FullName = Input.Username, // You might want to add a FullName field to your form
                        AdminLevel = "Standard"
                    };
                }
                else if (Input.SelectedRoles.Contains("Staff"))
                {
                    user = new StaffUser
                    {
                        UserName = Input.Username,
                        Email = Input.Email,
                        EmailConfirmed = true,
                        CreatedDate = DateTime.Now,
                        FullName = Input.Username,
                        Department = "General",
                        Position = "Staff"
                    };
                }
                else if (Input.SelectedRoles.Contains("HomeOwner"))
                {
                    user = new HomeOwnerUser
                    {
                        UserName = Input.Username,
                        Email = Input.Email,
                        EmailConfirmed = true,
                        CreatedDate = DateTime.Now,
                        FullName = Input.Username,
                        Address = "",
                        PropertyId = ""
                    };
                }
                else
                {
                    // Default to a standard IdentityUser if no specific role
                    user = new IdentityUser
                    {
                        UserName = Input.Username,
                        Email = Input.Email,
                        EmailConfirmed = true
                    };
                }

                var result = await _userManager.CreateAsync(user, Input.Password);

                if (result.Succeeded)
                {
                    _logger.LogInformation("Admin created a new user account for {UserName}.", user.UserName);

                    if (Input.SelectedRoles != null && Input.SelectedRoles.Any())
                    {
                        foreach (var role in Input.SelectedRoles)
                        {
                            // Check if role exists before adding user to it
                            if (await _roleManager.RoleExistsAsync(role))
                            {
                                await _userManager.AddToRoleAsync(user, role);
                                _logger.LogInformation("Added user {UserName} to role {Role}.", user.UserName, role);
                            }
                        }
                    }

                    TempData["StatusMessage"] = "User created successfully.";
                    TempData["StatusType"] = "Success";
                    return RedirectToPage("./Users");
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
