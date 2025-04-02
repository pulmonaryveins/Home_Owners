using System;
using Microsoft.AspNetCore.Identity;

namespace HomeOwners.Models.Users
{
    public class StaffUser : IdentityUser
    {
        public DateTime CreatedDate { get; set; } = DateTime.Now;
        public string FullName { get; set; } = string.Empty;
        public string Department { get; set; } = string.Empty;
        public string Position { get; set; } = string.Empty;
    }
}