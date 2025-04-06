// HomeOwners/Areas/Admin/Pages/Polls/Index.cshtml.cs
using HomeOwners.Models;
using HomeOwners.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace HomeOwners.Areas.Admin.Pages.Polls
{
    [Authorize(Policy = "RequireAdminRole")]
    public class IndexModel : PageModel
    {
        private readonly PollService _pollService;

        public IndexModel(PollService pollService)
        {
            _pollService = pollService;
        }

        public List<Poll> Polls { get; set; } = new();

        public async Task OnGetAsync()
        {
            Polls = await _pollService.GetAllPollsAsync();
        }

        public async Task<IActionResult> OnPostToggleActiveAsync(int id)
        {
            try
            {
                await _pollService.TogglePollActiveStatusAsync(id);
                TempData["StatusMessage"] = "Poll status updated successfully.";
                TempData["StatusType"] = "Success";
            }
            catch (Exception ex)
            {
                TempData["StatusMessage"] = "Failed to update poll status.";
                TempData["StatusType"] = "Error";
            }
            return RedirectToPage();
        }
    }
}
