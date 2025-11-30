using CollectorShop.Domain.Common;

namespace CollectorShop.Domain.Entities;

public class Shipment : BaseEntity
{
    public Guid OrderId { get; private set; }
    public Order Order { get; private set; } = null!;

    public string TrackingNumber { get; private set; }
    public string Carrier { get; private set; }
    public string? TrackingUrl { get; private set; }

    public DateTime ShippedAt { get; private set; }
    public DateTime? EstimatedDeliveryDate { get; private set; }
    public DateTime? DeliveredAt { get; private set; }

    private Shipment() { }

    public Shipment(Guid orderId, string trackingNumber, string carrier)
    {
        if (string.IsNullOrWhiteSpace(trackingNumber))
            throw new ArgumentException("Tracking number cannot be empty", nameof(trackingNumber));
        if (string.IsNullOrWhiteSpace(carrier))
            throw new ArgumentException("Carrier cannot be empty", nameof(carrier));

        OrderId = orderId;
        TrackingNumber = trackingNumber;
        Carrier = carrier;
        ShippedAt = DateTime.UtcNow;
    }

    public void SetTrackingUrl(string trackingUrl)
    {
        TrackingUrl = trackingUrl;
        UpdatedAt = DateTime.UtcNow;
    }

    public void SetEstimatedDeliveryDate(DateTime estimatedDate)
    {
        EstimatedDeliveryDate = estimatedDate;
        UpdatedAt = DateTime.UtcNow;
    }

    public void MarkAsDelivered()
    {
        DeliveredAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }
}
