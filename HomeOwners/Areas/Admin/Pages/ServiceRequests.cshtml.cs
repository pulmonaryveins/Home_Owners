// HomeOwners/Areas/Admin/Pages/ServiceRequests.cshtml.cs
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
    [Authorize(Roles = "Admin")]
    public class ServiceRequestsModel : PageModel
    {
        private readonly ServiceRequestService _serviceRequestService;
        private readonly ServicePersonnelService _servicePersonnelService;

        public ServiceRequestsModel(ServiceRequestService serviceRequestService, ServicePersonnelService servicePersonnelService)
        {
            _serviceRequestService = serviceRequestService;
            _servicePersonnelService = servicePersonnelService;
        }

        public List<ServiceRequest> PendingRequests { get; set; } = new List<ServiceRequest>();
        public List<ServiceRequest> AllRequests { get; set; } = new List<ServiceRequest>();
        public Dictionary<int, List<ServicePersonnel>> AvailableTeams { get; set; } = new Dictionary<int, List<ServicePersonnel>>();

        public async Task OnGetAsync()
        {
            PendingRequests = await _serviceRequestService.GetPendingServiceRequestsAsync();
            AllRequests = await _serviceRequestService.GetAllServiceRequestsAsync();

            // Get available teams for each pending request
            foreach (var request in PendingRequests.Where(r => r.ServiceId > 0))
            {
                var teams = await _servicePersonnelService.GetServicePersonnelByServiceIdAsync(request.ServiceId);
                AvailableTeams[request.Id] = teams.Where(t => t.IsActive).ToList();
            }
        }

        public async Task<IActionResult> OnPostAssignTeamAsync(int requestId, int teamId)
        {
            if (teamId <= 0)
            {
                TempData["StatusMessage"] = "Please select a valid team.";
                TempData["StatusType"] = "Error";
                return RedirectToPage();
            }

            await _serviceRequestService.AssignTeamToServiceRequestAsync(requestId, teamId);

            TempData["StatusMessage"] = "Team has been assigned to the service request.";
            TempData["StatusType"] = "Success";
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostApproveRequestAsync(int id, int teamId)
        {
            if (teamId <= 0)
            {
                TempData["StatusMessage"] = "You must assign a team before approving the request.";
                TempData["StatusType"] = "Error";
                return RedirectToPage();
            }

            await _serviceRequestService.AssignTeamToServiceRequestAsync(id, teamId);
            await _serviceRequestService.UpdateServiceRequestStatusAsync(id, ServiceRequestStatus.Approved);

            TempData["StatusMessage"] = "Service request has been approved and team has been assigned.";
            TempData["StatusType"] = "Success";
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostClearAllRequestsAsync()
        {
            await _serviceRequestService.DeleteAllServiceRequestsAsync();

            TempData["StatusMessage"] = "All service requests have been successfully cleared from the database.";
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

        public async Task<IActionResult> OnPostMarkCompletedAsync(int id)
        {
            await _serviceRequestService.UpdateServiceRequestStatusAsync(id, ServiceRequestStatus.Done);
            TempData["StatusMessage"] = "Service request has been marked as done.";
            TempData["StatusType"] = "Success";
            return RedirectToPage();
        }
    }
}