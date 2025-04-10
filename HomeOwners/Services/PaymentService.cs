// HomeOwners/Services/PaymentService.cs
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HomeOwners.Areas.Identity.Data;
using HomeOwners.Models;
using Microsoft.EntityFrameworkCore;

namespace HomeOwners.Services
{
    public class PaymentService
    {
        private readonly HomeDbContext _context;

        public PaymentService(HomeDbContext context)
        {
            _context = context;
        }

        public async Task<List<Payment>> GetAllPaymentsAsync()
        {
            return await _context.Payments
                .Include(p => p.Booking)
                .ThenInclude(b => b.Facility)
                .OrderByDescending(p => p.PaymentDate)
                .ToListAsync();
        }

        public async Task<List<Payment>> GetPaymentsByUserIdAsync(string userId)
        {
            return await _context.Payments
                .Include(p => p.Booking)
                .ThenInclude(b => b.Facility)
                .Where(p => p.UserId == userId)
                .OrderByDescending(p => p.PaymentDate)
                .ToListAsync();
        }

        public async Task<Payment> GetPaymentByIdAsync(int id)
        {
            return await _context.Payments
                .Include(p => p.Booking)
                .ThenInclude(b => b.Facility)
                .FirstOrDefaultAsync(p => p.Id == id);
        }

        public async Task<List<Booking>> GetCompletedUnpaidBookingsAsync(string userId = null)
        {
            var query = _context.Bookings
                .Include(b => b.Facility)
                .Where(b => b.Status == BookingStatus.Completed && b.PaymentStatus == PaymentStatus.Unpaid);

            if (!string.IsNullOrEmpty(userId))
            {
                query = query.Where(b => b.UserId == userId);
            }

            return await query.OrderByDescending(b => b.BookingDate).ToListAsync();
        }

        public async Task CreatePaymentAsync(Payment payment)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                // Add the payment record
                _context.Payments.Add(payment);
                await _context.SaveChangesAsync();

                // Update the booking payment status
                var booking = await _context.Bookings.FindAsync(payment.BookingId);
                if (booking != null)
                {
                    booking.PaymentStatus = PaymentStatus.Paid;
                    booking.PaidDate = payment.PaymentDate;
                    booking.TransactionId = payment.TransactionId;
                    booking.ReceiptNumber = payment.ReceiptNumber;

                    await _context.SaveChangesAsync();
                }

                await transaction.CommitAsync();
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task<string> GenerateReceiptNumberAsync()
        {
            var now = DateTime.Now;
            var year = now.ToString("yy");
            var month = now.ToString("MM");
            var day = now.ToString("dd");

            // Get count of existing payments for today to generate sequential number
            var todayPaymentsCount = await _context.Payments
                .Where(p => p.PaymentDate.Date == now.Date)
                .CountAsync();

            // Format: YY-MM-DD-NNNN (e.g., 22-04-10-0001)
            var sequenceNumber = (todayPaymentsCount + 1).ToString("0000");

            return $"{year}-{month}-{day}-{sequenceNumber}";
        }
    }
}