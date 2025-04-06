// HomeOwners/Areas/Admin/Pages/BookingRequests.cshtml.cs
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
    public class BookingRequestsModel : PageModel
    {
        private readonly BookingService _bookingService;

        public BookingRequestsModel(BookingService bookingService)
        {
            _bookingService = bookingService;
        }

        public List<Booking> PendingBookings { get; set; }
        public List<Booking> AllBookings { get; set; }

        public async Task OnGetAsync()
        {
            PendingBookings = await _bookingService.GetPendingBookingsAsync();
            AllBookings = await _bookingService.GetAllBookingsAsync();
        }

        public async Task<IActionResult> OnPostMarkCompleteAsync(int id)
        {
            // Make sure to check if the booking exists before updating
            var booking = await _bookingService.GetBookingByIdAsync(id);
            if (booking == null)
            {
                TempData["StatusMessage"] = "Booking not found.";
                TempData["StatusType"] = "Error";
                return RedirectToPage();
            }

            await _bookingService.UpdateBookingStatusAsync(id, BookingStatus.Completed);

            TempData["StatusMessage"] = "Booking has been marked as complete.";
            TempData["StatusType"] = "Success";
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostApproveAsync(int id)
        {
            await _bookingService.UpdateBookingStatusAsync(id, BookingStatus.Approved);
            TempData["StatusMessage"] = "Booking request has been approved successfully.";
            TempData["StatusType"] = "Success";
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostRejectAsync(int id)
        {
            await _bookingService.UpdateBookingStatusAsync(id, BookingStatus.Rejected);
            TempData["StatusMessage"] = "Booking request has been rejected.";
            TempData["StatusType"] = "Success";
            return RedirectToPage();
        }
    }
}