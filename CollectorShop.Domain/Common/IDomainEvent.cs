using MediatR;

namespace CollectorShop.Domain.Common;

public interface IDomainEvent : INotification
{
    DateTime OccurredOn { get; }
}
