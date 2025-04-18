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
    public class ManagePollsModel : PageModel
    {
        private readonly PollService _pollService;

        public ManagePollsModel(PollService pollService)
        {
            _pollService = pollService;
        }

        public List<Poll> Polls { get; set; }

        public async Task OnGetAsync()
        {
            Polls = await _pollService.GetAllPollsAsync();
        }

        public async Task<IActionResult> OnPostDeleteAsync(int id)
        {
            await _pollService.DeletePollAsync(id);

            TempData["StatusMessage"] = "Poll deleted successfully.";
            TempData["StatusType"] = "Success";

            return RedirectToPage();
        }
    }
}