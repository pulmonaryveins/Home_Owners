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


        public async Task<bool> HasActiveBookingsAsync(string userId)
        {
            return await _context.Bookings
                .AnyAsync(b => b.UserId == userId &&
                               (b.Status == BookingStatus.Pending ||
                                b.Status == BookingStatus.Approved) &&
                               b.BookingDate >= DateTime.Today);
        }

        public async Task<List<Booking>> GetPendingBookingsAsync()
        {
            return await _context.Bookings
                .Include(b => b.Facility)
                .Where(b => b.Status == BookingStatus.Pending)
                .OrderByDescending(b => b.CreatedDate)
                .ToListAsync();
        }

        public async Task ClearAllBookingsAsync()
        {
            // Delete all bookings
            var bookings = await _context.Bookings.ToListAsync();
            _context.Bookings.RemoveRange(bookings);

            // Also clear payments related to bookings
            var payments = await _context.Payments.ToListAsync();
            _context.Payments.RemoveRange(payments);

            await _context.SaveChangesAsync();
        }

        public async Task<Booking> CreateBookingAsync(Booking booking)
        {
            // Ensure TotalHours and TotalPrice are calculated if not already
            if (booking.TotalHours <= 0)
            {
                booking.TotalHours = (decimal)(booking.EndTime - booking.StartTime).TotalHours;
            }

            if (booking.TotalPrice <= 0)
            {
                // Get the facility to get the hourly rate
                var facility = await _context.Facilities.FindAsync(booking.FacilityId);
                if (facility != null)
                {
                    booking.TotalPrice = booking.TotalHours * facility.PricePerHour;
                }
            }

            _context.Bookings.Add(booking);
            await _context.SaveChangesAsync();
            return booking;
        }

        public async Task<Booking> GetBookingByIdAsync(int id)
        {
            return await _context.Bookings
                .Include(b => b.Facility)
                .FirstOrDefaultAsync(b => b.Id == id);
        }

        public async Task<IEnumerable<Booking>> GetBookingsByUserIdAsync(string userId)
        {
            return await _context.Bookings
                .Include(b => b.Facility)
                .Where(b => b.UserId == userId)
                .OrderByDescending(b => b.CreatedDate)
                .ToListAsync();
        }

        public async Task<IEnumerable<Booking>> GetAllBookingsAsync()
        {
            return await _context.Bookings
                .Include(b => b.Facility)
                .OrderByDescending(b => b.CreatedDate)
                .ToListAsync();
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

        public async Task<bool> DeleteBookingAsync(int id)
        {
            var booking = await _context.Bookings.FindAsync(id);
            if (booking == null)
            {
                return false;
            }

            _context.Bookings.Remove(booking);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}