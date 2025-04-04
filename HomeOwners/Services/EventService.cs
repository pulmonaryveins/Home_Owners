// Services/EventService.cs
using HomeOwners.Areas.Identity.Data;
using HomeOwners.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace HomeOwners.Services
{
    public class EventService
    {
        private readonly HomeDbContext _context;

        public EventService(HomeDbContext context)
        {
            _context = context;
        }

        // Get all events
        public async Task<List<Event>> GetAllEventsAsync()
        {
            return await _context.Events.OrderByDescending(e => e.StartTime).ToListAsync();
        }

        // Get active events
        public async Task<List<Event>> GetActiveEventsAsync()
        {
            return await _context.Events
                .Where(e => e.IsActive)
                .OrderByDescending(e => e.StartTime)
                .ToListAsync();
        }

        // Get upcoming events
        public async Task<List<Event>> GetUpcomingEventsAsync(int count = 5)
        {
            return await _context.Events
                .Where(e => e.IsActive && e.EndTime >= DateTime.Now)
                .OrderBy(e => e.StartTime)
                .Take(count)
                .ToListAsync();
        }

        // Get event by ID
        public async Task<Event> GetEventByIdAsync(int id)
        {
            return await _context.Events.FindAsync(id);
        }

        // Create new event
        public async Task<Event> CreateEventAsync(Event newEvent)
        {
            _context.Events.Add(newEvent);
            await _context.SaveChangesAsync();
            return newEvent;
        }

        // Update existing event
        public async Task<bool> UpdateEventAsync(Event updatedEvent)
        {
            _context.Events.Update(updatedEvent);
            var result = await _context.SaveChangesAsync();
            return result > 0;
        }

        // Delete event
        public async Task<bool> DeleteEventAsync(int id)
        {
            var eventToDelete = await _context.Events.FindAsync(id);
            if (eventToDelete == null)
                return false;

            _context.Events.Remove(eventToDelete);
            var result = await _context.SaveChangesAsync();
            return result > 0;
        }
    }
}