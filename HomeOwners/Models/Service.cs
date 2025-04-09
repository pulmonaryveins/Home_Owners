// HomeOwners/Models/Service.cs
using System;
using System.ComponentModel.DataAnnotations;

namespace HomeOwners.Models
{
    public class Service
    {
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; }

        [Required]
        [StringLength(500)]
        public string Description { get; set; }

        [StringLength(255)]
        [Display(Name = "Image URL")]
        public string ImageUrl { get; set; } = "/images/placeholder.jpg";

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