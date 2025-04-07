// Models/Announcement.cs
using System;
using System.ComponentModel.DataAnnotations;

namespace HomeOwners.Models
{
    public class Announcement
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Title is required")]
        [StringLength(100, ErrorMessage = "Title cannot be longer than 100 characters")]
        public string Title { get; set; }

        [Required(ErrorMessage = "Content is required")]
        public string Content { get; set; }

        [Display(Name = "Posted Date")]
        [DataType(DataType.DateTime)]
        public DateTime PostedDate { get; set; } = DateTime.Now;

        [Display(Name = "Event Date and Time")]
        [DataType(DataType.DateTime)]
        public DateTime? EventDateTime { get; set; } // Nullable as not all announcements have an event

        [Display(Name = "Expiry Date")]
        [DataType(DataType.Date)]
        public DateTime? ExpiryDate { get; set; } // Add this property

        [Display(Name = "Target Audience")]
        public string TargetAudience { get; set; } = "All"; // Add this property

        [Display(Name = "Is Urgent")]
        public bool IsUrgent { get; set; }

        [Display(Name = "Category")]
        public string Category { get; set; } // Event, Important, Update, etc.

        [Display(Name = "Is Active")]
        public bool IsActive { get; set; } = true;
    }
}