using CollectorShop.Domain.Entities;

namespace CollectorShop.Domain.Interfaces;

public interface IBrandRepository : IRepository<Brand>
{
    Task<Brand?> GetBySlugAsync(string slug, CancellationToken cancellationToken = default);
    Task<Brand?> GetByIdWithProductsAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Brand>> GetActiveBrandsAsync(CancellationToken cancellationToken = default);
}
