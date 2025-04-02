using HomeOwners.Areas.Identity.Data;
using HomeOwners.Models;
using HomeOwners.Models.Users;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace HomeOwners.Services
{
    public interface IUserPreferencesService
    {
        Task<UserPreferences> GetUserPreferencesAsync(string userId);
        Task SaveUserPreferencesAsync(string userId, bool emailNotifications, bool smsNotifications, bool marketingEmails);
    }

    public class UserPreferencesService : IUserPreferencesService
    {
        private readonly HomeDbContext _context;

        public UserPreferencesService(HomeDbContext context)
        {
            _context = context;
        }

        public async Task<UserPreferences> GetUserPreferencesAsync(string userId)
        {
            var prefs = await _context.UserPreferences
                .FirstOrDefaultAsync(p => p.UserId == userId);

            if (prefs == null)
            {
                prefs = new UserPreferences
                {
                    UserId = userId,
                    EmailNotifications = true,
                    SmsNotifications = false,
                    MarketingEmails = false
                };

                _context.UserPreferences.Add(prefs);
                await _context.SaveChangesAsync();
            }

            return prefs;
        }

        public async Task SaveUserPreferencesAsync(string userId, bool emailNotifications, bool smsNotifications, bool marketingEmails)
        {
            var prefs = await _context.UserPreferences
                .FirstOrDefaultAsync(p => p.UserId == userId);

            if (prefs == null)
            {
                prefs = new UserPreferences
                {
                    UserId = userId,
                    EmailNotifications = emailNotifications,
                    SmsNotifications = smsNotifications,
                    MarketingEmails = marketingEmails
                };

                _context.UserPreferences.Add(prefs);
            }
            else
            {
                prefs.EmailNotifications = emailNotifications;
                prefs.SmsNotifications = smsNotifications;
                prefs.MarketingEmails = marketingEmails;

                _context.UserPreferences.Update(prefs);
            }

            await _context.SaveChangesAsync();
        }
    }
}