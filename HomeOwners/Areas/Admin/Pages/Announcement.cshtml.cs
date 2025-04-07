// Areas/Admin/Pages/Announcement.cshtml.cs
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
    public class AnnouncementsModel : PageModel
    {
        private readonly AnnouncementService _announcementService;

        public AnnouncementsModel(AnnouncementService announcementService)
        {
            _announcementService = announcementService;
        }

        public List<Announcement> Announcements { get; set; }

        // Add these properties for pagination
        public int PageIndex { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public int TotalCount { get; set; }
        public int TotalPages => (int)Math.Ceiling(TotalCount / (double)PageSize);

        public async Task OnGetAsync(string searchString, string categoryFilter, string sortOrder, int? pageIndex)
        {
            PageIndex = pageIndex ?? 1;

            // Store filter/sort state in ViewData
            ViewData["CurrentFilter"] = searchString;
            ViewData["CategoryFilter"] = categoryFilter;
            ViewData["CurrentSort"] = sortOrder;

            // Set sort direction for view
            if (sortOrder != null)
            {
                ViewData["SortField"] = sortOrder.Contains("_desc")
                    ? sortOrder.Replace("_desc", "")
                    : sortOrder;
                ViewData["SortDirection"] = sortOrder.Contains("_desc") ? "desc" : "asc";
            }

            // Get announcements with filtering and pagination
            var result = await _announcementService.GetAnnouncementsAsync(
                searchString, categoryFilter, sortOrder, PageIndex, PageSize);

            Announcements = result.Items;
            TotalCount = result.TotalCount;
        }

        public async Task<IActionResult> OnPostDeleteAsync(int id)
        {
            await _announcementService.DeleteAnnouncementAsync(id);

            TempData["StatusMessage"] = "Announcement deleted successfully.";
            TempData["StatusType"] = "Success";

            return RedirectToPage();
        }
    }
}