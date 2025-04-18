// HomeOwners/Areas/Admin/Pages/ServicePersonnel.cshtml.cs
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HomeOwners.Models;
using HomeOwners.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace HomeOwners.Areas.Admin.Pages
{
    [Authorize(Policy = "RequireAdminRole")]
    public class ServicePersonnelModel : PageModel
    {
        private readonly ServicePersonnelService _servicePersonnelService;
        private readonly ServiceService _serviceService;

        public ServicePersonnelModel(ServicePersonnelService servicePersonnelService, ServiceService serviceService)
        {
            _servicePersonnelService = servicePersonnelService;
            _serviceService = serviceService;
        }

        public List<ServicePersonnel> ServicePersonnelList { get; set; } = new List<ServicePersonnel>();
        public List<Service> ServicesList { get; set; } = new List<Service>();
        public SelectList ServiceSelectList { get; set; }
        
        // Pagination properties
        public int PageSize { get; set; } = 10;
        public int CurrentPage { get; set; } = 1;
        public int TotalPages { get; set; }
        public int TotalCount { get; set; }
        
        // Status filter
        [BindProperty(SupportsGet = true)]
        public string StatusFilter { get; set; }

        public async Task OnGetAsync(string searchString, int? serviceFilter, string statusFilter, int pageNumber = 1)
        {
            CurrentPage = pageNumber < 1 ? 1 : pageNumber;

            ServicesList = await _serviceService.GetAllServicesAsync();
            var allPersonnel = await _servicePersonnelService.GetAllServicePersonnelAsync();

            // Apply filters if any
            if (!string.IsNullOrEmpty(searchString))
            {
                allPersonnel = allPersonnel.Where(p => p.TeamName.Contains(searchString, StringComparison.OrdinalIgnoreCase)).ToList();
                ViewData["CurrentFilter"] = searchString;
            }

            if (serviceFilter.HasValue && serviceFilter > 0)
            {
                allPersonnel = allPersonnel.Where(p => p.ServiceId == serviceFilter).ToList();
                var filteredService = ServicesList.FirstOrDefault(s => s.Id == serviceFilter);
                ViewData["ServiceFilter"] = filteredService?.Name ?? "All Services";
                ViewData["ServiceFilterId"] = serviceFilter;
            }

            // Apply status filter
            if (!string.IsNullOrEmpty(statusFilter))
            {
                if (statusFilter.Equals("Active", StringComparison.OrdinalIgnoreCase))
                {
                    allPersonnel = allPersonnel.Where(p => p.IsActive).ToList();
                    ViewData["StatusFilter"] = "Active";
                }
                else if (statusFilter.Equals("Inactive", StringComparison.OrdinalIgnoreCase))
                {
                    allPersonnel = allPersonnel.Where(p => !p.IsActive).ToList();
                    ViewData["StatusFilter"] = "Inactive";
                }
            }

            // Calculate total pages after filtering
            TotalCount = allPersonnel.Count;
            TotalPages = (int)Math.Ceiling(TotalCount / (double)PageSize);

            // Apply pagination
            ServicePersonnelList = allPersonnel
                .Skip((CurrentPage - 1) * PageSize)
                .Take(PageSize)
                .ToList();
        }

        public async Task<IActionResult> OnPostCreateAsync(string teamName, int numberOfPersonnel, int serviceId)
        {
            if (string.IsNullOrEmpty(teamName) || numberOfPersonnel <= 0 || serviceId <= 0)
            {
                TempData["StatusMessage"] = "Invalid form data. Please check your inputs and try again.";
                TempData["StatusType"] = "Error";
                return RedirectToPage();
            }

            var newPersonnel = new ServicePersonnel
            {
                TeamName = teamName,
                NumberOfPersonnel = numberOfPersonnel,
                ServiceId = serviceId,
                CreatedDate = DateTime.Now,
                IsActive = true
            };

            await _servicePersonnelService.CreateServicePersonnelAsync(newPersonnel);

            TempData["StatusMessage"] = "Service personnel team created successfully.";
            TempData["StatusType"] = "Success";

            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostEditAsync(int id, string teamName, int numberOfPersonnel, int serviceId, string isActive)
        {
            if (id <= 0 || string.IsNullOrEmpty(teamName) || numberOfPersonnel <= 0 || serviceId <= 0)
            {
                TempData["StatusMessage"] = "Invalid form data. Please check your inputs and try again.";
                TempData["StatusType"] = "Error";
                return RedirectToPage();
            }

            var personnel = await _servicePersonnelService.GetServicePersonnelByIdAsync(id);
            if (personnel == null)
            {
                TempData["StatusMessage"] = "Service personnel team not found.";
                TempData["StatusType"] = "Error";
                return RedirectToPage();
            }

            personnel.TeamName = teamName;
            personnel.NumberOfPersonnel = numberOfPersonnel;
            personnel.ServiceId = serviceId;

            // Fix for checkbox handling
            personnel.IsActive = isActive == "true";

            await _servicePersonnelService.UpdateServicePersonnelAsync(personnel);

            TempData["StatusMessage"] = "Service personnel team updated successfully.";
            TempData["StatusType"] = "Success";

            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostDeleteAsync(int id)
        {
            if (id <= 0)
            {
                TempData["StatusMessage"] = "Invalid ID.";
                TempData["StatusType"] = "Error";
                return RedirectToPage();
            }

            await _servicePersonnelService.DeleteServicePersonnelAsync(id);

            TempData["StatusMessage"] = "Service personnel team deleted successfully.";
            TempData["StatusType"] = "Success";

            return RedirectToPage();
        }
        
        // Helper method to generate page URL for pagination
        public string GetPageUrl(int pageNumber, string search = null, int? serviceFilter = null, string statusFilter = null)
        {
            return Url.Page("./ServicePersonnel", new { 
                pageNumber, 
                searchString = search ?? ViewData["CurrentFilter"],
                serviceFilter = serviceFilter ?? ViewData["ServiceFilterId"],
                statusFilter
            });
        }
    }
}