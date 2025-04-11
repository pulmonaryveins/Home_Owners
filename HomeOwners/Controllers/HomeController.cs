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

    [Authorize] // Ensure user is logged 
    [Authorize] // Ensure user is logged 
    public async Task<IActionResult> BookFacility(int id)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        // Check if user already has an active booking
        if (await _bookingService.HasActiveBookingsAsync(userId))
        {
            TempData["StatusMessage"] = "You already have an active facility booking. You can only have one active booking at a time.";
            TempData["StatusType"] = "Error";
            return RedirectToAction("MyBookings");
        }

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
            UserId = userId
        };

        // Set the facility image url in ViewBag
        ViewBag.FacilityImage = facility.ImageUrl;

        return View(model);
    }

    [HttpPost]
    [Authorize]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> BookFacility(BookingViewModel model)
    {
        if (ModelState.IsValid)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            // Check if user already has an active booking
            if (await _bookingService.HasActiveBookingsAsync(userId))
            {
                TempData["StatusMessage"] = "You can only have one active facility booking at a time. Please wait until your current booking is completed or contact an administrator.";
                TempData["StatusType"] = "Error";

                // Use a different variable name to avoid the scope conflict
                var currentFacility = await _facilityService.GetFacilityByIdAsync(model.FacilityId);
                model.FacilityName = currentFacility?.Name;
                model.PricePerHour = currentFacility?.PricePerHour ?? 0;
                ViewBag.FacilityImage = currentFacility?.ImageUrl;

                return View(model);
            }

            // Create a new booking entity
            var booking = new Booking
            {
                FacilityId = model.FacilityId,
                UserId = userId,
                FullName = model.FullName,
                ContactNumber = model.ContactNumber,
                HouseNumber = model.HouseNumber,
                BookingDate = model.BookingDate,
                StartTime = model.StartTime,
                EndTime = model.EndTime,
                TotalHours = model.TotalHours,
                TotalPrice = model.TotalPrice,
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
        // Use a different variable name here too to be consistent
        var selectedFacility = await _facilityService.GetFacilityByIdAsync(model.FacilityId);
        model.FacilityName = selectedFacility?.Name;
        model.PricePerHour = selectedFacility?.PricePerHour ?? 0;
        ViewBag.FacilityImage = selectedFacility?.ImageUrl;

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
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var hasActiveRequest = await _serviceRequestService.HasActiveServiceRequestsAsync(userId);

        ViewBag.HasActiveServiceRequest = hasActiveRequest;
        var services = await _serviceService.GetActiveServicesAsync();
        return View(services);
    }

    // Add this method to handle service requests
    [Authorize]
    public async Task<IActionResult> RequestService(int id)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        // Check if user already has an active service request
        if (await _serviceRequestService.HasActiveServiceRequestsAsync(userId))
        {
            TempData["StatusMessage"] = "You already have an active service request. You can only have one active request at a time.";
            TempData["StatusType"] = "Error";
            return RedirectToAction("MyServiceRequests");
        }

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
            UserId = userId
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
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            // Check if user already has an active service request
            if (await _serviceRequestService.HasActiveServiceRequestsAsync(userId))
            {
                TempData["StatusMessage"] = "You can only have one active service request at a time. Please wait until your current request is completed or contact an administrator.";
                TempData["StatusType"] = "Error";

                var currentService = await _serviceService.GetServiceByIdAsync(model.ServiceId);
                model.ServiceName = currentService?.Name;
                model.ServiceImageUrl = currentService?.ImageUrl;

                return View(model);
            }

            // Validate service hours
            var service = await _serviceService.GetServiceByIdAsync(model.ServiceId);
            if (service != null && service.AvailableFrom.HasValue && service.AvailableTo.HasValue)
            {
                // Check if preferred time is within service provider's available hours
                if (model.PreferredTime < service.AvailableFrom.Value ||
                    model.PreferredTime > service.AvailableTo.Value)
                {
                    ModelState.AddModelError("PreferredTime",
                        $"Service is only available from {DateTime.Today.Add(service.AvailableFrom.Value).ToString("hh:mm tt")} to {DateTime.Today.Add(service.AvailableTo.Value).ToString("hh:mm tt")}");

                    model.ServiceName = service.Name;
                    model.ServiceImageUrl = service.ImageUrl;

                    return View(model);
                }
            }

            // Convert the view model to a service request entity
            var serviceRequest = new ServiceRequest
            {
                ServiceId = model.ServiceId,
                UserId = userId,
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
        var selectedService = await _serviceService.GetServiceByIdAsync(model.ServiceId);
        model.ServiceName = selectedService?.Name;
        model.ServiceImageUrl = selectedService?.ImageUrl;

        return View(model);
    }

    [Authorize]
    public async Task<IActionResult> MyServiceRequests()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var serviceRequests = await _serviceRequestService.GetServiceRequestsByUserIdAsync(userId);

        // Filter out "DONE" service requests for the "All Service Requests" tab
        var allServiceRequests = (await _serviceRequestService.GetAllServiceRequestsAsync())
            .Where(r => r.Status != ServiceRequestStatus.Done)
            .ToList();

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
