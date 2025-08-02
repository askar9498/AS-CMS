# AS-CMS.Backend

A complete backend CMS system with user management, JWT authentication, role-based access control, and Entity Framework Core integration.

## 🚀 Features

- 🔐 **JWT Authentication** with refresh tokens
- 👥 **Complete User Management** (registration, login, profile management)
- 🛡️ **Role-based Access Control** (Admin, User, Moderator roles)
- 🗄️ **Entity Framework Core** with PostgreSQL
- 🔒 **Password Hashing** with BCrypt
- 📝 **Structured Logging** with Serilog
- ✅ **Validation** with FluentValidation
- 🗺️ **AutoMapper** for object mapping
- 🌐 **CORS** support
- 📚 **Swagger/OpenAPI** documentation

## 📦 Installation

### Option 1: NuGet Package (Recommended)
```bash
dotnet add package AS-CMS.Backend
```

### Option 2: Local Package
```bash
# Build the package
cd AS-CMS/backend
dotnet pack

# Install from local source
dotnet add package AS-CMS.Backend --source ./packages
```

### Option 3: Project Reference
```bash
dotnet add reference path/to/AS-CMS.Backend.csproj
```

## 🚀 Quick Start

### 1. Add to your project

```csharp
// Program.cs
using AS_CMS.Backend;

var builder = WebApplication.CreateBuilder(args);

// Add CMS services
builder.Services.AddCmsServices(builder.Configuration);

// Add authentication
builder.Services.AddCmsAuthentication(builder.Configuration);

// Add CORS
builder.Services.AddCmsCors();

var app = builder.Build();

// Use CMS middleware
app.UseCmsAuthentication();
app.UseCmsCors();
```

### 2. Configure database and JWT

```json
// appsettings.json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Database=myapp;Username=postgres;Password=password"
  },
  "Jwt": {
    "Key": "your-super-secret-key-with-at-least-32-characters",
    "Issuer": "MyApp",
    "Audience": "MyApp-Users",
    "ExpiryMinutes": 60,
    "RefreshTokenExpiryDays": 7
  },
  "Serilog": {
    "MinimumLevel": {
      "Default": "Information",
      "Override": {
        "Microsoft": "Warning"
      }
    }
  }
}
```

### 3. Use in controllers

```csharp
[ApiController]
[Route("api/[controller]")]
public class UserController : ControllerBase
{
    private readonly IAuthService _authService;

    public UserController(IAuthService authService)
    {
        _authService = authService;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register(RegisterRequest request)
    {
        var result = await _authService.RegisterAsync(request);
        return Ok(ApiResponse<AuthResponse>.SuccessResponse(result));
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginRequest request)
    {
        var result = await _authService.LoginAsync(request);
        return Ok(ApiResponse<AuthResponse>.SuccessResponse(result));
    }

    [HttpGet("profile")]
    [Authorize]
    public async Task<IActionResult> GetProfile()
    {
        var userId = User.FindFirst("sub")?.Value;
        var result = await _authService.GetUserProfileAsync(userId);
        return Ok(ApiResponse<UserResponse>.SuccessResponse(result));
    }
}
```

## 📚 Available Services

### Core Services
- `IAuthService` - User authentication and management
- `IJwtTokenGenerator` - JWT token generation
- `ApplicationDbContext` - Entity Framework context

### Extension Methods
- `AddCmsServices()` - Register all CMS services
- `AddCmsAuthentication()` - Configure JWT authentication
- `AddCmsCors()` - Configure CORS policy
- `UseCmsAuthentication()` - Use authentication middleware
- `UseCmsCors()` - Use CORS middleware

## 📋 Available DTOs

### Authentication
- `RegisterRequest` - User registration
- `LoginRequest` - User login
- `AuthResponse` - Authentication response
- `RefreshTokenRequest` - Token refresh

### User Management
- `UserResponse` - User profile
- `UpdateProfileRequest` - Profile updates

## 🗄️ Available Entities

### Core Entities
- `User` - User entity with domain logic
- `Role` - Role entity for access control
- `UserRole` - Many-to-many relationship
- `RefreshToken` - JWT refresh token management

### Enums
- `UserStatus` - User status enumeration

## 🔧 Configuration Options

### Database Configuration
```csharp
builder.Services.AddCmsServices(builder.Configuration, options =>
{
    options.UsePostgreSQL = true; // or false for SQL Server
    options.AutoMigrate = true;   // auto-create database
});
```

### JWT Configuration
```csharp
builder.Services.AddCmsAuthentication(builder.Configuration, options =>
{
    options.ValidateIssuer = true;
    options.ValidateAudience = true;
    options.ValidateLifetime = true;
});
```

### CORS Configuration
```csharp
builder.Services.AddCmsCors(options =>
{
    options.AllowAllOrigins = true; // or configure specific origins
    options.AllowCredentials = true;
});
```

## 🛡️ Security Features

- **JWT Tokens**: Secure access tokens with refresh mechanism
- **Password Hashing**: BCrypt for secure password storage
- **Role-based Access**: Fine-grained permission control
- **CORS Protection**: Configurable cross-origin policies
- **Input Validation**: FluentValidation for all inputs
- **Structured Logging**: Comprehensive audit trail

## 📊 Database Schema

The package automatically creates the following tables:
- `Users` - User accounts and profiles
- `Roles` - System roles (Admin, User, Moderator)
- `UserRoles` - Many-to-many user-role relationships
- `RefreshTokens` - JWT refresh token storage

## 🔄 Migration Support

```bash
# Create migration
dotnet ef migrations add InitialCreate --project AS-CMS.Backend

# Update database
dotnet ef database update --project AS-CMS.Backend
```

## 📝 Logging

The package uses Serilog for structured logging:
- Console output for development
- File output for production
- Configurable log levels
- Request/response logging

## 🧪 Testing

The package includes comprehensive test coverage:
- Unit tests for all services
- Integration tests for API endpoints
- Mock implementations for testing

## 📈 Performance

- **Async/Await**: All operations are asynchronous
- **Connection Pooling**: Optimized database connections
- **Caching**: Built-in caching for frequently accessed data
- **Compression**: Response compression support

## 🔄 Version History

### v1.0.0
- Initial release
- Complete user management system
- JWT authentication
- Role-based access control
- Entity Framework Core integration

## 📄 License

MIT License - see LICENSE file for details

## 🤝 Contributing

1. Fork the repository
2. Create a feature branch
3. Make your changes
4. Add tests
5. Submit a pull request

## 📞 Support

- 📧 Email: support@as-cms.com
- 📖 Documentation: https://docs.as-cms.com
- 🐛 Issues: https://github.com/as-cms/backend/issues

---

**AS-CMS.Backend** - The complete backend solution for your applications! 🚀 