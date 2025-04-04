// Areas/Admin/Pages/DeleteAnnouncement.cshtml.cs
using System.Threading.Tasks;
using HomeOwners.Models;
using HomeOwners.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace HomeOwners.Areas.Admin.Pages
{
    [Authorize(Policy = "RequireAdminRole")]
    public class DeleteAnnouncementModel : PageModel
    {
        private readonly AnnouncementService _announcementService;

        public DeleteAnnouncementModel(AnnouncementService announcementService)
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
            await _announcementService.DeleteAnnouncementAsync(Announcement.Id);

            TempData["StatusMessage"] = "Announcement deleted successfully.";
            TempData["StatusType"] = "Success";

            return RedirectToPage("./Announcements");
        }
    }
}