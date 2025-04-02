using Microsoft.AspNetCore.Identity;

namespace HomeOwners.Models.Users
{
    public class ApplicationUser : IdentityUser
    {
        public string FullName { get; set; } = string.Empty;
    }
}