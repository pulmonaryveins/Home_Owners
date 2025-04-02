using System;
using System.Threading.Tasks;
using HomeOwners.Areas.Identity.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using System.Linq;
using System.Collections.Generic;
using HomeOwners.Models.Users;

namespace HomeOwners.Areas.Admin.Pages
{
    [Authorize(Policy = "RequireAdminRole")]
    public class DeleteModel : PageModel
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly ILogger<DeleteModel> _logger;

        public DeleteModel(
            UserManager<IdentityUser> userManager,
            ILogger<DeleteModel> logger)
        {
            _userManager = userManager;
            _logger = logger;
        }

        [BindProperty]
        public IdentityUser User { get; set; }

        public List<string> UserRoles { get; set; }
        public string UserType { get; set; }

        public async Task<IActionResult> OnGetAsync(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            User = await _userManager.FindByIdAsync(id);
            if (User == null)
            {
                return NotFound();
            }

            // Check if trying to delete your own account
            if (User.UserName == _userManager.GetUserName(HttpContext.User))
            {
                TempData["StatusMessage"] = "Error: You cannot delete your own account.";
                TempData["StatusType"] = "Error";
                return RedirectToPage("./Users");
            }

            UserRoles = (await _userManager.GetRolesAsync(User)).ToList();

            // Identify user type
            if (User is AdminUser)
            {
                UserType = "Admin";
            }
            else if (User is StaffUser)
            {
                UserType = "Staff";
            }
            else if (User is HomeOwnerUser)
            {
                UserType = "HomeOwner";
            }
            else
            {
                UserType = "Standard";
            }

            return Page();
        }

        public async Task<IActionResult> OnPostAsync(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            User = await _userManager.FindByIdAsync(id);
            if (User == null)
            {
                return NotFound();
            }

            // Check if trying to delete your own account
            if (User.UserName == _userManager.GetUserName(HttpContext.User))
            {
                TempData["StatusMessage"] = "Error: You cannot delete your own account.";
                TempData["StatusType"] = "Error";
                return RedirectToPage("./Users");
            }

            var result = await _userManager.DeleteAsync(User);
            if (result.Succeeded)
            {
                _logger.LogInformation("User {Username} with ID {UserId} deleted by admin.", User.UserName, User.Id);

                TempData["StatusMessage"] = $"User '{User.UserName}' was successfully deleted.";
                TempData["StatusType"] = "Success";
                return RedirectToPage("./Users");
            }
            else
            {
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }

                UserRoles = (await _userManager.GetRolesAsync(User)).ToList();
                return Page();
            }
        }
    }
}
