﻿using System;
using Microsoft.AspNetCore.Identity;

namespace HomeOwners.Models.Users
{
    public class HomeOwnerUser : IdentityUser
    {
        public DateTime CreatedDate { get; set; } = DateTime.Now;
        public string FullName { get; set; } = string.Empty;
        public string HouseNumber { get; set; } = string.Empty;
        public string AccountStatus { get; set; } = "pending"; // Values: "pending", "approved", "rejected"
        public string RejectionReason { get; set; } = string.Empty;
        public virtual UserPreferences Preferences { get; set; }
        // Note: Email address is already included in IdentityUser base class
        // Phone number is already in IdentityUser, but we'll make sure it's used
    }
}