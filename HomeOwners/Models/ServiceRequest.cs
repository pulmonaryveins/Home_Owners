// HomeOwners/Models/ServiceRequest.cs
using System;
using System.ComponentModel.DataAnnotations;

namespace HomeOwners.Models
{

    public enum ServiceRequestStatus
    {
        Pending,
        Approved,
        Rejected,
        Done
    }
    public class ServiceRequest
    {
        public int Id { get; set; }

        [Required]
        public int ServiceId { get; set; }
        public Service Service { get; set; }

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

        [Required]
        public DateTime CreatedDate { get; set; } = DateTime.Now;

        [Required]
        [Display(Name = "Status")]
        public ServiceRequestStatus Status { get; set; } = ServiceRequestStatus.Pending;
    }

}