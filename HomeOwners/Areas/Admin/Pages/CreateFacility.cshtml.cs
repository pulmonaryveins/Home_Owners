// HomeOwners/Areas/Admin/Pages/CreateFacility.cshtml.cs
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
    public class CreateFacilityModel : PageModel
    {
        private readonly FacilityService _facilityService;
        private readonly IWebHostEnvironment _environment;

        public CreateFacilityModel(FacilityService facilityService, IWebHostEnvironment environment)
        {
            _facilityService = facilityService;
            _environment = environment;
        }

        [BindProperty]
        public Facility Facility { get; set; }

        [BindProperty]
        public IFormFile ImageFile { get; set; }

        public void OnGet()
        {
            Facility = new Facility
            {
                IsActive = true,
                CreatedDate = DateTime.Now
            };
        }

        public async Task<IActionResult> OnPostAsync()
        {
            // First handle image upload if provided, BEFORE model validation
            if (ImageFile != null)
            {
                try
                {
                    // Generate a unique filename
                    string uniqueFileName = Guid.NewGuid().ToString() + "_" + ImageFile.FileName;

                    // Ensure directory exists
                    string uploadsFolder = Path.Combine(_environment.WebRootPath, "images", "facilities");
                    if (!Directory.Exists(uploadsFolder))
                    {
                        Directory.CreateDirectory(uploadsFolder);
                    }

                    // Save the file
                    string filePath = Path.Combine(uploadsFolder, uniqueFileName);
                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        await ImageFile.CopyToAsync(fileStream);
                    }

                    // Set the ImageUrl property to the relative path
                    Facility.ImageUrl = "/images/facilities/" + uniqueFileName;
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError(string.Empty, $"Error uploading image: {ex.Message}");
                    TempData["ErrorMsg"] = $"Error uploading image: {ex.Message}";
                    return Page();
                }
            }
            else
            {
                // If no image is provided, use a default image
                Facility.ImageUrl = "/images/placeholder.jpg";
            }

            // Now check model validation
            if (!ModelState.IsValid)
            {
                foreach (var modelState in ModelState.Values)
                {
                    foreach (var error in modelState.Errors)
                    {
                        TempData["ErrorMsg"] = (TempData["ErrorMsg"] ?? string.Empty) + error.ErrorMessage + "; ";
                    }
                }
                return Page();
            }

            try
            {
                await _facilityService.CreateFacilityAsync(Facility);
                TempData["StatusMessage"] = "Facility created successfully.";
                TempData["StatusType"] = "Success";
                return RedirectToPage("./Facilities");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, $"Error creating facility: {ex.Message}");
                TempData["ErrorMsg"] = $"Error: {ex.Message}";
                return Page();
            }
        }
    }
}