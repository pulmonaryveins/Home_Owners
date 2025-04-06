// HomeOwners/Areas/Admin/Pages/Facilities.cshtml.cs
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
    public class FacilitiesModel : PageModel
    {
        private readonly FacilityService _facilityService;
        private readonly BookingService _bookingService;

        public FacilitiesModel(FacilityService facilityService, BookingService bookingService)
        {
            _facilityService = facilityService;
            _bookingService = bookingService;
        }

        public List<Facility> Facilities { get; set; }

        public async Task OnGetAsync()
        {
            Facilities = await _facilityService.GetAllFacilitiesAsync();

            // Get pending bookings count for the badge
            var pendingBookingsCount = await _bookingService.GetPendingBookingsAsync();
            ViewData["PendingBookingsCount"] = pendingBookingsCount?.Count ?? 0;
        }
    }
}