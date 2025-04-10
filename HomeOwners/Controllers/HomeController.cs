using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using HomeOwners.Models;
using HomeOwners.Services;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using HomeOwners.ViewModels;
using System;
using HomeOwners.Filters;

namespace HomeOwners.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly AnnouncementService _announcementService;
    private readonly EventService _eventService;
    private readonly FacilityService _facilityService;
    private readonly BookingService _bookingService;
    private readonly ServiceService _serviceService;
    private readonly ServiceRequestService _serviceRequestService;
    private readonly PaymentService _paymentService;




    public HomeController(ILogger<HomeController> logger, AnnouncementService announcementService, EventService eventService, FacilityService facilityService, BookingService bookingService, ServiceService serviceService, ServiceRequestService serviceRequestService, PaymentService paymentService)
    {
        _logger = logger;
        _announcementService = announcementService;
        _eventService = eventService;
        _facilityService = facilityService;
        _bookingService = bookingService;
        _serviceService = serviceService;
        _serviceRequestService = serviceRequestService;
        _paymentService = paymentService;
    }

    public IActionResult Index()
    {
        return View();
    }

    [RequireAuthentication]
    public IActionResult Profile()
    {
        // You can add user-specific data to ViewData here if needed
        ViewData["Email"] = User.Identity.IsAuthenticated ? User.Claims.FirstOrDefault(c => c.Type == "email")?.Value : "";
        ViewData["FullName"] = User.Identity.IsAuthenticated ? User.Claims.FirstOrDefault(c => c.Type == "name")?.Value : "";
        
        return View();
    }

    [RequireAuthentication]
    public async Task<IActionResult> Annoucements()
    {
        var announcements = await _announcementService.GetActiveAnnouncementsAsync();
        var upcomingEvents = await _eventService.GetUpcomingEventsAsync(3);

        ViewBag.UpcomingEvents = upcomingEvents;

        return View(announcements);
    }

    public async Task<IActionResult> Calendar()
    {
        var events = await _eventService.GetActiveEventsAsync();
        return View(events);
    }

    [RequireAuthentication]
    public async Task<IActionResult> Facilities()
    {
        var facilities = await _facilityService.GetActiveFacilitiesAsync();
        return View(facilities);
    }

    [Authorize] // Ensure user is logged in
    public async Task<IActionResult> BookFacility(int id)
    {
        var facility = await _facilityService.GetFacilityByIdAsync(id);
        if (facility == null)
        {
            return NotFound();
        }

        var model = new BookingViewModel
        {
            FacilityId = facility.Id,
            FacilityName = facility.Name,
            PricePerHour = facility.PricePerHour,
            AvailableFrom = facility.AvailableFrom,
            AvailableTo = facility.AvailableTo,
            BookingDate = DateTime.Today,
            UserId = User.FindFirstValue(ClaimTypes.NameIdentifier)
        };

        return View(model);
    }

    [HttpPost]
    [Authorize]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> BookFacility(BookingViewModel model)
    {
        if (ModelState.IsValid)
        {
            // Create a new booking entity
            var booking = new Booking
            {
                FacilityId = model.FacilityId,
                UserId = User.FindFirstValue(ClaimTypes.NameIdentifier),
                FullName = model.FullName,
                ContactNumber = model.ContactNumber,
                HouseNumber = model.HouseNumber,
                BookingDate = model.BookingDate,
                StartTime = model.StartTime,
                EndTime = model.EndTime,
                TotalHours = model.TotalHours,  // Ensure this property is set
                TotalPrice = model.TotalPrice,  // Ensure this property is set
                SpecialRequests = model.SpecialRequests,
                CreatedDate = DateTime.Now,
                Status = BookingStatus.Pending
            };

            await _bookingService.CreateBookingAsync(booking);

            TempData["StatusMessage"] = "Your booking request has been submitted and is pending approval.";
            TempData["StatusType"] = "Success";

            return RedirectToAction("MyBookings");
        }

        // If we got this far, something failed, redisplay form
        var facility = await _facilityService.GetFacilityByIdAsync(model.FacilityId);
        model.FacilityName = facility?.Name;
        model.PricePerHour = facility?.PricePerHour ?? 0;
        ViewBag.FacilityImage = facility?.ImageUrl;

        return View(model);
    }

    [Authorize]
    public async Task<IActionResult> MyBookings()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var bookings = await _bookingService.GetBookingsByUserIdAsync(userId);
        var allBookings = await _bookingService.GetAllBookingsAsync();

        ViewBag.AllBookings = allBookings;
        return View(bookings);
    }

    [Authorize]
    public async Task<IActionResult> AllBookings()
    {
        var bookings = await _bookingService.GetAllBookingsAsync();
        return View(bookings);
    }

    [RequireAuthentication]
    public async Task<IActionResult> Services()
    {
        var services = await _serviceService.GetActiveServicesAsync(); // Fetch active services
        return View(services);
    }

    // Add this method to handle service requests
    [Authorize]
    public async Task<IActionResult> RequestService(int id)
    {
        var service = await _serviceService.GetServiceByIdAsync(id);
        if (service == null)
        {
            return NotFound();
        }

        var model = new ServiceRequestViewModel
        {
            ServiceId = service.Id,
            ServiceName = service.Name,
            ServiceImageUrl = service.ImageUrl,
            AvailableFrom = service.AvailableFrom,
            AvailableTo = service.AvailableTo,
            RequestDate = DateTime.Today,
            UserId = User.FindFirstValue(ClaimTypes.NameIdentifier)
        };

        return View(model);
    }

    [HttpPost]
    [Authorize]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RequestService(ServiceRequestViewModel model)
    {
        if (ModelState.IsValid)
        {
            // Convert the view model to a service request entity
            var serviceRequest = new ServiceRequest
            {
                ServiceId = model.ServiceId,
                UserId = User.FindFirstValue(ClaimTypes.NameIdentifier),
                FullName = model.FullName,
                ContactNumber = model.ContactNumber,
                HouseNumber = model.HouseNumber,
                RequestDate = model.RequestDate,
                PreferredTime = model.PreferredTime,
                AdditionalDetails = model.AdditionalDetails,
                CreatedDate = DateTime.Now,
                Status = ServiceRequestStatus.Pending
            };

            await _serviceRequestService.CreateServiceRequestAsync(serviceRequest);

            TempData["StatusMessage"] = "Your service request has been submitted and is pending approval.";
            TempData["StatusType"] = "Success";

            return RedirectToAction("MyServiceRequests");
        }

        // If we got this far, something failed, redisplay form
        var service = await _serviceService.GetServiceByIdAsync(model.ServiceId);
        model.ServiceName = service?.Name;
        model.ServiceImageUrl = service?.ImageUrl;

        return View(model);
    }

    [Authorize]
    public async Task<IActionResult> MyServiceRequests()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var serviceRequests = await _serviceRequestService.GetServiceRequestsByUserIdAsync(userId);
        var allServiceRequests = await _serviceRequestService.GetAllServiceRequestsAsync();

        ViewBag.AllServiceRequests = allServiceRequests;
        return View(serviceRequests);
    }

    [HttpPost]
    [Authorize]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> MarkServiceRequestAsDone(int id)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var serviceRequest = await _serviceRequestService.GetServiceRequestByIdAsync(id);

        if (serviceRequest == null || serviceRequest.UserId != userId)
        {
            return Unauthorized();
        }

        await _serviceRequestService.UpdateServiceRequestStatusAsync(id, ServiceRequestStatus.Done);

        TempData["StatusMessage"] = "Service request has been marked as done.";
        TempData["StatusType"] = "Success";

        return RedirectToAction("MyServiceRequests");
    }

    [HttpPost]
    [Authorize]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CancelServiceRequest(int id)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var serviceRequest = await _serviceRequestService.GetServiceRequestByIdAsync(id);

        if (serviceRequest == null || serviceRequest.UserId != userId)
        {
            return Unauthorized();
        }

        await _serviceRequestService.DeleteServiceRequestAsync(id);

        TempData["StatusMessage"] = "Service request has been cancelled.";
        TempData["StatusType"] = "Success";

        return RedirectToAction("MyServiceRequests");
    }

    [RequireAuthentication]
    public IActionResult Contact()
    {
        return View();
    }

    [Authorize]
    public async Task<IActionResult> Billing()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var unpaidBookings = await _paymentService.GetCompletedUnpaidBookingsAsync(userId);
        var userPayments = await _paymentService.GetPaymentsByUserIdAsync(userId);

        var model = new BillingViewModel
        {
            UnpaidBookings = unpaidBookings,
            Payments = userPayments,
            TotalUnpaidAmount = unpaidBookings.Sum(b => b.TotalPrice)
        };

        return View(model);
    }

    [HttpPost]
    [Authorize]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ProcessPayment(int bookingId, PaymentMethod paymentMethod, string notes)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var booking = await _bookingService.GetBookingByIdAsync(bookingId);

        if (booking == null || booking.UserId != userId)
        {
            return Unauthorized();
        }

        // Generate receipt number
        var receiptNumber = await _paymentService.GenerateReceiptNumberAsync();

        // Create payment record
        var payment = new Payment
        {
            BookingId = bookingId,
            UserId = userId,
            AmountPaid = booking.TotalPrice,
            PaymentDate = DateTime.Now,
            PaymentMethod = paymentMethod,
            TransactionId = $"TXN-{DateTime.Now.Ticks}",
            ReceiptNumber = receiptNumber,
            Notes = notes
        };

        await _paymentService.CreatePaymentAsync(payment);

        TempData["StatusMessage"] = "Payment processed successfully. A receipt has been generated.";
        TempData["StatusType"] = "Success";

        return RedirectToAction("PaymentReceipt", new { id = payment.Id });
    }


    [Authorize]
    public async Task<IActionResult> PaymentReceipt(int id)
    {
        var payment = await _paymentService.GetPaymentByIdAsync(id);
        if (payment == null)
        {
            return NotFound();
        }

        // Verify that the current user is authorized to view this receipt
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (payment.UserId != userId && !User.IsInRole("Admin"))
        {
            return Unauthorized();
        }

        return View(payment);
    }

    [Authorize]
    public async Task<IActionResult> PaymentHistory()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var payments = await _paymentService.GetPaymentsByUserIdAsync(userId);

        return View(payments);
    }


    public IActionResult Notifications()
    {
        return View();
    }

    [HttpPost]
    public IActionResult UpdateProfile(ProfileViewModel model)
    {
        if (ModelState.IsValid)
        {
            // Here you would update the user profile in your system
            // This is a placeholder for actual implementation
            TempData["StatusMessage"] = "Your profile has been updated successfully.";
            TempData["StatusType"] = "Success";
        }
        else
        {
            TempData["StatusMessage"] = "An error occurred while updating your profile.";
            TempData["StatusType"] = "Error";
        }
        
        return RedirectToAction("Profile");
    }

    [HttpPost]
    public IActionResult UpdatePassword(string CurrentPassword, string NewPassword, string ConfirmPassword)
    {
        if (!string.IsNullOrWhiteSpace(CurrentPassword) && !string.IsNullOrWhiteSpace(NewPassword) 
            && NewPassword == ConfirmPassword)
        {
            // Here you would update the user's password in your system
            // This is a placeholder for actual implementation
            TempData["StatusMessage"] = "Your password has been updated successfully.";
            TempData["StatusType"] = "Success";
        }
        else
        {
            TempData["StatusMessage"] = "An error occurred while updating your password.";
            TempData["StatusType"] = "Error";
        }
        
        return RedirectToAction("Profile");
    }

    [HttpPost]
    public IActionResult UpdatePreferences(bool EmailNotifications, bool SmsNotifications)
    {
        // Here you would update the user's preferences in your system
        // This is a placeholder for actual implementation
        TempData["StatusMessage"] = "Your preferences have been updated successfully.";
        TempData["StatusType"] = "Success";
        
        return RedirectToAction("Profile");
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
