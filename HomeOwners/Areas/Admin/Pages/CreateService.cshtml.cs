// HomeOwners/Areas/Admin/Pages/CreateService.cshtml.cs
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
    public class CreateServiceModel : PageModel
    {
        private readonly ServiceService _serviceService;
        private readonly IWebHostEnvironment _environment;

        public CreateServiceModel(ServiceService serviceService, IWebHostEnvironment environment)
        {
            _serviceService = serviceService;
            _environment = environment;
        }

        [BindProperty]
        public Service Service { get; set; }

        [BindProperty]
        public IFormFile ImageFile { get; set; }

        public void OnGet()
        {
            Service = new Service
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
                    string uploadsFolder = Path.Combine(_environment.WebRootPath, "images", "services");
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
                    Service.ImageUrl = "/images/services/" + uniqueFileName;
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
                Service.ImageUrl = "/images/placeholder.jpg";
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
                await _serviceService.CreateServiceAsync(Service);
                TempData["StatusMessage"] = "Service created successfully.";
                TempData["StatusType"] = "Success";
                return RedirectToPage("./Services");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, $"Error creating service: {ex.Message}");
                TempData["ErrorMsg"] = $"Error: {ex.Message}";
                return Page();
            }
        }
    }
}