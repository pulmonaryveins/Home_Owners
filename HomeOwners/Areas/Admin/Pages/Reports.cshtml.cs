using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using HomeOwners.Areas.Identity.Data;
using HomeOwners.Models;
using HomeOwners.Models.Users;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using System.Text.Json;
using Microsoft.AspNetCore.Identity;

namespace HomeOwners.Areas.Admin.Pages
{
    [Authorize(Roles = "Admin")]
    public class ReportsModel : PageModel
    {
        private readonly HomeDbContext _context;

        public ReportsModel(HomeDbContext context)
        {
            _context = context;
        }

        [BindProperty(SupportsGet = true)]
        public string ReportType { get; set; } = "Overview";

        [BindProperty(SupportsGet = true)]
        public string DateRange { get; set; } = "30days";

        [BindProperty(SupportsGet = true)]
        public DateTime? StartDate { get; set; }

        [BindProperty(SupportsGet = true)]
        public DateTime? EndDate { get; set; }

        public string ReportTitle { get; set; }
        public string ReportDescription { get; set; }
        public string ReportPeriod { get; set; }
        public List<MetricViewModel> KeyMetrics { get; set; }
        public string PrimaryChartTitle { get; set; }
        public string SecondaryChartTitle { get; set; }
        public string DataTableTitle { get; set; }
        public List<string> TableHeaders { get; set; }
        public List<List<string>> TableData { get; set; }
        public string AdditionalChart1Title { get; set; }
        public string AdditionalChart2Title { get; set; }
        public List<StatisticViewModel> Statistics { get; set; }
        public List<InsightViewModel> Insights { get; set; }

        // Chart data properties
        public dynamic PrimaryChartData { get; set; }
        public dynamic SecondaryChartData { get; set; }
        public dynamic AdditionalChart1Data { get; set; }
        public dynamic AdditionalChart2Data { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            // Set date range based on selection
            SetDateRange();

            // Generate the appropriate report based on type
            switch (ReportType)
            {
                case "Facilities":
                    await GenerateFacilitiesReportAsync();
                    break;
                case "Services":
                    await GenerateServicesReportAsync();
                    break;
                case "Users":
                    await GenerateUsersReportAsync();
                    break;
                case "Financial":
                    await GenerateFinancialReportAsync();
                    break;
                default:
                    await GenerateOverviewReportAsync();
                    break;
            }

            return Page();
        }

        private void SetDateRange()
        {
            DateTime now = DateTime.Now;

            switch (DateRange)
            {
                case "7days":
                    StartDate = now.AddDays(-7);
                    EndDate = now;
                    ReportPeriod = $"{StartDate:MMM dd, yyyy} - {EndDate:MMM dd, yyyy}";
                    break;
                case "30days":
                    StartDate = now.AddDays(-30);
                    EndDate = now;
                    ReportPeriod = $"{StartDate:MMM dd, yyyy} - {EndDate:MMM dd, yyyy}";
                    break;
                case "90days":
                    StartDate = now.AddDays(-90);
                    EndDate = now;
                    ReportPeriod = $"{StartDate:MMM dd, yyyy} - {EndDate:MMM dd, yyyy}";
                    break;
                case "year":
                    StartDate = now.AddYears(-1);
                    EndDate = now;
                    ReportPeriod = $"{StartDate:MMM dd, yyyy} - {EndDate:MMM dd, yyyy}";
                    break;
                case "custom":
                    if (StartDate.HasValue && EndDate.HasValue)
                    {
                        ReportPeriod = $"{StartDate:MMM dd, yyyy} - {EndDate:MMM dd, yyyy}";
                    }
                    else
                    {
                        StartDate = now.AddDays(-30);
                        EndDate = now;
                        ReportPeriod = $"{StartDate:MMM dd, yyyy} - {EndDate:MMM dd, yyyy}";
                    }
                    break;
            }
        }

