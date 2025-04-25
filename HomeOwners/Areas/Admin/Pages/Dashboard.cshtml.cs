using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HomeOwners.Areas.Identity.Data;
using HomeOwners.Models;
using HomeOwners.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace HomeOwners.Areas.Admin.Pages
{
    [Authorize(Policy = "RequireAdminRole")]
    public class DashboardModel : PageModel
    {
        private readonly HomeDbContext _context;
        private readonly EventService _eventService;
        private readonly PaymentService _paymentService;

        public DashboardModel(HomeDbContext context, EventService eventService, PaymentService paymentService)
        {
            _context = context;
            _eventService = eventService;
            _paymentService = paymentService;
        }

        // Stats Summary Properties
        public int TotalUsers { get; set; }
        public double NewUsersPercentage { get; set; }
        public int ActiveServices { get; set; }
        public double ServiceGrowthRate { get; set; }
        public int ActiveFacilities { get; set; }  // Changed from PendingApprovals
        public double ActiveFacilitiesChange { get; set; }  // Changed from PendingApprovalsChange
        public decimal MonthlyRevenue { get; set; }
        public double RevenueChange { get; set; }

        // Community Engagement Properties
        public int ActivePolls { get; set; }
        public int ForumPosts { get; set; }
        public int ForumComments { get; set; }

        // Charts Data
        public ActivityTrendsData ActivityTrends { get; set; }
        public List<int> ServiceStats { get; set; }
        public FacilityUsageData FacilityUsage { get; set; }
        public PaymentStatsData PaymentStats { get; set; }

        // Recent Activities
        public List<DashboardActivity> RecentActivities { get; set; }
        public List<Event> UpcomingEvents { get; set; }
        public List<UserApprovalModel> PendingUserApprovals { get; set; }

        public async Task OnGetAsync()
        {
            // Get total and new users
            TotalUsers = await _context.HomeOwnerUsers.CountAsync();
            var lastMonthUsers = await _context.HomeOwnerUsers
                .Where(u => u.CreatedDate <= DateTime.Now.AddMonths(-1))
                .CountAsync();

            // Calculate percentage growth
            if (lastMonthUsers > 0)
            {
                NewUsersPercentage = Math.Round(((double)TotalUsers - lastMonthUsers) / lastMonthUsers * 100, 1);
            }
            else
            {
                NewUsersPercentage = 100; // If there were 0 users last month
            }

            // Get active services
            ActiveServices = await _context.Services.Where(s => s.IsActive).CountAsync();
            var lastMonthServices = await _context.Services
                .Where(s => s.IsActive && s.CreatedDate <= DateTime.Now.AddMonths(-1))
                .CountAsync();

            // Calculate service growth rate
            if (lastMonthServices > 0)
            {
                ServiceGrowthRate = Math.Round(((double)ActiveServices - lastMonthServices) / lastMonthServices * 100, 1);
            }
            else
            {
                ServiceGrowthRate = 100;
            }

            // Get active facilities (replacing pending approvals)
            ActiveFacilities = await _context.Facilities.Where(f => f.IsActive).CountAsync();

            // Calculate change in active facilities
            var lastWeekActiveFacilities = await _context.Facilities
                .Where(f => f.IsActive && f.CreatedDate <= DateTime.Now.AddDays(-7))
                .CountAsync();

            if (lastWeekActiveFacilities > 0)
            {
                ActiveFacilitiesChange = Math.Round(((double)ActiveFacilities - lastWeekActiveFacilities) / lastWeekActiveFacilities * 100, 1);
            }
            else
            {
                ActiveFacilitiesChange = 0;
            }

            // Calculate monthly revenue
            var currentMonthPayments = await _context.Payments
                .Where(p => p.PaymentDate.Month == DateTime.Now.Month && p.PaymentDate.Year == DateTime.Now.Year)
                .ToListAsync();
            MonthlyRevenue = currentMonthPayments.Sum(p => p.AmountPaid);

            // Calculate revenue change
            var lastMonthPayments = await _context.Payments
                .Where(p => p.PaymentDate.Month == DateTime.Now.AddMonths(-1).Month && p.PaymentDate.Year == DateTime.Now.AddMonths(-1).Year)
                .ToListAsync();
            var lastMonthRevenue = lastMonthPayments.Sum(p => p.AmountPaid);

            if (lastMonthRevenue > 0)
            {
                RevenueChange = Math.Round(((double)MonthlyRevenue - (double)lastMonthRevenue) / (double)lastMonthRevenue * 100, 1);
            }
            else
            {
                RevenueChange = 100;
            }

            // Get community engagement data (Polls and Forum)
            ActivePolls = await _context.Polls
                .Where(p => p.IsActive && p.StartDate <= DateTime.Now && p.EndDate >= DateTime.Now)
                .CountAsync();

            ForumPosts = await _context.ForumPosts
                .Where(p => p.IsVisible)  // Changed from p.IsActive
                .CountAsync();

            ForumComments = await _context.ForumComments
                .CountAsync();

            // Prepare activity trends data
            ActivityTrends = new ActivityTrendsData();
            ActivityTrends.Labels = new List<string>();
            ActivityTrends.RegistrationData = new List<int>();
            ActivityTrends.ServiceRequestData = new List<int>();

            // Generate last 6 months labels and data
            var today = DateTime.Today;
            for (int i = 5; i >= 0; i--)
            {
                var month = today.AddMonths(-i);
                ActivityTrends.Labels.Add(month.ToString("MMM yyyy"));

                // Count registrations for this month
                var registrations = await _context.HomeOwnerUsers
                    .Where(u => u.CreatedDate.Month == month.Month && u.CreatedDate.Year == month.Year)
                    .CountAsync();
                ActivityTrends.RegistrationData.Add(registrations);

                // Count service requests for this month
                var serviceRequests = await _context.ServiceRequests
                    .Where(s => s.CreatedDate.Month == month.Month && s.CreatedDate.Year == month.Year)
                    .CountAsync();
                ActivityTrends.ServiceRequestData.Add(serviceRequests);
            }

            // Prepare service stats data
            var openRequests = await _context.ServiceRequests.Where(s => s.Status == ServiceRequestStatus.Pending).CountAsync();
            var inProgressRequests = await _context.ServiceRequests
                .Where(s => s.Status == ServiceRequestStatus.Approved) // or whichever value is appropriate
                .CountAsync();
            var completedRequests = await _context.ServiceRequests
                .Where(s => s.Status == ServiceRequestStatus.Done) // instead of Completed
                .CountAsync();
            var cancelledRequests = await _context.ServiceRequests
                .Where(s => s.Status == ServiceRequestStatus.Rejected) // instead of Cancelled
                .CountAsync();
            ServiceStats = new List<int> { openRequests, inProgressRequests, completedRequests, cancelledRequests };

            // Prepare facility usage data
            FacilityUsage = new FacilityUsageData();
            var facilities = await _context.Facilities.Take(5).ToListAsync();
            FacilityUsage.Labels = facilities.Select(f => f.Name).ToList();

            // Count bookings for each facility
            FacilityUsage.Data = new List<int>();
            foreach (var facility in facilities)
            {
                var bookingCount = await _context.Bookings.Where(b => b.FacilityId == facility.Id).CountAsync();
                FacilityUsage.Data.Add(bookingCount);
            }

            // Prepare payment stats
            var totalPayments = await _context.Payments.CountAsync();
            var paidPayments = await _context.Payments.Where(p => p.Booking.PaymentStatus == PaymentStatus.Paid).CountAsync();
            var pendingPayments = totalPayments - paidPayments;
            var overduePayments = await _context.Bookings
                .Where(b => b.PaymentStatus == PaymentStatus.Unpaid && b.BookingDate.Date < DateTime.Today)
                .CountAsync();

            double paidPercentage = totalPayments > 0 ? Math.Round((double)paidPayments / totalPayments * 100, 1) : 0;
            double pendingPercentage = totalPayments > 0 ? Math.Round((double)pendingPayments / totalPayments * 100, 1) : 0;
            double overduePercentage = totalPayments > 0 ? Math.Round((double)overduePayments / totalPayments * 100, 1) : 0;

            PaymentStats = new PaymentStatsData
            {
                PaidPercentage = paidPercentage,
                PendingPercentage = pendingPercentage,
                OverduePercentage = overduePercentage
            };

            // Get recent activities
            RecentActivities = new List<DashboardActivity>();

            // Get recent bookings
            var recentBookings = await _context.Bookings
                .Include(b => b.Facility)
                .OrderByDescending(b => b.CreatedDate)
                .Take(3)
                .ToListAsync();

            foreach (var booking in recentBookings)
            {
                RecentActivities.Add(new DashboardActivity
                {
                    Title = $"New Booking: {booking.Facility?.Name}",
                    Subtitle = $"by {booking.FullName} - {booking.Status}",
                    Time = FormatTimeAgo(booking.CreatedDate),
                    Icon = "bi-calendar-check",
                    IconClass = "bg-primary"
                });
            }

            // Get recent service requests
            var recentServiceRequests = await _context.ServiceRequests
                .Include(s => s.Service)
                .OrderByDescending(s => s.CreatedDate)
                .Take(3)
                .ToListAsync();

            foreach (var request in recentServiceRequests)
            {
                RecentActivities.Add(new DashboardActivity
                {
                    Title = $"Service Request: {request.Service?.Name}",
                    Subtitle = $"by {request.FullName} - {request.Status}",
                    Time = FormatTimeAgo(request.CreatedDate),
                    Icon = "bi-tools",
                    IconClass = "bg-info"
                });
            }

            // Get recent forum posts
            var recentForumPosts = await _context.ForumPosts
                .OrderByDescending(p => p.PostedDate)  
                .Take(2)
                .ToListAsync();

            foreach (var post in recentForumPosts)
            {
                RecentActivities.Add(new DashboardActivity
                {
                    Title = $"Forum Post: {post.Title}",
                    Subtitle = $"by {post.UserName}",
                    Time = FormatTimeAgo(post.PostedDate),  
                    Icon = "bi-chat-left-text",
                    IconClass = "bg-warning"
                });
            }

            // Get recent payments
            var recentPayments = await _context.Payments
                .Include(p => p.Booking)
                .Include(p => p.Booking.Facility)
                .OrderByDescending(p => p.PaymentDate)
                .Take(2)
                .ToListAsync();

            foreach (var payment in recentPayments)
            {
                RecentActivities.Add(new DashboardActivity
                {
                    Title = $"Payment: {payment.Booking?.Facility?.Name}",
                    Subtitle = $"₱{payment.AmountPaid} - {payment.PaymentMethod}", // Changed $ to ₱
                    Time = FormatTimeAgo(payment.PaymentDate),
                    Icon = "bi-credit-card",
                    IconClass = "bg-success"
                });
            }

            // Sort by most recent
            RecentActivities = RecentActivities.OrderByDescending(a => a.Time).ToList();

            // Get upcoming events
            UpcomingEvents = await _eventService.GetUpcomingEventsAsync(5);

            // Get pending user approvals
            var pendingUsers = await _context.HomeOwnerUsers
                .Where(u => u.AccountStatus == "pending")
                .OrderByDescending(u => u.CreatedDate)
                .Take(5)
                .ToListAsync();
            PendingUserApprovals = pendingUsers.Select(u => new UserApprovalModel
            {
                Id = u.Id,
                FullName = u.FullName,
                HouseNumber = u.HouseNumber,
                RequestDate = u.CreatedDate
            }).ToList();
        }

        private string FormatTimeAgo(DateTime dateTime)
        {
            TimeSpan timeDiff = DateTime.Now - dateTime;

            if (timeDiff.TotalMinutes < 1)
                return "Just now";
            if (timeDiff.TotalHours < 1)
                return $"{(int)timeDiff.TotalMinutes}m ago";
            if (timeDiff.TotalDays < 1)
                return $"{(int)timeDiff.TotalHours}h ago";
            if (timeDiff.TotalDays < 7)
                return $"{(int)timeDiff.TotalDays}d ago";

            return dateTime.ToString("MMM dd");
        }
    }

    // Supporting model classes remain the same
    public class ActivityTrendsData
    {
        public List<string> Labels { get; set; }
        public List<int> RegistrationData { get; set; }
        public List<int> ServiceRequestData { get; set; }
    }

    public class FacilityUsageData
    {
        public List<string> Labels { get; set; }
        public List<int> Data { get; set; }
    }

    public class PaymentStatsData
    {
        public double PaidPercentage { get; set; }
        public double PendingPercentage { get; set; }
        public double OverduePercentage { get; set; }
    }

    public class DashboardActivity
    {
        public string Title { get; set; }
        public string Subtitle { get; set; }
        public string Time { get; set; }
        public string Icon { get; set; }
        public string IconClass { get; set; }
    }

    public class UserApprovalModel
    {
        public string Id { get; set; }
        public string FullName { get; set; }
        public string HouseNumber { get; set; }
        public DateTime RequestDate { get; set; }
    }

    // Define this enum if not already exists in your models
    public enum HomeOwnerUserStatus
    {
        Pending,
        Approved,
        Rejected
    }
}