using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace HomeOwners.Areas.Staff.Pages
{
    [Authorize(Policy = "RequireStaffRole")]
    public class DashboardModel : PageModel
    {
        public void OnGet()
        {
            // Add any data needed for the staff dashboard
        }
    }
}