        private async Task GenerateOverviewReportAsync()
        {
            ReportTitle = "System Overview Report";
            ReportDescription = "Comprehensive overview of system performance, user activity, and key metrics";

            try
            {
                // Count entities
                int userCount = await _context.Users.CountAsync();
                int bookingsCount = await _context.Bookings
                    .Where(b => b.BookingDate >= StartDate && b.BookingDate <= EndDate)
                    .CountAsync();
                int serviceRequestsCount = await _context.ServiceRequests
                    .Where(s => s.RequestDate >= StartDate && s.RequestDate <= EndDate)
                    .CountAsync();
                int facilityCount = await _context.Facilities.CountAsync();

                // Calculate revenue
                decimal totalRevenue = await _context.Payments
                    .Where(p => p.PaymentDate >= StartDate && p.PaymentDate <= EndDate)
                    .SumAsync(p => p.AmountPaid);

                // Previous period metrics for comparison
                DateTime previousStart = StartDate.Value.AddDays(-(EndDate.Value - StartDate.Value).Days);
                DateTime previousEnd = StartDate.Value.AddDays(-1);

                int previousBookings = await _context.Bookings
                    .Where(b => b.BookingDate >= previousStart && b.BookingDate <= previousEnd)
                    .CountAsync();

                decimal previousRevenue = await _context.Payments
                    .Where(p => p.PaymentDate >= previousStart && p.PaymentDate <= previousEnd)
                    .SumAsync(p => p.AmountPaid);

                // Calculate percent changes
                string bookingsChange = bookingsCount > 0 && previousBookings > 0
                    ? $"{Math.Round(((double)bookingsCount / previousBookings - 1) * 100)}% from previous period"
                    : "";

                string revenueChange = totalRevenue > 0 && previousRevenue > 0
                    ? $"{Math.Round(((double)totalRevenue / (double)previousRevenue - 1) * 100)}% from previous period"
                    : "";

                // Set key metrics
                KeyMetrics = new List<MetricViewModel>
                {
                    new MetricViewModel
                    {
                        Name = "Total Users",
                        Value = userCount.ToString(),
                        Icon = "bi-people-fill",
                        IconClass = "bg-users"
                    },
                    new MetricViewModel
                    {
                        Name = "Bookings",
                        Value = bookingsCount.ToString(),
                        Icon = "bi-calendar-check-fill",
                        IconClass = "bg-bookings",
                        Change = bookingsChange,
                        IsPositive = bookingsCount >= previousBookings
                    },
                    new MetricViewModel
                    {
                        Name = "Service Requests",
                        Value = serviceRequestsCount.ToString(),
                        Icon = "bi-tools",
                        IconClass = "bg-services"
                    },
                    new MetricViewModel
                    {
                        Name = "Total Revenue",
                        Value = $"₱{totalRevenue:N2}",
                        Icon = "bi-cash-stack",
                        IconClass = "bg-revenue",
                        Change = revenueChange,
                        IsPositive = totalRevenue >= previousRevenue
                    }
                };

                // Prepare monthly data for primary chart (user registrations and bookings)
                var monthlyData = new Dictionary<string, (int Users, int Bookings)>();

                // Get last 6 months
                for (int i = 5; i >= 0; i--)
                {
                    var month = DateTime.Now.AddMonths(-i);
                    var monthName = month.ToString("MMM yyyy");
                    monthlyData[monthName] = (0, 0);
                }

                // Get users by registration date - Fixed: Use CreatedDate instead of CreationTime
                var usersByMonth = await _context.HomeOwnerUsers
                    .Where(u => u.CreatedDate >= DateTime.Now.AddMonths(-6))
                    .GroupBy(u => new { Month = u.CreatedDate.Month, Year = u.CreatedDate.Year })
                    .Select(g => new {
                        Month = g.Key.Month,
                        Year = g.Key.Year,
                        Count = g.Count()
                    })
                    .ToListAsync();

                // Get bookings by month
                var bookingsByMonth = await _context.Bookings
                    .Where(b => b.BookingDate >= DateTime.Now.AddMonths(-6))
                    .GroupBy(b => new { Month = b.BookingDate.Month, Year = b.BookingDate.Year })
                    .Select(g => new {
                        Month = g.Key.Month,
                        Year = g.Key.Year,
                        Count = g.Count()
                    })
                    .ToListAsync();

                // Populate monthly data
                foreach (var user in usersByMonth)
                {
                    var month = new DateTime(user.Year, user.Month, 1);
                    var monthName = month.ToString("MMM yyyy");
                    if (monthlyData.ContainsKey(monthName))
                    {
                        monthlyData[monthName] = (user.Count, monthlyData[monthName].Bookings);
                    }
                }

                foreach (var booking in bookingsByMonth)
                {
                    var month = new DateTime(booking.Year, booking.Month, 1);
                    var monthName = month.ToString("MMM yyyy");
                    if (monthlyData.ContainsKey(monthName))
                    {
                        monthlyData[monthName] = (monthlyData[monthName].Users, booking.Count);
                    }
                }

                // Setup primary chart data
                PrimaryChartTitle = "User Registrations & Bookings (Last 6 Months)";
                PrimaryChartData = new
                {
                    type = "bar",
                    data = new
                    {
                        labels = monthlyData.Keys.ToArray(),
                        datasets = new[]
                        {
                            new
                            {
                                label = "New Users",
                                data = monthlyData.Values.Select(v => v.Users).ToArray(),
                                backgroundColor = "rgba(66, 153, 225, 0.7)",
                                borderColor = "rgba(66, 153, 225, 1)",
                                borderWidth = 1
                            },
                            new
                            {
                                label = "Bookings",
                                data = monthlyData.Values.Select(v => v.Bookings).ToArray(),
                                backgroundColor = "rgba(237, 137, 54, 0.7)",
                                borderColor = "rgba(237, 137, 54, 1)",
                                borderWidth = 1
                            }
                        }
                    },
                    options = new
                    {
                        responsive = true,
                        scales = new
                        {
                            y = new
                            {
                                beginAtZero = true,
                                title = new
                                {
                                    display = true,
                                    text = "Count"
                                }
                            },
                            x = new
                            {
                                title = new
                                {
                                    display = true,
                                    text = "Month"
                                }
                            }
                        }
                    }
                };

                // Secondary chart: Service Distribution
                SecondaryChartTitle = "Service Request Distribution";

                // Get service request counts by service type
                var serviceRequests = await _context.ServiceRequests
                    .Where(sr => sr.RequestDate >= StartDate && sr.RequestDate <= EndDate)
                    .Include(sr => sr.Service)
                    .GroupBy(sr => sr.Service.Name)
                    .Select(g => new { ServiceName = g.Key, Count = g.Count() })
                    .ToListAsync();

                SecondaryChartData = new
                {
                    type = "pie",
                    data = new
                    {
                        labels = serviceRequests.Select(sr => sr.ServiceName).ToArray(),
                        datasets = new[]
                        {
                            new
                            {
                                data = serviceRequests.Select(sr => sr.Count).ToArray(),
                                backgroundColor = new[]
                                {
                                    "rgba(72, 187, 120, 0.7)",
                                    "rgba(66, 153, 225, 0.7)",
                                    "rgba(237, 137, 54, 0.7)",
                                    "rgba(128, 90, 213, 0.7)",
                                    "rgba(226, 74, 117, 0.7)",
                                    "rgba(109, 120, 133, 0.7)"
                                }
                            }
                        }
                    },
                    options = new
                    {
                        responsive = true
                    }
                };

                // Table data: Recent bookings
                DataTableTitle = "Recent Facility Bookings";
                TableHeaders = new List<string> { "Facility", "Homeowner", "Booking Date", "Status", "Amount" };

                var recentBookings = await _context.Bookings
                    .Include(b => b.Facility)
                    .OrderByDescending(b => b.BookingDate)
                    .Take(10)
                    .Select(b => new List<string>
                    {
                        b.Facility.Name,
                        b.FullName,
                        b.BookingDate.ToString("MMM dd, yyyy"),
                        b.Status.ToString(),
                        $"₱{b.TotalPrice:N2}"
                    })
                    .ToListAsync();

                TableData = recentBookings;

                // Additional Charts
                AdditionalChart1Title = "Booking Status Distribution";
                var bookingStatusCounts = await _context.Bookings
                    .Where(b => b.BookingDate >= StartDate && b.BookingDate <= EndDate)
                    .GroupBy(b => b.Status)
                    .Select(g => new { Status = g.Key.ToString(), Count = g.Count() })
                    .ToListAsync();

                AdditionalChart1Data = new
                {
                    type = "doughnut",
                    data = new
                    {
                        labels = bookingStatusCounts.Select(b => b.Status).ToArray(),
                        datasets = new[]
                        {
                            new
                            {
                                data = bookingStatusCounts.Select(b => b.Count).ToArray(),
                                backgroundColor = new[]
                                {
                                    "rgba(72, 187, 120, 0.7)",
                                    "rgba(237, 137, 54, 0.7)",
                                    "rgba(226, 74, 117, 0.7)",
                                    "rgba(66, 153, 225, 0.7)"
                                }
                            }
                        }
                    },
                    options = new
                    {
                        responsive = true
                    }
                };

                AdditionalChart2Title = "Revenue by Month";
                var revenueByMonth = await _context.Payments
                    .Where(p => p.PaymentDate >= DateTime.Now.AddMonths(-6))
                    .GroupBy(p => new { Month = p.PaymentDate.Month, Year = p.PaymentDate.Year })
                    .Select(g => new {
                        Month = g.Key.Month,
                        Year = g.Key.Year,
                        TotalRevenue = g.Sum(p => p.AmountPaid)
                    })
                    .ToListAsync();

                var monthLabels = new List<string>();
                var monthlyRevenues = new List<decimal>();

                for (int i = 5; i >= 0; i--)
                {
                    var month = DateTime.Now.AddMonths(-i);
                    var monthName = month.ToString("MMM yyyy");
                    monthLabels.Add(monthName);

                    var revenue = revenueByMonth
                        .FirstOrDefault(r => r.Month == month.Month && r.Year == month.Year);

                    monthlyRevenues.Add(revenue?.TotalRevenue ?? 0);
                }

                AdditionalChart2Data = new
                {
                    type = "line",
                    data = new
                    {
                        labels = monthLabels,
                        datasets = new[]
                        {
                            new
                            {
                                label = "Revenue",
                                data = monthlyRevenues,
                                borderColor = "rgba(128, 90, 213, 1)",
                                backgroundColor = "rgba(128, 90, 213, 0.1)",
                                tension = 0.4,
                                fill = true
                            }
                        }
                    },
                    options = new
                    {
                        responsive = true,
                        scales = new
                        {
                            y = new
                            {
                                beginAtZero = true,
                                title = new
                                {
                                    display = true,
                                    text = "Revenue (₱)"
                                }
                            }
                        }
                    }
                };

                // Statistical Summaries
                Statistics = new List<StatisticViewModel>();

                // Average bookings per day
                double daysInRange = (EndDate.Value - StartDate.Value).TotalDays;
                double avgBookingsPerDay = daysInRange > 0 ? bookingsCount / daysInRange : 0;
                Statistics.Add(new StatisticViewModel
                {
                    Label = "Average Bookings Per Day",
                    Value = avgBookingsPerDay.ToString("F1")
                });

                // Average revenue per booking
                decimal avgRevenuePerBooking = bookingsCount > 0 ? totalRevenue / bookingsCount : 0;
                Statistics.Add(new StatisticViewModel
                {
                    Label = "Average Revenue Per Booking",
                    Value = $"₱{avgRevenuePerBooking:N2}"
                });

                // Most popular facility
                var mostPopularFacility = await _context.Bookings
                    .Where(b => b.BookingDate >= StartDate && b.BookingDate <= EndDate)
                    .Include(b => b.Facility)
                    .GroupBy(b => b.Facility.Name)
                    .Select(g => new { FacilityName = g.Key, Count = g.Count() })
                    .OrderByDescending(x => x.Count)
                    .FirstOrDefaultAsync();

                if (mostPopularFacility != null)
                {
                    Statistics.Add(new StatisticViewModel
                    {
                        Label = "Most Popular Facility",
                        Value = $"{mostPopularFacility.FacilityName} ({mostPopularFacility.Count} bookings)"
                    });
                }

                // Most requested service
                var mostRequestedService = await _context.ServiceRequests
                    .Where(sr => sr.RequestDate >= StartDate && sr.RequestDate <= EndDate)
                    .Include(sr => sr.Service)
                    .GroupBy(sr => sr.Service.Name)
                    .Select(g => new { ServiceName = g.Key, Count = g.Count() })
                    .OrderByDescending(x => x.Count)
                    .FirstOrDefaultAsync();

                if (mostRequestedService != null)
                {
                    Statistics.Add(new StatisticViewModel
                    {
                        Label = "Most Requested Service",
                        Value = $"{mostRequestedService.ServiceName} ({mostRequestedService.Count} requests)"
                    });
                }

                // Active home owners
                int activeHomeOwners = await _context.HomeOwnerUsers.CountAsync(h => h.AccountStatus == "Active");
                Statistics.Add(new StatisticViewModel
                {
                    Label = "Active Home Owners",
                    Value = activeHomeOwners.ToString()
                });

                // Active staff members - Fixed: StaffUser doesn't have IsActive property
                // Count all staff members instead
                int staffCount = await _context.StaffUsers.CountAsync();
                Statistics.Add(new StatisticViewModel
                {
                    Label = "Staff Members",
                    Value = staffCount.ToString()
                });

                // Generate Insights
                Insights = new List<InsightViewModel>();

                // Booking trend insight
                bool bookingsIncreasing = bookingsCount > previousBookings;
                Insights.Add(new InsightViewModel
                {
                    Title = "Booking Trend Analysis",
                    Icon = bookingsIncreasing ? "bi-graph-up-arrow" : "bi-graph-down-arrow",
                    Text = bookingsIncreasing
                        ? "Facility bookings are showing an upward trend compared to the previous period, indicating increasing community engagement."
                        : "Facility bookings have decreased compared to the previous period. Consider promotional activities to increase usage."
                });

                // Revenue insight
                bool revenueIncreasing = totalRevenue > previousRevenue;
                Insights.Add(new InsightViewModel
                {
                    Title = "Revenue Performance",
                    Icon = revenueIncreasing ? "bi-currency-dollar" : "bi-exclamation-triangle",
                    Text = revenueIncreasing
                        ? "Revenue has increased compared to the previous period, showing healthy financial growth."
                        : "Revenue has decreased compared to the previous period. Consider reviewing pricing strategies."
                });

                // Facility utilization insight
                if (mostPopularFacility != null)
                {
                    Insights.Add(new InsightViewModel
                    {
                        Title = "Facility Utilization",
                        Icon = "bi-building",
                        Text = $"The {mostPopularFacility.FacilityName} is your most booked facility. Consider similar amenities in future development plans."
                    });
                }

                // Service popularity insight
                if (mostRequestedService != null)
                {
                    Insights.Add(new InsightViewModel
                    {
                        Title = "Service Demand",
                        Icon = "bi-tools",
                        Text = $"The {mostRequestedService.ServiceName} service is in high demand. Consider expanding capacity for this service."
                    });
                }
            }
            catch (Exception ex)
            {
                // Log the error and set default data
                Console.Error.WriteLine($"Error generating overview report: {ex.Message}");
                TempData["StatusMessage"] = "An error occurred while generating the report.";
                TempData["StatusType"] = "Error";

                // Set empty collections to prevent null reference exceptions
                KeyMetrics = new List<MetricViewModel>();
                TableHeaders = new List<string>();
                TableData = new List<List<string>>();
                Statistics = new List<StatisticViewModel>();
                Insights = new List<InsightViewModel>();
            }
        }

