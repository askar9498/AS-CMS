namespace AS_CMS.Infrastructure.Configuration;

/// <summary>
/// Configuration options for AS-CMS
/// </summary>
public class ASCMSOptions
{
    /// <summary>
    /// The connection string for the AS-CMS database
    /// </summary>
    public string ConnectionString { get; set; } = string.Empty;

    /// <summary>
    /// JWT configuration options
    /// </summary>
    public JwtOptions Jwt { get; set; } = new();

    /// <summary>
    /// Database configuration options
    /// </summary>
    public DatabaseOptions Database { get; set; } = new();
}

/// <summary>
/// JWT configuration options
/// </summary>
public class JwtOptions
{
    /// <summary>
    /// The JWT signing key
    /// </summary>
    public string Key { get; set; } = string.Empty;

    /// <summary>
    /// The JWT issuer
    /// </summary>
    public string Issuer { get; set; } = "AS-CMS";

    /// <summary>
    /// The JWT audience
    /// </summary>
    public string Audience { get; set; } = "AS-CMS-Users";

    /// <summary>
    /// Token expiry in minutes
    /// </summary>
    public int ExpiryMinutes { get; set; } = 60;

    /// <summary>
    /// Refresh token expiry in days
    /// </summary>
    public int RefreshTokenExpiryDays { get; set; } = 7;
}

/// <summary>
/// Database configuration options
/// </summary>
public class DatabaseOptions
{
    /// <summary>
    /// Whether to automatically create the database if it doesn't exist
    /// </summary>
    public bool AutoCreateDatabase { get; set; } = false;

    /// <summary>
    /// Whether to automatically apply migrations on startup
    /// </summary>
    public bool AutoApplyMigrations { get; set; } = false;

    /// <summary>
    /// Whether to seed initial data
    /// </summary>
    public bool SeedInitialData { get; set; } = true;
} 