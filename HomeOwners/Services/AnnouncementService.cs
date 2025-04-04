// Services/AnnouncementService.cs
using HomeOwners.Areas.Identity.Data;
using HomeOwners.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HomeOwners.Services
{
    public class AnnouncementService
    {
        private readonly HomeDbContext _context;

        public AnnouncementService(HomeDbContext context)
        {
            _context = context;
        }

        public async Task<List<Announcement>> GetAllAnnouncementsAsync()
        {
            return await _context.Announcements.OrderByDescending(a => a.PostedDate).ToListAsync();
        }

        public async Task<List<Announcement>> GetActiveAnnouncementsAsync()
        {
            return await _context.Announcements
                .Where(a => a.IsActive)
                .OrderByDescending(a => a.IsUrgent)
                .ThenByDescending(a => a.PostedDate)
                .ToListAsync();
        }

        public async Task<Announcement> GetAnnouncementByIdAsync(int id)
        {
            return await _context.Announcements.FindAsync(id);
        }

        public async Task CreateAnnouncementAsync(Announcement announcement)
        {
            _context.Announcements.Add(announcement);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAnnouncementAsync(Announcement announcement)
        {
            _context.Announcements.Update(announcement);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAnnouncementAsync(int id)
        {
            var announcement = await _context.Announcements.FindAsync(id);
            if (announcement != null)
            {
                _context.Announcements.Remove(announcement);
                await _context.SaveChangesAsync();
            }
        }
    }
}