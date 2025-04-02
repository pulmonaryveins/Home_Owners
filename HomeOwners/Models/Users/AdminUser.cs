using System;
using Microsoft.AspNetCore.Identity;

namespace HomeOwners.Models.Users
{
    public class AdminUser : IdentityUser
    {
        public DateTime CreatedDate { get; set; } = DateTime.Now;
        public string FullName { get; set; } = string.Empty;
        public string AdminLevel { get; set; } = "Standard";
    }
}