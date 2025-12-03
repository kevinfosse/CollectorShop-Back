using CollectorShop.Domain.Common;

namespace CollectorShop.Domain.Entities;

public class RefreshToken : BaseEntity
{
    public string Token { get; private set; }
    public string UserId { get; private set; }
    public DateTime ExpiresAt { get; private set; }
    public DateTime? RevokedAt { get; private set; }
    public string? ReplacedByToken { get; private set; }
    public string? RevokedReason { get; private set; }

    public bool IsExpired => DateTime.UtcNow >= ExpiresAt;
    public bool IsRevoked => RevokedAt != null;
    public bool IsActive => !IsRevoked && !IsExpired;

    private RefreshToken()
    {
        Token = null!;
        UserId = null!;
    }

    public RefreshToken(string token, string userId, int expirationDays)
    {
        if (string.IsNullOrWhiteSpace(token))
            throw new ArgumentException("Token cannot be empty", nameof(token));
        if (string.IsNullOrWhiteSpace(userId))
            throw new ArgumentException("UserId cannot be empty", nameof(userId));
        if (expirationDays <= 0)
            throw new ArgumentException("Expiration days must be positive", nameof(expirationDays));

        Token = token;
        UserId = userId;
        ExpiresAt = DateTime.UtcNow.AddDays(expirationDays);
    }

    public void Revoke(string? reason = null, string? replacedByToken = null)
    {
        RevokedAt = DateTime.UtcNow;
        RevokedReason = reason;
        ReplacedByToken = replacedByToken;
        UpdatedAt = DateTime.UtcNow;
    }
}
