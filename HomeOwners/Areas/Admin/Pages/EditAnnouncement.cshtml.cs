// Areas/Admin/Pages/EditAnnouncement.cshtml.cs
using System.Threading.Tasks;
using HomeOwners.Models;
using HomeOwners.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace HomeOwners.Areas.Admin.Pages
{
    [Authorize(Policy = "RequireAdminRole")]
    public class EditAnnouncementModel : PageModel
    {
        private readonly AnnouncementService _announcementService;

        public EditAnnouncementModel(AnnouncementService announcementService)
        {
            _announcementService = announcementService;
        }

        [BindProperty]
        public Announcement Announcement { get; set; }

        public async Task<IActionResult> OnGetAsync(int id)
        {
            Announcement = await _announcementService.GetAnnouncementByIdAsync(id);

            if (Announcement == null)
            {
                return NotFound();
            }

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            await _announcementService.UpdateAnnouncementAsync(Announcement);

            TempData["StatusMessage"] = "Announcement updated successfully.";
            TempData["StatusType"] = "Success";

            // Redirect back to Announcements page
            return RedirectToPage("/Announcement");
        }
    }
}