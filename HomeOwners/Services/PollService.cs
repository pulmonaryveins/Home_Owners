using HomeOwners.Areas.Identity.Data;
using HomeOwners.Models;
using HomeOwners.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace HomeOwners.Services
{
    public class PollService
    {
        private readonly HomeDbContext _context;

        public PollService(HomeDbContext context)
        {
            _context = context;
        }

        public async Task<PollViewModel?> GetActivePollAsync()
        {
            var poll = await _context.Polls
                .Include(p => p.Options)
                .Where(p => p.IsActive)
                .FirstOrDefaultAsync();

            if (poll == null) return null;

            return new PollViewModel
            {
                PollId = poll.PollId,
                Question = poll.Question,
                Options = poll.Options.Select(o => new PollOptionViewModel
                {
                    PollOptionId = o.PollOptionId,
                    OptionText = o.OptionText,
                    Votes = o.Votes
                }).ToList()
            };
        }
    }
}