        private async Task GenerateFacilitiesReportAsync()
        {
            ReportTitle = "Facilities Usage Report";
            ReportDescription = "Detailed analysis of facility bookings, usage patterns, and revenue generation";

            // Implementation similar to overview but focused on facilities
            // ...

            // Example placeholder implementation
            KeyMetrics = new List<MetricViewModel>();
            TableHeaders = new List<string>();
            TableData = new List<List<string>>();
            Statistics = new List<StatisticViewModel>();
            Insights = new List<InsightViewModel>();

            // Initialize chart data with empty placeholder values
            PrimaryChartData = new { type = "bar", data = new { labels = new string[0], datasets = new[] { new { label = "", data = new int[0] } } } };
            SecondaryChartData = new { type = "pie", data = new { labels = new string[0], datasets = new[] { new { data = new int[0] } } } };
            AdditionalChart1Data = new { type = "bar", data = new { labels = new string[0], datasets = new[] { new { label = "", data = new int[0] } } } };
            AdditionalChart2Data = new { type = "line", data = new { labels = new string[0], datasets = new[] { new { label = "", data = new int[0] } } } };
        }

        private async Task GenerateServicesReportAsync()
        {
            ReportTitle = "Services Performance Report";
            ReportDescription = "Analysis of service requests, response times, and customer satisfaction";

            // Implementation similar to overview but focused on services
            // ...

            // Example placeholder implementation
            KeyMetrics = new List<MetricViewModel>();
            TableHeaders = new List<string>();
            TableData = new List<List<string>>();
            Statistics = new List<StatisticViewModel>();
            Insights = new List<InsightViewModel>();

            // Initialize chart data with empty placeholder values
            PrimaryChartData = new { type = "bar", data = new { labels = new string[0], datasets = new[] { new { label = "", data = new int[0] } } } };
            SecondaryChartData = new { type = "pie", data = new { labels = new string[0], datasets = new[] { new { data = new int[0] } } } };
            AdditionalChart1Data = new { type = "bar", data = new { labels = new string[0], datasets = new[] { new { label = "", data = new int[0] } } } };
            AdditionalChart2Data = new { type = "line", data = new { labels = new string[0], datasets = new[] { new { label = "", data = new int[0] } } } };
        }

