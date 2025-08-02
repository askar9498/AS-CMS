# Example: Using AS-CMS Package in Consumer Project

This example shows how to use the AS-CMS package in a new ASP.NET Core project.

## Step 1: Create New Project

```bash
dotnet new webapi -n MyApp
cd MyApp
```

## Step 2: Add AS-CMS Package

```bash
dotnet add package AS-CMS.Backend
```

## Step 3: Configure appsettings.json

```json
{
  "ConnectionStrings": {
    "AS_CMS_Connection": "Server=localhost;Database=MyApp_CMS;Trusted_Connection=true;TrustServerCertificate=true;MultipleActiveResultSets=true"
  },
  "AS_CMS_Jwt": {
    "Key": "my-super-secret-jwt-key-with-at-least-32-characters-long",
    "Issuer": "MyApp",
    "Audience": "MyApp-Users",
    "ExpiryMinutes": 60,
    "RefreshTokenExpiryDays": 7
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "AllowedHosts": "*"
}
```

## Step 4: Configure Program.cs

```csharp
using AS_CMS.Infrastructure.Extensions;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Add AS-CMS services
builder.Services.AddASCMS(builder.Configuration);

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
```

## Step 5: Create Controllers

### AuthController.cs

```csharp
using Microsoft.AspNetCore.Mvc;
using AS_CMS.Application.Interfaces;
using AS_CMS.Application.DTOs;

namespace MyApp.Controllers;

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
    public async Task<IActionResult> Register([FromBody] RegisterRequest request)
    {
        var result = await _authService.RegisterAsync(request);
        return Ok(result);
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        var result = await _authService.LoginAsync(request);
        return Ok(result);
    }

    [HttpPost("refresh")]
    public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenRequest request)
    {
        var result = await _authService.RefreshTokenAsync(request.Token);
        return Ok(result);
    }
}
```

### UserController.cs

```csharp
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using AS_CMS.Application.Interfaces;
using AS_CMS.Application.DTOs;

namespace MyApp.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class UserController : ControllerBase
{
    private readonly IAuthService _authService;

    public UserController(IAuthService authService)
    {
        _authService = authService;
    }

    [HttpGet("profile")]
    public async Task<IActionResult> GetProfile()
    {
        var userId = User.FindFirst("sub")?.Value;
        if (string.IsNullOrEmpty(userId))
            return Unauthorized();

        var user = await _authService.GetUserByIdAsync(Guid.Parse(userId));
        return Ok(user);
    }

    [HttpPut("profile")]
    public async Task<IActionResult> UpdateProfile([FromBody] UpdateUserRequest request)
    {
        var userId = User.FindFirst("sub")?.Value;
        if (string.IsNullOrEmpty(userId))
            return Unauthorized();

        var result = await _authService.UpdateUserAsync(Guid.Parse(userId), request);
        return Ok(result);
    }
}
```

## Step 6: Apply Database Migrations

```bash
# Install EF Core tools
dotnet tool install --global dotnet-ef

# Apply migrations
dotnet ef database update --project [path-to-your-project] --startup-project [path-to-your-project]
```

## Step 7: Test the API

### Register a new user:

```bash
curl -X POST "https://localhost:7001/api/auth/register" \
  -H "Content-Type: application/json" \
  -d '{
    "FirstName": "John",
    "LastName": "Doe",
    "Email": "john.doe@example.com",
    "Password": "SecurePassword123!"
  }'
```

### Login:

```bash
curl -X POST "https://localhost:7001/api/auth/login" \
  -H "Content-Type: application/json" \
  -d '{
    "Email": "john.doe@example.com",
    "Password": "SecurePassword123!"
  }'
```

### Get user profile (with JWT token):

```bash
curl -X GET "https://localhost:7001/api/user/profile" \
  -H "Authorization: Bearer YOUR_JWT_TOKEN_HERE"
```

## Alternative: Using Explicit Configuration

If you prefer not to use configuration files, you can configure AS-CMS explicitly:

```csharp
using AS_CMS.Infrastructure.Extensions;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Add AS-CMS services with explicit configuration
builder.Services.AddASCMS(
    connectionString: "Server=localhost;Database=MyApp_CMS;Trusted_Connection=true;TrustServerCertificate=true;MultipleActiveResultSets=true",
    jwtKey: "my-super-secret-jwt-key-with-at-least-32-characters-long",
    jwtIssuer: "MyApp",
    jwtAudience: "MyApp-Users"
);

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
```

## Environment-Specific Configuration

### Development (appsettings.Development.json)

```json
{
  "ConnectionStrings": {
    "AS_CMS_Connection": "Server=localhost;Database=MyApp_CMS_Dev;Trusted_Connection=true;TrustServerCertificate=true;MultipleActiveResultSets=true"
  },
  "AS_CMS_Jwt": {
    "Key": "dev-jwt-key-not-for-production-use-32-chars-minimum",
    "Issuer": "MyApp-Dev",
    "Audience": "MyApp-Dev-Users"
  }
}
```

### Production (appsettings.Production.json)

```json
{
  "ConnectionStrings": {
    "AS_CMS_Connection": "Server=prod-server;Database=MyApp_CMS_Prod;User Id=app-user;Password=secure-password;TrustServerCertificate=true;MultipleActiveResultSets=true"
  },
  "AS_CMS_Jwt": {
    "Key": "production-jwt-key-here-32-characters-minimum-secure",
    "Issuer": "MyApp-Prod",
    "Audience": "MyApp-Prod-Users"
  }
}
```

## Security Notes

1. **Never commit sensitive data** like connection strings or JWT keys to source control
2. **Use User Secrets** in development: `dotnet user-secrets set "ConnectionStrings:AS_CMS_Connection" "your-connection-string"`
3. **Use Environment Variables** in production
4. **Use strong JWT keys** with at least 32 characters
5. **Enable HTTPS** in production

## Next Steps

1. Customize the user registration/login flow as needed
2. Add additional business logic to your controllers
3. Implement role-based authorization using the built-in role system
4. Add logging and monitoring
5. Set up CI/CD pipelines 