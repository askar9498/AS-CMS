using System.ComponentModel.DataAnnotations;
using AS_CMS.Domain.Entities;
using Microsoft.AspNetCore.Http;

namespace AS_CMS.Application.DTOs;

// Base user DTO for common operations
public class BaseUserDto
{
    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;
    
    [Required]
    [MinLength(6)]
    public string Password { get; set; } = string.Empty;
    
    [Required]
    [Phone]
    public string PhoneNumber { get; set; } = string.Empty;
    
    public string? NationalCode { get; set; }
    public IFormFile? ProfileImage { get; set; }
}

// Individual user registration DTO
public class IndividualUserRegistrationDto : BaseUserDto
{
    [Required]
    [MaxLength(100)]
    public string FirstName { get; set; } = string.Empty;
    
    [Required]
    [MaxLength(100)]
    public string LastName { get; set; } = string.Empty;
    
    public DateTime? BirthDate { get; set; }
    public string? EducationLevel { get; set; }
    public string? Expertise { get; set; }
    public IFormFile? Resume { get; set; }
    public List<string>? Interests { get; set; }
    public string? SkillLevel { get; set; }
    public string? ResearchGateLink { get; set; }
    public string? OrcidLink { get; set; }
    public string? GoogleScholarLink { get; set; }
    public List<string>? SavedInterests { get; set; }
    public bool TwoFactorEnabled { get; set; } = false;
}

// Corporate user registration DTO
public class CorporateUserRegistrationDto : BaseUserDto
{
    [Required]
    [MaxLength(255)]
    public string CompanyName { get; set; } = string.Empty;
    
    [Required]
    [MaxLength(50)]
    public string CompanyNationalId { get; set; } = string.Empty;
    
    public string? RegistrationNumber { get; set; }
    public string? ActivityField { get; set; }
    public string? CompanyPhone { get; set; }
    public string? CompanyDescription { get; set; }
    public string? Website { get; set; }
    public IFormFile? Logo { get; set; }
    public string? RepresentativeName { get; set; }
    public string? RepresentativeEmail { get; set; }
    public string? RepresentativeNationalId { get; set; }
    public string? RepresentativePhone { get; set; }
    public string? FullAddress { get; set; }
    public IFormFile? OfficialDocuments { get; set; }
    public bool ShowPublicProfile { get; set; } = false;
}

// Individual user profile completion DTO
public class IndividualProfileCompletionDto
{
    [Required]
    [MaxLength(100)]
    public string FirstName { get; set; } = string.Empty;
    
    [Required]
    [MaxLength(100)]
    public string LastName { get; set; } = string.Empty;
    
    public DateTime? BirthDate { get; set; }
    public string? EducationLevel { get; set; }
    public string? Expertise { get; set; }
    public IFormFile? Resume { get; set; }
    public List<string>? Interests { get; set; }
    public string? SkillLevel { get; set; }
    public string? ResearchGateLink { get; set; }
    public string? OrcidLink { get; set; }
    public string? GoogleScholarLink { get; set; }
    public List<string>? SavedInterests { get; set; }
    public bool TwoFactorEnabled { get; set; } = false;
}

// Corporate user profile completion DTO
public class CorporateProfileCompletionDto
{
    [Required]
    [MaxLength(255)]
    public string CompanyName { get; set; } = string.Empty;
    
    [Required]
    [MaxLength(50)]
    public string CompanyNationalId { get; set; } = string.Empty;
    
    public string? RegistrationNumber { get; set; }
    public string? ActivityField { get; set; }
    public string? CompanyPhone { get; set; }
    public string? CompanyDescription { get; set; }
    public string? Website { get; set; }
    public IFormFile? Logo { get; set; }
    public string? RepresentativeName { get; set; }
    public string? RepresentativeEmail { get; set; }
    public string? RepresentativeNationalId { get; set; }
    public string? RepresentativePhone { get; set; }
    public string? FullAddress { get; set; }
    public IFormFile? OfficialDocuments { get; set; }
    public bool ShowPublicProfile { get; set; } = false;
}

// User update DTO
public class UpdateUserDto
{
    [Required]
    [MaxLength(100)]
    public string FirstName { get; set; } = string.Empty;
    
