// HomeOwners/ViewModels/BookingViewModel.cs
using System;
using System.ComponentModel.DataAnnotations;
using HomeOwners.Models;

namespace HomeOwners.ViewModels
{

    public class BookingViewModel
    {
        public int FacilityId { get; set; }

        public string FacilityName { get; set; }

        public decimal PricePerHour { get; set; }

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

        [Required(ErrorMessage = "Booking date is required")]
        [Display(Name = "Booking Date")]
        [DataType(DataType.Date)]
        public DateTime BookingDate { get; set; }

        [Required(ErrorMessage = "Start time is required")]
        [Display(Name = "Start Time")]
        [DataType(DataType.Time)]
        public TimeSpan StartTime { get; set; }

        [Required(ErrorMessage = "End time is required")]
        [Display(Name = "End Time")]
        [DataType(DataType.Time)]
        public TimeSpan EndTime { get; set; }

        [Display(Name = "Total Hours")]
        public decimal TotalHours { get; set; }

        [Display(Name = "Total Price")]
        [DataType(DataType.Currency)]
        public decimal TotalPrice { get; set; }

        [Display(Name = "Special Requests")]
        public string SpecialRequests { get; set; }
    }

}