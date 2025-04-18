// Models/ForumPost.cs
using System;
using System.ComponentModel.DataAnnotations;

namespace HomeOwners.Models
{
    public class ForumPost
    {
        public int Id { get; set; }

        [Required]
        [StringLength(100, MinimumLength = 5)]
        public string Title { get; set; }

        [Required]
        [StringLength(1000, MinimumLength = 10)]
        public string Content { get; set; }

        [Required]
        public string UserId { get; set; }

        [Required]
        [StringLength(100)]
        public string UserName { get; set; }

        [Required]
        [StringLength(100)]
        public string FullName { get; set; }

        [Required]
        [StringLength(20)]
        public string HouseNumber { get; set; }

        [StringLength(50)]
        public string Category { get; set; } = "General";

        public DateTime PostedDate { get; set; } = DateTime.Now;

        public DateTime? EditedDate { get; set; }

        public bool IsVisible { get; set; } = true;

        public bool IsFlagged { get; set; } = false;

        public string? AdminNotes { get; set; }
    }
}