        private async Task GenerateUsersReportAsync()
        {
            ReportTitle = "User Activity Report";
            ReportDescription = "Analysis of user registrations, engagement patterns, and account status";

            // Implementation similar to overview but focused on users
            // ...

            // Example placeholder implementation
            KeyMetrics = new List<MetricViewModel>();
            TableHeaders = new List<string>();
            TableData = new List<List<string>>();
            Statistics = new List<StatisticViewModel>();
            Insights = new List<InsightViewModel>();

            // Initialize chart data with empty placeholder values
            PrimaryChartData = new { type = "bar", data = new { labels = new string[0], datasets = new[] { new { label = "", data = new int[0] } } } };
            SecondaryChartData = new { type = "pie", data = new { labels = new string[0], datasets = new[] { new { data = new int[0] } } } };
            AdditionalChart1Data = new { type = "bar", data = new { labels = new string[0], datasets = new[] { new { label = "", data = new int[0] } } } };
            AdditionalChart2Data = new { type = "line", data = new { labels = new string[0], datasets = new[] { new { label = "", data = new int[0] } } } };
        }

        private async Task GenerateFinancialReportAsync()
        {
            ReportTitle = "Financial Performance Report";
            ReportDescription = "Comprehensive overview of revenue streams, payment history, and financial trends";

            // Implementation similar to overview but focused on financial data
            // ...

            // Example placeholder implementation
            KeyMetrics = new List<MetricViewModel>();
            TableHeaders = new List<string>();
            TableData = new List<List<string>>();
            Statistics = new List<StatisticViewModel>();
            Insights = new List<InsightViewModel>();

            // Initialize chart data with empty placeholder values
            PrimaryChartData = new { type = "bar", data = new { labels = new string[0], datasets = new[] { new { label = "", data = new int[0] } } } };
            SecondaryChartData = new { type = "pie", data = new { labels = new string[0], datasets = new[] { new { data = new int[0] } } } };
            AdditionalChart1Data = new { type = "bar", data = new { labels = new string[0], datasets = new[] { new { label = "", data = new int[0] } } } };
            AdditionalChart2Data = new { type = "line", data = new { labels = new string[0], datasets = new[] { new { label = "", data = new int[0] } } } };
        }
    }

    // Helper view models
    public class MetricViewModel
    {
        public string Name { get; set; }
        public string Value { get; set; }
        public string Icon { get; set; }
        public string IconClass { get; set; }
        public string Change { get; set; } = "";
        public bool IsPositive { get; set; } = true;
    }

    public class StatisticViewModel
    {
        public string Label { get; set; }
        public string Value { get; set; }
    }

    public class InsightViewModel
    {
        public string Title { get; set; }
        public string Icon { get; set; }
        public string Text { get; set; }
    }
}