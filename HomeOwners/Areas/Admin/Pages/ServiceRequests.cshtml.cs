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
        public Dictionary<int, ServiceRatingViewModel> ServiceRatings { get; set; } = new Dictionary<int, ServiceRatingViewModel>();

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

            // Populate ServiceRatings dictionary 
            ServiceRatings = new Dictionary<int, ServiceRatingViewModel>();

            // Use the service to get ratings instead of directly accessing _context
            var serviceRatings = await _serviceRequestService.GetServiceRatingsAsync();

            foreach (var rating in serviceRatings)
            {
                if (rating.ServiceRequestId > 0)
                {
                    ServiceRatings[rating.ServiceRequestId] = new ServiceRatingViewModel
                    {
                        Rating = rating.Rating,
                        Feedback = rating.Feedback,
                        SubmittedDate = rating.SubmittedDate  
                    };

                    // Mark the service request as having a rating
                    var request = AllRequests.FirstOrDefault(r => r.Id == rating.ServiceRequestId);
                    if (request != null)
                    {
                        request.HasRating = true;
                    }
                }
            }
        }

        public async Task<IActionResult> OnGetServiceRatingAsync(int id)
        {
            var rating = await _serviceRequestService.GetServiceRatingByServiceRequestIdAsync(id);
            if (rating == null)
            {
                return NotFound();
            }

            return new JsonResult(rating);
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

    public class ServiceRatingViewModel
    {
        public int Rating { get; set; }
        public string Feedback { get; set; }
        public DateTime SubmittedDate { get; set; }
    }
}