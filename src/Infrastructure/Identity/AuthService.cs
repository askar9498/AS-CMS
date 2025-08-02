using AS_CMS.Application.Interfaces;
using AS_CMS.Application.DTOs;
using AS_CMS.Domain.Entities;
using AS_CMS.Infrastructure.Persistence;
using AS_CMS.Shared.Exceptions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using BCrypt.Net;

namespace AS_CMS.Infrastructure.Identity;

public class AuthService : IAuthService
{
    private readonly ApplicationDbContext _context;
    private readonly IJwtTokenGenerator _jwtTokenGenerator;
    private readonly ILogger<AuthService> _logger;

    public AuthService(
        ApplicationDbContext context,
        IJwtTokenGenerator jwtTokenGenerator,
        ILogger<AuthService> logger)
    {
        _context = context;
        _jwtTokenGenerator = jwtTokenGenerator;
        _logger = logger;
    }

    public async Task<AuthResponse> RegisterAsync(RegisterRequest request)
    {
        // Check if user already exists
        var existingUser = await _context.Users
            .FirstOrDefaultAsync(u => u.Email == request.Email.ToLowerInvariant());

        if (existingUser != null)
        {
            throw new ConflictException("User with this email already exists");
        }

        // Hash password
        var passwordHash = BCrypt.Net.BCrypt.HashPassword(request.Password);

        // Create new user
        var user = User.Create(
            request.FirstName,
            request.LastName,
            request.Email,
            passwordHash);

        // Add default role (User)
        var defaultRole = await _context.Roles
            .FirstOrDefaultAsync(r => r.Name == "User");

        if (defaultRole != null)
        {
            var userRole = UserRole.Create(user.Id, defaultRole.Id);
            user.UserRoles.Add(userRole);
        }

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        // Generate tokens
        var accessToken = _jwtTokenGenerator.GenerateAccessToken(user);
        var refreshToken = _jwtTokenGenerator.GenerateRefreshToken();

        // Save refresh token
        var refreshTokenEntity = RefreshToken.Create(
            user.Id,
            refreshToken,
            DateTime.UtcNow.AddDays(7));

        _context.RefreshTokens.Add(refreshTokenEntity);
        await _context.SaveChangesAsync();

        // Update last login
        user.UpdateLastLogin();
        await _context.SaveChangesAsync();

        return new AuthResponse
        {
            AccessToken = accessToken,
            RefreshToken = refreshToken,
            ExpiresAt = DateTime.UtcNow.AddHours(1),
            User = await MapToUserResponse(user)
        };
    }

    public async Task<AuthResponse> LoginAsync(LoginRequest request)
    {
        var user = await _context.Users
            .Include(u => u.UserRoles)
            .ThenInclude(ur => ur.Role)
            .FirstOrDefaultAsync(u => u.Email == request.Email.ToLowerInvariant());

        if (user == null || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
        {
            throw new UnauthorizedException("Invalid email or password");
        }

        if (!user.IsActive)
        {
            throw new UnauthorizedException("Account is deactivated");
        }

        // Generate tokens
        var accessToken = _jwtTokenGenerator.GenerateAccessToken(user);
        var refreshToken = _jwtTokenGenerator.GenerateRefreshToken();

        // Revoke existing refresh tokens
        var existingTokens = await _context.RefreshTokens
            .Where(rt => rt.UserId == user.Id && rt.IsActive)
            .ToListAsync();

        foreach (var token in existingTokens)
        {
            token.Revoke(replacedByToken: refreshToken);
        }

        // Save new refresh token
        var refreshTokenEntity = RefreshToken.Create(
            user.Id,
            refreshToken,
            DateTime.UtcNow.AddDays(7));

        _context.RefreshTokens.Add(refreshTokenEntity);

        // Update last login
        user.UpdateLastLogin();
        await _context.SaveChangesAsync();

        return new AuthResponse
        {
            AccessToken = accessToken,
            RefreshToken = refreshToken,
            ExpiresAt = DateTime.UtcNow.AddHours(1),
            User = await MapToUserResponse(user)
        };
    }

    public async Task<AuthResponse> RefreshTokenAsync(string refreshToken)
    {
        var tokenEntity = await _context.RefreshTokens
            .Include(rt => rt.User)
            .ThenInclude(u => u.UserRoles)
            .ThenInclude(ur => ur.Role)
            .FirstOrDefaultAsync(rt => rt.Token == refreshToken);

        if (tokenEntity == null || !tokenEntity.IsActive)
        {
            throw new UnauthorizedException("Invalid refresh token");
        }

        var user = tokenEntity.User;

        if (!user.IsActive)
        {
            throw new UnauthorizedException("Account is deactivated");
        }

        // Generate new tokens
        var newAccessToken = _jwtTokenGenerator.GenerateAccessToken(user);
        var newRefreshToken = _jwtTokenGenerator.GenerateRefreshToken();

        // Revoke current token
        tokenEntity.Revoke(replacedByToken: newRefreshToken);

        // Save new refresh token
        var newRefreshTokenEntity = RefreshToken.Create(
            user.Id,
            newRefreshToken,
            DateTime.UtcNow.AddDays(7));

        _context.RefreshTokens.Add(newRefreshTokenEntity);
        await _context.SaveChangesAsync();

        return new AuthResponse
        {
            AccessToken = newAccessToken,
            RefreshToken = newRefreshToken,
            ExpiresAt = DateTime.UtcNow.AddHours(1),
            User = await MapToUserResponse(user)
        };
    }

    public async Task<UserResponse> GetUserProfileAsync(string userId)
    {
        var user = await _context.Users
            .Include(u => u.UserRoles)
            .ThenInclude(ur => ur.Role)
            .FirstOrDefaultAsync(u => u.Id.ToString() == userId);

        if (user == null)
        {
            throw new NotFoundException("User not found");
        }

        return await MapToUserResponse(user);
    }

    public async Task<UserResponse> UpdateProfileAsync(string userId, UpdateProfileRequest request)
    {
        var user = await _context.Users
            .Include(u => u.UserRoles)
            .ThenInclude(ur => ur.Role)
            .FirstOrDefaultAsync(u => u.Id.ToString() == userId);

        if (user == null)
        {
            throw new NotFoundException("User not found");
        }

        user.UpdateProfile(request.FirstName, request.LastName, request.PhoneNumber);
        await _context.SaveChangesAsync();

        return await MapToUserResponse(user);
    }

    public async Task LogoutAsync(string userId)
    {
        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Id.ToString() == userId);

        if (user == null)
        {
            throw new NotFoundException("User not found");
        }

        // Revoke all active refresh tokens for the user
        var activeTokens = await _context.RefreshTokens
            .Where(rt => rt.UserId == user.Id && rt.IsActive)
            .ToListAsync();

        foreach (var token in activeTokens)
        {
            token.Revoke(revokedBy: "logout");
        }

        await _context.SaveChangesAsync();
    }

    private async Task<UserResponse> MapToUserResponse(User user)
    {
        return new UserResponse
        {
            Id = user.Id,
            FirstName = user.FirstName,
            LastName = user.LastName,
            Email = user.Email,
            PhoneNumber = user.PhoneNumber,
            FullName = user.FullName,
            CreatedAt = user.CreatedAt,
            UpdatedAt = user.UpdatedAt,
            LastLoginAt = user.LastLoginAt,
            IsActive = user.IsActive,
            EmailConfirmed = user.EmailConfirmed,
            Roles = user.UserRoles.Select(ur => ur.Role.Name).ToList()
        };
    }
} 