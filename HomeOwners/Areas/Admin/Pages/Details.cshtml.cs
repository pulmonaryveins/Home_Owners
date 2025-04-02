using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Collections.Generic;
using System.Threading.Tasks;
using HomeOwners.Models.Users;
using Microsoft.AspNetCore.Authorization;
using System;
using HomeOwners.Services;

namespace HomeOwners.Areas.Admin.Pages
{
    [Authorize(Policy = "RequireAdminRole")]
    public class DetailsModel : PageModel
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly UserService _userService;

        public DetailsModel(UserManager<IdentityUser> userManager, UserService userService)
        {
            _userManager = userManager;
            _userService = userService;
        }

        public IdentityUser UserDetails { get; set; }
        public List<string> UserRoles { get; set; }
        public DateTime? CreatedDate { get; set; }
        public string FullName { get; set; }

        public async Task<IActionResult> OnGetAsync(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            UserDetails = await _userManager.FindByIdAsync(id);
            if (UserDetails == null)
            {
                return NotFound();
            }

            UserRoles = (await _userManager.GetRolesAsync(UserDetails)).ToList();

            // Set default values
            CreatedDate = DateTime.Now;
            FullName = string.Empty;

            // Populate type-specific properties based on user role
            if (UserRoles.Contains("HomeOwner"))
            {
                var homeOwner = await _userService.GetHomeOwnerUserAsync(id);
                if (homeOwner != null)
                {
                    CreatedDate = homeOwner.CreatedDate;
                    FullName = homeOwner.FullName;
                }
            }
            else if (UserRoles.Contains("Admin"))
            {
                var admin = await _userService.GetAdminUserAsync(id);
                if (admin != null)
                {
                    CreatedDate = admin.CreatedDate;
                    FullName = admin.FullName;
                }
            }
            else if (UserRoles.Contains("Staff"))
            {
                var staff = await _userService.GetStaffUserAsync(id);
                if (staff != null)
                {
                    CreatedDate = staff.CreatedDate;
                    FullName = staff.FullName;
                }
            }

            return Page();
        }
    }
}
