using AS_CMS.Domain.Entities;

namespace AS_CMS.Application.DTOs;

public class UserResponse
{
    public Guid Id { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? PhoneNumber { get; set; }
    public string? NationalCode { get; set; }
    public string FullName { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public DateTime? LastLoginAt { get; set; }
    public bool IsActive { get; set; }
    public bool EmailConfirmed { get; set; }
    public bool TwoFactorEnabled { get; set; }
    public UserType UserType { get; set; }
    public string? ProfileImageUrl { get; set; }
    public UserGroupDto UserGroup { get; set; } = new();
    public List<string> Roles { get; set; } = new List<string>();
    public List<PermissionDto> Permissions { get; set; } = new List<PermissionDto>();
    
    // Individual User Specific Fields
    public DateTime? BirthDate { get; set; }
    public string? EducationLevel { get; set; }
    public string? Expertise { get; set; }
    public string? ResumeUrl { get; set; }
    public List<string>? Interests { get; set; }
    public string? SkillLevel { get; set; }
    public string? ResearchGateLink { get; set; }
    public string? OrcidLink { get; set; }
    public string? GoogleScholarLink { get; set; }
    public List<string>? SavedInterests { get; set; }
    
    // Corporate User Specific Fields
    public string? CompanyName { get; set; }
    public string? CompanyNationalId { get; set; }
    public string? RegistrationNumber { get; set; }
    public string? ActivityField { get; set; }
    public string? CompanyPhone { get; set; }
    public string? CompanyDescription { get; set; }
    public string? Website { get; set; }
    public string? LogoUrl { get; set; }
    public string? RepresentativeName { get; set; }
    public string? RepresentativeEmail { get; set; }
    public string? RepresentativeNationalId { get; set; }
    public string? RepresentativePhone { get; set; }
    public string? FullAddress { get; set; }
    public string? OfficialDocumentsUrl { get; set; }
    public bool ShowPublicProfile { get; set; }
}

public class UserGroupDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public bool IsActive { get; set; }
    public List<PermissionDto> Permissions { get; set; } = new List<PermissionDto>();
}

public class PermissionDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public string? Description { get; set; }
    public PermissionEnums PermissionEnum { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public bool IsActive { get; set; }
} 