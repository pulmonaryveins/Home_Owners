using System;
using Microsoft.AspNetCore.Identity;

namespace HomeOwners.Models.Users
{
    public class HomeOwnerUser : IdentityUser
    {
        public DateTime CreatedDate { get; set; } = DateTime.Now;
        public string FullName { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public string PropertyId { get; set; } = string.Empty;
    }
}