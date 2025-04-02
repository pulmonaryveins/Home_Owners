using System;
using Microsoft.AspNetCore.Identity;

namespace HomeOwners.Models.Users
{
    public class HomeOwnerUser : IdentityUser
    {
        public DateTime CreatedDate { get; set; } = DateTime.Now;
        public string FullName { get; set; } = string.Empty;
        public string HouseNumber { get; set; } = string.Empty;
        // Note: Email address is already included in IdentityUser base class
        // Phone number is already in IdentityUser, but we'll make sure it's used
    }
}