namespace CollectorShop.Infrastructure.Services;

public interface IEmailService
{
    Task SendEmailAsync(string to, string subject, string body, CancellationToken cancellationToken = default);
    Task SendOrderConfirmationAsync(string to, string orderNumber, CancellationToken cancellationToken = default);
    Task SendOrderShippedAsync(string to, string orderNumber, string trackingNumber, CancellationToken cancellationToken = default);
    Task SendPasswordResetAsync(string to, string resetLink, CancellationToken cancellationToken = default);
    Task SendWelcomeEmailAsync(string to, string firstName, CancellationToken cancellationToken = default);
}

public class EmailService : IEmailService
{
    // TODO: Implement with actual email provider (SendGrid, SMTP, etc.)

    public Task SendEmailAsync(string to, string subject, string body, CancellationToken cancellationToken = default)
    {
        // Implementation placeholder
        return Task.CompletedTask;
    }

    public Task SendOrderConfirmationAsync(string to, string orderNumber, CancellationToken cancellationToken = default)
    {
        // Implementation placeholder
        return Task.CompletedTask;
    }

    public Task SendOrderShippedAsync(string to, string orderNumber, string trackingNumber, CancellationToken cancellationToken = default)
    {
        // Implementation placeholder
        return Task.CompletedTask;
    }

    public Task SendPasswordResetAsync(string to, string resetLink, CancellationToken cancellationToken = default)
    {
        // Implementation placeholder
        return Task.CompletedTask;
    }

    public Task SendWelcomeEmailAsync(string to, string firstName, CancellationToken cancellationToken = default)
    {
        // Implementation placeholder
        return Task.CompletedTask;
    }
}
