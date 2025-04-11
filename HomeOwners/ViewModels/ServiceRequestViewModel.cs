// HomeOwners/ViewModels/ServiceRequestViewModel.cs
using System;
using System.ComponentModel.DataAnnotations;
using HomeOwners.Models;

namespace HomeOwners.ViewModels
{
    public class ServiceRequestViewModel
    {
        public int ServiceId { get; set; }

        public string ServiceName { get; set; }

        public string ServiceImageUrl { get; set; }

        public TimeSpan? AvailableFrom { get; set; }

        public TimeSpan? AvailableTo { get; set; }

        public string UserId { get; set; }

        [Required(ErrorMessage = "Full name is required")]
        [Display(Name = "Full Name")]
        public string FullName { get; set; }

        [Required(ErrorMessage = "Contact number is required")]
        [Display(Name = "Contact Number")]
        public string ContactNumber { get; set; }

        [Required(ErrorMessage = "House number is required")]
        [Display(Name = "House Number")]
        public string HouseNumber { get; set; }

        [Required(ErrorMessage = "Service date is required")]
        [Display(Name = "Service Date")]
        [DataType(DataType.Date)]
        public DateTime RequestDate { get; set; }

        [Required(ErrorMessage = "Time is required")]
        [Display(Name = "Preferred Time")]
        [DataType(DataType.Time)]
        public TimeSpan PreferredTime { get; set; }

        [Display(Name = "Additional Details")]
        public string AdditionalDetails { get; set; }
    }
}