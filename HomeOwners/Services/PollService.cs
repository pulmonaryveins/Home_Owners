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

        public async Task<List<Poll>> GetAllPollsAsync()
        {
            return await _context.Polls
                .Include(p => p.Options)
                .OrderByDescending(p => p.IsActive)
                .ToListAsync();
        }

        public async Task<Poll> GetPollByIdAsync(int id)
        {
            return await _context.Polls
                .Include(p => p.Options)
                .FirstOrDefaultAsync(p => p.PollId == id);
        }

        public async Task CreatePollAsync(Poll poll)
        {
            _context.Polls.Add(poll);
            await _context.SaveChangesAsync();
        }

        public async Task UpdatePollAsync(Poll poll)
        {
            _context.Polls.Update(poll);
            await _context.SaveChangesAsync();
        }

        public async Task DeletePollAsync(int id)
        {
            var poll = await _context.Polls.FindAsync(id);
            if (poll != null)
            {
                _context.Polls.Remove(poll);
                await _context.SaveChangesAsync();
            }
        }

        public async Task TogglePollActiveStatusAsync(int id)
        {
            var poll = await _context.Polls.FindAsync(id);
            if (poll != null)
            {
                // If we're activating this poll, deactivate all others
                if (!poll.IsActive)
                {
                    var activePolls = await _context.Polls
                        .Where(p => p.IsActive && p.PollId != id)
                        .ToListAsync();

                    foreach (var activePoll in activePolls)
                    {
                        activePoll.IsActive = false;
                    }
                }

                poll.IsActive = !poll.IsActive;
                await _context.SaveChangesAsync();
            }
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
