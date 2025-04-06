// HomeOwners/Areas/Admin/Pages/Facilities.cshtml.cs
using System.Collections.Generic;
using System.Threading.Tasks;
using HomeOwners.Models;
using HomeOwners.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace HomeOwners.Areas.Admin.Pages
{
    [Authorize(Policy = "RequireAdminRole")]
    public class FacilitiesModel : PageModel
    {
        private readonly FacilityService _facilityService;

        public FacilitiesModel(FacilityService facilityService)
        {
            _facilityService = facilityService;
        }

        public List<Facility> Facilities { get; set; }

        public async Task OnGetAsync()
        {
            Facilities = await _facilityService.GetAllFacilitiesAsync();
        }
    }
}