// Models/Event.cs
using System;
using System.ComponentModel.DataAnnotations;

namespace HomeOwners.Models
{
    public class Event
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Title is required")]
        [StringLength(100, ErrorMessage = "Title cannot be longer than 100 characters")]
        public string Title { get; set; }

        [Required(ErrorMessage = "Description is required")]
        public string Description { get; set; }

        [Required(ErrorMessage = "Start date and time is required")]
        [Display(Name = "Start Date & Time")]
        [DataType(DataType.DateTime)]
        public DateTime StartTime { get; set; }

        [Required(ErrorMessage = "End date and time is required")]
        [Display(Name = "End Date & Time")]
        [DataType(DataType.DateTime)]
        public DateTime EndTime { get; set; }

        [Display(Name = "Location")]
        public string Location { get; set; }

        [Display(Name = "Is All Day Event")]
        public bool IsAllDay { get; set; }

        [Display(Name = "Category")]
        public string Category { get; set; } // Community, Sports, Meeting, Social, etc.

        [Display(Name = "Color")]
        public string Color { get; set; } = "#4e6e4d"; // Default to your theme color

        [Display(Name = "Is Active")]
        public bool IsActive { get; set; } = true;
    }
}