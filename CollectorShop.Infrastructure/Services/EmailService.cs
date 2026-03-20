using MailKit.Net.Smtp;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MimeKit;

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
    private readonly string _host;
    private readonly int _port;
    private readonly bool _useSsl;
    private readonly string _senderEmail;
    private readonly string _senderName;
    private readonly ILogger<EmailService> _logger;

    public EmailService(IConfiguration configuration, ILogger<EmailService> logger)
    {
        var smtpSettings = configuration.GetSection("SmtpSettings");
        _host = smtpSettings["Host"] ?? "localhost";
        _port = int.Parse(smtpSettings["Port"] ?? "25");
        _useSsl = bool.Parse(smtpSettings["UseSsl"] ?? "false");
        _senderEmail = smtpSettings["SenderEmail"] ?? "noreply@maalsikube.dev";
        _senderName = smtpSettings["SenderName"] ?? "CollectorShop";
        _logger = logger;
    }

    public async Task SendEmailAsync(string to, string subject, string body, CancellationToken cancellationToken = default)
    {
        var message = new MimeMessage();
        message.From.Add(new MailboxAddress(_senderName, _senderEmail));
        message.To.Add(MailboxAddress.Parse(to));
        message.Subject = subject;

        message.Body = new TextPart("html") { Text = body };

        using var client = new SmtpClient();
        await client.ConnectAsync(_host, _port, _useSsl ? MailKit.Security.SecureSocketOptions.StartTls : MailKit.Security.SecureSocketOptions.None, cancellationToken);
        await client.SendAsync(message, cancellationToken);
        await client.DisconnectAsync(true, cancellationToken);

        _logger.LogInformation("Email sent to {To} with subject '{Subject}'", to, subject);
    }

    public async Task SendOrderConfirmationAsync(string to, string orderNumber, CancellationToken cancellationToken = default)
    {
        var subject = $"Order Confirmation - {orderNumber}";
        var body = $"""
            <h2>Thank you for your order!</h2>
            <p>Your order <strong>{orderNumber}</strong> has been confirmed.</p>
            <p>We will notify you when your order ships.</p>
            <br/>
            <p>— The CollectorShop Team</p>
            """;

        await SendEmailAsync(to, subject, body, cancellationToken);
    }

    public async Task SendOrderShippedAsync(string to, string orderNumber, string trackingNumber, CancellationToken cancellationToken = default)
    {
        var subject = $"Your Order {orderNumber} Has Shipped!";
        var body = $"""
            <h2>Your order is on its way!</h2>
            <p>Order <strong>{orderNumber}</strong> has been shipped.</p>
            <p>Tracking number: <strong>{trackingNumber}</strong></p>
            <br/>
            <p>— The CollectorShop Team</p>
            """;

        await SendEmailAsync(to, subject, body, cancellationToken);
    }

    public async Task SendPasswordResetAsync(string to, string resetLink, CancellationToken cancellationToken = default)
    {
        var subject = "Reset Your Password - CollectorShop";
        var body = $"""
            <h2>Password Reset Request</h2>
            <p>Click the link below to reset your password:</p>
            <p><a href="{resetLink}">Reset Password</a></p>
            <p>If you did not request this, please ignore this email.</p>
            <br/>
            <p>— The CollectorShop Team</p>
            """;

        await SendEmailAsync(to, subject, body, cancellationToken);
    }

    public async Task SendWelcomeEmailAsync(string to, string firstName, CancellationToken cancellationToken = default)
    {
        var subject = "Welcome to CollectorShop!";
        var body = $"""
            <h2>Welcome, {firstName}!</h2>
            <p>Thank you for joining CollectorShop.</p>
            <p>Start exploring our collection of unique collectible items.</p>
            <br/>
            <p>— The CollectorShop Team</p>
            """;

        await SendEmailAsync(to, subject, body, cancellationToken);
    }
}