    [Required]
    [MaxLength(100)]
    public string LastName { get; set; } = string.Empty;
    
    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;
    
    [Phone]
    public string? PhoneNumber { get; set; }
    
    public string? NationalCode { get; set; }
    public IFormFile? ProfileImage { get; set; }
}

// Authentication DTO
public class LoginRequestDto
{
    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;
    
    [Required]
    public string Password { get; set; } = string.Empty;
    
    public bool RememberMe { get; set; } = false;
}

public class LoginResponseDto
{
    public string AccessToken { get; set; } = string.Empty;
    public string RefreshToken { get; set; } = string.Empty;
    public DateTime ExpiresAt { get; set; }
    public UserResponse User { get; set; } = new();
}

// User search and filtering DTOs
public class SearchUserDto
{
    public string Text { get; set; } = string.Empty;
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 10;
    public UserType? UserType { get; set; }
    public bool? IsActive { get; set; }
    public Guid? UserGroupId { get; set; }
}

public class UserFilterDto
{
    public string? SearchTerm { get; set; }
    public UserType? UserType { get; set; }
    public bool? IsActive { get; set; }
    public Guid? UserGroupId { get; set; }
    public DateTime? CreatedFrom { get; set; }
    public DateTime? CreatedTo { get; set; }
    public DateTime? LastLoginFrom { get; set; }
    public DateTime? LastLoginTo { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 10;
    public string? SortBy { get; set; }
    public bool SortDescending { get; set; } = false;
}

// Paged response DTO
public class PagedResponseDto<T>
{
    public List<T> Items { get; set; } = new();
    public int TotalCount { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalPages { get; set; }
    public bool HasNextPage { get; set; }
    public bool HasPreviousPage { get; set; }
}

// Role and permission DTOs
public class CreateRoleDto
{
    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;
    
    public string? Description { get; set; }
    public List<PermissionEnums> Permissions { get; set; } = new();
}

public class UpdateRoleDto
{
    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;
    
    public string? Description { get; set; }
    public List<PermissionEnums> Permissions { get; set; } = new();
}

public class SetUserRoleDto
{
    public Guid UserId { get; set; }
    public Guid UserGroupId { get; set; }
}

public class SetRolePermissionsDto
{
    public Guid UserGroupId { get; set; }
    public List<PermissionEnums> Permissions { get; set; } = new();
}

// User management DTOs
public class DeleteUserDto
{
    public Guid UserId { get; set; }
}

public class ResetPasswordDto
{
    public Guid UserId { get; set; }
    public string NewPassword { get; set; } = string.Empty;
}

public class SetUserAccuracyDto
{
    public string Email { get; set; } = string.Empty;
    public bool IsActive { get; set; }
}

// User login logs DTOs
public class UserLoginLogDto
{
    public Guid Id { get; set; }
    public DateTime LoginTime { get; set; }
    public DateTime? LogoutTime { get; set; }
    public string? IpAddress { get; set; }
    public string? UserAgent { get; set; }
    public string? DeviceType { get; set; }
    public string? Browser { get; set; }
    public string? OperatingSystem { get; set; }
    public bool IsSuccessful { get; set; }
    public string? FailureReason { get; set; }
    public TimeSpan? SessionDuration { get; set; }
}

public class UserLoginLogsResponseDto
{
    public List<UserLoginLogDto> Items { get; set; } = new();
    public int TotalCount { get; set; }
}

// Response DTOs
public class UsersResponseDto
{
    public List<UserResponse> Items { get; set; } = new();
    public int TotalCount { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalPages { get; set; }
}

public class UserGroupsResponseDto
{
    public List<UserGroupDto> Items { get; set; } = new();
    public int TotalCount { get; set; }
}

public class PermissionsResponseDto
{
    public List<PermissionDto> Items { get; set; } = new();
    public int TotalCount { get; set; }
}

// File upload response DTO
public class FileUploadResponseDto
{
    public string FileName { get; set; } = string.Empty;
    public string FileUrl { get; set; } = string.Empty;
    public long FileSize { get; set; }
    public string ContentType { get; set; } = string.Empty;
}

// API response wrapper
public class ApiResponseDto<T>
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public T? Data { get; set; }
    public List<string> Errors { get; set; } = new();
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
} 