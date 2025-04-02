using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HomeOwners.Models
{
    public class UserPreferences
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string UserId { get; set; }

        public bool EmailNotifications { get; set; } = true;

        public bool SmsNotifications { get; set; } = false;

        public bool MarketingEmails { get; set; } = false;

        [ForeignKey("UserId")]
        public virtual HomeOwners.Models.Users.HomeOwnerUser User { get; set; }
    }
}