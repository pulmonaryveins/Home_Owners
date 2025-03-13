using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace HomeOwners.Areas.Admin.Pages
{
    [Authorize(Policy = "RequireAdminRole")]
    public class DashboardModel : PageModel
    {
        // ...existing code...
        public void OnGet()
        {
            // Add any data needed for the dashboard
        }
    }
}