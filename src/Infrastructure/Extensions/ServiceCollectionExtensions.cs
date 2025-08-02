using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using AS_CMS.Infrastructure.Persistence;
using AS_CMS.Application.Interfaces;
using AS_CMS.Infrastructure.Identity;
using AS_CMS.Infrastructure.Configuration;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace AS_CMS.Infrastructure.Extensions;

public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds AS-CMS services to the service collection with configuration
    /// </summary>
    /// <param name="services">The service collection</param>
    /// <param name="configuration">The configuration</param>
    /// <param name="configureOptions">Action to configure AS-CMS options</param>
    /// <returns>The service collection for chaining</returns>
    public static IServiceCollection AddASCMS(
        this IServiceCollection services,
        IConfiguration configuration,
        Action<ASCMSOptions>? configureOptions = null)
    {
        var options = new ASCMSOptions();
        configureOptions?.Invoke(options);

        // Configure from configuration if not explicitly set
        if (string.IsNullOrEmpty(options.ConnectionString))
        {
            options.ConnectionString = configuration.GetConnectionString("AS_CMS_Connection") 
                ?? throw new InvalidOperationException("AS_CMS_Connection connection string not found in configuration");
        }

        if (string.IsNullOrEmpty(options.Jwt.Key))
        {
            // Try multiple possible JWT configuration sections
            var jwtKey = configuration["AS_CMS_Jwt:Key"] 
                ?? configuration["Jwt:Key"]
                ?? configuration["AS_CMS_Jwt:Key"] 
                ?? throw new InvalidOperationException(GetJwtConfigurationError(configuration));
            
            options.Jwt.Key = jwtKey;
            options.Jwt.Issuer = configuration["AS_CMS_Jwt:Issuer"] 
                ?? configuration["Jwt:Issuer"] 
                ?? options.Jwt.Issuer;
            options.Jwt.Audience = configuration["AS_CMS_Jwt:Audience"] 
                ?? configuration["Jwt:Audience"] 
                ?? options.Jwt.Audience;
        }

        // Add Database Context
        services.AddDbContext<ApplicationDbContext>(dbOptions =>
            dbOptions.UseSqlServer(options.ConnectionString));

        // Register Services
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IJwtTokenGenerator, JwtTokenGenerator>();

        // Configure JWT Authentication
        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(jwtOptions =>
            {
                jwtOptions.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = options.Jwt.Issuer,
                    ValidAudience = options.Jwt.Audience,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(options.Jwt.Key))
                };
            });

        services.AddAuthorization();

        return services;
    }

    /// <summary>
    /// Adds AS-CMS services to the service collection with explicit connection string and JWT settings
    /// </summary>
    /// <param name="services">The service collection</param>
    /// <param name="connectionString">The connection string</param>
    /// <param name="jwtKey">The JWT signing key</param>
    /// <param name="jwtIssuer">The JWT issuer (optional)</param>
    /// <param name="jwtAudience">The JWT audience (optional)</param>
    /// <returns>The service collection for chaining</returns>
    public static IServiceCollection AddASCMS(
        this IServiceCollection services,
        string connectionString,
        string jwtKey,
        string jwtIssuer = "AS-CMS",
        string jwtAudience = "AS-CMS-Users")
    {
        // Add Database Context
        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseSqlServer(connectionString));

        // Register Services
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IJwtTokenGenerator, JwtTokenGenerator>();

        // Configure JWT Authentication
        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = jwtIssuer,
                    ValidAudience = jwtAudience,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey))
                };
            });

        services.AddAuthorization();

        return services;
    }

    private static string GetJwtConfigurationError(IConfiguration configuration)
    {
        var availableKeys = new List<string>();
        
        // Check for common JWT configuration patterns
        var possibleKeys = new[]
        {
            "AS_CMS_Jwt:Key",
            "Jwt:Key",
            "Authentication:JwtBearer:Key",
            "JwtSettings:Key"
        };

        foreach (var key in possibleKeys)
        {
            if (!string.IsNullOrEmpty(configuration[key]))
            {
                availableKeys.Add(key);
            }
        }

        var errorMessage = "JWT Key not configured. ";
        
        if (availableKeys.Any())
        {
            errorMessage += $"Found these JWT keys in configuration: {string.Join(", ", availableKeys)}. ";
        }
        
        errorMessage += "Please add one of the following to your configuration:\n" +
                       "- 'AS_CMS_Jwt:Key'\n" +
                       "- 'Jwt:Key'\n" +
                       "- Or use the explicit configuration method: AddASCMS(connectionString, jwtKey, ...)";

        return errorMessage;
    }
} 