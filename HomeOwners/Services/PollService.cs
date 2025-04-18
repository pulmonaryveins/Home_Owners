using HomeOwners.Areas.Identity.Data;
using HomeOwners.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

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
            return await _context.Polls.OrderByDescending(p => p.CreatedDate).ToListAsync();
        }

        public async Task<List<Poll>> GetActivePollsAsync()
        {
            var currentDate = DateTime.Now.Date;
            return await _context.Polls
                .Where(p => p.IsActive && p.StartDate <= currentDate && p.EndDate >= currentDate)
                .OrderByDescending(p => p.CreatedDate)
                .ToListAsync();
        }

        public async Task<Poll> GetPollByIdAsync(int id)
        {
            return await _context.Polls.FindAsync(id);
        }

        public async Task CreatePollAsync(Poll poll)
        {
            poll.CreatedDate = DateTime.Now;
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
                // Also delete all votes for this poll
                var votes = await _context.PollVotes.Where(v => v.PollId == id).ToListAsync();
                _context.PollVotes.RemoveRange(votes);

                _context.Polls.Remove(poll);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<bool> HasUserVotedAsync(int pollId, string userId)
        {
            return await _context.PollVotes
                .AnyAsync(v => v.PollId == pollId && v.UserId == userId);
        }

        public async Task CastVoteAsync(int pollId, string userId, bool voteValue)
        {
            // Check if user has already voted
            if (await HasUserVotedAsync(pollId, userId))
            {
                throw new InvalidOperationException("You have already voted in this poll.");
            }

            // Create a new vote
            var vote = new PollVote
            {
                PollId = pollId,
                UserId = userId,
                VoteValue = voteValue,
                VoteDate = DateTime.Now
            };

            // Add vote to the database
            _context.PollVotes.Add(vote);

            // Update poll statistics
            var poll = await _context.Polls.FindAsync(pollId);
            if (poll != null)
            {
                if (voteValue)
                {
                    poll.YesVotes++;
                }
                else
                {
                    poll.NoVotes++;
                }
            }

            await _context.SaveChangesAsync();
        }
    }
}