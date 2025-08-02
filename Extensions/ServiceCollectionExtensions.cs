using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.AspNetCore.Cors;
using System.Text;
using AS_CMS.Infrastructure.Persistence;
using AS_CMS.Application.Interfaces;
using AS_CMS.Infrastructure.Identity;
using AS_CMS.Shared.Responses;
using Microsoft.AspNetCore.Cors.Infrastructure;

namespace AS_CMS.Backend.Extensions;

public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds all CMS services to the service collection
    /// </summary>
    public static IServiceCollection AddCmsServices(this IServiceCollection services, IConfiguration configuration)
    {
        // Add Database Context with SQL Server
        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));

        // Register CMS Services
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IJwtTokenGenerator, JwtTokenGenerator>();

        // Add AutoMapper
        services.AddAutoMapper(typeof(ServiceCollectionExtensions).Assembly);

        return services;
    }

    /// <summary>
    /// Adds CMS services with custom database configuration
    /// </summary>
    public static IServiceCollection AddCmsServices(this IServiceCollection services, IConfiguration configuration, Action<DbContextOptionsBuilder> configureDbContext)
    {
        // Add Database Context with custom configuration
        services.AddDbContext<ApplicationDbContext>(configureDbContext);

        // Register CMS Services
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IJwtTokenGenerator, JwtTokenGenerator>();

        // Add AutoMapper
        services.AddAutoMapper(typeof(ServiceCollectionExtensions).Assembly);

        return services;
    }

    /// <summary>
    /// Adds CMS authentication with JWT
    /// </summary>
    public static IServiceCollection AddCmsAuthentication(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = configuration["Jwt:Issuer"],
                    ValidAudience = configuration["Jwt:Audience"],
                    IssuerSigningKey = new SymmetricSecurityKey(
                        Encoding.UTF8.GetBytes(configuration["Jwt:Key"] ?? throw new InvalidOperationException("JWT Key not configured")))
                };
            });

        services.AddAuthorization();

        return services;
    }

    /// <summary>
    /// Adds CMS CORS policy
    /// </summary>
    public static IServiceCollection AddCmsCors(this IServiceCollection services)
    {
        services.AddCors(options =>
        {
            options.AddPolicy("CmsPolicy", policy =>
            {
                policy.AllowAnyOrigin()
                      .AllowAnyMethod()
                      .AllowAnyHeader();
            });
        });

        return services;
    }

    /// <summary>
    /// Adds CMS CORS policy with custom configuration
    /// </summary>
    public static IServiceCollection AddCmsCors(this IServiceCollection services, Action<CorsPolicyBuilder> configurePolicy)
    {
        services.AddCors(options =>
        {
            options.AddPolicy("CmsPolicy", configurePolicy);
        });

        return services;
    }
}

public static class ApplicationBuilderExtensions
{
    /// <summary>
    /// Uses CMS authentication middleware
    /// </summary>
    public static IApplicationBuilder UseCmsAuthentication(this IApplicationBuilder app)
    {
        app.UseAuthentication();
        app.UseAuthorization();
        return app;
    }

    /// <summary>
    /// Uses CMS CORS middleware
    /// </summary>
    public static IApplicationBuilder UseCmsCors(this IApplicationBuilder app)
    {
        app.UseCors("CmsPolicy");
        return app;
    }

    /// <summary>
    /// Ensures CMS database is created
    /// </summary>
    public static IApplicationBuilder EnsureCmsDatabase(this IApplicationBuilder app)
    {
        using var scope = app.ApplicationServices.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        context.Database.EnsureCreated();
        return app;
    }
} 