using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using AS_CMS.Application.DTOs;
using AS_CMS.Application.Interfaces;
using AS_CMS.Shared.Responses;

namespace AS_CMS.Api.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
public class UserController : ControllerBase
{
    private readonly IAuthService _authService;
    private readonly ILogger<UserController> _logger;

    public UserController(IAuthService authService, ILogger<UserController> logger)
    {
        _authService = authService;
        _logger = logger;
    }

    /// <summary>
    /// Register a new user
    /// </summary>
    /// <param name="request">Registration request</param>
    /// <returns>Authentication result</returns>
    [HttpPost("register")]
    [ProducesResponseType(typeof(ApiResponse<AuthResponse>), 200)]
    [ProducesResponseType(typeof(ApiResponse<object>), 400)]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request)
    {
        try
        {
            _logger.LogInformation("User registration attempt for email: {Email}", request.Email);
            
            var result = await _authService.RegisterAsync(request);
            
            _logger.LogInformation("User registered successfully: {Email}", request.Email);
            return Ok(ApiResponse<AuthResponse>.SuccessResponse(result));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during user registration for email: {Email}", request.Email);
            return BadRequest(ApiResponse<object>.Error("Registration failed"));
        }
    }

    /// <summary>
    /// Authenticate user and get JWT token
    /// </summary>
    /// <param name="request">Login request</param>
    /// <returns>Authentication result</returns>
    [HttpPost("login")]
    [ProducesResponseType(typeof(ApiResponse<AuthResponse>), 200)]
    [ProducesResponseType(typeof(ApiResponse<object>), 401)]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        try
        {
            _logger.LogInformation("Login attempt for user: {Email}", request.Email);
            
            var result = await _authService.LoginAsync(request);
            
            _logger.LogInformation("User logged in successfully: {Email}", request.Email);
            return Ok(ApiResponse<AuthResponse>.SuccessResponse(result));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Login failed for user: {Email}", request.Email);
            return Unauthorized(ApiResponse<object>.Error("Invalid credentials"));
        }
    }

    /// <summary>
    /// Refresh JWT token
    /// </summary>
    /// <param name="request">Token refresh request</param>
    /// <returns>New authentication result</returns>
    [HttpPost("refresh-token")]
    [ProducesResponseType(typeof(ApiResponse<AuthResponse>), 200)]
    [ProducesResponseType(typeof(ApiResponse<object>), 401)]
    public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenRequest request)
    {
        try
        {
            _logger.LogInformation("Token refresh attempt");
            
            var result = await _authService.RefreshTokenAsync(request.RefreshToken);
            
            _logger.LogInformation("Token refreshed successfully");
            return Ok(ApiResponse<AuthResponse>.SuccessResponse(result));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Token refresh failed");
            return Unauthorized(ApiResponse<object>.Error("Invalid refresh token"));
        }
    }

    /// <summary>
    /// Get current user profile
    /// </summary>
    /// <returns>User profile information</returns>
    [HttpGet("profile")]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponse<UserResponse>), 200)]
    [ProducesResponseType(typeof(ApiResponse<object>), 401)]
    public async Task<IActionResult> GetProfile()
    {
        try
        {
            var userId = User.FindFirst("sub")?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized(ApiResponse<object>.Error("Invalid token"));
            }

            _logger.LogInformation("Profile request for user: {UserId}", userId);
            
            var result = await _authService.GetUserProfileAsync(userId);
            
            return Ok(ApiResponse<UserResponse>.SuccessResponse(result));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving user profile");
            return BadRequest(ApiResponse<object>.Error("Failed to retrieve profile"));
        }
    }

    /// <summary>
    /// Update user profile
    /// </summary>
    /// <param name="request">Profile update request</param>
    /// <returns>Updated user profile</returns>
    [HttpPut("profile")]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponse<UserResponse>), 200)]
    [ProducesResponseType(typeof(ApiResponse<object>), 400)]
    public async Task<IActionResult> UpdateProfile([FromBody] UpdateProfileRequest request)
    {
        try
        {
            var userId = User.FindFirst("sub")?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized(ApiResponse<object>.Error("Invalid token"));
            }

            _logger.LogInformation("Profile update request for user: {UserId}", userId);
            
            var result = await _authService.UpdateProfileAsync(userId, request);
            
            _logger.LogInformation("Profile updated successfully for user: {UserId}", userId);
            return Ok(ApiResponse<UserResponse>.SuccessResponse(result));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating user profile");
            return BadRequest(ApiResponse<object>.Error("Failed to update profile"));
        }
    }

    /// <summary>
    /// Logout user (revoke refresh token)
    /// </summary>
    /// <returns>Success response</returns>
    [HttpPost("logout")]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponse<object>), 200)]
    public async Task<IActionResult> Logout()
    {
        try
        {
            var userId = User.FindFirst("sub")?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized(ApiResponse<object>.Error("Invalid token"));
            }

            _logger.LogInformation("Logout request for user: {UserId}", userId);
            
            await _authService.LogoutAsync(userId);
            
            _logger.LogInformation("User logged out successfully: {UserId}", userId);
            return Ok(ApiResponse<object>.SuccessResponse("Logged out successfully"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during logout");
            return BadRequest(ApiResponse<object>.Error("Logout failed"));
        }
    }
} 