using System.ComponentModel.DataAnnotations;

namespace AS_CMS.Domain.Entities;

public class RefreshToken
{
    public Guid Id { get; private set; }
    public Guid UserId { get; private set; }
    
    [Required]
    [MaxLength(255)]
    public string Token { get; private set; } = string.Empty;
    
    public DateTime ExpiresAt { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? RevokedAt { get; private set; }
    
    [MaxLength(255)]
    public string? RevokedBy { get; private set; }
    
    [MaxLength(255)]
    public string? ReplacedByToken { get; private set; }
    
    public bool IsExpired => DateTime.UtcNow >= ExpiresAt;
    public bool IsRevoked => RevokedAt != null;
    public bool IsActive => !IsRevoked && !IsExpired;
    
    // Navigation properties
    public virtual User User { get; private set; } = null!;

    // Private constructor for EF Core
    private RefreshToken() { }

    // Factory method for creating new refresh tokens
    public static RefreshToken Create(Guid userId, string token, DateTime expiresAt)
    {
        if (string.IsNullOrWhiteSpace(token))
            throw new ArgumentException("Token is required", nameof(token));

        return new RefreshToken
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Token = token,
            ExpiresAt = expiresAt,
            CreatedAt = DateTime.UtcNow
        };
    }

    // Business methods
    public void Revoke(string? revokedBy = null, string? replacedByToken = null)
    {
        RevokedAt = DateTime.UtcNow;
        RevokedBy = revokedBy;
        ReplacedByToken = replacedByToken;
    }
} 