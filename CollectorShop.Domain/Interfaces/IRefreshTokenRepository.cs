using CollectorShop.Domain.Entities;

namespace CollectorShop.Domain.Interfaces;

public interface IRefreshTokenRepository : IRepository<RefreshToken>
{
    Task<RefreshToken?> GetByTokenAsync(string token, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<RefreshToken>> GetActiveTokensByUserIdAsync(string userId, CancellationToken cancellationToken = default);
    Task RevokeAllUserTokensAsync(string userId, string reason, CancellationToken cancellationToken = default);
    Task RemoveExpiredTokensAsync(CancellationToken cancellationToken = default);
}
