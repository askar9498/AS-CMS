using System.ComponentModel.DataAnnotations;

namespace AS_CMS.Domain.Entities;

public class UserLoginLog
{
    public Guid Id { get; private set; }
    
    public Guid UserId { get; private set; }
    public virtual User User { get; private set; } = null!;
    
    public DateTime LoginTime { get; private set; }
    public DateTime? LogoutTime { get; private set; }
    
    [MaxLength(45)]
    public string? IpAddress { get; private set; }
    
    [MaxLength(500)]
    public string? UserAgent { get; private set; }
    
    [MaxLength(100)]
    public string? DeviceType { get; private set; }
    
    [MaxLength(100)]
    public string? Browser { get; private set; }
    
    [MaxLength(100)]
    public string? OperatingSystem { get; private set; }
    
    public bool IsSuccessful { get; private set; }
    
    [MaxLength(500)]
    public string? FailureReason { get; private set; }
    
    public DateTime CreatedAt { get; private set; }

    // Private constructor for EF Core
    private UserLoginLog() { }

    // Factory method for creating successful login logs
    public static UserLoginLog CreateSuccessfulLogin(
        Guid userId,
        string? ipAddress = null,
        string? userAgent = null,
        string? deviceType = null,
        string? browser = null,
        string? operatingSystem = null)
    {
        return new UserLoginLog
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            LoginTime = DateTime.UtcNow,
            IpAddress = ipAddress,
            UserAgent = userAgent,
            DeviceType = deviceType,
            Browser = browser,
            OperatingSystem = operatingSystem,
            IsSuccessful = true,
            CreatedAt = DateTime.UtcNow
        };
    }

    // Factory method for creating failed login logs
    public static UserLoginLog CreateFailedLogin(
        Guid userId,
        string failureReason,
        string? ipAddress = null,
        string? userAgent = null,
        string? deviceType = null,
        string? browser = null,
        string? operatingSystem = null)
    {
        return new UserLoginLog
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            LoginTime = DateTime.UtcNow,
            IpAddress = ipAddress,
            UserAgent = userAgent,
            DeviceType = deviceType,
            Browser = browser,
            OperatingSystem = operatingSystem,
            IsSuccessful = false,
            FailureReason = failureReason,
            CreatedAt = DateTime.UtcNow
        };
    }

    // Record logout
    public void RecordLogout()
    {
        LogoutTime = DateTime.UtcNow;
    }

    // Get session duration
    public TimeSpan? GetSessionDuration()
    {
        if (LogoutTime.HasValue)
        {
            return LogoutTime.Value - LoginTime;
        }
        return null;
    }

    // Check if session is still active
    public bool IsSessionActive()
    {
        return !LogoutTime.HasValue;
    }
} 