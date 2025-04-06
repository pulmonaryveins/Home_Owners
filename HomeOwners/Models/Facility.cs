// HomeOwners/Models/Facility.cs
using System;
using System.ComponentModel.DataAnnotations;

namespace HomeOwners.Models
{
    public class Facility
    {
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; }

        [Required]
        [StringLength(500)]
        public string Description { get; set; }

        [Required]
        [Range(0.01, 100000)]
        [Display(Name = "Price per hour")]
        public decimal PricePerHour { get; set; }

        [StringLength(255)]
        [Display(Name = "Image URL")]
        public string ImageUrl { get; set; } = "/images/placeholder.jpg"; // Added default value

        [Display(Name = "Capacity")]
        public int? Capacity { get; set; }

        [Display(Name = "Available From")]
        [DataType(DataType.Time)]
        public TimeSpan? AvailableFrom { get; set; }

        [Display(Name = "Available To")]
        [DataType(DataType.Time)]
        public TimeSpan? AvailableTo { get; set; }

        public bool IsActive { get; set; } = true;

        [Display(Name = "Created Date")]
        public DateTime CreatedDate { get; set; } = DateTime.Now;

        [Display(Name = "Last Updated")]
        public DateTime? LastUpdated { get; set; }
    }
}