// HomeOwners/Areas/Admin/Pages/Billing.cshtml.cs
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HomeOwners.Models;
using HomeOwners.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace HomeOwners.Areas.Admin.Pages
{
    [Authorize(Policy = "RequireAdminRole")]
    public class BillingModel : PageModel
    {
        private readonly PaymentService _paymentService;
        private readonly BookingService _bookingService;

        public BillingModel(PaymentService paymentService, BookingService bookingService)
        {
            _paymentService = paymentService;
            _bookingService = bookingService;
        }

        public List<Booking> UnpaidBookings { get; set; }
        public List<Payment> RecentPayments { get; set; }
        public decimal TotalUnpaidAmount { get; set; }
        public decimal TotalPaidAmount { get; set; }

        public async Task OnGetAsync()
        {
            UnpaidBookings = await _paymentService.GetCompletedUnpaidBookingsAsync();
            RecentPayments = await _paymentService.GetAllPaymentsAsync();

            TotalUnpaidAmount = UnpaidBookings.Sum(b => b.TotalPrice);
            TotalPaidAmount = RecentPayments.Sum(p => p.AmountPaid);
        }

        public async Task<IActionResult> OnPostMarkCompleteAsync(int id)
        {
            var booking = await _bookingService.GetBookingByIdAsync(id);
            if (booking == null)
            {
                return NotFound();
            }

            await _bookingService.UpdateBookingStatusAsync(id, BookingStatus.Completed);

            TempData["StatusMessage"] = "Booking has been marked as completed and is now in the billing section.";
            TempData["StatusType"] = "Success";

            return RedirectToPage();
        }
    }
}