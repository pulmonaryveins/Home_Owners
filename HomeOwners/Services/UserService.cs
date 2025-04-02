using HomeOwners.Areas.Identity.Data; // Changed from HomeOwners.Data
using HomeOwners.Models.Users;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace HomeOwners.Services
{
    public class UserService
    {
        private readonly HomeDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;

        public UserService(HomeDbContext context, UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<AdminUser?> GetAdminUserAsync(string userId)
        {
            return await _context.AdminUsers.FindAsync(userId);
        }

        public async Task<StaffUser?> GetStaffUserAsync(string userId)
        {
            return await _context.StaffUsers.FindAsync(userId);
        }

        public async Task<HomeOwnerUser?> GetHomeOwnerUserAsync(string userId)
        {
            return await _context.HomeOwnerUsers.FindAsync(userId);
        }

        public async Task<string> GetUserRoleAsync(IdentityUser user)
        {
            var roles = await _userManager.GetRolesAsync(user);
            return roles.FirstOrDefault() ?? string.Empty;
        }

        public async Task<List<AdminUser>> GetAllAdminUsersAsync()
        {
            return await _context.AdminUsers.ToListAsync();
        }

        public async Task<List<StaffUser>> GetAllStaffUsersAsync()
        {
            return await _context.StaffUsers.ToListAsync();
        }

        public async Task<List<HomeOwnerUser>> GetAllHomeOwnerUsersAsync()
        {
            return await _context.HomeOwnerUsers.ToListAsync();
        }
    }
}