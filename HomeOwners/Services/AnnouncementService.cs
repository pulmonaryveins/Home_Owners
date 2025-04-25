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
                .OrderByDescending(a => a.IsPinned) // Pinned announcements first
                .ThenByDescending(a => a.IsUrgent)
                .ThenByDescending(a => a.PostedDate)
                .ToListAsync();
        }

        public async Task<(List<Announcement> Items, int TotalCount)> GetAnnouncementsAsync(
        string searchString, string categoryFilter, string sortOrder, int pageIndex, int pageSize)
        {
            // Implement filtering, sorting and pagination
            var query = _context.Announcements.AsQueryable();

            // Apply filters
            if (!string.IsNullOrEmpty(searchString))
            {
                query = query.Where(a => a.Title.Contains(searchString) ||
                                        a.Content.Contains(searchString));
            }

            if (!string.IsNullOrEmpty(categoryFilter))
            {
                query = query.Where(a => a.Category == categoryFilter);
            }

            // Apply sorting
            query = sortOrder switch
            {
                "date" => query.OrderBy(a => a.PostedDate),
                "date_desc" => query.OrderByDescending(a => a.PostedDate),
                "title" => query.OrderBy(a => a.Title),
                "title_desc" => query.OrderByDescending(a => a.Title),
                _ => query.OrderByDescending(a => a.PostedDate) // default sort
            };

            // Get total count
            int totalCount = await query.CountAsync();

            // Apply pagination
            var items = await query
                .Skip((pageIndex - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (items, totalCount);
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

        // Add these methods for pinning/unpinning announcements
        public async Task PinAnnouncementAsync(int id)
        {
            var announcement = await _context.Announcements.FindAsync(id);
            if (announcement != null)
            {
                announcement.IsPinned = true;
                await _context.SaveChangesAsync();
            }
        }

        public async Task UnpinAnnouncementAsync(int id)
        {
            var announcement = await _context.Announcements.FindAsync(id);
            if (announcement != null)
            {
                announcement.IsPinned = false;
                await _context.SaveChangesAsync();
            }
        }
    }
}