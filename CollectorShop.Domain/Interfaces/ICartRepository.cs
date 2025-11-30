using CollectorShop.Domain.Entities;

namespace CollectorShop.Domain.Interfaces;

public interface ICartRepository : IRepository<Cart>
{
    Task<Cart?> GetByCustomerIdAsync(Guid customerId, CancellationToken cancellationToken = default);
    Task<Cart?> GetByCustomerIdWithItemsAsync(Guid customerId, CancellationToken cancellationToken = default);
    Task<Cart?> GetByIdWithItemsAsync(Guid id, CancellationToken cancellationToken = default);
    Task RemoveExpiredCartsAsync(CancellationToken cancellationToken = default);
}
