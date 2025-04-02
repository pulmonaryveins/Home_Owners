using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace HomeOwners.Services
{
    public class NoOpEmailSender : IEmailSender
    {
        private readonly ILogger<NoOpEmailSender> _logger;

        public NoOpEmailSender(ILogger<NoOpEmailSender> logger)
        {
            _logger = logger;
        }

        public Task SendEmailAsync(string email, string subject, string htmlMessage)
        {
            _logger.LogInformation($"Email notification would be sent to: {email}");
            _logger.LogInformation($"Subject: {subject}");
            _logger.LogInformation($"Message: {htmlMessage}");

            // In a real implementation, you would send an actual email here
            return Task.CompletedTask;
        }
    }
}
