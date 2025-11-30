using CollectorShop.Domain.Entities;
using CollectorShop.Domain.Interfaces;
using CollectorShop.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace CollectorShop.Infrastructure.Repositories;

public class CouponRepository : Repository<Coupon>, ICouponRepository
{
    public CouponRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<Coupon?> GetByCodeAsync(string code, CancellationToken cancellationToken = default)
    {
        return await _dbSet.FirstOrDefaultAsync(c => c.Code == code.ToUpperInvariant(), cancellationToken);
    }

    public async Task<IReadOnlyList<Coupon>> GetActiveCouponsAsync(CancellationToken cancellationToken = default)
    {
        var now = DateTime.UtcNow;
        return await _dbSet
            .Where(c => c.IsActive 
                && (!c.StartsAt.HasValue || c.StartsAt.Value <= now)
                && (!c.ExpiresAt.HasValue || c.ExpiresAt.Value > now))
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Coupon>> GetExpiredCouponsAsync(CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(c => c.ExpiresAt.HasValue && c.ExpiresAt.Value < DateTime.UtcNow)
            .ToListAsync(cancellationToken);
    }
}
