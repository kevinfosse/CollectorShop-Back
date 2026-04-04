using CollectorShop.Domain.Events;
using MassTransit;

namespace CollectorShop.Notifications.Consumers;

public class ProductCreatedConsumer : IConsumer<ProductCreatedEvent>
{
    private readonly ILogger<ProductCreatedConsumer> _logger;

    public ProductCreatedConsumer(ILogger<ProductCreatedConsumer> logger)
    {
        _logger = logger;
    }

    public Task Consume(ConsumeContext<ProductCreatedEvent> context)
    {
        _logger.LogInformation(
            "[NOTIFICATION] Nouveau produit créé: {ProductName} (SKU: {Sku})",
            context.Message.ProductName,
            context.Message.Sku);
        return Task.CompletedTask;
    }
}
