// HomeOwners/Areas/Admin/Pages/Facilities.cshtml.cs
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

        // Pagination properties
        public int PageIndex { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public int TotalCount { get; set; }
        public int TotalPages => (int)Math.Ceiling(TotalCount / (double)PageSize);

        // Sorting and filtering properties
        public string CurrentSort { get; set; }
        public string CurrentFilter { get; set; }
        public string StatusFilter { get; set; }
        public string SortField { get; set; }
        public string SortDirection { get; set; }

        public async Task OnGetAsync(string sortOrder, string currentFilter, string searchString, string statusFilter, int? pageIndex)
        {
            // Store current sort for maintaining state across paging
            CurrentSort = sortOrder;

            // Handle search string
            if (searchString != null)
            {
                pageIndex = 1; // Reset page index when new search is performed
            }
            else
            {
                searchString = currentFilter;
            }

            // Set current filter and status filter
            CurrentFilter = searchString;
            StatusFilter = statusFilter;

            // Determine sort field and direction from sortOrder
            if (string.IsNullOrEmpty(sortOrder))
            {
                SortField = "name";
                SortDirection = "asc";
            }
            else if (sortOrder == "name_desc")
            {
                SortField = "name";
                SortDirection = "desc";
            }
            else if (sortOrder == "price")
            {
                SortField = "price";
                SortDirection = "asc";
            }
            else if (sortOrder == "price_desc")
            {
                SortField = "price";
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

            // Get all facilities (you may need to update FacilityService to support filtering and sorting)
            var allFacilities = await _facilityService.GetAllFacilitiesAsync();

            // Apply filtering by name if search string provided
            if (!string.IsNullOrEmpty(searchString))
            {
                allFacilities = allFacilities.Where(f => f.Name.Contains(searchString, StringComparison.OrdinalIgnoreCase)).ToList();
            }

            // Apply status filtering
            if (statusFilter == "Active")
            {
                allFacilities = allFacilities.Where(f => f.IsActive).ToList();
            }
            else if (statusFilter == "Inactive")
            {
                allFacilities = allFacilities.Where(f => !f.IsActive).ToList();
            }

            // Apply sorting
            allFacilities = ApplySorting(allFacilities, SortField, SortDirection);

            // Set total count for pagination
            TotalCount = allFacilities.Count;

            // Apply pagination
            PageIndex = pageIndex ?? 1;
            Facilities = allFacilities
                .Skip((PageIndex - 1) * PageSize)
                .Take(PageSize)
                .ToList();

            // Get pending bookings count for the badge
            var pendingBookings = await _bookingService.GetPendingBookingsAsync();
            ViewData["PendingBookingsCount"] = pendingBookings?.Count ?? 0;
        }

        private List<Facility> ApplySorting(List<Facility> facilities, string field, string direction)
        {
            if (direction == "asc")
            {
                switch (field)
                {
                    case "name":
                        return facilities.OrderBy(f => f.Name).ToList();
                    case "price":
                        return facilities.OrderBy(f => f.PricePerHour).ToList();
                    case "date":
                        return facilities.OrderBy(f => f.CreatedDate).ToList();
                    default:
                        return facilities.OrderBy(f => f.Name).ToList();
                }
            }
            else
            {
                switch (field)
                {
                    case "name":
                        return facilities.OrderByDescending(f => f.Name).ToList();
                    case "price":
                        return facilities.OrderByDescending(f => f.PricePerHour).ToList();
                    case "date":
                        return facilities.OrderByDescending(f => f.CreatedDate).ToList();
                    default:
                        return facilities.OrderByDescending(f => f.Name).ToList();
                }
            }
        }
    }
}