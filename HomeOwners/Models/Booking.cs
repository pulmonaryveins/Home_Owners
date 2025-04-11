// HomeOwners/Models/Booking.cs
using System;
using System.ComponentModel.DataAnnotations;

namespace HomeOwners.Models
{
    public class Booking
    {
        public int Id { get; set; }

        [Required]
        public int FacilityId { get; set; }
        public Facility Facility { get; set; }
        public decimal TotalPrice { get; set; }

        [Display(Name = "Total Hours")]
        public decimal TotalHours { get; set; }

        public PaymentStatus PaymentStatus { get; set; } = PaymentStatus.Unpaid;
        public DateTime? PaidDate { get; set; }
        public string? TransactionId { get; set; }
        public string? ReceiptNumber { get; set; }

        [Required]
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

        [Display(Name = "Special Requests")]
        public string SpecialRequests { get; set; }

        [Required]
        public DateTime CreatedDate { get; set; } = DateTime.Now;

        [Required]
        [Display(Name = "Status")]
        public BookingStatus Status { get; set; } = BookingStatus.Pending;
    }

    public enum PaymentStatus
    {
        Unpaid,
        Paid
    }

    public enum BookingStatus
    {
        Pending,
        Approved,
        Rejected,
        Completed
    }
}