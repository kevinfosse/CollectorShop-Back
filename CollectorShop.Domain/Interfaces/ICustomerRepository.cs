using CollectorShop.Domain.Entities;

namespace CollectorShop.Domain.Interfaces;

public interface ICustomerRepository : IRepository<Customer>
{
    Task<Customer?> GetByEmailAsync(string email, CancellationToken cancellationToken = default);
    Task<Customer?> GetByUserIdAsync(string userId, CancellationToken cancellationToken = default);
    Task<Customer?> GetByIdWithOrdersAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Customer?> GetByIdWithAddressesAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Customer?> GetByIdWithCartAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Customer?> GetByIdWithWishlistAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Customer?> GetByUserIdWithWishlistAsync(string userId, CancellationToken cancellationToken = default);
}
