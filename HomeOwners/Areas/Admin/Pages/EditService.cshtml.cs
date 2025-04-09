// HomeOwners/Areas/Admin/Pages/EditService.cshtml.cs
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
    public class EditServiceModel : PageModel
    {
        private readonly ServiceService _serviceService;
        private readonly IWebHostEnvironment _environment;

        public EditServiceModel(ServiceService serviceService, IWebHostEnvironment environment)
        {
            _serviceService = serviceService;
            _environment = environment;
        }

        [BindProperty]
        public Service Service { get; set; }

        [BindProperty]
        public IFormFile ImageFile { get; set; }

        public async Task<IActionResult> OnGetAsync(int id)
        {
            Service = await _serviceService.GetServiceByIdAsync(id);

            if (Service == null)
            {
                return NotFound();
            }

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            // Explicitly remove ImageFile validation errors if present
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
                string originalImageUrl = Service.ImageUrl;

                // Handle image upload if a new image is provided
                if (ImageFile != null && ImageFile.Length > 0)
                {
                    // Generate a unique filename
                    string uniqueFileName = Guid.NewGuid().ToString() + "_" + ImageFile.FileName;

                    // Ensure directory exists
                    string uploadsFolder = Path.Combine(_environment.WebRootPath, "images", "services");
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

                    // Update the ImageUrl property
                    Service.ImageUrl = "/images/services/" + uniqueFileName;
                }

                await _serviceService.UpdateServiceAsync(Service);
                TempData["StatusMessage"] = "Service updated successfully.";
                TempData["StatusType"] = "Success";
                return RedirectToPage("./Services");
            }
            catch (Exception ex)
            {
                TempData["ErrorMsg"] = $"Error updating service: {ex.Message}";
                return Page();
            }
        }
    }
}