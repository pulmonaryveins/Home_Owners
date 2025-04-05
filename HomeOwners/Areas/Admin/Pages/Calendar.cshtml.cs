// Areas/Admin/Pages/Calendar.cshtml.cs
using System;
using System.Threading.Tasks;
using HomeOwners.Models;
using HomeOwners.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace HomeOwners.Areas.Admin.Pages
{
    [Authorize(Policy = "RequireAdminRole")]
    public class CalendarModel : PageModel
    {
        private readonly EventService _eventService;

        public CalendarModel(EventService eventService)
        {
            _eventService = eventService;
        }

        [BindProperty]
        public Event Event { get; set; }

        public void OnGet()
        {
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                TempData["StatusMessage"] = "Error: Please check your input.";
                TempData["StatusType"] = "Error";
                return Page();
            }

            // Check for overlapping events before creating a new one
            bool hasOverlap = await _eventService.HasOverlappingEventsAsync(Event.StartTime, Event.EndTime);
            if (hasOverlap)
            {
                // Updated error message about event overlap
                TempData["StatusMessage"] = "There's an ongoing event scheduled on this date and time. Please select a different time slot.";
                TempData["StatusType"] = "Error";
                return Page();
            }

            await _eventService.CreateEventAsync(Event);

            TempData["StatusMessage"] = "Event created successfully.";
            TempData["StatusType"] = "Success";

            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostEditAsync()
        {
            if (!ModelState.IsValid)
            {
                TempData["StatusMessage"] = "Error: Please check your input.";
                TempData["StatusType"] = "Error";
                return Page();
            }

            // Check for overlapping events before updating, excluding the current event being edited
            bool hasOverlap = await _eventService.HasOverlappingEventsAsync(Event.StartTime, Event.EndTime, Event.Id);
            if (hasOverlap)
            {
                // Updated error message about event overlap
                TempData["StatusMessage"] = "There's an ongoing event scheduled on this date and time. Please select a different time slot.";
                TempData["StatusType"] = "Error";
                return Page();
            }

            await _eventService.UpdateEventAsync(Event);

            TempData["StatusMessage"] = "Event updated successfully.";
            TempData["StatusType"] = "Success";

            return RedirectToPage();
        }
    }
}