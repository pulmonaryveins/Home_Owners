using System;
using System.ComponentModel.DataAnnotations;

namespace HomeOwners.Models
{
    public class Poll
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Title is required")]
        [StringLength(100, ErrorMessage = "Title cannot be longer than 100 characters")]
        public string Title { get; set; }

        [Required(ErrorMessage = "Question is required")]
        [StringLength(200, ErrorMessage = "Question cannot be longer than 200 characters")]
        public string Question { get; set; }

        [Required(ErrorMessage = "Start date is required")]
        [Display(Name = "Start Date")]
        [DataType(DataType.Date)]
        public DateTime StartDate { get; set; }

        [Required(ErrorMessage = "End date is required")]
        [Display(Name = "End Date")]
        [DataType(DataType.Date)]
        public DateTime EndDate { get; set; }

        [Display(Name = "Created Date")]
        public DateTime CreatedDate { get; set; } = DateTime.Now;

        [Display(Name = "Is Active")]
        public bool IsActive { get; set; } = true;

        // Statistics for the poll
        public int YesVotes { get; set; } = 0;
        public int NoVotes { get; set; } = 0;

        // Calculate if the poll is currently active based on date range
        [Display(Name = "Currently Active")]
        public bool IsCurrentlyActive => IsActive && DateTime.Now >= StartDate && DateTime.Now <= EndDate;

        // Calculate total votes
        public int TotalVotes => YesVotes + NoVotes;

        // Calculate percentage for yes votes
        public int YesPercentage => TotalVotes > 0 ? (int)Math.Round((double)YesVotes / TotalVotes * 100) : 0;

        // Calculate percentage for no votes
        public int NoPercentage => TotalVotes > 0 ? (int)Math.Round((double)NoVotes / TotalVotes * 100) : 0;

        // Calculate days remaining
        public int DaysRemaining => EndDate > DateTime.Now ? (EndDate - DateTime.Now).Days : 0;
    }
}