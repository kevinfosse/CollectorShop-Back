using MailKit.Net.Smtp;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MimeKit;

namespace CollectorShop.Infrastructure.Services;

public interface IEmailService
{
    Task SendEmailAsync(string to, string subject, string body, CancellationToken cancellationToken = default);
    Task SendWelcomeEmailAsync(string to, string firstName, CancellationToken cancellationToken = default);
    Task SendPasswordResetAsync(string to, string firstName, string resetLink, CancellationToken cancellationToken = default);
    Task SendOrderConfirmationAsync(string to, string firstName, string orderNumber, decimal totalAmount, string currency, IEnumerable<OrderItemEmail> items, CancellationToken cancellationToken = default);
    Task SendOrderConfirmedAsync(string to, string firstName, string orderNumber, CancellationToken cancellationToken = default);
    Task SendOrderProcessingAsync(string to, string firstName, string orderNumber, CancellationToken cancellationToken = default);
    Task SendOrderShippedAsync(string to, string firstName, string orderNumber, string trackingNumber, string? carrier, CancellationToken cancellationToken = default);
    Task SendOrderDeliveredAsync(string to, string firstName, string orderNumber, CancellationToken cancellationToken = default);
    Task SendOrderCancelledAsync(string to, string firstName, string orderNumber, string? reason, CancellationToken cancellationToken = default);
}

