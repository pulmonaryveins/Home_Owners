// HomeOwners/Areas/Admin/Pages/Services.cshtml.cs
using System.Collections.Generic;
using System.Threading.Tasks;
using HomeOwners.Models;
using HomeOwners.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace HomeOwners.Areas.Admin.Pages
{
    [Authorize(Policy = "RequireAdminRole")]
    public class ServicesModel : PageModel
    {
        private readonly ServiceService _serviceService;

        public ServicesModel(ServiceService serviceService)
        {
            _serviceService = serviceService;
        }

        public List<Service> Services { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            Services = await _serviceService.GetAllServicesAsync();
            return Page();
        }

        public async Task<IActionResult> OnPostDeleteAsync(int id)
        {
            await _serviceService.DeleteServiceAsync(id);

            TempData["StatusMessage"] = "Service deleted successfully.";
            TempData["StatusType"] = "Success";

            return RedirectToPage();
        }
    }
}