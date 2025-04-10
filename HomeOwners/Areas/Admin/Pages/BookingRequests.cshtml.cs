// HomeOwners/Areas/Admin/Pages/BookingRequests.cshtml.cs
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
    public class BookingRequestsModel : PageModel
    {
        private readonly BookingService _bookingService;

        public BookingRequestsModel(BookingService bookingService)
        {
            _bookingService = bookingService;
        }

        public List<Booking> PendingBookings { get; set; }
        public List<Booking> AllBookings { get; set; }

        // Common properties
        public int PageSize { get; set; } = 10;
        public string CurrentSort { get; set; }
        public string CurrentFilter { get; set; }
        public string StatusFilter { get; set; }
        public string SortField { get; set; }
        public string SortDirection { get; set; }
        public string TabFilter { get; set; }

        // Pending tab pagination properties
        public int PendingPageIndex { get; set; } = 1;
        public int PendingTotalCount { get; set; }
        public int PendingTotalPages => (int)Math.Ceiling(PendingTotalCount / (double)PageSize);

        // All bookings tab pagination properties
        public int AllPageIndex { get; set; } = 1;
        public int AllTotalCount { get; set; }
        public int AllTotalPages => (int)Math.Ceiling(AllTotalCount / (double)PageSize);

        public async Task OnGetAsync(
            string sortOrder,
            string currentFilter,
            string searchString,
            string statusFilter,
            string tabFilter,
            int? pendingPageIndex,
            int? allPageIndex)
        {
            // Store current sort and filters
            CurrentSort = sortOrder;
            TabFilter = tabFilter;
            StatusFilter = statusFilter;

            // Handle search string
            if (searchString != null)
            {
                pendingPageIndex = allPageIndex = 1; // Reset page indices when new search is performed
            }
            else
            {
                searchString = currentFilter;
            }

            CurrentFilter = searchString;

            // Set page indices
            PendingPageIndex = pendingPageIndex ?? 1;
            AllPageIndex = allPageIndex ?? 1;

            // Determine sort field and direction
            if (string.IsNullOrEmpty(sortOrder))
            {
                SortField = "date";
                SortDirection = "desc";
            }
            else if (sortOrder == "date")
            {
                SortField = "date";
                SortDirection = "asc";
            }
            else if (sortOrder == "date_desc")
            {
                SortField = "date";
                SortDirection = "desc";
            }
            else if (sortOrder == "name")
            {
                SortField = "name";
                SortDirection = "asc";
            }
            else if (sortOrder == "name_desc")
            {
                SortField = "name";
                SortDirection = "desc";
            }
            else if (sortOrder == "bookingDate")
            {
                SortField = "bookingDate";
                SortDirection = "asc";
            }
            else if (sortOrder == "bookingDate_desc")
            {
                SortField = "bookingDate";
                SortDirection = "desc";
            }

            // Fetch all bookings first to apply filtering and sorting
            var allBookingsFromDb = (await _bookingService.GetAllBookingsAsync()).ToList();
            var pendingBookingsFromDb = allBookingsFromDb.Where(b => b.Status == BookingStatus.Pending).ToList();

            // Apply search filtering to pending bookings
            if (!string.IsNullOrEmpty(searchString))
            {
                pendingBookingsFromDb = pendingBookingsFromDb
                    .Where(b =>
                        b.FullName.Contains(searchString, StringComparison.OrdinalIgnoreCase) ||
                        (b.Facility?.Name?.Contains(searchString, StringComparison.OrdinalIgnoreCase) ?? false))
                    .ToList();
            }

            // Apply search and status filtering to all bookings
            if (!string.IsNullOrEmpty(searchString))
            {
                allBookingsFromDb = allBookingsFromDb
                    .Where(b =>
                        b.FullName.Contains(searchString, StringComparison.OrdinalIgnoreCase) ||
                        (b.Facility?.Name?.Contains(searchString, StringComparison.OrdinalIgnoreCase) ?? false))
                    .ToList();
            }

            if (!string.IsNullOrEmpty(statusFilter))
            {
                if (Enum.TryParse<BookingStatus>(statusFilter, out var bookingStatus))
                {
                    allBookingsFromDb = allBookingsFromDb
                        .Where(b => b.Status == bookingStatus)
                        .ToList();
                }
            }

            // Apply sorting to pending bookings
            pendingBookingsFromDb = ApplySorting(pendingBookingsFromDb, SortField, SortDirection);

            // Apply sorting to all bookings
            allBookingsFromDb = ApplySorting(allBookingsFromDb, SortField, SortDirection);

            // Store total counts for pagination
            PendingTotalCount = pendingBookingsFromDb.Count;
            AllTotalCount = allBookingsFromDb.Count;

            // Apply pagination
            PendingBookings = pendingBookingsFromDb
                .Skip((PendingPageIndex - 1) * PageSize)
                .Take(PageSize)
                .ToList();

            AllBookings = allBookingsFromDb
                .Skip((AllPageIndex - 1) * PageSize)
                .Take(PageSize)
                .ToList();
        }

        private List<Booking> ApplySorting(List<Booking> bookings, string field, string direction)
        {
            if (direction == "asc")
            {
                switch (field)
                {
                    case "name":
                        return bookings.OrderBy(b => b.FullName).ToList();
                    case "date":
                        return bookings.OrderBy(b => b.CreatedDate).ToList();
                    case "bookingDate":
                        return bookings.OrderBy(b => b.BookingDate).ToList();
                    default:
                        return bookings.OrderBy(b => b.CreatedDate).ToList();
                }
            }
            else
            {
                switch (field)
                {
                    case "name":
                        return bookings.OrderByDescending(b => b.FullName).ToList();
                    case "date":
                        return bookings.OrderByDescending(b => b.CreatedDate).ToList();
                    case "bookingDate":
                        return bookings.OrderByDescending(b => b.BookingDate).ToList();
                    default:
                        return bookings.OrderByDescending(b => b.CreatedDate).ToList();
                }
            }
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
            return RedirectToPage(new { tabFilter = "all" });
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