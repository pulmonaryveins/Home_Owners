// HomeOwners/Services/BookingService.cs
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HomeOwners.Areas.Identity.Data;
using HomeOwners.Models;
using Microsoft.EntityFrameworkCore;

namespace HomeOwners.Services
{
    public class BookingService
    {
        private readonly HomeDbContext _context;

        public BookingService(HomeDbContext context)
        {
            _context = context;
        }

        public async Task<List<Booking>> GetAllBookingsAsync()
        {
            return await _context.Bookings
                .Include(b => b.Facility)
                .OrderByDescending(b => b.CreatedDate)
                .ToListAsync();
        }

        public async Task<List<Booking>> GetBookingsByUserIdAsync(string userId)
        {
            return await _context.Bookings
                .Include(b => b.Facility)
                .Where(b => b.UserId == userId)
                .OrderByDescending(b => b.CreatedDate)
                .ToListAsync();
        }

        public async Task<List<Booking>> GetPendingBookingsAsync()
        {
            return await _context.Bookings
                .Include(b => b.Facility)
                .Where(b => b.Status == BookingStatus.Pending)
                .OrderByDescending(b => b.CreatedDate)
                .ToListAsync();
        }

        public async Task<Booking> GetBookingByIdAsync(int id)
        {
            return await _context.Bookings
                .Include(b => b.Facility)
                .FirstOrDefaultAsync(b => b.Id == id);
        }

        public async Task CreateBookingAsync(Booking booking)
        {
            booking.CreatedDate = DateTime.Now;
            booking.Status = BookingStatus.Pending;

            _context.Bookings.Add(booking);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateBookingStatusAsync(int id, BookingStatus status)
        {
            var booking = await _context.Bookings.FindAsync(id);
            if (booking != null)
            {
                booking.Status = status;
                await _context.SaveChangesAsync();
            }
        }

        public async Task DeleteBookingAsync(int id)
        {
            var booking = await _context.Bookings.FindAsync(id);
            if (booking != null)
            {
                _context.Bookings.Remove(booking);
                await _context.SaveChangesAsync();
            }
        }
    }
}