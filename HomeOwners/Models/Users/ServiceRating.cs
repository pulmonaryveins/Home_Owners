// HomeOwners/Models/ServiceRating.cs
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HomeOwners.Models
{
    public class ServiceRating
    {
        public int Id { get; set; }

        [Required]
        public int ServiceRequestId { get; set; }

        [ForeignKey("ServiceRequestId")]
        public ServiceRequest ServiceRequest { get; set; }

        [Required]
        public string UserId { get; set; }

        [Required]
        [Range(1, 5)]
        public int Rating { get; set; }

        [StringLength(500)]
        public string Feedback { get; set; }

        [Required]
        public DateTime SubmittedDate { get; set; } = DateTime.Now;
    }
}