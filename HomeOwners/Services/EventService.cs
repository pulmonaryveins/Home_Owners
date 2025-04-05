// Services/EventService.cs - updated with new HasOverlappingEvents method
using HomeOwners.Areas.Identity.Data;
using HomeOwners.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
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

        // Check if there are any overlapping events for the given time range
        public async Task<bool> HasOverlappingEventsAsync(DateTime startTime, DateTime endTime, int? excludeEventId = null)
        {
            // An event overlaps if:
            // 1. Its start time falls within our event's timespan, OR
            // 2. Its end time falls within our event's timespan, OR
            // 3. It completely envelops our event's timespan

            var query = _context.Events
                .Where(e => e.IsActive &&
                           ((e.StartTime >= startTime && e.StartTime < endTime) || // Start time falls within our range
                           (e.EndTime > startTime && e.EndTime <= endTime) || // End time falls within our range
                           (e.StartTime <= startTime && e.EndTime >= endTime))); // Event envelops our range

            // If we're updating an existing event, exclude it from the check
            if (excludeEventId.HasValue)
            {
                query = query.Where(e => e.Id != excludeEventId.Value);
            }

            // If any events match our criteria, we have an overlap
            return await query.AnyAsync();
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