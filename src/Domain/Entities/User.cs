using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace AS_CMS.Domain.Entities;

public class User
{
    public Guid Id { get; private set; }
    
    [Required]
    [MaxLength(100)]
    public string FirstName { get; private set; } = string.Empty;
    
    [Required]
    [MaxLength(100)]
    public string LastName { get; private set; } = string.Empty;
    
    [Required]
    [EmailAddress]
    [MaxLength(255)]
    public string Email { get; private set; } = string.Empty;
    
    [Required]
    [MaxLength(255)]
    public string PasswordHash { get; private set; } = string.Empty;
    
    [MaxLength(20)]
    public string? PhoneNumber { get; private set; }
    
    [MaxLength(50)]
    public string? NationalCode { get; private set; }
    
    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }
    public DateTime? LastLoginAt { get; private set; }
    
    public bool IsActive { get; private set; } = true;
    public bool EmailConfirmed { get; private set; } = false;
    public bool TwoFactorEnabled { get; private set; } = false;
    
    // User Type and Group
    public UserType UserType { get; private set; }
    public Guid? UserGroupId { get; private set; }
    public virtual UserGroup? UserGroup { get; private set; }
    
    // Profile Image
    public string? ProfileImageUrl { get; private set; }
    
    // Individual User Specific Fields
    public DateTime? BirthDate { get; private set; }
    [MaxLength(100)]
    public string? EducationLevel { get; private set; }
    [MaxLength(255)]
    public string? Expertise { get; private set; }
    public string? ResumeUrl { get; private set; }
    public string? Interests { get; private set; } // JSON string
    [MaxLength(100)]
    public string? SkillLevel { get; private set; }
    [MaxLength(255)]
    public string? ResearchGateLink { get; private set; }
    [MaxLength(255)]
    public string? OrcidLink { get; private set; }
    [MaxLength(255)]
    public string? GoogleScholarLink { get; private set; }
    public string? SavedInterests { get; private set; } // JSON string
    
    // Corporate User Specific Fields
    [MaxLength(255)]
    public string? CompanyName { get; private set; }
    [MaxLength(50)]
    public string? CompanyNationalId { get; private set; }
    [MaxLength(50)]
    public string? RegistrationNumber { get; private set; }
    [MaxLength(255)]
    public string? ActivityField { get; private set; }
    [MaxLength(255)]
    public string? CompanyPhone { get; private set; }
    [MaxLength(255)]
    public string? CompanyDescription { get; private set; }
    [MaxLength(255)]
    public string? Website { get; private set; }
    [MaxLength(255)]
    public string? LogoUrl { get; private set; }
    [MaxLength(255)]
    public string? RepresentativeName { get; private set; }
    [MaxLength(255)]
    public string? RepresentativeEmail { get; private set; }
    [MaxLength(50)]
    public string? RepresentativeNationalId { get; private set; }
    [MaxLength(20)]
    public string? RepresentativePhone { get; private set; }
    [MaxLength(500)]
    public string? FullAddress { get; private set; }
    public string? OfficialDocumentsUrl { get; private set; }
    public bool ShowPublicProfile { get; private set; } = false;
    
    // Navigation properties
    public virtual ICollection<UserRole> UserRoles { get; private set; } = new List<UserRole>();
    public virtual ICollection<RefreshToken> RefreshTokens { get; private set; } = new List<RefreshToken>();
    public virtual ICollection<UserLoginLog> LoginLogs { get; private set; } = new List<UserLoginLog>();

    // Private constructor for EF Core
    private User() { }

    // Factory method for creating new users
    public static User Create(
        string firstName,
        string lastName,
        string email,
        string passwordHash,
        string phoneNumber,
        UserType userType,
        Guid? userGroupId = null,
        string? nationalCode = null)
    {
        return new User
        {
            Id = Guid.NewGuid(),
            FirstName = firstName,
            LastName = lastName,
            Email = email,
            PasswordHash = passwordHash,
            PhoneNumber = phoneNumber,
            NationalCode = nationalCode,
            UserType = userType,
            UserGroupId = userGroupId,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
    }

    // Update basic information
    public void UpdateBasicInfo(string firstName, string lastName, string email, string phoneNumber, string? nationalCode)
    {
        FirstName = firstName;
        LastName = lastName;
        Email = email;
        PhoneNumber = phoneNumber;
        NationalCode = nationalCode;
        UpdatedAt = DateTime.UtcNow;
    }

    // Update password
    public void UpdatePassword(string passwordHash)
    {
        PasswordHash = passwordHash;
        UpdatedAt = DateTime.UtcNow;
    }

    // Update profile image
    public void UpdateProfileImage(string? profileImageUrl)
    {
        ProfileImageUrl = profileImageUrl;
        UpdatedAt = DateTime.UtcNow;
    }

    // Complete individual profile
    public void CompleteIndividualProfile(
        DateTime? birthDate,
        string? educationLevel,
        string? expertise,
        string? resumeUrl,
        List<string>? interests,
        string? skillLevel,
        string? researchGateLink,
        string? orcidLink,
        string? googleScholarLink,
        List<string>? savedInterests)
    {
        BirthDate = birthDate;
        EducationLevel = educationLevel;
        Expertise = expertise;
        ResumeUrl = resumeUrl;
        Interests = interests != null ? System.Text.Json.JsonSerializer.Serialize(interests) : null;
        SkillLevel = skillLevel;
        ResearchGateLink = researchGateLink;
        OrcidLink = orcidLink;
        GoogleScholarLink = googleScholarLink;
        SavedInterests = savedInterests != null ? System.Text.Json.JsonSerializer.Serialize(savedInterests) : null;
        UpdatedAt = DateTime.UtcNow;
    }

    // Complete corporate profile
    public void CompleteCorporateProfile(
        string companyName,
        string companyNationalId,
        string? registrationNumber,
        string? activityField,
        string? companyPhone,
        string? companyDescription,
        string? website,
        string? logoUrl,
        string? representativeName,
        string? representativeEmail,
        string? representativeNationalId,
        string? representativePhone,
        string? fullAddress,
        string? officialDocumentsUrl)
    {
        CompanyName = companyName;
        CompanyNationalId = companyNationalId;
        RegistrationNumber = registrationNumber;
        ActivityField = activityField;
        CompanyPhone = companyPhone;
        CompanyDescription = companyDescription;
        Website = website;
        LogoUrl = logoUrl;
        RepresentativeName = representativeName;
        RepresentativeEmail = representativeEmail;
        RepresentativeNationalId = representativeNationalId;
        RepresentativePhone = representativePhone;
        FullAddress = fullAddress;
        OfficialDocumentsUrl = officialDocumentsUrl;
        UpdatedAt = DateTime.UtcNow;
    }

    // Update user group
    public void UpdateUserGroup(Guid userGroupId)
    {
        UserGroupId = userGroupId;
        UpdatedAt = DateTime.UtcNow;
    }

    // Set two-factor authentication
    public void SetTwoFactorEnabled(bool enabled)
    {
        TwoFactorEnabled = enabled;
        UpdatedAt = DateTime.UtcNow;
    }

    // Set public profile visibility
    public void SetPublicProfileVisibility(bool showPublic)
    {
        ShowPublicProfile = showPublic;
        UpdatedAt = DateTime.UtcNow;
    }

    // Record login
    public void RecordLogin()
    {
        LastLoginAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    // Deactivate user
    public void Deactivate()
    {
        IsActive = false;
        UpdatedAt = DateTime.UtcNow;
    }

    // Activate user
    public void Activate()
    {
        IsActive = true;
        UpdatedAt = DateTime.UtcNow;
    }

    // Confirm email
    public void ConfirmEmail()
    {
        EmailConfirmed = true;
        UpdatedAt = DateTime.UtcNow;
    }

    // Get full name
    public string GetFullName() => $"{FirstName} {LastName}".Trim();
    
    // Full name property for backward compatibility
    public string FullName => GetFullName();
    
    // Update last login (alias for RecordLogin for backward compatibility)
    public void UpdateLastLogin() => RecordLogin();
    
    // Update profile information
    public void UpdateProfile(string firstName, string lastName, string? phoneNumber)
    {
        FirstName = firstName;
        LastName = lastName;
        PhoneNumber = phoneNumber;
        UpdatedAt = DateTime.UtcNow;
    }

    // Get interests as list
    public List<string>? GetInterestsList()
    {
        if (string.IsNullOrEmpty(Interests))
            return null;
        
        try
        {
            return System.Text.Json.JsonSerializer.Deserialize<List<string>>(Interests);
        }
        catch
        {
            return null;
        }
    }

    // Get saved interests as list
    public List<string>? GetSavedInterestsList()
    {
        if (string.IsNullOrEmpty(SavedInterests))
            return null;
        
        try
        {
            return System.Text.Json.JsonSerializer.Deserialize<List<string>>(SavedInterests);
        }
        catch
        {
            return null;
        }
    }
}

public enum UserType
{
    Individual = 0,
    Corporate = 1
} 