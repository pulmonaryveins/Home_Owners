// HomeOwners/Areas/Admin/Pages/ServiceRequests.cshtml.cs
using System.Collections.Generic;
using System.Threading.Tasks;
using HomeOwners.Models;
using HomeOwners.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace HomeOwners.Areas.Admin.Pages
{
    [Authorize(Roles = "Admin")]
    public class ServiceRequestsModel : PageModel
    {
        private readonly ServiceRequestService _serviceRequestService;

        public ServiceRequestsModel(ServiceRequestService serviceRequestService)
        {
            _serviceRequestService = serviceRequestService;
        }

        public List<ServiceRequest> PendingRequests { get; set; } = new List<ServiceRequest>();
        public List<ServiceRequest> AllRequests { get; set; } = new List<ServiceRequest>();

        public async Task OnGetAsync()
        {
            PendingRequests = await _serviceRequestService.GetPendingServiceRequestsAsync();
            AllRequests = await _serviceRequestService.GetAllServiceRequestsAsync();
        }

        public async Task<IActionResult> OnPostApproveRequestAsync(int id)
        {
            await _serviceRequestService.UpdateServiceRequestStatusAsync(id, ServiceRequestStatus.Approved);
            TempData["StatusMessage"] = "Service request has been approved successfully.";
            TempData["StatusType"] = "Success";
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostRejectRequestAsync(int id)
        {
            await _serviceRequestService.UpdateServiceRequestStatusAsync(id, ServiceRequestStatus.Rejected);
            TempData["StatusMessage"] = "Service request has been rejected.";
            TempData["StatusType"] = "Success";
            return RedirectToPage();
        }
    }
}