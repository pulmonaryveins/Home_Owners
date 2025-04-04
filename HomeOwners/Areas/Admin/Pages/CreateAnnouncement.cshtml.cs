﻿// Areas/Admin/Pages/CreateAnnouncement.cshtml.cs
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
    public class CreateAnnouncementModel : PageModel
    {
        private readonly AnnouncementService _announcementService;

        public CreateAnnouncementModel(AnnouncementService announcementService)
        {
            _announcementService = announcementService;
        }

        [BindProperty]
        public Announcement Announcement { get; set; }

        public void OnGet()
        {
            Announcement = new Announcement
            {
                PostedDate = DateTime.Now,
                IsActive = true
            };
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            await _announcementService.CreateAnnouncementAsync(Announcement);

            TempData["StatusMessage"] = "Announcement created successfully.";
            TempData["StatusType"] = "Success";

            return RedirectToPage("./Announcements");
        }
    }
}