using AS_CMS.Application.DTOs;
using AS_CMS.Domain.Entities;

namespace AS_CMS.Application.Interfaces;

public interface IUserService
{
    // Authentication
    Task<LoginResponseDto> AuthenticateAsync(LoginRequestDto request);
    Task<LoginResponseDto> RefreshTokenAsync(string refreshToken);
    Task RevokeTokenAsync(string refreshToken);
    
    // User CRUD Operations
    Task<UserResponse?> GetUserAsync(Guid userId);
    Task<UserResponse?> GetUserByEmailAsync(string email);
    Task<PagedResponseDto<UserResponse>> GetUsersAsync(UserFilterDto filter);
    Task<List<UserResponse>> SearchUsersAsync(string searchTerm);
    
    // User Registration
    Task<UserResponse> CreateIndividualUserAsync(IndividualUserRegistrationDto request);
    Task<UserResponse> CreateCorporateUserAsync(CorporateUserRegistrationDto request);
    Task<UserResponse> UpdateUserAsync(Guid userId, UpdateUserDto request);
    Task DeleteUserAsync(Guid userId);
    
    // Profile Completion
    Task<UserResponse> CompleteIndividualProfileAsync(Guid userId, IndividualProfileCompletionDto request);
    Task<UserResponse> CompleteCorporateProfileAsync(Guid userId, CorporateProfileCompletionDto request);
    
    // Password Management
    Task ResetPasswordAsync(Guid userId);
    Task ChangePasswordAsync(Guid userId, string currentPassword, string newPassword);
    Task<bool> ValidatePasswordAsync(Guid userId, string password);
    
    // User Status Management
    Task SetUserAccuracyAsync(string email, bool isActive);
    Task ActivateUserAsync(Guid userId);
    Task DeactivateUserAsync(Guid userId);
    Task ConfirmEmailAsync(Guid userId);
    
    // Role Management
    Task<List<UserGroupDto>> GetRolesAsync();
    Task<UserGroupDto> CreateRoleAsync(CreateRoleDto request);
    Task<UserGroupDto> UpdateRoleAsync(Guid roleId, UpdateRoleDto request);
    Task DeleteRoleAsync(Guid roleId);
    Task SetUserRoleAsync(Guid userId, Guid userGroupId);
    
    // Permission Management
    Task<List<PermissionDto>> GetPermissionsAsync();
    Task<List<PermissionDto>> GetUserPermissionsAsync(Guid userId);
    Task SetUserPermissionsAsync(Guid userId, List<PermissionEnums> permissions);
    Task SetRolePermissionsAsync(Guid userGroupId, List<PermissionEnums> permissions);
    
    // Login Logs
    Task<List<UserLoginLogDto>> GetUserLoginLogsAsync(Guid userId, int count = 10);
    Task LogUserLoginAsync(Guid userId, bool isSuccessful, string? failureReason = null, string? ipAddress = null, string? userAgent = null);
    Task LogUserLogoutAsync(Guid userId);
    
    // File Upload
    Task<FileUploadResponseDto> UploadProfileImageAsync(Guid userId, IFormFile file);
    Task<FileUploadResponseDto> UploadResumeAsync(Guid userId, IFormFile file);
    Task<FileUploadResponseDto> UploadLogoAsync(Guid userId, IFormFile file);
    Task<FileUploadResponseDto> UploadOfficialDocumentsAsync(Guid userId, IFormFile file);
    
    // Two-Factor Authentication
    Task EnableTwoFactorAsync(Guid userId);
    Task DisableTwoFactorAsync(Guid userId);
    Task<bool> ValidateTwoFactorCodeAsync(Guid userId, string code);
    
    // Public Profile
    Task SetPublicProfileVisibilityAsync(Guid userId, bool showPublic);
    Task<UserResponse?> GetPublicProfileAsync(Guid userId);
    
    // User Statistics
    Task<Dictionary<string, int>> GetUserStatisticsAsync();
    Task<int> GetActiveUsersCountAsync();
    Task<int> GetNewUsersCountAsync(DateTime fromDate);
    
    // Bulk Operations
    Task<List<UserResponse>> GetUsersByIdsAsync(List<Guid> userIds);
    Task BulkUpdateUserStatusAsync(List<Guid> userIds, bool isActive);
    Task BulkAssignRoleAsync(List<Guid> userIds, Guid userGroupId);
    
    // Validation
    Task<bool> IsEmailUniqueAsync(string email);
    Task<bool> IsNationalCodeUniqueAsync(string nationalCode);
    Task<bool> IsCompanyNationalIdUniqueAsync(string companyNationalId);
    
    // Security
    Task<bool> HasPermissionAsync(Guid userId, PermissionEnums permission);
    Task<List<PermissionEnums>> GetUserPermissionEnumsAsync(Guid userId);
    Task<bool> IsUserInRoleAsync(Guid userId, string roleName);
} 