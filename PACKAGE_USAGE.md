# AS-CMS Package Usage Guide

## Overview

AS-CMS is a complete backend CMS system that can be used as a NuGet package in your ASP.NET Core applications. This guide shows you how to integrate AS-CMS with your own connection string and configuration.

## Installation

Add the AS-CMS package to your project:

```bash
dotnet add package AS-CMS.Backend
```

## Configuration Options

### Option 1: Using Configuration Files (Recommended)

Add the following to your `appsettings.json`:

```json
{
  "ConnectionStrings": {
    "AS_CMS_Connection": "Server=your-server;Database=your-database;Trusted_Connection=true;TrustServerCertificate=true;MultipleActiveResultSets=true"
  },
  "AS_CMS_Jwt": {
    "Key": "your-super-secret-key-with-at-least-32-characters",
    "Issuer": "YourApp",
    "Audience": "YourApp-Users",
    "ExpiryMinutes": 60,
    "RefreshTokenExpiryDays": 7
  }
}
```

Then in your `Program.cs`:

```csharp
using AS_CMS.Infrastructure.Extensions;

var builder = WebApplication.CreateBuilder(args);

// Add AS-CMS services
builder.Services.AddASCMS(builder.Configuration);

var app = builder.Build();

// Your middleware configuration...
app.UseAuthentication();
app.UseAuthorization();

app.Run();
```

### Option 2: Using Explicit Configuration

In your `Program.cs`:

```csharp
using AS_CMS.Infrastructure.Extensions;

var builder = WebApplication.CreateBuilder(args);

// Add AS-CMS services with explicit configuration
builder.Services.AddASCMS(
    connectionString: "Server=your-server;Database=your-database;Trusted_Connection=true;TrustServerCertificate=true;MultipleActiveResultSets=true",
    jwtKey: "your-super-secret-key-with-at-least-32-characters",
    jwtIssuer: "YourApp",
    jwtAudience: "YourApp-Users"
);

var app = builder.Build();

// Your middleware configuration...
app.UseAuthentication();
app.UseAuthorization();

app.Run();
```

### Option 3: Using Configuration Options

```csharp
using AS_CMS.Infrastructure.Extensions;
using AS_CMS.Infrastructure.Configuration;

var builder = WebApplication.CreateBuilder(args);

// Add AS-CMS services with custom options
builder.Services.AddASCMS(builder.Configuration, options =>
{
    options.ConnectionString = "your-custom-connection-string";
    options.Jwt.Key = "your-jwt-key";
    options.Jwt.Issuer = "YourApp";
    options.Jwt.Audience = "YourApp-Users";
    options.Database.AutoCreateDatabase = true;
    options.Database.AutoApplyMigrations = true;
});

var app = builder.Build();

// Your middleware configuration...
app.UseAuthentication();
app.UseAuthorization();

app.Run();
```

## Database Setup

### 1. Create Database

The package will automatically create the database if you set `AutoCreateDatabase = true` in the options.

### 2. Apply Migrations

If you want to apply migrations manually:

```bash
# Install EF Core tools if not already installed
dotnet tool install --global dotnet-ef

# Apply migrations
dotnet ef database update --project [path-to-your-project] --startup-project [path-to-your-startup-project]
```

### 3. Manual Database Creation

If you prefer to create the database manually:

```sql
CREATE DATABASE YourDatabaseName;
```

## Available Services

The package registers the following services:

- `IAuthService`: Authentication and user management
- `IJwtTokenGenerator`: JWT token generation and validation
- `ApplicationDbContext`: Entity Framework context for database operations

## Available Entities

- `User`: User accounts with authentication info
- `Role`: User roles (Admin, User, Moderator)
- `UserRole`: Many-to-many relationship between users and roles
- `RefreshToken`: JWT refresh tokens for authentication

## Using the Services

### Authentication Service

```csharp
[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register(RegisterRequest request)
    {
        var result = await _authService.RegisterAsync(request);
        return Ok(result);
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginRequest request)
    {
        var result = await _authService.LoginAsync(request);
        return Ok(result);
    }
}
```

### JWT Token Generator

```csharp
[ApiController]
[Route("api/[controller]")]
public class TokenController : ControllerBase
{
    private readonly IJwtTokenGenerator _jwtTokenGenerator;

    public TokenController(IJwtTokenGenerator jwtTokenGenerator)
    {
        _jwtTokenGenerator = jwtTokenGenerator;
    }

    [HttpPost("refresh")]
    public async Task<IActionResult> RefreshToken(RefreshTokenRequest request)
    {
        var result = await _jwtTokenGenerator.RefreshTokenAsync(request.Token);
        return Ok(result);
    }
}
```

## Security Considerations

1. **JWT Key**: Use a strong, randomly generated key with at least 32 characters
2. **Connection String**: Store connection strings securely (use User Secrets in development)
3. **HTTPS**: Always use HTTPS in production
4. **Token Expiry**: Set appropriate token expiry times
5. **Database Security**: Use appropriate database permissions

## Environment-Specific Configuration

### Development

```json
{
  "ConnectionStrings": {
    "AS_CMS_Connection": "Server=localhost;Database=AS_CMS_Dev;Trusted_Connection=true;TrustServerCertificate=true;MultipleActiveResultSets=true"
  },
  "AS_CMS_Jwt": {
    "Key": "dev-key-not-for-production-use-32-chars-min",
    "Issuer": "AS-CMS-Dev",
    "Audience": "AS-CMS-Dev-Users"
  }
}
```

### Production

```json
{
  "ConnectionStrings": {
    "AS_CMS_Connection": "Server=prod-server;Database=AS_CMS_Prod;User Id=app-user;Password=secure-password;TrustServerCertificate=true;MultipleActiveResultSets=true"
  },
  "AS_CMS_Jwt": {
    "Key": "your-production-jwt-key-here-32-characters-minimum",
    "Issuer": "YourApp-Prod",
    "Audience": "YourApp-Prod-Users"
  }
}
```

## Troubleshooting

### Common Issues

1. **Connection String Not Found**: Ensure your connection string is named `AS_CMS_Connection` or use explicit configuration
2. **JWT Key Not Configured**: This is the most common issue. Make sure you have one of these configurations:

   **Option A: Using AS_CMS_Jwt section (Recommended)**
   ```json
   {
     "AS_CMS_Jwt": {
       "Key": "your-super-secret-key-with-at-least-32-characters",
       "Issuer": "YourApp",
       "Audience": "YourApp-Users"
     }
   }
   ```

   **Option B: Using Jwt section**
   ```json
   {
     "Jwt": {
       "Key": "your-super-secret-key-with-at-least-32-characters",
       "Issuer": "YourApp",
       "Audience": "YourApp-Users"
     }
   }
   ```

   **Option C: Using explicit configuration (Most reliable)**
   ```csharp
   builder.Services.AddASCMS(
       connectionString: "your-connection-string",
       jwtKey: "your-super-secret-key-with-at-least-32-characters",
       jwtIssuer: "YourApp",
       jwtAudience: "YourApp-Users"
   );
   ```

3. **Database Connection Failed**: Verify your SQL Server is running and accessible
4. **Migration Errors**: Ensure you have the correct database permissions

### Logging

The package uses Serilog for logging. Configure logging in your `appsettings.json`:

```json
{
  "Serilog": {
    "MinimumLevel": {
      "Default": "Information",
      "Override": {
        "Microsoft": "Warning",
        "AS_CMS": "Information"
      }
    }
  }
}
```

## Support

For issues and questions, please refer to the package documentation or create an issue in the repository. 