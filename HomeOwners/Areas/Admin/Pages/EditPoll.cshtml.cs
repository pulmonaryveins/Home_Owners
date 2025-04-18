// Areas/Admin/Pages/EditPoll.cshtml.cs
using System.Threading.Tasks;
using HomeOwners.Models;
using HomeOwners.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace HomeOwners.Areas.Admin.Pages
{
    [Authorize(Policy = "RequireAdminRole")]
    public class EditPollModel : PageModel
    {
        private readonly PollService _pollService;

        public EditPollModel(PollService pollService)
        {
            _pollService = pollService;
        }

        [BindProperty]
        public Poll Poll { get; set; }

        public async Task<IActionResult> OnGetAsync(int id)
        {
            Poll = await _pollService.GetPollByIdAsync(id);

            if (Poll == null)
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

            if (Poll.EndDate < Poll.StartDate)
            {
                ModelState.AddModelError("Poll.EndDate", "End date must be after start date");
                return Page();
            }

            await _pollService.UpdatePollAsync(Poll);

            TempData["StatusMessage"] = "Poll updated successfully.";
            TempData["StatusType"] = "Success";

            return RedirectToPage("./ManagePolls");
        }
    }
}