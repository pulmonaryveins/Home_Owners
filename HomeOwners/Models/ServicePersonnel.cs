// HomeOwners/Models/ServicePersonnel.cs
using System;
using System.ComponentModel.DataAnnotations;

namespace HomeOwners.Models
{
    public class ServicePersonnel
    {
        public int Id { get; set; }

        [Required]
        [StringLength(100, ErrorMessage = "Team name cannot exceed 100 characters.")]
        [Display(Name = "Team Name")]
        public string TeamName { get; set; }

        [Required]
        [Range(1, 100, ErrorMessage = "Number of personnel must be between 1 and 100.")]
        [Display(Name = "Number of Personnel")]
        public int NumberOfPersonnel { get; set; }

        [Required]
        [Display(Name = "Service Specialization")]
        public int ServiceId { get; set; }

        // Navigation property for service
        public Service Service { get; set; }

        [Display(Name = "Created Date")]
        public DateTime CreatedDate { get; set; } = DateTime.Now;

        [Display(Name = "Is Active")]
        public bool IsActive { get; set; } = true;
    }
}