using AS_CMS.Application.DTOs;

namespace AS_CMS.Application.Interfaces;

public interface IAuthService
{
    Task<AuthResponse> RegisterAsync(RegisterRequest request);
    Task<AuthResponse> LoginAsync(LoginRequest request);
    Task<AuthResponse> RefreshTokenAsync(string refreshToken);
    Task<UserResponse> GetUserProfileAsync(string userId);
    Task<UserResponse> UpdateProfileAsync(string userId, UpdateProfileRequest request);
    Task LogoutAsync(string userId);
} 