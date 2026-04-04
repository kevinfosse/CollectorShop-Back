using CollectorShop.Domain.Events;
using MassTransit;

namespace CollectorShop.Notifications.Consumers;

public class ProductStatusChangedConsumer : IConsumer<ProductStatusChangedEvent>
{
    private readonly ILogger<ProductStatusChangedConsumer> _logger;

    public ProductStatusChangedConsumer(ILogger<ProductStatusChangedConsumer> logger)
    {
        _logger = logger;
    }

    public Task Consume(ConsumeContext<ProductStatusChangedEvent> context)
    {
        var status = context.Message.IsActive ? "actif" : "inactif";
        _logger.LogInformation(
            "[NOTIFICATION] Produit {ProductId} est maintenant {Status}",
            context.Message.ProductId,
            status);
        return Task.CompletedTask;
    }
}
