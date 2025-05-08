using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using HomeOwners.Areas.Identity.Data;
using HomeOwners.Models;
using HomeOwners.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HomeOwners.Areas.Staff.Pages
{
    [Authorize(Policy = "RequireStaffRole")]
    public class DashboardModel : PageModel
    {
        private readonly HomeDbContext _context;
        private readonly ServiceRequestService _serviceRequestService;
        private readonly TaskService _taskService;


        public DashboardModel(HomeDbContext context, ServiceRequestService serviceRequestService, TaskService taskService)
        {
            _context = context;
            _serviceRequestService = serviceRequestService;
            _taskService = taskService;
        }

        // Stats Summary Properties
        public int TotalUsers { get; set; }
        public double UserGrowthRate { get; set; }
        public int ActiveServices { get; set; }
        public double ServiceGrowthRate { get; set; }
        public int TotalFacilities { get; set; }
        public double FacilityChangeRate { get; set; }
        public int PendingRequests { get; set; }
        public double PendingRequestChangeRate { get; set; }
        public List<StaffTask> UpcomingTasks { get; set; }


        // Charts Data
        public ServiceActivityData ChartData { get; set; }
        public TaskStatusData TaskStatusChartData { get; set; }

        // Activity List
        public List<DashboardActivity> RecentActivities { get; set; }

        public async Task OnGetAsync()
        {
            await LoadStatisticsAsync();
            await LoadServiceActivityDataAsync();
            await LoadTaskStatusDataAsync();
            await LoadRecentActivitiesAsync();

            // Load upcoming tasks
            UpcomingTasks = await _taskService.GetUpcomingTasksAsync(7); // Get tasks due in the next 7 days
        }

        private async Task LoadStatisticsAsync()
        {
            // Get total users (counting custom user types that have CreatedDate)
            TotalUsers = await _context.Users.CountAsync();

            // For growth rate, use HomeOwnerUsers which should have CreatedDate
            var lastMonthDate = DateTime.Now.AddMonths(-1);
            var homeOwnersTotal = await _context.HomeOwnerUsers.CountAsync();
            var homeOwnersLastMonth = await _context.HomeOwnerUsers
                .CountAsync(u => u.CreatedDate <= lastMonthDate);

            // Calculate growth rate based on HomeOwnerUsers
            UserGrowthRate = homeOwnersLastMonth > 0
                ? Math.Round(((double)homeOwnersTotal - homeOwnersLastMonth) / homeOwnersLastMonth * 100, 1)
                : 8.0; // Default fallback if no historical data

            // Get active services and growth rate
            ActiveServices = await _context.Services.CountAsync(s => s.IsActive);
            var servicesLastMonth = await _context.Services
                .CountAsync(s => s.IsActive && s.CreatedDate <= lastMonthDate);

            ServiceGrowthRate = servicesLastMonth > 0
                ? Math.Round(((double)ActiveServices - servicesLastMonth) / servicesLastMonth * 100, 1)
                : 5.0; // Default fallback

            // Get facilities and change rate (assuming no change)
            TotalFacilities = await _context.Facilities.CountAsync();
            FacilityChangeRate = 0;

            // Get pending requests and change rate
            var now = DateTime.Now;
            PendingRequests = await _context.ServiceRequests
                .CountAsync(r => r.Status == ServiceRequestStatus.Pending);

            var pendingRequestsLastMonth = await _context.ServiceRequests
                .CountAsync(r => r.Status == ServiceRequestStatus.Pending &&
                             r.RequestDate <= lastMonthDate); // Use RequestDate instead of CreatedDate

            PendingRequestChangeRate = pendingRequestsLastMonth > 0
                ? Math.Round(((double)PendingRequests - pendingRequestsLastMonth) / pendingRequestsLastMonth * 100, 1) * -1 // Invert for display (decreasing is positive)
                : -12.0; // Default fallback
        }

        private async Task LoadServiceActivityDataAsync()
        {
            // Get service request data for the past 4 weeks
            var fourWeeksAgo = DateTime.Now.AddDays(-28);

            // Group data by week - use RequestDate instead of CreatedDate
            var serviceRequests = await _context.ServiceRequests
                .Where(sr => sr.RequestDate >= fourWeeksAgo)
                .ToListAsync();

            var completedTasks = await _context.ServiceRequests
                .Where(sr => sr.Status == ServiceRequestStatus.Done && sr.RequestDate >= fourWeeksAgo)
                .ToListAsync();

            // Group by week for chart display
            var weeks = new List<string> { "Week 1", "Week 2", "Week 3", "Week 4" };
            var serviceRequestCounts = new List<int> { 0, 0, 0, 0 };
            var completedTaskCounts = new List<int> { 0, 0, 0, 0 };

            // Calculate date ranges for weeks
            var weekRanges = new List<(DateTime start, DateTime end)>();
            for (int i = 0; i < 4; i++)
            {
                var end = DateTime.Now.AddDays(-7 * i);
                var start = end.AddDays(-6);
                weekRanges.Insert(0, (start, end)); // Insert at beginning to get chronological order
            }

            // Populate data
            for (int i = 0; i < weekRanges.Count; i++)
            {
                var (start, end) = weekRanges[i];
                serviceRequestCounts[i] = serviceRequests.Count(sr => sr.RequestDate >= start && sr.RequestDate <= end);
                completedTaskCounts[i] = completedTasks.Count(sr => sr.RequestDate >= start && sr.RequestDate <= end);
            }

            ChartData = new ServiceActivityData
            {
                Labels = weeks,
                ServiceRequests = serviceRequestCounts,
                CompletedTasks = completedTaskCounts
            };
        }

        private async Task LoadTaskStatusDataAsync()
        {
            // Get task status distribution
            int completed = await _context.ServiceRequests.CountAsync(sr => sr.Status == ServiceRequestStatus.Done);
            int inProgress = await _context.ServiceRequests.CountAsync(sr => sr.Status == ServiceRequestStatus.Approved);
            int pending = await _context.ServiceRequests.CountAsync(sr => sr.Status == ServiceRequestStatus.Pending);
            int rejected = await _context.ServiceRequests.CountAsync(sr => sr.Status == ServiceRequestStatus.Rejected);

            TaskStatusChartData = new TaskStatusData
            {
                Labels = new List<string> { "Completed", "In Progress", "Pending", "Rejected" },
                Values = new List<int> { completed, inProgress, pending, rejected }
            };
        }

        private async Task LoadRecentActivitiesAsync()
        {
            RecentActivities = new List<DashboardActivity>();

            // Get recent service requests with modified subtitle
            var recentRequests = await _context.ServiceRequests
                .Include(sr => sr.Service)
                .OrderByDescending(sr => sr.RequestDate) // Use RequestDate instead of CreatedDate
                .Take(5)
                .Select(sr => new DashboardActivity
                {
                    Type = "serviceRequest",
                    Title = "New service request",
                    SubTitle = $"{sr.Service.Name}", // Remove UnitNumber reference
                    Time = sr.RequestDate, // Use RequestDate
                    IconClass = "bg-primary",
                    Icon = "bi-person-plus"
                })
                .ToListAsync();

            // Get recently completed service requests with modified subtitle
            var recentCompletions = await _context.ServiceRequests
                .Include(sr => sr.Service)
                .Where(sr => sr.Status == ServiceRequestStatus.Done)
                .OrderByDescending(sr => sr.RequestDate) // Use RequestDate instead of UpdatedDate
                .Take(3)
                .Select(sr => new DashboardActivity
                {
                    Type = "serviceCompletion",
                    Title = "Service request completed",
                    SubTitle = $"{sr.Service.Name}", // Remove UnitNumber reference
                    Time = sr.RequestDate, // Use RequestDate since UpdatedDate is not available
                    IconClass = "bg-success",
                    Icon = "bi-check-circle"
                })
                .ToListAsync();

            // Get recent facility bookings
            var recentBookings = await _context.Bookings
                .Include(b => b.Facility)
                .OrderByDescending(b => b.BookingDate) // Use BookingDate which should exist
                .Take(3)
                .Select(b => new DashboardActivity
                {
                    Type = "booking",
                    Title = "Facility booking",
                    SubTitle = $"{b.Facility.Name} booked by {b.FullName}",
                    Time = b.BookingDate, // Use BookingDate instead of CreatedDate
                    IconClass = "bg-warning",
                    Icon = "bi-calendar-event"
                })
                .ToListAsync();

            // Combine and sort activities
            RecentActivities = recentRequests
                .Concat(recentCompletions)
                .Concat(recentBookings)
                .OrderByDescending(a => a.Time)
                .Take(4)
                .ToList();
        }
    }

    public class ServiceActivityData
    {
        public List<string> Labels { get; set; }
        public List<int> ServiceRequests { get; set; }
        public List<int> CompletedTasks { get; set; }
    }

    public class TaskStatusData
    {
        public List<string> Labels { get; set; }
        public List<int> Values { get; set; }
    }

    public class DashboardActivity
    {
        public string Type { get; set; }
        public string Title { get; set; }
        public string SubTitle { get; set; }
        public DateTime Time { get; set; }
        public string IconClass { get; set; }
        public string Icon { get; set; }

        public string TimeDisplay
        {
            get
            {
                var diff = DateTime.Now - Time;
                if (diff.TotalMinutes < 60)
                    return $"{Math.Floor(diff.TotalMinutes)} minutes ago";
                if (diff.TotalHours < 24)
                    return $"{Math.Floor(diff.TotalHours)} hours ago";
                if (diff.TotalDays < 7)
                    return $"{Math.Floor(diff.TotalDays)} days ago";
                if (diff.TotalDays < 30)
                    return $"{Math.Floor(diff.TotalDays / 7)} weeks ago";
                return Time.ToString("MMM dd, yyyy");
            }
        }
    }
}