using FoodKeep.Application.Common.Interfaces;
using Microsoft.Extensions.Logging;

namespace FoodKeep.Infrastructure.Services;

public class EmailService : IEmailService
{
    private readonly ILogger<EmailService> _logger;

    public EmailService(ILogger<EmailService> logger)
    {
        _logger = logger;
    }

    public Task SendAsync(string to, string subject, string body)
    {
        // TODO: Implémenter l'envoi réel d'email
        _logger.LogInformation("Email sent to {To}: {Subject}", to, subject);
        return Task.CompletedTask;
    }
}
