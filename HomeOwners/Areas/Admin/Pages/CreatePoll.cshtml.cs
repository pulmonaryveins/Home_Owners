// Areas/Admin/Pages/CreatePoll.cshtml.cs
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
    public class CreatePollModel : PageModel
    {
        private readonly PollService _pollService;

        public CreatePollModel(PollService pollService)
        {
            _pollService = pollService;
        }

        [BindProperty]
        public Poll Poll { get; set; }

        public void OnGet()
        {
            // Initialize a new poll with default values
            Poll = new Poll
            {
                IsActive = true,
                StartDate = DateTime.Now.Date,
                EndDate = DateTime.Now.Date.AddDays(7)
            };
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            if (Poll.EndDate < Poll.StartDate)
            {
                ModelState.AddModelError("Poll.EndDate", "End date must be after start date");
                return Page();
            }

            await _pollService.CreatePollAsync(Poll);

            TempData["StatusMessage"] = "Poll created successfully.";
            TempData["StatusType"] = "Success";

            return RedirectToPage("./ManagePolls");
        }
    }
}