public class OrderItemEmail
{
    public string ProductName { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal TotalPrice { get; set; }
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
        try
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
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send email to {To} with subject '{Subject}'", to, subject);
        }
    }

    public Task SendWelcomeEmailAsync(string to, string firstName, CancellationToken cancellationToken = default)
    {
        var body = WrapInLayout($"""
            <h2 style="color: #1a1a2e; margin: 0 0 16px;">Welcome to CollectorShop, {firstName}!</h2>
            <p style="color: #444; font-size: 16px; line-height: 1.6;">
                Thank you for creating your account. You now have access to our curated collection of unique collectible items.
            </p>
            <p style="color: #444; font-size: 16px; line-height: 1.6;">
                Start browsing and find your next treasure!
            </p>
            <div style="text-align: center; margin: 32px 0;">
                <a href="https://maalsikube.dev" style="background-color: #1a1a2e; color: #ffffff; padding: 14px 32px; text-decoration: none; border-radius: 6px; font-weight: bold; font-size: 16px;">
                    Browse Collection
                </a>
            </div>
            """);

        return SendEmailAsync(to, "Welcome to CollectorShop!", body, cancellationToken);
    }

    public Task SendPasswordResetAsync(string to, string firstName, string resetLink, CancellationToken cancellationToken = default)
    {
        var body = WrapInLayout($"""
            <h2 style="color: #1a1a2e; margin: 0 0 16px;">Password Reset Request</h2>
            <p style="color: #444; font-size: 16px; line-height: 1.6;">
                Hi {firstName}, we received a request to reset your password.
            </p>
            <p style="color: #444; font-size: 16px; line-height: 1.6;">
                Click the button below to set a new password. This link will expire in 24 hours.
            </p>
            <div style="text-align: center; margin: 32px 0;">
                <a href="{resetLink}" style="background-color: #e63946; color: #ffffff; padding: 14px 32px; text-decoration: none; border-radius: 6px; font-weight: bold; font-size: 16px;">
                    Reset Password
                </a>
            </div>
            <p style="color: #888; font-size: 13px;">
                If you didn't request this, you can safely ignore this email. Your password will remain unchanged.
            </p>
            """);

        return SendEmailAsync(to, "Reset Your Password - CollectorShop", body, cancellationToken);
    }

    public Task SendOrderConfirmationAsync(string to, string firstName, string orderNumber, decimal totalAmount, string currency, IEnumerable<OrderItemEmail> items, CancellationToken cancellationToken = default)
    {
        var itemRows = string.Join("", items.Select(i => $"""
            <tr>
                <td style="padding: 10px 12px; border-bottom: 1px solid #eee; color: #444;">{i.ProductName}</td>
                <td style="padding: 10px 12px; border-bottom: 1px solid #eee; color: #444; text-align: center;">{i.Quantity}</td>
                <td style="padding: 10px 12px; border-bottom: 1px solid #eee; color: #444; text-align: right;">{i.UnitPrice:F2} {currency}</td>
                <td style="padding: 10px 12px; border-bottom: 1px solid #eee; color: #444; text-align: right;">{i.TotalPrice:F2} {currency}</td>
            </tr>
            """));

        var body = WrapInLayout($"""
            <h2 style="color: #1a1a2e; margin: 0 0 16px;">Order Confirmed!</h2>
            <p style="color: #444; font-size: 16px; line-height: 1.6;">
                Hi {firstName}, thank you for your order! Here's your order summary:
            </p>
            <div style="background: #f8f9fa; border-radius: 8px; padding: 16px; margin: 20px 0;">
                <p style="margin: 0; color: #1a1a2e; font-weight: bold; font-size: 15px;">Order #{orderNumber}</p>
            </div>
            <table style="width: 100%; border-collapse: collapse; margin: 20px 0;">
                <thead>
                    <tr style="background: #1a1a2e; color: #fff;">
                        <th style="padding: 10px 12px; text-align: left;">Product</th>
                        <th style="padding: 10px 12px; text-align: center;">Qty</th>
                        <th style="padding: 10px 12px; text-align: right;">Unit Price</th>
                        <th style="padding: 10px 12px; text-align: right;">Total</th>
                    </tr>
                </thead>
                <tbody>
                    {itemRows}
                </tbody>
            </table>
            <div style="text-align: right; margin: 16px 0; padding: 16px; background: #f8f9fa; border-radius: 8px;">
                <p style="margin: 0; font-size: 18px; font-weight: bold; color: #1a1a2e;">Total: {totalAmount:F2} {currency}</p>
            </div>
            <p style="color: #444; font-size: 16px; line-height: 1.6;">
                We'll send you an email when your order status changes.
            </p>
            """);

        return SendEmailAsync(to, $"Order Confirmation - #{orderNumber}", body, cancellationToken);
    }

    public Task SendOrderConfirmedAsync(string to, string firstName, string orderNumber, CancellationToken cancellationToken = default)
    {
        var body = WrapInLayout($"""
            <h2 style="color: #1a1a2e; margin: 0 0 16px;">Order Confirmed</h2>
            <p style="color: #444; font-size: 16px; line-height: 1.6;">
                Hi {firstName}, great news! Your order <strong>#{orderNumber}</strong> has been confirmed and will be prepared shortly.
            </p>
            {StatusTracker("Confirmed")}
            <p style="color: #444; font-size: 16px; line-height: 1.6;">
                We'll notify you once your order is being prepared.
            </p>
            """);

        return SendEmailAsync(to, $"Order #{orderNumber} - Confirmed", body, cancellationToken);
    }

    public Task SendOrderProcessingAsync(string to, string firstName, string orderNumber, CancellationToken cancellationToken = default)
    {
        var body = WrapInLayout($"""
            <h2 style="color: #1a1a2e; margin: 0 0 16px;">Order Being Prepared</h2>
            <p style="color: #444; font-size: 16px; line-height: 1.6;">
                Hi {firstName}, your order <strong>#{orderNumber}</strong> is now being prepared for shipment.
            </p>
            {StatusTracker("Processing")}
            <p style="color: #444; font-size: 16px; line-height: 1.6;">
                We'll send you the tracking information once it ships.
            </p>
            """);

        return SendEmailAsync(to, $"Order #{orderNumber} - Being Prepared", body, cancellationToken);
    }

    public Task SendOrderShippedAsync(string to, string firstName, string orderNumber, string trackingNumber, string? carrier, CancellationToken cancellationToken = default)
    {
        var carrierInfo = !string.IsNullOrEmpty(carrier) ? $" via <strong>{carrier}</strong>" : "";

        var body = WrapInLayout($"""
            <h2 style="color: #1a1a2e; margin: 0 0 16px;">Your Order Has Shipped!</h2>
            <p style="color: #444; font-size: 16px; line-height: 1.6;">
                Hi {firstName}, your order <strong>#{orderNumber}</strong> is on its way{carrierInfo}!
            </p>
            {StatusTracker("Shipped")}
            <div style="background: #f8f9fa; border-radius: 8px; padding: 20px; margin: 24px 0; text-align: center;">
                <p style="margin: 0 0 4px; color: #888; font-size: 13px; text-transform: uppercase; letter-spacing: 1px;">Tracking Number</p>
                <p style="margin: 0; font-size: 20px; font-weight: bold; color: #1a1a2e; letter-spacing: 1px;">{trackingNumber}</p>
            </div>
            """);

        return SendEmailAsync(to, $"Order #{orderNumber} - Shipped!", body, cancellationToken);
    }

    public Task SendOrderDeliveredAsync(string to, string firstName, string orderNumber, CancellationToken cancellationToken = default)
    {
        var body = WrapInLayout($"""
            <h2 style="color: #1a1a2e; margin: 0 0 16px;">Order Delivered!</h2>
            <p style="color: #444; font-size: 16px; line-height: 1.6;">
                Hi {firstName}, your order <strong>#{orderNumber}</strong> has been delivered. We hope you love your new items!
            </p>
            {StatusTracker("Delivered")}
            <p style="color: #444; font-size: 16px; line-height: 1.6;">
                If you have any issues, don't hesitate to contact us.
            </p>
            """);

        return SendEmailAsync(to, $"Order #{orderNumber} - Delivered", body, cancellationToken);
    }

    public Task SendOrderCancelledAsync(string to, string firstName, string orderNumber, string? reason, CancellationToken cancellationToken = default)
    {
        var reasonText = !string.IsNullOrEmpty(reason)
            ? $"""<p style="color: #444; font-size: 16px; line-height: 1.6;"><strong>Reason:</strong> {reason}</p>"""
            : "";

        var body = WrapInLayout($"""
            <h2 style="color: #e63946; margin: 0 0 16px;">Order Cancelled</h2>
            <p style="color: #444; font-size: 16px; line-height: 1.6;">
                Hi {firstName}, your order <strong>#{orderNumber}</strong> has been cancelled.
            </p>
            {reasonText}
            <p style="color: #444; font-size: 16px; line-height: 1.6;">
                If this was a mistake or you have questions, please contact our support team.
            </p>
            """);

        return SendEmailAsync(to, $"Order #{orderNumber} - Cancelled", body, cancellationToken);
    }

    private static string StatusTracker(string currentStatus)
    {
        var steps = new[] { "Confirmed", "Processing", "Shipped", "Delivered" };
        var currentIndex = Array.IndexOf(steps, currentStatus);

        var stepsHtml = string.Join("", steps.Select((step, i) =>
        {
            var color = i <= currentIndex ? "#1a1a2e" : "#ccc";
            var bgColor = i <= currentIndex ? "#1a1a2e" : "#eee";
            var textColor = i <= currentIndex ? "#fff" : "#999";
            return $"""
                <td style="text-align: center; padding: 0 8px;">
                    <div style="width: 32px; height: 32px; border-radius: 50%; background: {bgColor}; color: {textColor}; line-height: 32px; margin: 0 auto 6px; font-size: 14px; font-weight: bold;">{i + 1}</div>
                    <div style="font-size: 11px; color: {color}; font-weight: {(i == currentIndex ? "bold" : "normal")};">{step}</div>
                </td>
                """;
        }));

        return $"""
            <table style="width: 100%; margin: 24px 0;">
                <tr>{stepsHtml}</tr>
            </table>
            """;
    }

    private static string WrapInLayout(string content)
    {
        return $"""
            <!DOCTYPE html>
            <html>
            <head><meta charset="utf-8" /></head>
            <body style="margin: 0; padding: 0; background-color: #f4f4f7; font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, sans-serif;">
                <table style="max-width: 600px; margin: 0 auto; background: #ffffff;" cellpadding="0" cellspacing="0" width="100%">
                    <tr>
                        <td style="background: #1a1a2e; padding: 24px 32px; text-align: center;">
                            <h1 style="margin: 0; color: #ffffff; font-size: 24px; letter-spacing: 1px;">CollectorShop</h1>
                        </td>
                    </tr>
                    <tr>
                        <td style="padding: 32px;">
                            {content}
                        </td>
                    </tr>
                    <tr>
                        <td style="background: #f8f9fa; padding: 20px 32px; text-align: center; border-top: 1px solid #eee;">
                            <p style="margin: 0; color: #999; font-size: 12px;">
                                &copy; {DateTime.UtcNow.Year} CollectorShop. All rights reserved.<br/>
                                <a href="https://maalsikube.dev" style="color: #1a1a2e;">maalsikube.dev</a>
                            </p>
                        </td>
                    </tr>
                </table>
            </body>
            </html>
            """;
    }
}
