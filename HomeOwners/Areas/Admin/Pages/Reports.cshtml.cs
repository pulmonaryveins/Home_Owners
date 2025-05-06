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
    public class ReportsModel : PageModel
    {
        private readonly HomeDbContext _context;
        private readonly PaymentService _paymentService;
        private readonly ServiceService _serviceService;

        public ReportsModel(HomeDbContext context, PaymentService paymentService, ServiceService serviceService)
        {
            _context = context;
            _paymentService = paymentService;
            _serviceService = serviceService;
        }

        // Filter properties
        [BindProperty(SupportsGet = true)]
        public string TimeRange { get; set; } = "month"; // week, month, quarter, year

        // Revenue Report Data
        public RevenueData Revenue { get; set; }

        // User Statistics
        public UserStatistics UserStats { get; set; }

        // Service Request Statistics
        public ServiceRequestStats ServiceStats { get; set; }

        // Property Statistics
        public FacilityStats FacilityStatistics { get; set; }

        // Forum Activity
        public ForumActivityData ForumActivity { get; set; }

        public async Task OnGetAsync()
        {
            // Set date ranges based on selected time range
            DateTime startDate, endDate = DateTime.Now;

            switch (TimeRange)
            {
                case "week":
                    startDate = DateTime.Now.AddDays(-7);
                    break;
                case "month":
                    startDate = DateTime.Now.AddMonths(-1);
                    break;
                case "quarter":
                    startDate = DateTime.Now.AddMonths(-3);
                    break;
                case "year":
                default:
                    startDate = DateTime.Now.AddYears(-1);
                    break;
            }

            // Initialize data models
            Revenue = new RevenueData();
            UserStats = new UserStatistics();
            ServiceStats = new ServiceRequestStats();
            FacilityStatistics = new FacilityStats();
            ForumActivity = new ForumActivityData();

            // Generate Revenue Report Data
            await GenerateRevenueReportAsync(startDate, endDate);

            // Generate User Statistics
            await GenerateUserStatsAsync(startDate, endDate);

            // Generate Service Request Statistics
            await GenerateServiceStatsAsync(startDate, endDate);

            // Generate Facility Statistics
            await GenerateFacilityStatsAsync(startDate, endDate);

            // Generate Forum Activity Data
            await GenerateForumActivityDataAsync(startDate, endDate);
        }

        private async Task GenerateRevenueReportAsync(DateTime startDate, DateTime endDate)
        {
            // Get all payments within the date range
            var payments = await _context.Payments
                .Where(p => p.PaymentDate >= startDate && p.PaymentDate <= endDate)
                .ToListAsync();

            // Generate labels (x-axis) and data points based on the selected time range
            Revenue.Labels = new List<string>();
            Revenue.Data = new List<decimal>();

            if (TimeRange == "week")
            {
                // For weekly report, show daily revenue
                for (int i = 6; i >= 0; i--)
                {
                    var date = DateTime.Now.AddDays(-i);
                    Revenue.Labels.Add(date.ToString("ddd"));

                    var dailyPayments = payments
                        .Where(p => p.PaymentDate.Date == date.Date)
                        .Sum(p => p.AmountPaid);

                    Revenue.Data.Add(dailyPayments);
                }
            }
            else if (TimeRange == "month")
            {
                // For monthly report, show weekly revenue
                for (int i = 3; i >= 0; i--)
                {
                    var weekStart = DateTime.Now.AddDays(-i * 7 - 6);
                    var weekEnd = DateTime.Now.AddDays(-i * 7);
                    Revenue.Labels.Add($"{weekStart:MMM dd} - {weekEnd:MMM dd}");

                    var weeklyPayments = payments
                        .Where(p => p.PaymentDate >= weekStart && p.PaymentDate <= weekEnd)
                        .Sum(p => p.AmountPaid);

                    Revenue.Data.Add(weeklyPayments);
                }
            }
            else if (TimeRange == "quarter")
            {
                // For quarterly report, show monthly revenue
                for (int i = 2; i >= 0; i--)
                {
                    var month = DateTime.Now.AddMonths(-i);
                    Revenue.Labels.Add(month.ToString("MMMM"));

                    var monthlyPayments = payments
                        .Where(p => p.PaymentDate.Month == month.Month && p.PaymentDate.Year == month.Year)
                        .Sum(p => p.AmountPaid);

                    Revenue.Data.Add(monthlyPayments);
                }
            }
            else // year
            {
                // For yearly report, show quarterly revenue
                for (int i = 3; i >= 0; i--)
                {
                    var quarterStart = DateTime.Now.AddMonths(-i * 3);
                    Revenue.Labels.Add($"Q{(quarterStart.Month / 3) + 1}");

                    var quarterlyPayments = payments
                        .Where(p => p.PaymentDate.Month >= quarterStart.Month &&
                                   p.PaymentDate.Month < quarterStart.Month + 3 &&
                                   p.PaymentDate.Year == quarterStart.Year)
                        .Sum(p => p.AmountPaid);

                    Revenue.Data.Add(quarterlyPayments);
                }
            }

            // Set the total revenue for the period
            Revenue.TotalRevenue = payments.Sum(p => p.AmountPaid);

            // Get payment methods breakdown
            var paymentMethods = payments
                .GroupBy(p => p.PaymentMethod)
                .Select(g => new PaymentMethodData
                {
                    Method = g.Key.ToString(), // Convert enum to string
                    Amount = g.Sum(p => p.AmountPaid),
                    Percentage = (decimal)g.Count() / payments.Count * 100
                })
                .ToList();

            Revenue.PaymentMethods = paymentMethods;

            // Compare with previous period
            var previousStartDate = startDate.AddDays(-(endDate - startDate).Days);
            var previousEndDate = startDate.AddDays(-1);

            var previousPayments = await _context.Payments
                .Where(p => p.PaymentDate >= previousStartDate && p.PaymentDate <= previousEndDate)
                .ToListAsync();

            var previousTotal = previousPayments.Sum(p => p.AmountPaid);

            if (previousTotal > 0)
            {
                Revenue.ChangePercentage = (Revenue.TotalRevenue - previousTotal) / previousTotal * 100;
            }
            else
            {
                Revenue.ChangePercentage = 100; // If there were no payments in the previous period
            }
        }

        private async Task GenerateUserStatsAsync(DateTime startDate, DateTime endDate)
        {
            // Total users
            UserStats.TotalUsers = await _context.HomeOwnerUsers.CountAsync();

            // New users in the selected period
            UserStats.NewUsers = await _context.HomeOwnerUsers
                .Where(u => u.CreatedDate >= startDate && u.CreatedDate <= endDate)
                .CountAsync();

            // Active users (with activity in the last period)
            var activeUserIds = await _context.ServiceRequests
                .Where(s => s.CreatedDate >= startDate)
                .Select(s => s.UserId)
                .Distinct()
                .ToListAsync();

            var activeBookingUserIds = await _context.Bookings
                .Where(b => b.CreatedDate >= startDate)
                .Select(b => b.UserId)
                .Distinct()
                .ToListAsync();

            activeUserIds.AddRange(activeBookingUserIds);
            UserStats.ActiveUsers = activeUserIds.Distinct().Count();

            // Calculate active percentage
            UserStats.ActivePercentage = (double)UserStats.ActiveUsers / UserStats.TotalUsers * 100;

            // User growth trend
            UserStats.GrowthLabels = new List<string>();
            UserStats.GrowthData = new List<int>();

            // Generate growth data based on time range
            if (TimeRange == "week" || TimeRange == "month")
            {
                // Weekly growth over days
                int days = TimeRange == "week" ? 7 : 30;
                for (int i = days - 1; i >= 0; i--)
                {
                    var date = DateTime.Now.AddDays(-i);
                    UserStats.GrowthLabels.Add(date.ToString("MMM dd"));

                    var newUsersCount = await _context.HomeOwnerUsers
                        .Where(u => u.CreatedDate.Date == date.Date)
                        .CountAsync();

                    UserStats.GrowthData.Add(newUsersCount);
                }
            }
            else // quarter or year
            {
                // Monthly growth
                int months = TimeRange == "quarter" ? 3 : 12;
                for (int i = months - 1; i >= 0; i--)
                {
                    var month = DateTime.Now.AddMonths(-i);
                    UserStats.GrowthLabels.Add(month.ToString("MMM yy"));

                    var newUsersCount = await _context.HomeOwnerUsers
                        .Where(u => u.CreatedDate.Month == month.Month && u.CreatedDate.Year == month.Year)
                        .CountAsync();

                    UserStats.GrowthData.Add(newUsersCount);
                }
            }
        }

        private async Task GenerateServiceStatsAsync(DateTime startDate, DateTime endDate)
        {
            // Get service requests in the period
            var serviceRequests = await _context.ServiceRequests
                .Where(s => s.CreatedDate >= startDate && s.CreatedDate <= endDate)
                .ToListAsync();

            ServiceStats.TotalRequests = serviceRequests.Count;

            // Status breakdown
            var pendingCount = serviceRequests.Count(s => s.Status == ServiceRequestStatus.Pending);
            var approvedCount = serviceRequests.Count(s => s.Status == ServiceRequestStatus.Approved);
            var doneCount = serviceRequests.Count(s => s.Status == ServiceRequestStatus.Done);
            var rejectedCount = serviceRequests.Count(s => s.Status == ServiceRequestStatus.Rejected);

            ServiceStats.StatusLabels = new List<string> { "Pending", "Approved", "Completed", "Rejected" };
            ServiceStats.StatusData = new List<int> { pendingCount, approvedCount, doneCount, rejectedCount };

            // Service type breakdown
            var serviceTypes = await _context.ServiceRequests
                .Where(s => s.CreatedDate >= startDate && s.CreatedDate <= endDate)
                .GroupBy(s => s.ServiceId)
                .Select(g => new { ServiceId = g.Key, Count = g.Count() })
                .ToListAsync();

            ServiceStats.ServiceTypeLabels = new List<string>();
            ServiceStats.ServiceTypeData = new List<int>();

            foreach (var serviceType in serviceTypes.OrderByDescending(s => s.Count).Take(5))
            {
                var service = await _context.Services.FindAsync(serviceType.ServiceId);
                ServiceStats.ServiceTypeLabels.Add(service?.Name ?? "Unknown");
                ServiceStats.ServiceTypeData.Add(serviceType.Count);
            }

            // Average resolution time
            var completedRequests = await _context.ServiceRequests
                .Where(s => s.Status == ServiceRequestStatus.Done &&
                         s.CreatedDate >= startDate)
                .ToListAsync();

            if (completedRequests.Any())
            {
                // Since CompletedDate is missing, estimate based on request creation date
                // You may want to adjust this logic based on your actual data structure
                ServiceStats.AvgResolutionTime = TimeSpan.FromDays(3); // Default placeholder value

                // Alternatively, if you have a field that records completion:
                // ServiceStats.AvgResolutionTime = TimeSpan.FromMinutes(
                //    completedRequests.Average(s => (s.LastUpdatedDate - s.CreatedDate).TotalMinutes)
                // );
            }
            else
            {
                ServiceStats.AvgResolutionTime = TimeSpan.Zero;
            }

            // Request volume over time
            ServiceStats.VolumeLabels = new List<string>();
            ServiceStats.VolumeData = new List<int>();

            if (TimeRange == "week")
            {
                // Daily volume for week
                for (int i = 6; i >= 0; i--)
                {
                    var date = DateTime.Now.AddDays(-i);
                    ServiceStats.VolumeLabels.Add(date.ToString("ddd"));

                    var count = serviceRequests.Count(s => s.CreatedDate.Date == date.Date);
                    ServiceStats.VolumeData.Add(count);
                }
            }
            else if (TimeRange == "month")
            {
                // Weekly volume for month
                for (int i = 3; i >= 0; i--)
                {
                    var weekStart = DateTime.Now.AddDays(-i * 7 - 6);
                    var weekEnd = DateTime.Now.AddDays(-i * 7);
                    ServiceStats.VolumeLabels.Add($"Week {4 - i}");

                    var count = serviceRequests.Count(s =>
                        s.CreatedDate >= weekStart && s.CreatedDate <= weekEnd);
                    ServiceStats.VolumeData.Add(count);
                }
            }
            else // quarter or year
            {
                // Monthly volume
                int months = TimeRange == "quarter" ? 3 : 12;
                for (int i = months - 1; i >= 0; i--)
                {
                    var month = DateTime.Now.AddMonths(-i);
                    ServiceStats.VolumeLabels.Add(month.ToString("MMM"));

                    var count = serviceRequests.Count(s =>
                        s.CreatedDate.Month == month.Month && s.CreatedDate.Year == month.Year);
                    ServiceStats.VolumeData.Add(count);
                }
            }
        }

        private async Task GenerateFacilityStatsAsync(DateTime startDate, DateTime endDate)
        {
            // Get facility bookings in the period
            var bookings = await _context.Bookings
                .Include(b => b.Facility)
                .Where(b => b.CreatedDate >= startDate && b.CreatedDate <= endDate)
                .ToListAsync();

            FacilityStatistics.TotalBookings = bookings.Count;

            // Most popular facilities
            var facilityUsage = bookings
                .GroupBy(b => b.FacilityId)
                .Select(g => new { FacilityId = g.Key, Name = g.First().Facility?.Name, Count = g.Count() })
                .OrderByDescending(f => f.Count)
                .Take(5)
                .ToList();

            FacilityStatistics.PopularFacilityLabels = facilityUsage.Select(f => f.Name ?? "Unknown").ToList();
            FacilityStatistics.PopularFacilityData = facilityUsage.Select(f => f.Count).ToList();

            // Occupancy rate
            var facilities = await _context.Facilities.ToListAsync();
            FacilityStatistics.TotalFacilities = facilities.Count;

            int totalBookingHours = 0;
            int totalAvailableHours = 0;

            foreach (var facility in facilities)
            {
                var facilityBookings = bookings.Where(b => b.FacilityId == facility.Id).ToList();

                // Calculate total hours booked
                foreach (var booking in facilityBookings)
                {
                    totalBookingHours += (int)(booking.EndTime - booking.StartTime).TotalHours;
                }

                // Calculate total available hours (assuming 12h/day)
                int daysInPeriod = (int)(endDate - startDate).TotalDays + 1;
                totalAvailableHours += 12 * daysInPeriod;
            }

            // Calculate occupancy rate
            if (totalAvailableHours > 0)
            {
                FacilityStatistics.OccupancyRate = (double)totalBookingHours / totalAvailableHours * 100;
            }

            // Revenue by facility
            FacilityStatistics.RevenueFacilityLabels = new List<string>();
            FacilityStatistics.RevenueFacilityData = new List<decimal>();

            var facilityRevenue = await _context.Payments
                .Include(p => p.Booking)
                .Include(p => p.Booking.Facility)
                .Where(p => p.PaymentDate >= startDate && p.PaymentDate <= endDate)
                .GroupBy(p => p.Booking.FacilityId)
                .Select(g => new {
                    FacilityId = g.Key,
                    Name = g.First().Booking.Facility.Name,
                    Revenue = g.Sum(p => p.AmountPaid)
                })
                .OrderByDescending(f => f.Revenue)
                .Take(5)
                .ToListAsync();

            FacilityStatistics.RevenueFacilityLabels = facilityRevenue.Select(f => f.Name).ToList();
            FacilityStatistics.RevenueFacilityData = facilityRevenue.Select(f => f.Revenue).ToList();
        }

        private async Task GenerateForumActivityDataAsync(DateTime startDate, DateTime endDate)
        {
            // Total posts and comments in the period
            ForumActivity.TotalPosts = await _context.ForumPosts
                .Where(p => p.PostedDate >= startDate && p.PostedDate <= endDate)
                .CountAsync();

            ForumActivity.TotalComments = await _context.ForumComments
                .Where(c => c.PostedDate >= startDate && c.PostedDate <= endDate)
                .CountAsync();

            // Popular categories
            var categoryActivity = await _context.ForumPosts
                .Where(p => p.PostedDate >= startDate && p.PostedDate <= endDate)
                .GroupBy(p => p.Category)
                .Select(g => new { Category = g.Key, Count = g.Count() })
                .OrderByDescending(c => c.Count)
                .Take(5)
                .ToListAsync();

            ForumActivity.CategoryLabels = categoryActivity.Select(c => c.Category).ToList();
            ForumActivity.CategoryData = categoryActivity.Select(c => c.Count).ToList();

            // Top contributors
            var topContributors = await _context.ForumPosts
                .Where(p => p.PostedDate >= startDate && p.PostedDate <= endDate)
                .GroupBy(p => p.UserId)
                .Select(g => new { UserId = g.Key, UserName = g.First().UserName, Count = g.Count() })
                .OrderByDescending(c => c.Count)
                .Take(5)
                .ToListAsync();

            ForumActivity.ContributorLabels = topContributors.Select(c => c.UserName).ToList();
            ForumActivity.ContributorData = topContributors.Select(c => c.Count).ToList();

            // Activity trend over time
            ForumActivity.ActivityLabels = new List<string>();
            ForumActivity.PostsData = new List<int>();
            ForumActivity.CommentsData = new List<int>();

            if (TimeRange == "week")
            {
                // Daily activity for week
                for (int i = 6; i >= 0; i--)
                {
                    var date = DateTime.Now.AddDays(-i);
                    ForumActivity.ActivityLabels.Add(date.ToString("ddd"));

                    var postsCount = await _context.ForumPosts
                        .Where(p => p.PostedDate.Date == date.Date)
                        .CountAsync();

                    var commentsCount = await _context.ForumComments
                        .Where(c => c.PostedDate.Date == date.Date)
                        .CountAsync();

                    ForumActivity.PostsData.Add(postsCount);
                    ForumActivity.CommentsData.Add(commentsCount);
                }
            }
            else if (TimeRange == "month")
            {
                // Weekly activity for month
                for (int i = 3; i >= 0; i--)
                {
                    var weekStart = DateTime.Now.AddDays(-i * 7 - 6);
                    var weekEnd = DateTime.Now.AddDays(-i * 7);
                    ForumActivity.ActivityLabels.Add($"Week {4 - i}");

                    var postsCount = await _context.ForumPosts
                        .Where(p => p.PostedDate >= weekStart && p.PostedDate <= weekEnd)
                        .CountAsync();

                    var commentsCount = await _context.ForumComments
                        .Where(c => c.PostedDate >= weekStart && c.PostedDate <= weekEnd)
                        .CountAsync();

                    ForumActivity.PostsData.Add(postsCount);
                    ForumActivity.CommentsData.Add(commentsCount);
                }
            }
            else // quarter or year
            {
                // Monthly activity
                int months = TimeRange == "quarter" ? 3 : 12;
                for (int i = months - 1; i >= 0; i--)
                {
                    var month = DateTime.Now.AddMonths(-i);
                    ForumActivity.ActivityLabels.Add(month.ToString("MMM"));

                    var postsCount = await _context.ForumPosts
                        .Where(p => p.PostedDate.Month == month.Month && p.PostedDate.Year == month.Year)
                        .CountAsync();

                    var commentsCount = await _context.ForumComments
                        .Where(c => c.PostedDate.Month == month.Month && c.PostedDate.Year == month.Year)
                        .CountAsync();

                    ForumActivity.PostsData.Add(postsCount);
                    ForumActivity.CommentsData.Add(commentsCount);
                }
            }
        }
    }

    // Supporting model classes
    public class RevenueData
    {
        public List<string> Labels { get; set; } = new List<string>();
        public List<decimal> Data { get; set; } = new List<decimal>();
        public decimal TotalRevenue { get; set; }
        public decimal ChangePercentage { get; set; }
        public List<PaymentMethodData> PaymentMethods { get; set; } = new List<PaymentMethodData>();
    }

    public class PaymentMethodData
    {
        public string Method { get; set; }
        public decimal Amount { get; set; }
        public decimal Percentage { get; set; }
    }

    public class UserStatistics
    {
        public int TotalUsers { get; set; }
        public int NewUsers { get; set; }
        public int ActiveUsers { get; set; }
        public double ActivePercentage { get; set; }
        public List<string> GrowthLabels { get; set; } = new List<string>();
        public List<int> GrowthData { get; set; } = new List<int>();
    }

    public class ServiceRequestStats
    {
        public int TotalRequests { get; set; }
        public List<string> StatusLabels { get; set; } = new List<string>();
        public List<int> StatusData { get; set; } = new List<int>();
        public List<string> ServiceTypeLabels { get; set; } = new List<string>();
        public List<int> ServiceTypeData { get; set; } = new List<int>();
        public TimeSpan AvgResolutionTime { get; set; }
        public List<string> VolumeLabels { get; set; } = new List<string>();
        public List<int> VolumeData { get; set; } = new List<int>();
    }

    public class FacilityStats
    {
        public int TotalFacilities { get; set; }
        public int TotalBookings { get; set; }
        public List<string> PopularFacilityLabels { get; set; } = new List<string>();
        public List<int> PopularFacilityData { get; set; } = new List<int>();
        public double OccupancyRate { get; set; }
        public List<string> RevenueFacilityLabels { get; set; } = new List<string>();
        public List<decimal> RevenueFacilityData { get; set; } = new List<decimal>();
    }

    public class ForumActivityData
    {
        public int TotalPosts { get; set; }
        public int TotalComments { get; set; }
        public List<string> CategoryLabels { get; set; } = new List<string>();
        public List<int> CategoryData { get; set; } = new List<int>();
        public List<string> ContributorLabels { get; set; } = new List<string>();
        public List<int> ContributorData { get; set; } = new List<int>();
        public List<string> ActivityLabels { get; set; } = new List<string>();
        public List<int> PostsData { get; set; } = new List<int>();
        public List<int> CommentsData { get; set; } = new List<int>();
    }
}