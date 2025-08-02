using AS_CMS.Application.DTOs;
using AS_CMS.Application.Interfaces;
using AS_CMS.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Serilog;

namespace AS_CMS.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class UserController : ControllerBase
{
    private readonly IUserService _userService;
    private readonly ILogger<UserController> _logger;

    public UserController(IUserService userService, ILogger<UserController> logger)
    {
        _userService = userService;
        _logger = logger;
    }

    #region Authentication

    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<ActionResult<ApiResponseDto<LoginResponseDto>>> Login([FromBody] LoginRequestDto request)
    {
        try
        {
            _logger.LogInformation("Login attempt for user: {Email}", request.Email);
            
            var result = await _userService.AuthenticateAsync(request);
            
            _logger.LogInformation("User {Email} logged in successfully", request.Email);
            
            return Ok(new ApiResponseDto<LoginResponseDto>
            {
                Success = true,
                Message = "Login successful",
                Data = result
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Login failed for user: {Email}", request.Email);
            return BadRequest(new ApiResponseDto<LoginResponseDto>
            {
                Success = false,
                Message = "Invalid email or password",
                Errors = new List<string> { ex.Message }
            });
        }
    }

    [HttpPost("refresh-token")]
    [AllowAnonymous]
    public async Task<ActionResult<ApiResponseDto<LoginResponseDto>>> RefreshToken([FromBody] string refreshToken)
    {
        try
        {
            var result = await _userService.RefreshTokenAsync(refreshToken);
            
            return Ok(new ApiResponseDto<LoginResponseDto>
            {
                Success = true,
                Message = "Token refreshed successfully",
                Data = result
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Token refresh failed");
            return BadRequest(new ApiResponseDto<LoginResponseDto>
            {
                Success = false,
                Message = "Invalid refresh token",
                Errors = new List<string> { ex.Message }
            });
        }
    }

    [HttpPost("revoke-token")]
    public async Task<ActionResult<ApiResponseDto<object>>> RevokeToken([FromBody] string refreshToken)
    {
        try
        {
            await _userService.RevokeTokenAsync(refreshToken);
            
            return Ok(new ApiResponseDto<object>
            {
                Success = true,
                Message = "Token revoked successfully"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Token revocation failed");
            return BadRequest(new ApiResponseDto<object>
            {
                Success = false,
                Message = "Failed to revoke token",
                Errors = new List<string> { ex.Message }
            });
        }
    }

    #endregion

    #region User Management

    [HttpGet]
    [Authorize(Policy = "RequireGetUsersPermission")]
    public async Task<ActionResult<ApiResponseDto<PagedResponseDto<UserResponse>>>> GetUsers([FromQuery] UserFilterDto filter)
    {
        try
        {
            var users = await _userService.GetUsersAsync(filter);
            
            return Ok(new ApiResponseDto<PagedResponseDto<UserResponse>>
            {
                Success = true,
                Message = "Users retrieved successfully",
                Data = users
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve users");
            return BadRequest(new ApiResponseDto<PagedResponseDto<UserResponse>>
            {
                Success = false,
                Message = "Failed to retrieve users",
                Errors = new List<string> { ex.Message }
            });
        }
    }

    [HttpGet("{userId:guid}")]
    [Authorize(Policy = "RequireGetUserPermission")]
    public async Task<ActionResult<ApiResponseDto<UserResponse>>> GetUser(Guid userId)
    {
        try
        {
            var user = await _userService.GetUserAsync(userId);
            
            if (user == null)
            {
                return NotFound(new ApiResponseDto<UserResponse>
                {
                    Success = false,
                    Message = "User not found"
                });
            }
            
            return Ok(new ApiResponseDto<UserResponse>
            {
                Success = true,
                Message = "User retrieved successfully",
                Data = user
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve user: {UserId}", userId);
            return BadRequest(new ApiResponseDto<UserResponse>
            {
                Success = false,
                Message = "Failed to retrieve user",
                Errors = new List<string> { ex.Message }
            });
        }
    }

    [HttpGet("by-email")]
    [Authorize(Policy = "RequireGetUserByEmailPermission")]
    public async Task<ActionResult<ApiResponseDto<UserResponse>>> GetUserByEmail([FromQuery] string email)
    {
        try
        {
            var user = await _userService.GetUserByEmailAsync(email);
            
            if (user == null)
            {
                return NotFound(new ApiResponseDto<UserResponse>
                {
                    Success = false,
                    Message = "User not found"
                });
            }
            
            return Ok(new ApiResponseDto<UserResponse>
            {
                Success = true,
                Message = "User retrieved successfully",
                Data = user
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve user by email: {Email}", email);
            return BadRequest(new ApiResponseDto<UserResponse>
            {
                Success = false,
                Message = "Failed to retrieve user",
                Errors = new List<string> { ex.Message }
            });
        }
    }

    [HttpPost("search")]
    [Authorize(Policy = "RequireSearchUserPermission")]
    public async Task<ActionResult<ApiResponseDto<List<UserResponse>>>> SearchUsers([FromBody] SearchUserDto request)
    {
        try
        {
            var users = await _userService.SearchUsersAsync(request.Text);
            
            return Ok(new ApiResponseDto<List<UserResponse>>
            {
                Success = true,
                Message = "Users search completed successfully",
                Data = users
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to search users");
            return BadRequest(new ApiResponseDto<List<UserResponse>>
            {
                Success = false,
                Message = "Failed to search users",
                Errors = new List<string> { ex.Message }
            });
        }
    }

    [HttpPost("register/individual")]
    [AllowAnonymous]
    public async Task<ActionResult<ApiResponseDto<UserResponse>>> RegisterIndividual([FromForm] IndividualUserRegistrationDto request)
    {
        try
        {
            _logger.LogInformation("Individual user registration attempt: {Email}", request.Email);
            
            var user = await _userService.CreateIndividualUserAsync(request);
            
            _logger.LogInformation("Individual user registered successfully: {Email}", request.Email);
            
            return Ok(new ApiResponseDto<UserResponse>
            {
                Success = true,
                Message = "Individual user registered successfully",
                Data = user
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Individual user registration failed: {Email}", request.Email);
            return BadRequest(new ApiResponseDto<UserResponse>
            {
                Success = false,
                Message = "Failed to register individual user",
                Errors = new List<string> { ex.Message }
            });
        }
    }

    [HttpPost("register/corporate")]
    [AllowAnonymous]
    public async Task<ActionResult<ApiResponseDto<UserResponse>>> RegisterCorporate([FromForm] CorporateUserRegistrationDto request)
    {
        try
        {
            _logger.LogInformation("Corporate user registration attempt: {Email}", request.Email);
            
            var user = await _userService.CreateCorporateUserAsync(request);
            
            _logger.LogInformation("Corporate user registered successfully: {Email}", request.Email);
            
            return Ok(new ApiResponseDto<UserResponse>
            {
                Success = true,
                Message = "Corporate user registered successfully",
                Data = user
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Corporate user registration failed: {Email}", request.Email);
            return BadRequest(new ApiResponseDto<UserResponse>
            {
                Success = false,
                Message = "Failed to register corporate user",
                Errors = new List<string> { ex.Message }
            });
        }
    }

    [HttpPut("{userId:guid}")]
    [Authorize(Policy = "RequireUpdateUserPermission")]
    public async Task<ActionResult<ApiResponseDto<UserResponse>>> UpdateUser(Guid userId, [FromForm] UpdateUserDto request)
    {
        try
        {
            var user = await _userService.UpdateUserAsync(userId, request);
            
            return Ok(new ApiResponseDto<UserResponse>
            {
                Success = true,
                Message = "User updated successfully",
                Data = user
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to update user: {UserId}", userId);
            return BadRequest(new ApiResponseDto<UserResponse>
            {
                Success = false,
                Message = "Failed to update user",
                Errors = new List<string> { ex.Message }
            });
        }
    }

    [HttpDelete("{userId:guid}")]
    [Authorize(Policy = "RequireDeleteUserPermission")]
    public async Task<ActionResult<ApiResponseDto<object>>> DeleteUser(Guid userId)
    {
        try
        {
            await _userService.DeleteUserAsync(userId);
            
            return Ok(new ApiResponseDto<object>
            {
                Success = true,
                Message = "User deleted successfully"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to delete user: {UserId}", userId);
            return BadRequest(new ApiResponseDto<object>
            {
                Success = false,
                Message = "Failed to delete user",
                Errors = new List<string> { ex.Message }
            });
        }
    }

    #endregion

    #region Profile Completion

    [HttpPost("complete-profile/individual")]
    public async Task<ActionResult<ApiResponseDto<UserResponse>>> CompleteIndividualProfile([FromForm] IndividualProfileCompletionDto request)
    {
        try
        {
            var userId = GetCurrentUserId();
            var user = await _userService.CompleteIndividualProfileAsync(userId, request);
            
            return Ok(new ApiResponseDto<UserResponse>
            {
                Success = true,
                Message = "Individual profile completed successfully",
                Data = user
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to complete individual profile");
            return BadRequest(new ApiResponseDto<UserResponse>
            {
                Success = false,
                Message = "Failed to complete individual profile",
                Errors = new List<string> { ex.Message }
            });
        }
    }

    [HttpPost("complete-profile/corporate")]
    public async Task<ActionResult<ApiResponseDto<UserResponse>>> CompleteCorporateProfile([FromForm] CorporateProfileCompletionDto request)
    {
        try
        {
            var userId = GetCurrentUserId();
            var user = await _userService.CompleteCorporateProfileAsync(userId, request);
            
            return Ok(new ApiResponseDto<UserResponse>
            {
                Success = true,
                Message = "Corporate profile completed successfully",
                Data = user
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to complete corporate profile");
            return BadRequest(new ApiResponseDto<UserResponse>
            {
                Success = false,
                Message = "Failed to complete corporate profile",
                Errors = new List<string> { ex.Message }
            });
        }
    }

    #endregion

    #region Password Management

    [HttpPost("reset-password/{userId:guid}")]
    [Authorize(Policy = "RequireResetPasswordPermission")]
    public async Task<ActionResult<ApiResponseDto<object>>> ResetPassword(Guid userId)
    {
        try
        {
            await _userService.ResetPasswordAsync(userId);
            
            return Ok(new ApiResponseDto<object>
            {
                Success = true,
                Message = "Password reset successfully"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to reset password for user: {UserId}", userId);
            return BadRequest(new ApiResponseDto<object>
            {
                Success = false,
                Message = "Failed to reset password",
                Errors = new List<string> { ex.Message }
            });
        }
    }

    [HttpPost("change-password")]
    public async Task<ActionResult<ApiResponseDto<object>>> ChangePassword([FromBody] ChangePasswordRequestDto request)
    {
        try
        {
            var userId = GetCurrentUserId();
            await _userService.ChangePasswordAsync(userId, request.CurrentPassword, request.NewPassword);
            
            return Ok(new ApiResponseDto<object>
            {
                Success = true,
                Message = "Password changed successfully"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to change password");
            return BadRequest(new ApiResponseDto<object>
            {
                Success = false,
                Message = "Failed to change password",
                Errors = new List<string> { ex.Message }
            });
        }
    }

    #endregion

    #region User Status Management

    [HttpPost("set-accuracy")]
    [Authorize(Policy = "RequireSetAccuracyPermission")]
    public async Task<ActionResult<ApiResponseDto<object>>> SetUserAccuracy([FromBody] SetUserAccuracyDto request)
    {
        try
        {
            await _userService.SetUserAccuracyAsync(request.Email, request.IsActive);
            
            return Ok(new ApiResponseDto<object>
            {
                Success = true,
                Message = "User accuracy updated successfully"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to set user accuracy: {Email}", request.Email);
            return BadRequest(new ApiResponseDto<object>
            {
                Success = false,
                Message = "Failed to update user accuracy",
                Errors = new List<string> { ex.Message }
            });
        }
    }

    [HttpPost("activate/{userId:guid}")]
    [Authorize(Policy = "RequireUpdateUserPermission")]
    public async Task<ActionResult<ApiResponseDto<object>>> ActivateUser(Guid userId)
    {
        try
        {
            await _userService.ActivateUserAsync(userId);
            
            return Ok(new ApiResponseDto<object>
            {
                Success = true,
                Message = "User activated successfully"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to activate user: {UserId}", userId);
            return BadRequest(new ApiResponseDto<object>
            {
                Success = false,
                Message = "Failed to activate user",
                Errors = new List<string> { ex.Message }
            });
        }
    }

    [HttpPost("deactivate/{userId:guid}")]
    [Authorize(Policy = "RequireUpdateUserPermission")]
    public async Task<ActionResult<ApiResponseDto<object>>> DeactivateUser(Guid userId)
    {
        try
        {
            await _userService.DeactivateUserAsync(userId);
            
            return Ok(new ApiResponseDto<object>
            {
                Success = true,
                Message = "User deactivated successfully"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to deactivate user: {UserId}", userId);
            return BadRequest(new ApiResponseDto<object>
            {
                Success = false,
                Message = "Failed to deactivate user",
                Errors = new List<string> { ex.Message }
            });
        }
    }

    #endregion

    #region Role Management

    [HttpGet("roles")]
    [Authorize(Policy = "RequireGetRolesPermission")]
    public async Task<ActionResult<ApiResponseDto<List<UserGroupDto>>>> GetRoles()
    {
        try
        {
            var roles = await _userService.GetRolesAsync();
            
            return Ok(new ApiResponseDto<List<UserGroupDto>>
            {
                Success = true,
                Message = "Roles retrieved successfully",
                Data = roles
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve roles");
            return BadRequest(new ApiResponseDto<List<UserGroupDto>>
            {
                Success = false,
                Message = "Failed to retrieve roles",
                Errors = new List<string> { ex.Message }
            });
        }
    }

    [HttpPost("roles")]
    [Authorize(Policy = "RequireAddRolePermission")]
    public async Task<ActionResult<ApiResponseDto<UserGroupDto>>> CreateRole([FromBody] CreateRoleDto request)
    {
        try
        {
            var role = await _userService.CreateRoleAsync(request);
            
            return Ok(new ApiResponseDto<UserGroupDto>
            {
                Success = true,
                Message = "Role created successfully",
                Data = role
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create role");
            return BadRequest(new ApiResponseDto<UserGroupDto>
            {
                Success = false,
                Message = "Failed to create role",
                Errors = new List<string> { ex.Message }
            });
        }
    }

    [HttpPut("roles/{roleId:guid}")]
    [Authorize(Policy = "RequireUpdateRolePermission")]
    public async Task<ActionResult<ApiResponseDto<UserGroupDto>>> UpdateRole(Guid roleId, [FromBody] UpdateRoleDto request)
    {
        try
        {
            var role = await _userService.UpdateRoleAsync(roleId, request);
            
            return Ok(new ApiResponseDto<UserGroupDto>
            {
                Success = true,
                Message = "Role updated successfully",
                Data = role
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to update role: {RoleId}", roleId);
            return BadRequest(new ApiResponseDto<UserGroupDto>
            {
                Success = false,
                Message = "Failed to update role",
                Errors = new List<string> { ex.Message }
            });
        }
    }

    [HttpDelete("roles/{roleId:guid}")]
    [Authorize(Policy = "RequireDeleteRolePermission")]
    public async Task<ActionResult<ApiResponseDto<object>>> DeleteRole(Guid roleId)
    {
        try
        {
            await _userService.DeleteRoleAsync(roleId);
            
            return Ok(new ApiResponseDto<object>
            {
                Success = true,
                Message = "Role deleted successfully"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to delete role: {RoleId}", roleId);
            return BadRequest(new ApiResponseDto<object>
            {
                Success = false,
                Message = "Failed to delete role",
                Errors = new List<string> { ex.Message }
            });
        }
    }

    [HttpPost("set-role")]
    [Authorize(Policy = "RequireSetRolePermission")]
    public async Task<ActionResult<ApiResponseDto<object>>> SetUserRole([FromBody] SetUserRoleDto request)
    {
        try
        {
            await _userService.SetUserRoleAsync(request.UserId, request.UserGroupId);
            
            return Ok(new ApiResponseDto<object>
            {
                Success = true,
                Message = "User role set successfully"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to set user role");
            return BadRequest(new ApiResponseDto<object>
            {
                Success = false,
                Message = "Failed to set user role",
                Errors = new List<string> { ex.Message }
            });
        }
    }

    #endregion

    #region Permission Management

    [HttpGet("permissions")]
    [Authorize(Policy = "RequireGetPermissionsPermission")]
    public async Task<ActionResult<ApiResponseDto<List<PermissionDto>>>> GetPermissions()
    {
        try
        {
            var permissions = await _userService.GetPermissionsAsync();
            
            return Ok(new ApiResponseDto<List<PermissionDto>>
            {
                Success = true,
                Message = "Permissions retrieved successfully",
                Data = permissions
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve permissions");
            return BadRequest(new ApiResponseDto<List<PermissionDto>>
            {
                Success = false,
                Message = "Failed to retrieve permissions",
                Errors = new List<string> { ex.Message }
            });
        }
    }

    [HttpGet("permissions/{userId:guid}")]
    [Authorize(Policy = "RequireGetUserPermissionsPermission")]
    public async Task<ActionResult<ApiResponseDto<List<PermissionDto>>>> GetUserPermissions(Guid userId)
    {
        try
        {
            var permissions = await _userService.GetUserPermissionsAsync(userId);
            
            return Ok(new ApiResponseDto<List<PermissionDto>>
            {
                Success = true,
                Message = "User permissions retrieved successfully",
                Data = permissions
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve user permissions: {UserId}", userId);
            return BadRequest(new ApiResponseDto<List<PermissionDto>>
            {
                Success = false,
                Message = "Failed to retrieve user permissions",
                Errors = new List<string> { ex.Message }
            });
        }
    }

    [HttpPost("permissions/{userId:guid}")]
    [Authorize(Policy = "RequireSetUserPermissionsPermission")]
    public async Task<ActionResult<ApiResponseDto<object>>> SetUserPermissions(Guid userId, [FromBody] List<PermissionEnums> permissions)
    {
        try
        {
            await _userService.SetUserPermissionsAsync(userId, permissions);
            
            return Ok(new ApiResponseDto<object>
            {
                Success = true,
                Message = "User permissions set successfully"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to set user permissions: {UserId}", userId);
            return BadRequest(new ApiResponseDto<object>
            {
                Success = false,
                Message = "Failed to set user permissions",
                Errors = new List<string> { ex.Message }
            });
        }
    }

    [HttpPost("roles/{roleId:guid}/permissions")]
    [Authorize(Policy = "RequireSetRolePermissionsPermission")]
    public async Task<ActionResult<ApiResponseDto<object>>> SetRolePermissions(Guid roleId, [FromBody] List<PermissionEnums> permissions)
    {
        try
        {
            await _userService.SetRolePermissionsAsync(roleId, permissions);
            
            return Ok(new ApiResponseDto<object>
            {
                Success = true,
                Message = "Role permissions set successfully"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to set role permissions: {RoleId}", roleId);
            return BadRequest(new ApiResponseDto<object>
            {
                Success = false,
                Message = "Failed to set role permissions",
                Errors = new List<string> { ex.Message }
            });
        }
    }

    #endregion

    #region Login Logs

    [HttpGet("login-logs/{userId:guid}")]
    [Authorize(Policy = "RequireGetUserLoginLogsPermission")]
    public async Task<ActionResult<ApiResponseDto<List<UserLoginLogDto>>>> GetUserLoginLogs(Guid userId, [FromQuery] int count = 10)
    {
        try
        {
            var logs = await _userService.GetUserLoginLogsAsync(userId, count);
            
            return Ok(new ApiResponseDto<List<UserLoginLogDto>>
            {
                Success = true,
                Message = "User login logs retrieved successfully",
                Data = logs
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve user login logs: {UserId}", userId);
            return BadRequest(new ApiResponseDto<List<UserLoginLogDto>>
            {
                Success = false,
                Message = "Failed to retrieve user login logs",
                Errors = new List<string> { ex.Message }
            });
        }
    }

    #endregion

    #region File Upload

    [HttpPost("upload/profile-image")]
    public async Task<ActionResult<ApiResponseDto<FileUploadResponseDto>>> UploadProfileImage(IFormFile file)
    {
        try
        {
            var userId = GetCurrentUserId();
            var result = await _userService.UploadProfileImageAsync(userId, file);
            
            return Ok(new ApiResponseDto<FileUploadResponseDto>
            {
                Success = true,
                Message = "Profile image uploaded successfully",
                Data = result
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to upload profile image");
            return BadRequest(new ApiResponseDto<FileUploadResponseDto>
            {
                Success = false,
                Message = "Failed to upload profile image",
                Errors = new List<string> { ex.Message }
            });
        }
    }

    [HttpPost("upload/resume")]
    public async Task<ActionResult<ApiResponseDto<FileUploadResponseDto>>> UploadResume(IFormFile file)
    {
        try
        {
            var userId = GetCurrentUserId();
            var result = await _userService.UploadResumeAsync(userId, file);
            
            return Ok(new ApiResponseDto<FileUploadResponseDto>
            {
                Success = true,
                Message = "Resume uploaded successfully",
                Data = result
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to upload resume");
            return BadRequest(new ApiResponseDto<FileUploadResponseDto>
            {
                Success = false,
                Message = "Failed to upload resume",
                Errors = new List<string> { ex.Message }
            });
        }
    }

    [HttpPost("upload/logo")]
    public async Task<ActionResult<ApiResponseDto<FileUploadResponseDto>>> UploadLogo(IFormFile file)
    {
        try
        {
            var userId = GetCurrentUserId();
            var result = await _userService.UploadLogoAsync(userId, file);
            
            return Ok(new ApiResponseDto<FileUploadResponseDto>
            {
                Success = true,
                Message = "Logo uploaded successfully",
                Data = result
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to upload logo");
            return BadRequest(new ApiResponseDto<FileUploadResponseDto>
            {
                Success = false,
                Message = "Failed to upload logo",
                Errors = new List<string> { ex.Message }
            });
        }
    }

    [HttpPost("upload/official-documents")]
    public async Task<ActionResult<ApiResponseDto<FileUploadResponseDto>>> UploadOfficialDocuments(IFormFile file)
    {
        try
        {
            var userId = GetCurrentUserId();
            var result = await _userService.UploadOfficialDocumentsAsync(userId, file);
            
            return Ok(new ApiResponseDto<FileUploadResponseDto>
            {
                Success = true,
                Message = "Official documents uploaded successfully",
                Data = result
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to upload official documents");
            return BadRequest(new ApiResponseDto<FileUploadResponseDto>
            {
                Success = false,
                Message = "Failed to upload official documents",
                Errors = new List<string> { ex.Message }
            });
        }
    }

    #endregion

    #region Profile Management

    [HttpGet("profile")]
    public async Task<ActionResult<ApiResponseDto<UserResponse>>> GetProfile()
    {
        try
        {
            var userId = GetCurrentUserId();
            var user = await _userService.GetUserAsync(userId);
            
            if (user == null)
            {
                return NotFound(new ApiResponseDto<UserResponse>
                {
                    Success = false,
                    Message = "Profile not found"
                });
            }
            
            return Ok(new ApiResponseDto<UserResponse>
            {
                Success = true,
                Message = "Profile retrieved successfully",
                Data = user
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve profile");
            return BadRequest(new ApiResponseDto<UserResponse>
            {
                Success = false,
                Message = "Failed to retrieve profile",
                Errors = new List<string> { ex.Message }
            });
        }
    }

    [HttpPost("public-profile/{userId:guid}")]
    [AllowAnonymous]
    public async Task<ActionResult<ApiResponseDto<UserResponse>>> GetPublicProfile(Guid userId)
    {
        try
        {
            var user = await _userService.GetPublicProfileAsync(userId);
            
            if (user == null)
            {
                return NotFound(new ApiResponseDto<UserResponse>
                {
                    Success = false,
                    Message = "Public profile not found"
                });
            }
            
            return Ok(new ApiResponseDto<UserResponse>
            {
                Success = true,
                Message = "Public profile retrieved successfully",
                Data = user
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve public profile: {UserId}", userId);
            return BadRequest(new ApiResponseDto<UserResponse>
            {
                Success = false,
                Message = "Failed to retrieve public profile",
                Errors = new List<string> { ex.Message }
            });
        }
    }

    [HttpPost("set-public-profile")]
    public async Task<ActionResult<ApiResponseDto<object>>> SetPublicProfileVisibility([FromBody] bool showPublic)
    {
        try
        {
            var userId = GetCurrentUserId();
            await _userService.SetPublicProfileVisibilityAsync(userId, showPublic);
            
            return Ok(new ApiResponseDto<object>
            {
                Success = true,
                Message = "Public profile visibility updated successfully"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to set public profile visibility");
            return BadRequest(new ApiResponseDto<object>
            {
                Success = false,
                Message = "Failed to update public profile visibility",
                Errors = new List<string> { ex.Message }
            });
        }
    }

    #endregion

    #region Two-Factor Authentication

    [HttpPost("enable-2fa")]
    public async Task<ActionResult<ApiResponseDto<object>>> EnableTwoFactor()
    {
        try
        {
            var userId = GetCurrentUserId();
            await _userService.EnableTwoFactorAsync(userId);
            
            return Ok(new ApiResponseDto<object>
            {
                Success = true,
                Message = "Two-factor authentication enabled successfully"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to enable two-factor authentication");
            return BadRequest(new ApiResponseDto<object>
            {
                Success = false,
                Message = "Failed to enable two-factor authentication",
                Errors = new List<string> { ex.Message }
            });
        }
    }

    [HttpPost("disable-2fa")]
    public async Task<ActionResult<ApiResponseDto<object>>> DisableTwoFactor()
    {
        try
        {
            var userId = GetCurrentUserId();
            await _userService.DisableTwoFactorAsync(userId);
            
            return Ok(new ApiResponseDto<object>
            {
                Success = true,
                Message = "Two-factor authentication disabled successfully"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to disable two-factor authentication");
            return BadRequest(new ApiResponseDto<object>
            {
                Success = false,
                Message = "Failed to disable two-factor authentication",
                Errors = new List<string> { ex.Message }
            });
        }
    }

    [HttpPost("validate-2fa")]
    public async Task<ActionResult<ApiResponseDto<bool>>> ValidateTwoFactorCode([FromBody] string code)
    {
        try
        {
            var userId = GetCurrentUserId();
            var isValid = await _userService.ValidateTwoFactorCodeAsync(userId, code);
            
            return Ok(new ApiResponseDto<bool>
            {
                Success = true,
                Message = "Two-factor code validation completed",
                Data = isValid
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to validate two-factor code");
            return BadRequest(new ApiResponseDto<bool>
            {
                Success = false,
                Message = "Failed to validate two-factor code",
                Errors = new List<string> { ex.Message }
            });
        }
    }

    #endregion

    #region Statistics

    [HttpGet("statistics")]
    [Authorize(Policy = "RequireAdminPermission")]
    public async Task<ActionResult<ApiResponseDto<Dictionary<string, int>>>> GetUserStatistics()
    {
        try
        {
            var statistics = await _userService.GetUserStatisticsAsync();
            
            return Ok(new ApiResponseDto<Dictionary<string, int>>
            {
                Success = true,
                Message = "User statistics retrieved successfully",
                Data = statistics
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve user statistics");
            return BadRequest(new ApiResponseDto<Dictionary<string, int>>
            {
                Success = false,
                Message = "Failed to retrieve user statistics",
                Errors = new List<string> { ex.Message }
            });
        }
    }

    #endregion

    #region Validation

    [HttpGet("validate/email")]
    [AllowAnonymous]
    public async Task<ActionResult<ApiResponseDto<bool>>> ValidateEmail([FromQuery] string email)
    {
        try
        {
            var isUnique = await _userService.IsEmailUniqueAsync(email);
            
            return Ok(new ApiResponseDto<bool>
            {
                Success = true,
                Message = "Email validation completed",
                Data = isUnique
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to validate email: {Email}", email);
            return BadRequest(new ApiResponseDto<bool>
            {
                Success = false,
                Message = "Failed to validate email",
                Errors = new List<string> { ex.Message }
            });
        }
    }

    [HttpGet("validate/national-code")]
    [AllowAnonymous]
    public async Task<ActionResult<ApiResponseDto<bool>>> ValidateNationalCode([FromQuery] string nationalCode)
    {
        try
        {
            var isUnique = await _userService.IsNationalCodeUniqueAsync(nationalCode);
            
            return Ok(new ApiResponseDto<bool>
            {
                Success = true,
                Message = "National code validation completed",
                Data = isUnique
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to validate national code: {NationalCode}", nationalCode);
            return BadRequest(new ApiResponseDto<bool>
            {
                Success = false,
                Message = "Failed to validate national code",
                Errors = new List<string> { ex.Message }
            });
        }
    }

    [HttpGet("validate/company-national-id")]
    [AllowAnonymous]
    public async Task<ActionResult<ApiResponseDto<bool>>> ValidateCompanyNationalId([FromQuery] string companyNationalId)
    {
        try
        {
            var isUnique = await _userService.IsCompanyNationalIdUniqueAsync(companyNationalId);
            
            return Ok(new ApiResponseDto<bool>
            {
                Success = true,
                Message = "Company national ID validation completed",
                Data = isUnique
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to validate company national ID: {CompanyNationalId}", companyNationalId);
            return BadRequest(new ApiResponseDto<bool>
            {
                Success = false,
                Message = "Failed to validate company national ID",
                Errors = new List<string> { ex.Message }
            });
        }
    }

    #endregion

    #region Helper Methods

    private Guid GetCurrentUserId()
    {
        var userIdClaim = User.Claims.FirstOrDefault(c => c.Type == "userId")?.Value;
        if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
        {
            throw new UnauthorizedAccessException("Invalid user ID in token");
        }
        return userId;
    }

    #endregion
}

// Additional DTOs for the controller
public class ChangePasswordRequestDto
{
    public string CurrentPassword { get; set; } = string.Empty;
    public string NewPassword { get; set; } = string.Empty;
} 