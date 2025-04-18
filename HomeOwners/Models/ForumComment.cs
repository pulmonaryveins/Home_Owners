// Models/ForumComment.cs
using System;
using System.ComponentModel.DataAnnotations;

namespace HomeOwners.Models
{
    public class ForumComment
    {
        public int Id { get; set; }

        public int ForumPostId { get; set; }

        [Required]
        [StringLength(500)]
        public string Content { get; set; }

        [Required]
        public string UserId { get; set; }

        [StringLength(100)]
        public string UserName { get; set; }

        public DateTime PostedDate { get; set; } = DateTime.Now;

        public DateTime? EditedDate { get; set; }

        public bool IsVisible { get; set; } = true;

        // Navigation property
        public ForumPost ForumPost { get; set; }
    }
}