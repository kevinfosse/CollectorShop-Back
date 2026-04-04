using CollectorShop.Domain.Events;
using MassTransit;

namespace CollectorShop.Notifications.Consumers;

public class ProductPriceChangedConsumer : IConsumer<ProductPriceChangedEvent>
{
    private readonly ILogger<ProductPriceChangedConsumer> _logger;

    public ProductPriceChangedConsumer(ILogger<ProductPriceChangedConsumer> logger)
    {
        _logger = logger;
    }

    public Task Consume(ConsumeContext<ProductPriceChangedEvent> context)
    {
        _logger.LogInformation(
            "[NOTIFICATION] Prix changé: {ProductId} — {OldPrice}€ → {NewPrice}€",
            context.Message.ProductId,
            context.Message.OldPrice,
            context.Message.NewPrice);
        return Task.CompletedTask;
    }
}
