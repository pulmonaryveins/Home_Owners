// Areas/Admin/Pages/Announcements.cshtml.cs
using System;
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
    public class AnnouncementsModel : PageModel
    {
        private readonly AnnouncementService _announcementService;

        public AnnouncementsModel(AnnouncementService announcementService)
        {
            _announcementService = announcementService;
        }

        public List<Announcement> Announcements { get; set; }

        public async Task OnGetAsync()
        {
            Announcements = await _announcementService.GetAllAnnouncementsAsync();
        }
    }
}