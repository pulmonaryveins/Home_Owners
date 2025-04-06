// HomeOwners/Areas/Admin/Pages/DeleteFacility.cshtml.cs
using System.IO;
using System.Threading.Tasks;
using HomeOwners.Models;
using HomeOwners.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace HomeOwners.Areas.Admin.Pages
{
    [Authorize(Policy = "RequireAdminRole")]
    public class DeleteFacilityModel : PageModel
    {
        private readonly FacilityService _facilityService;
        private readonly IWebHostEnvironment _environment;

        public DeleteFacilityModel(FacilityService facilityService, IWebHostEnvironment environment)
        {
            _facilityService = facilityService;
            _environment = environment;
        }

        [BindProperty]
        public Facility Facility { get; set; }

        public async Task<IActionResult> OnGetAsync(int id)
        {
            Facility = await _facilityService.GetFacilityByIdAsync(id);

            if (Facility == null)
            {
                return NotFound();
            }

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            var facilityToDelete = await _facilityService.GetFacilityByIdAsync(Facility.Id);

            if (facilityToDelete == null)
            {
                return NotFound();
            }

            // Delete the facility image if it exists and is not a placeholder
            if (!string.IsNullOrEmpty(facilityToDelete.ImageUrl) && facilityToDelete.ImageUrl != "/images/placeholder.jpg")
            {
                string imagePath = Path.Combine(_environment.WebRootPath, facilityToDelete.ImageUrl.TrimStart('/'));
                if (System.IO.File.Exists(imagePath))
                {
                    System.IO.File.Delete(imagePath);
                }
            }

            await _facilityService.DeleteFacilityAsync(Facility.Id);

            TempData["StatusMessage"] = "Facility deleted successfully.";
            TempData["StatusType"] = "Success";

            return RedirectToPage("./Facilities");
        }
    }
}