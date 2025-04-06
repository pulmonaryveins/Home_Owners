using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using HomeOwners.Models;
using HomeOwners.Services;

namespace HomeOwners.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly AnnouncementService _announcementService;
    private readonly EventService _eventService;
    private readonly FacilityService _facilityService;

    public HomeController(ILogger<HomeController> logger, AnnouncementService announcementService, EventService eventService, FacilityService facilityService)
    {
        _logger = logger;
        _announcementService = announcementService;
        _eventService = eventService;
        _facilityService = facilityService;
    }

    public IActionResult Index()
    {
        return View();
    }

    public IActionResult Profile()
    {
        // You can add user-specific data to ViewData here if needed
        ViewData["Email"] = User.Identity.IsAuthenticated ? User.Claims.FirstOrDefault(c => c.Type == "email")?.Value : "";
        ViewData["FullName"] = User.Identity.IsAuthenticated ? User.Claims.FirstOrDefault(c => c.Type == "name")?.Value : "";
        
        return View();
    }

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

    public async Task<IActionResult> Facilities()
    {
        var facilities = await _facilityService.GetActiveFacilitiesAsync();
        return View(facilities);
    }

    public IActionResult Services()
    {
        return View();
    }

    public IActionResult Contact()
    {
        return View();
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
