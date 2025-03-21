using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using HomeOwners.Models;

namespace HomeOwners.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;

    public HomeController(ILogger<HomeController> logger)
    {
        _logger = logger;
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

    public IActionResult Annoucements()
    {
        return View();
    }

    public IActionResult Facilities()
    {
        return View();
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
