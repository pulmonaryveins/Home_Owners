// HomeOwners/Areas/Admin/Pages/EditFacility.cshtml.cs
using System;
using System.IO;
using System.Threading.Tasks;
using HomeOwners.Models;
using HomeOwners.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace HomeOwners.Areas.Admin.Pages
{
    [Authorize(Policy = "RequireAdminRole")]
    public class EditFacilityModel : PageModel
    {
        private readonly FacilityService _facilityService;
        private readonly IWebHostEnvironment _environment;

        public EditFacilityModel(FacilityService facilityService, IWebHostEnvironment environment)
        {
            _facilityService = facilityService;
            _environment = environment;
        }

        [BindProperty]
        public Facility Facility { get; set; }

        [BindProperty]
        public IFormFile ImageFile { get; set; }

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
            // Explicitly remove ImageFile validation errors if present
            // This ensures the model is valid even if no image is provided
            if (ModelState.ContainsKey("ImageFile"))
            {
                ModelState.Remove("ImageFile");
            }
            
            if (!ModelState.IsValid)
            {
                TempData["ErrorMsg"] = "Please check the form for errors.";
                return Page();
            }

            try
            {
                // Store the original image URL in case we need to restore it
                string originalImageUrl = Facility.ImageUrl;
                
                // Handle image upload if a new image is provided
                if (ImageFile != null && ImageFile.Length > 0)
                {
                    // Generate a unique filename
                    string uniqueFileName = Guid.NewGuid().ToString() + "_" + ImageFile.FileName;

                    // Ensure directory exists
                    string uploadsFolder = Path.Combine(_environment.WebRootPath, "images", "facilities");
                    if (!Directory.Exists(uploadsFolder))
                    {
                        Directory.CreateDirectory(uploadsFolder);
                    }

                    // Delete old image if exists and is not a placeholder
                    if (!string.IsNullOrEmpty(originalImageUrl) && !originalImageUrl.EndsWith("placeholder.jpg"))
                    {
                        string oldImagePath = Path.Combine(_environment.WebRootPath, originalImageUrl.TrimStart('/'));
                        if (System.IO.File.Exists(oldImagePath))
                        {
                            System.IO.File.Delete(oldImagePath);
                        }
                    }

                    // Save the new file
                    string filePath = Path.Combine(uploadsFolder, uniqueFileName);
                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        await ImageFile.CopyToAsync(fileStream);
                    }

                    // Set the ImageUrl property to the relative path
                    Facility.ImageUrl = "/images/facilities/" + uniqueFileName;
                }
                // No else clause needed - if no image is uploaded, keep the existing ImageUrl

                // Update the LastUpdated timestamp
                Facility.LastUpdated = DateTime.Now;

                await _facilityService.UpdateFacilityAsync(Facility);

                TempData["StatusMessage"] = "Facility updated successfully.";
                TempData["StatusType"] = "Success";

                return RedirectToPage("./Facilities");
            }
            catch (Exception ex)
            {
                TempData["ErrorMsg"] = $"Error updating facility: {ex.Message}";
                return Page();
            }
        }
    }
}