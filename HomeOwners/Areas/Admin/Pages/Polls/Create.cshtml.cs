// HomeOwners/Areas/Admin/Pages/Polls/Create.cshtml.cs
using HomeOwners.Models;
using HomeOwners.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace HomeOwners.Areas.Admin.Pages.Polls
{
    [Authorize(Policy = "RequireAdminRole")]
    public class CreateModel : PageModel
    {
        private readonly PollService _pollService;

        public CreateModel(PollService pollService)
        {
            _pollService = pollService;
        }

        [BindProperty]
        public Poll Poll { get; set; } = new Poll
        {
            Options = new List<PollOption>
            {
                new PollOption { OptionText = "" },
                new PollOption { OptionText = "" }
            }
        };

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            // Filter out empty options
            Poll.Options = Poll.Options.Where(o => !string.IsNullOrWhiteSpace(o.OptionText)).ToList();

            if (Poll.Options.Count < 2)
            {
                ModelState.AddModelError(string.Empty, "A poll must have at least 2 options.");
                return Page();
            }

            try
            {
                // Initialize votes to 0 for each option
                foreach (var option in Poll.Options)
                {
                    option.Votes = 0;
                }

                await _pollService.CreatePollAsync(Poll);
                TempData["StatusMessage"] = "Poll created successfully.";
                TempData["StatusType"] = "Success";
                return RedirectToPage("./Index");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, "An error occurred while creating the poll.");
                return Page();
            }
        }
    }
}
