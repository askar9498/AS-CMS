using AS_CMS.Application.DTOs;
using AS_CMS.Application.Interfaces;
using AS_CMS.Domain.Entities;
using AS_CMS.Infrastructure.Identity;
using AS_CMS.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Serilog;

namespace AS_CMS.Application.Services;

public class UserService : IUserService
{
    private readonly ApplicationDbContext _context;
    private readonly IJwtTokenGenerator _jwtTokenGenerator;
    private readonly IFileStorageService _fileStorageService;
    private readonly IEmailService _emailService;
    private readonly ILogger<UserService> _logger;

    public UserService(
        ApplicationDbContext context,
        IJwtTokenGenerator jwtTokenGenerator,
        IFileStorageService fileStorageService,
        IEmailService emailService,
        ILogger<UserService> logger)
    {
        _context = context;
        _jwtTokenGenerator = jwtTokenGenerator;
        _fileStorageService = fileStorageService;
        _emailService = emailService;
        _logger = logger;
    }

    #region Authentication

    public async Task<LoginResponseDto> AuthenticateAsync(LoginRequestDto request)
    {
        _logger.Information("Authenticating user: {Email}", request.Email);

        var user = await _context.Users
            .Include(u => u.UserGroup)
            .ThenInclude(ug => ug.Permissions)
            .FirstOrDefaultAsync(u => u.Email == request.Email && u.IsActive);

        if (user == null)
        {
            _logger.Warning("Authentication failed: User not found or inactive - {Email}", request.Email);
            throw new UnauthorizedAccessException("Invalid email or password");
        }

        if (!BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
        {
            _logger.Warning("Authentication failed: Invalid password for user - {Email}", request.Email);
            throw new UnauthorizedAccessException("Invalid email or password");
        }

        // Record successful login
        await LogUserLoginAsync(user.Id, true, ipAddress: GetClientIpAddress(), userAgent: GetUserAgent());

        // Update last login time
        user.RecordLogin();
        await _context.SaveChangesAsync();

        // Generate tokens
        var accessToken = _jwtTokenGenerator.GenerateAccessToken(user);
        var refreshToken = _jwtTokenGenerator.GenerateRefreshToken(user.Id);

        _logger.Information("User authenticated successfully: {Email}", request.Email);

        return new LoginResponseDto
        {
            AccessToken = accessToken,
            RefreshToken = refreshToken,
            ExpiresAt = DateTime.UtcNow.AddHours(1),
            User = await MapToUserResponseAsync(user)
        };
    }

    public async Task<LoginResponseDto> RefreshTokenAsync(string refreshToken)
    {
        var userId = _jwtTokenGenerator.ValidateRefreshToken(refreshToken);
        if (userId == Guid.Empty)
        {
            throw new UnauthorizedAccessException("Invalid refresh token");
        }

        var user = await _context.Users
            .Include(u => u.UserGroup)
            .ThenInclude(ug => ug.Permissions)
            .FirstOrDefaultAsync(u => u.Id == userId && u.IsActive);

        if (user == null)
        {
            throw new UnauthorizedAccessException("User not found or inactive");
        }

        var newAccessToken = _jwtTokenGenerator.GenerateAccessToken(user);
        var newRefreshToken = _jwtTokenGenerator.GenerateRefreshToken(user.Id);

        return new LoginResponseDto
        {
            AccessToken = newAccessToken,
            RefreshToken = newRefreshToken,
            ExpiresAt = DateTime.UtcNow.AddHours(1),
            User = await MapToUserResponseAsync(user)
        };
    }

    public async Task RevokeTokenAsync(string refreshToken)
    {
        // In a real implementation, you would add the token to a blacklist
        // For now, we'll just validate it
        var userId = _jwtTokenGenerator.ValidateRefreshToken(refreshToken);
        if (userId != Guid.Empty)
        {
            _logger.Information("Token revoked for user: {UserId}", userId);
        }
    }

    #endregion

    #region User CRUD Operations

    public async Task<UserResponse?> GetUserAsync(Guid userId)
    {
        var user = await _context.Users
            .Include(u => u.UserGroup)
            .ThenInclude(ug => ug.Permissions)
            .FirstOrDefaultAsync(u => u.Id == userId);

        return user != null ? await MapToUserResponseAsync(user) : null;
    }

    public async Task<UserResponse?> GetUserByEmailAsync(string email)
    {
        var user = await _context.Users
            .Include(u => u.UserGroup)
            .ThenInclude(ug => ug.Permissions)
            .FirstOrDefaultAsync(u => u.Email == email);

        return user != null ? await MapToUserResponseAsync(user) : null;
    }

    public async Task<PagedResponseDto<UserResponse>> GetUsersAsync(UserFilterDto filter)
    {
        var query = _context.Users
            .Include(u => u.UserGroup)
            .ThenInclude(ug => ug.Permissions)
            .AsQueryable();

        // Apply filters
        if (!string.IsNullOrEmpty(filter.SearchTerm))
        {
            query = query.Where(u => 
                u.FirstName.Contains(filter.SearchTerm) ||
                u.LastName.Contains(filter.SearchTerm) ||
                u.Email.Contains(filter.SearchTerm) ||
                u.PhoneNumber.Contains(filter.SearchTerm));
        }

        if (filter.UserType.HasValue)
        {
            query = query.Where(u => u.UserType == filter.UserType.Value);
        }

        if (filter.IsActive.HasValue)
        {
            query = query.Where(u => u.IsActive == filter.IsActive.Value);
        }

        if (filter.UserGroupId.HasValue)
        {
            query = query.Where(u => u.UserGroupId == filter.UserGroupId.Value);
        }

        if (filter.CreatedFrom.HasValue)
        {
            query = query.Where(u => u.CreatedAt >= filter.CreatedFrom.Value);
        }

        if (filter.CreatedTo.HasValue)
        {
            query = query.Where(u => u.CreatedAt <= filter.CreatedTo.Value);
        }

        if (filter.LastLoginFrom.HasValue)
        {
            query = query.Where(u => u.LastLoginAt >= filter.LastLoginFrom.Value);
        }

        if (filter.LastLoginTo.HasValue)
        {
            query = query.Where(u => u.LastLoginAt <= filter.LastLoginTo.Value);
        }

        // Apply sorting
        if (!string.IsNullOrEmpty(filter.SortBy))
        {
            query = filter.SortBy.ToLower() switch
            {
                "firstname" => filter.SortDescending ? query.OrderByDescending(u => u.FirstName) : query.OrderBy(u => u.FirstName),
                "lastname" => filter.SortDescending ? query.OrderByDescending(u => u.LastName) : query.OrderBy(u => u.LastName),
                "email" => filter.SortDescending ? query.OrderByDescending(u => u.Email) : query.OrderBy(u => u.Email),
                "createdat" => filter.SortDescending ? query.OrderByDescending(u => u.CreatedAt) : query.OrderBy(u => u.CreatedAt),
                "lastloginat" => filter.SortDescending ? query.OrderByDescending(u => u.LastLoginAt) : query.OrderBy(u => u.LastLoginAt),
                _ => query.OrderByDescending(u => u.CreatedAt)
            };
        }
        else
        {
            query = query.OrderByDescending(u => u.CreatedAt);
        }

        var totalCount = await query.CountAsync();
        var totalPages = (int)Math.Ceiling((double)totalCount / filter.PageSize);

        var users = await query
            .Skip((filter.Page - 1) * filter.PageSize)
            .Take(filter.PageSize)
            .ToListAsync();

        var userResponses = new List<UserResponse>();
        foreach (var user in users)
        {
            userResponses.Add(await MapToUserResponseAsync(user));
        }

        return new PagedResponseDto<UserResponse>
        {
            Items = userResponses,
            TotalCount = totalCount,
            Page = filter.Page,
            PageSize = filter.PageSize,
            TotalPages = totalPages,
            HasNextPage = filter.Page < totalPages,
            HasPreviousPage = filter.Page > 1
        };
    }

    public async Task<List<UserResponse>> SearchUsersAsync(string searchTerm)
    {
        var users = await _context.Users
            .Include(u => u.UserGroup)
            .ThenInclude(ug => ug.Permissions)
            .Where(u => 
                u.FirstName.Contains(searchTerm) ||
                u.LastName.Contains(searchTerm) ||
                u.Email.Contains(searchTerm) ||
                u.PhoneNumber.Contains(searchTerm) ||
                u.NationalCode.Contains(searchTerm))
            .Take(20)
            .ToListAsync();

        var userResponses = new List<UserResponse>();
        foreach (var user in users)
        {
            userResponses.Add(await MapToUserResponseAsync(user));
        }

        return userResponses;
    }

    #endregion

    #region User Registration

    public async Task<UserResponse> CreateIndividualUserAsync(IndividualUserRegistrationDto request)
    {
        _logger.Information("Creating individual user: {Email}", request.Email);

        // Validate unique email
        if (!await IsEmailUniqueAsync(request.Email))
        {
            throw new InvalidOperationException("Email already exists");
        }

        // Get default user group for individuals
        var defaultGroup = await _context.UserGroups.FirstOrDefaultAsync(ug => ug.Name == "Individual");
        if (defaultGroup == null)
        {
            defaultGroup = UserGroup.Create("Individual", "Default group for individual users");
            _context.UserGroups.Add(defaultGroup);
            await _context.SaveChangesAsync();
        }

        // Hash password
        var passwordHash = BCrypt.Net.BCrypt.HashPassword(request.Password);

        // Create user
        var user = User.Create(
            request.FirstName,
            request.LastName,
            request.Email,
            passwordHash,
            request.PhoneNumber,
            UserType.Individual,
            defaultGroup.Id,
            request.NationalCode
        );

        // Handle profile image upload
        if (request.ProfileImage != null)
        {
            var imageUrl = await _fileStorageService.UploadProfileImageAsync(request.ProfileImage, user.Id);
            user.UpdateProfileImage(imageUrl);
        }

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        // Send welcome email
        await _emailService.SendWelcomeEmailAsync(request.Email, user.GetFullName());

        _logger.Information("Individual user created successfully: {Email}", request.Email);

        return await MapToUserResponseAsync(user);
    }

    public async Task<UserResponse> CreateCorporateUserAsync(CorporateUserRegistrationDto request)
    {
        _logger.Information("Creating corporate user: {Email}", request.Email);

        // Validate unique email
        if (!await IsEmailUniqueAsync(request.Email))
        {
            throw new InvalidOperationException("Email already exists");
        }

        // Get default user group for corporate users
        var defaultGroup = await _context.UserGroups.FirstOrDefaultAsync(ug => ug.Name == "Corporate");
        if (defaultGroup == null)
        {
            defaultGroup = UserGroup.Create("Corporate", "Default group for corporate users");
            _context.UserGroups.Add(defaultGroup);
            await _context.SaveChangesAsync();
        }

        // Hash password
        var passwordHash = BCrypt.Net.BCrypt.HashPassword(request.Password);

        // Create user
        var user = User.Create(
            request.CompanyName, // Use company name as first name for corporate users
            "", // Empty last name for corporate users
            request.Email,
            passwordHash,
            request.PhoneNumber,
            UserType.Corporate,
            defaultGroup.Id
        );

        // Complete corporate profile
        user.CompleteCorporateProfile(
            request.CompanyName,
            request.CompanyNationalId,
            request.RegistrationNumber,
            request.ActivityField,
            request.CompanyPhone,
            request.CompanyDescription,
            request.Website,
            null, // Logo URL will be set after upload
            request.RepresentativeName,
            request.RepresentativeEmail,
            request.RepresentativeNationalId,
            request.RepresentativePhone,
            request.FullAddress,
            null // Official documents URL will be set after upload
        );

        // Handle logo upload
        if (request.Logo != null)
        {
            var logoUrl = await _fileStorageService.UploadLogoAsync(request.Logo, user.Id);
            // Update logo URL in user entity
        }

        // Handle official documents upload
        if (request.OfficialDocuments != null)
        {
            var documentsUrl = await _fileStorageService.UploadOfficialDocumentsAsync(request.OfficialDocuments, user.Id);
            // Update documents URL in user entity
        }

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        // Send welcome email
        await _emailService.SendWelcomeEmailAsync(request.Email, request.CompanyName);

        _logger.Information("Corporate user created successfully: {Email}", request.Email);

        return await MapToUserResponseAsync(user);
    }

    public async Task<UserResponse> UpdateUserAsync(Guid userId, UpdateUserDto request)
    {
        var user = await _context.Users.FindAsync(userId);
        if (user == null)
        {
            throw new InvalidOperationException("User not found");
        }

        // Check if email is unique (if changed)
        if (user.Email != request.Email && !await IsEmailUniqueAsync(request.Email))
        {
            throw new InvalidOperationException("Email already exists");
        }

        // Update basic information
        user.UpdateBasicInfo(request.FirstName, request.LastName, request.Email, request.PhoneNumber, request.NationalCode);

        // Handle profile image upload
        if (request.ProfileImage != null)
        {
            var imageUrl = await _fileStorageService.UploadProfileImageAsync(request.ProfileImage, userId);
            user.UpdateProfileImage(imageUrl);
        }

        await _context.SaveChangesAsync();

        // Send profile update notification
        await _emailService.SendProfileUpdateNotificationAsync(request.Email, user.GetFullName());

        return await MapToUserResponseAsync(user);
    }

    public async Task DeleteUserAsync(Guid userId)
    {
        var user = await _context.Users.FindAsync(userId);
        if (user == null)
        {
            throw new InvalidOperationException("User not found");
        }

        // Soft delete by deactivating
        user.Deactivate();
        await _context.SaveChangesAsync();

        _logger.Information("User deleted (deactivated): {UserId}", userId);
    }

    #endregion

    #region Profile Completion

    public async Task<UserResponse> CompleteIndividualProfileAsync(Guid userId, IndividualProfileCompletionDto request)
    {
        var user = await _context.Users.FindAsync(userId);
        if (user == null)
        {
            throw new InvalidOperationException("User not found");
        }

        if (user.UserType != UserType.Individual)
        {
            throw new InvalidOperationException("User is not an individual user");
        }

        // Update basic information
        user.UpdateBasicInfo(request.FirstName, request.LastName, user.Email, user.PhoneNumber, user.NationalCode);

        // Handle resume upload
        string? resumeUrl = null;
        if (request.Resume != null)
        {
            resumeUrl = await _fileStorageService.UploadResumeAsync(request.Resume, userId);
        }

        // Complete individual profile
        user.CompleteIndividualProfile(
            request.BirthDate,
            request.EducationLevel,
            request.Expertise,
            resumeUrl,
            request.Interests,
            request.SkillLevel,
            request.ResearchGateLink,
            request.OrcidLink,
            request.GoogleScholarLink,
            request.SavedInterests
        );

        // Set two-factor authentication
        if (request.TwoFactorEnabled)
        {
            user.SetTwoFactorEnabled(true);
        }

        await _context.SaveChangesAsync();

        return await MapToUserResponseAsync(user);
    }

    public async Task<UserResponse> CompleteCorporateProfileAsync(Guid userId, CorporateProfileCompletionDto request)
    {
        var user = await _context.Users.FindAsync(userId);
        if (user == null)
        {
            throw new InvalidOperationException("User not found");
        }

        if (user.UserType != UserType.Corporate)
        {
            throw new InvalidOperationException("User is not a corporate user");
        }

        // Handle logo upload
        string? logoUrl = null;
        if (request.Logo != null)
        {
            logoUrl = await _fileStorageService.UploadLogoAsync(request.Logo, userId);
        }

        // Handle official documents upload
        string? documentsUrl = null;
        if (request.OfficialDocuments != null)
        {
            documentsUrl = await _fileStorageService.UploadOfficialDocumentsAsync(request.OfficialDocuments, userId);
        }

        // Complete corporate profile
        user.CompleteCorporateProfile(
            request.CompanyName,
            request.CompanyNationalId,
            request.RegistrationNumber,
            request.ActivityField,
            request.CompanyPhone,
            request.CompanyDescription,
            request.Website,
            logoUrl,
            request.RepresentativeName,
            request.RepresentativeEmail,
            request.RepresentativeNationalId,
            request.RepresentativePhone,
            request.FullAddress,
            documentsUrl
        );

        // Set public profile visibility
        user.SetPublicProfileVisibility(request.ShowPublicProfile);

        await _context.SaveChangesAsync();

        return await MapToUserResponseAsync(user);
    }

    #endregion

    #region Password Management

    public async Task ResetPasswordAsync(Guid userId)
    {
        var user = await _context.Users.FindAsync(userId);
        if (user == null)
        {
            throw new InvalidOperationException("User not found");
        }

        // Generate new password
        var newPassword = GenerateRandomPassword();
        var passwordHash = BCrypt.Net.BCrypt.HashPassword(newPassword);

        user.UpdatePassword(passwordHash);
        await _context.SaveChangesAsync();

        // Send password reset email
        await _emailService.SendPasswordResetEmailAsync(user.Email, newPassword);

        _logger.Information("Password reset for user: {UserId}", userId);
    }

    public async Task ChangePasswordAsync(Guid userId, string currentPassword, string newPassword)
    {
        var user = await _context.Users.FindAsync(userId);
        if (user == null)
        {
            throw new InvalidOperationException("User not found");
        }

        // Verify current password
        if (!BCrypt.Net.BCrypt.Verify(currentPassword, user.PasswordHash))
        {
            throw new InvalidOperationException("Current password is incorrect");
        }

        // Update password
        var passwordHash = BCrypt.Net.BCrypt.HashPassword(newPassword);
        user.UpdatePassword(passwordHash);
        await _context.SaveChangesAsync();

        _logger.Information("Password changed for user: {UserId}", userId);
    }

    public async Task<bool> ValidatePasswordAsync(Guid userId, string password)
    {
        var user = await _context.Users.FindAsync(userId);
        if (user == null)
        {
            return false;
        }

        return BCrypt.Net.BCrypt.Verify(password, user.PasswordHash);
    }

    #endregion

    #region User Status Management

    public async Task SetUserAccuracyAsync(string email, bool isActive)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
        if (user == null)
        {
            throw new InvalidOperationException("User not found");
        }

        if (isActive)
        {
            user.Activate();
            await _emailService.SendAccountActivationEmailAsync(email, user.GetFullName());
        }
        else
        {
            user.Deactivate();
            await _emailService.SendAccountDeactivationEmailAsync(email, user.GetFullName());
        }

        await _context.SaveChangesAsync();

        _logger.Information("User accuracy updated: {Email} - Active: {IsActive}", email, isActive);
    }

    public async Task ActivateUserAsync(Guid userId)
    {
        var user = await _context.Users.FindAsync(userId);
        if (user == null)
        {
            throw new InvalidOperationException("User not found");
        }

        user.Activate();
        await _context.SaveChangesAsync();

        await _emailService.SendAccountActivationEmailAsync(user.Email, user.GetFullName());
    }

    public async Task DeactivateUserAsync(Guid userId)
    {
        var user = await _context.Users.FindAsync(userId);
        if (user == null)
        {
            throw new InvalidOperationException("User not found");
        }

        user.Deactivate();
        await _context.SaveChangesAsync();

        await _emailService.SendAccountDeactivationEmailAsync(user.Email, user.GetFullName());
    }

    public async Task ConfirmEmailAsync(Guid userId)
    {
        var user = await _context.Users.FindAsync(userId);
        if (user == null)
        {
            throw new InvalidOperationException("User not found");
        }

        user.ConfirmEmail();
        await _context.SaveChangesAsync();
    }

    #endregion

    #region Role Management

    public async Task<List<UserGroupDto>> GetRolesAsync()
    {
        var roles = await _context.UserGroups
            .Include(ug => ug.Permissions)
            .Where(ug => ug.IsActive)
            .ToListAsync();

        return roles.Select(role => new UserGroupDto
        {
            Id = role.Id,
            Name = role.Name,
            Description = role.Description,
            CreatedAt = role.CreatedAt,
            UpdatedAt = role.UpdatedAt,
            IsActive = role.IsActive,
            Permissions = role.Permissions.Select(p => new PermissionDto
            {
                Id = p.Id,
                Name = p.Name,
                Code = p.Code,
                Description = p.Description,
                PermissionEnum = p.PermissionEnum,
                CreatedAt = p.CreatedAt,
                UpdatedAt = p.UpdatedAt,
                IsActive = p.IsActive
            }).ToList()
        }).ToList();
    }

    public async Task<UserGroupDto> CreateRoleAsync(CreateRoleDto request)
    {
        var role = UserGroup.Create(request.Name, request.Description);
        _context.UserGroups.Add(role);

        // Add permissions
        foreach (var permissionEnum in request.Permissions)
        {
            var permission = await _context.Permissions.FirstOrDefaultAsync(p => p.PermissionEnum == permissionEnum);
            if (permission != null)
            {
                role.AddPermission(permission);
            }
        }

        await _context.SaveChangesAsync();

        return new UserGroupDto
        {
            Id = role.Id,
            Name = role.Name,
            Description = role.Description,
            CreatedAt = role.CreatedAt,
            UpdatedAt = role.UpdatedAt,
            IsActive = role.IsActive,
            Permissions = role.Permissions.Select(p => new PermissionDto
            {
                Id = p.Id,
                Name = p.Name,
                Code = p.Code,
                Description = p.Description,
                PermissionEnum = p.PermissionEnum,
                CreatedAt = p.CreatedAt,
                UpdatedAt = p.UpdatedAt,
                IsActive = p.IsActive
            }).ToList()
        };
    }

    public async Task<UserGroupDto> UpdateRoleAsync(Guid roleId, UpdateRoleDto request)
    {
        var role = await _context.UserGroups
            .Include(ug => ug.Permissions)
            .FirstOrDefaultAsync(ug => ug.Id == roleId);

        if (role == null)
        {
            throw new InvalidOperationException("Role not found");
        }

        role.Update(request.Name, request.Description);

        // Clear existing permissions
        role.Permissions.Clear();

        // Add new permissions
        foreach (var permissionEnum in request.Permissions)
        {
            var permission = await _context.Permissions.FirstOrDefaultAsync(p => p.PermissionEnum == permissionEnum);
            if (permission != null)
            {
                role.AddPermission(permission);
            }
        }

        await _context.SaveChangesAsync();

        return new UserGroupDto
        {
            Id = role.Id,
            Name = role.Name,
            Description = role.Description,
            CreatedAt = role.CreatedAt,
            UpdatedAt = role.UpdatedAt,
            IsActive = role.IsActive,
            Permissions = role.Permissions.Select(p => new PermissionDto
            {
                Id = p.Id,
                Name = p.Name,
                Code = p.Code,
                Description = p.Description,
                PermissionEnum = p.PermissionEnum,
                CreatedAt = p.CreatedAt,
                UpdatedAt = p.UpdatedAt,
                IsActive = p.IsActive
            }).ToList()
        };
    }

    public async Task DeleteRoleAsync(Guid roleId)
    {
        var role = await _context.UserGroups.FindAsync(roleId);
        if (role == null)
        {
            throw new InvalidOperationException("Role not found");
        }

        role.Deactivate();
        await _context.SaveChangesAsync();
    }

    public async Task SetUserRoleAsync(Guid userId, Guid userGroupId)
    {
        var user = await _context.Users.FindAsync(userId);
        if (user == null)
        {
            throw new InvalidOperationException("User not found");
        }

        var userGroup = await _context.UserGroups.FindAsync(userGroupId);
        if (userGroup == null)
        {
            throw new InvalidOperationException("User group not found");
        }

        user.UpdateUserGroup(userGroupId);
        await _context.SaveChangesAsync();

        await _emailService.SendRoleAssignmentEmailAsync(user.Email, user.GetFullName(), userGroup.Name);
    }

    #endregion

    #region Permission Management

    public async Task<List<PermissionDto>> GetPermissionsAsync()
    {
        var permissions = await _context.Permissions
            .Where(p => p.IsActive)
            .ToListAsync();

        return permissions.Select(p => new PermissionDto
        {
            Id = p.Id,
            Name = p.Name,
            Code = p.Code,
            Description = p.Description,
            PermissionEnum = p.PermissionEnum,
            CreatedAt = p.CreatedAt,
            UpdatedAt = p.UpdatedAt,
            IsActive = p.IsActive
        }).ToList();
    }

    public async Task<List<PermissionDto>> GetUserPermissionsAsync(Guid userId)
    {
        var user = await _context.Users
            .Include(u => u.UserGroup)
            .ThenInclude(ug => ug.Permissions)
            .FirstOrDefaultAsync(u => u.Id == userId);

        if (user == null)
        {
            return new List<PermissionDto>();
        }

        return user.UserGroup.Permissions.Select(p => new PermissionDto
        {
            Id = p.Id,
            Name = p.Name,
            Code = p.Code,
            Description = p.Description,
            PermissionEnum = p.PermissionEnum,
            CreatedAt = p.CreatedAt,
            UpdatedAt = p.UpdatedAt,
            IsActive = p.IsActive
        }).ToList();
    }

    public async Task SetUserPermissionsAsync(Guid userId, List<PermissionEnums> permissions)
    {
        var user = await _context.Users
            .Include(u => u.UserGroup)
            .FirstOrDefaultAsync(u => u.Id == userId);

        if (user == null)
        {
            throw new InvalidOperationException("User not found");
        }

        // Clear existing permissions
        user.UserGroup.Permissions.Clear();

        // Add new permissions
        foreach (var permissionEnum in permissions)
        {
            var permission = await _context.Permissions.FirstOrDefaultAsync(p => p.PermissionEnum == permissionEnum);
            if (permission != null)
            {
                user.UserGroup.AddPermission(permission);
            }
        }

        await _context.SaveChangesAsync();
    }

    public async Task SetRolePermissionsAsync(Guid userGroupId, List<PermissionEnums> permissions)
    {
        var userGroup = await _context.UserGroups
            .Include(ug => ug.Permissions)
            .FirstOrDefaultAsync(ug => ug.Id == userGroupId);

        if (userGroup == null)
        {
            throw new InvalidOperationException("User group not found");
        }

        // Clear existing permissions
        userGroup.Permissions.Clear();

        // Add new permissions
        foreach (var permissionEnum in permissions)
        {
            var permission = await _context.Permissions.FirstOrDefaultAsync(p => p.PermissionEnum == permissionEnum);
            if (permission != null)
            {
                userGroup.AddPermission(permission);
            }
        }

        await _context.SaveChangesAsync();
    }

    #endregion

    #region Login Logs

    public async Task<List<UserLoginLogDto>> GetUserLoginLogsAsync(Guid userId, int count = 10)
    {
        var logs = await _context.UserLoginLogs
            .Where(log => log.UserId == userId)
            .OrderByDescending(log => log.LoginTime)
            .Take(count)
            .ToListAsync();

        return logs.Select(log => new UserLoginLogDto
        {
            Id = log.Id,
            LoginTime = log.LoginTime,
            LogoutTime = log.LogoutTime,
            IpAddress = log.IpAddress,
            UserAgent = log.UserAgent,
            DeviceType = log.DeviceType,
            Browser = log.Browser,
            OperatingSystem = log.OperatingSystem,
            IsSuccessful = log.IsSuccessful,
            FailureReason = log.FailureReason,
            SessionDuration = log.GetSessionDuration()
        }).ToList();
    }

    public async Task LogUserLoginAsync(Guid userId, bool isSuccessful, string? failureReason = null, string? ipAddress = null, string? userAgent = null)
    {
        UserLoginLog log;
        if (isSuccessful)
        {
            log = UserLoginLog.CreateSuccessfulLogin(userId, ipAddress, userAgent);
        }
        else
        {
            log = UserLoginLog.CreateFailedLogin(userId, failureReason ?? "Unknown error", ipAddress, userAgent);
        }

        _context.UserLoginLogs.Add(log);
        await _context.SaveChangesAsync();
    }

    public async Task LogUserLogoutAsync(Guid userId)
    {
        var activeSession = await _context.UserLoginLogs
            .Where(log => log.UserId == userId && log.IsSessionActive())
            .OrderByDescending(log => log.LoginTime)
            .FirstOrDefaultAsync();

        if (activeSession != null)
        {
            activeSession.RecordLogout();
            await _context.SaveChangesAsync();
        }
    }

    #endregion

    #region File Upload

    public async Task<FileUploadResponseDto> UploadProfileImageAsync(Guid userId, IFormFile file)
    {
        var fileUrl = await _fileStorageService.UploadProfileImageAsync(file, userId);
        
        var user = await _context.Users.FindAsync(userId);
        if (user != null)
        {
            user.UpdateProfileImage(fileUrl);
            await _context.SaveChangesAsync();
        }

        return new FileUploadResponseDto
        {
            FileName = file.FileName,
            FileUrl = fileUrl,
            FileSize = file.Length,
            ContentType = file.ContentType
        };
    }

    public async Task<FileUploadResponseDto> UploadResumeAsync(Guid userId, IFormFile file)
    {
        var fileUrl = await _fileStorageService.UploadResumeAsync(file, userId);
        
        var user = await _context.Users.FindAsync(userId);
        if (user != null)
        {
            // Update resume URL in user entity
            await _context.SaveChangesAsync();
        }

        return new FileUploadResponseDto
        {
            FileName = file.FileName,
            FileUrl = fileUrl,
            FileSize = file.Length,
            ContentType = file.ContentType
        };
    }

    public async Task<FileUploadResponseDto> UploadLogoAsync(Guid userId, IFormFile file)
    {
        var fileUrl = await _fileStorageService.UploadLogoAsync(file, userId);
        
        var user = await _context.Users.FindAsync(userId);
        if (user != null)
        {
            // Update logo URL in user entity
            await _context.SaveChangesAsync();
        }

        return new FileUploadResponseDto
        {
            FileName = file.FileName,
            FileUrl = fileUrl,
            FileSize = file.Length,
            ContentType = file.ContentType
        };
    }

    public async Task<FileUploadResponseDto> UploadOfficialDocumentsAsync(Guid userId, IFormFile file)
    {
        var fileUrl = await _fileStorageService.UploadOfficialDocumentsAsync(file, userId);
        
        var user = await _context.Users.FindAsync(userId);
        if (user != null)
        {
            // Update official documents URL in user entity
            await _context.SaveChangesAsync();
        }

        return new FileUploadResponseDto
        {
            FileName = file.FileName,
            FileUrl = fileUrl,
            FileSize = file.Length,
            ContentType = file.ContentType
        };
    }

    #endregion

    #region Two-Factor Authentication

    public async Task EnableTwoFactorAsync(Guid userId)
    {
        var user = await _context.Users.FindAsync(userId);
        if (user == null)
        {
            throw new InvalidOperationException("User not found");
        }

        user.SetTwoFactorEnabled(true);
        await _context.SaveChangesAsync();
    }

    public async Task DisableTwoFactorAsync(Guid userId)
    {
        var user = await _context.Users.FindAsync(userId);
        if (user == null)
        {
            throw new InvalidOperationException("User not found");
        }

        user.SetTwoFactorEnabled(false);
        await _context.SaveChangesAsync();
    }

    public async Task<bool> ValidateTwoFactorCodeAsync(Guid userId, string code)
    {
        // In a real implementation, you would validate the 2FA code
        // For now, we'll return true for demonstration
        return true;
    }

    #endregion

    #region Public Profile

    public async Task SetPublicProfileVisibilityAsync(Guid userId, bool showPublic)
    {
        var user = await _context.Users.FindAsync(userId);
        if (user == null)
        {
            throw new InvalidOperationException("User not found");
        }

        user.SetPublicProfileVisibility(showPublic);
        await _context.SaveChangesAsync();
    }

    public async Task<UserResponse?> GetPublicProfileAsync(Guid userId)
    {
        var user = await _context.Users
            .Include(u => u.UserGroup)
            .FirstOrDefaultAsync(u => u.Id == userId && u.ShowPublicProfile && u.IsActive);

        return user != null ? await MapToUserResponseAsync(user) : null;
    }

    #endregion

    #region User Statistics

    public async Task<Dictionary<string, int>> GetUserStatisticsAsync()
    {
        var totalUsers = await _context.Users.CountAsync();
        var activeUsers = await _context.Users.CountAsync(u => u.IsActive);
        var individualUsers = await _context.Users.CountAsync(u => u.UserType == UserType.Individual);
        var corporateUsers = await _context.Users.CountAsync(u => u.UserType == UserType.Corporate);
        var newUsersThisMonth = await _context.Users.CountAsync(u => u.CreatedAt >= DateTime.UtcNow.AddMonths(-1));

        return new Dictionary<string, int>
        {
            ["TotalUsers"] = totalUsers,
            ["ActiveUsers"] = activeUsers,
            ["IndividualUsers"] = individualUsers,
            ["CorporateUsers"] = corporateUsers,
            ["NewUsersThisMonth"] = newUsersThisMonth
        };
    }

    public async Task<int> GetActiveUsersCountAsync()
    {
        return await _context.Users.CountAsync(u => u.IsActive);
    }

    public async Task<int> GetNewUsersCountAsync(DateTime fromDate)
    {
        return await _context.Users.CountAsync(u => u.CreatedAt >= fromDate);
    }

    #endregion

    #region Bulk Operations

    public async Task<List<UserResponse>> GetUsersByIdsAsync(List<Guid> userIds)
    {
        var users = await _context.Users
            .Include(u => u.UserGroup)
            .ThenInclude(ug => ug.Permissions)
            .Where(u => userIds.Contains(u.Id))
            .ToListAsync();

        var userResponses = new List<UserResponse>();
        foreach (var user in users)
        {
            userResponses.Add(await MapToUserResponseAsync(user));
        }

        return userResponses;
    }

    public async Task BulkUpdateUserStatusAsync(List<Guid> userIds, bool isActive)
    {
        var users = await _context.Users.Where(u => userIds.Contains(u.Id)).ToListAsync();
        
        foreach (var user in users)
        {
            if (isActive)
            {
                user.Activate();
            }
            else
            {
                user.Deactivate();
            }
        }

        await _context.SaveChangesAsync();
    }

    public async Task BulkAssignRoleAsync(List<Guid> userIds, Guid userGroupId)
    {
        var users = await _context.Users.Where(u => userIds.Contains(u.Id)).ToListAsync();
        
        foreach (var user in users)
        {
            user.UpdateUserGroup(userGroupId);
        }

        await _context.SaveChangesAsync();
    }

    #endregion

    #region Validation

    public async Task<bool> IsEmailUniqueAsync(string email)
    {
        return !await _context.Users.AnyAsync(u => u.Email == email);
    }

    public async Task<bool> IsNationalCodeUniqueAsync(string nationalCode)
    {
        return !await _context.Users.AnyAsync(u => u.NationalCode == nationalCode);
    }

    public async Task<bool> IsCompanyNationalIdUniqueAsync(string companyNationalId)
    {
        return !await _context.Users.AnyAsync(u => u.CompanyNationalId == companyNationalId);
    }

    #endregion

    #region Security

    public async Task<bool> HasPermissionAsync(Guid userId, PermissionEnums permission)
    {
        var user = await _context.Users
            .Include(u => u.UserGroup)
            .ThenInclude(ug => ug.Permissions)
            .FirstOrDefaultAsync(u => u.Id == userId);

        if (user == null)
        {
            return false;
        }

        return user.UserGroup.HasPermission(permission);
    }

    public async Task<List<PermissionEnums>> GetUserPermissionEnumsAsync(Guid userId)
    {
        var user = await _context.Users
            .Include(u => u.UserGroup)
            .ThenInclude(ug => ug.Permissions)
            .FirstOrDefaultAsync(u => u.Id == userId);

        if (user == null)
        {
            return new List<PermissionEnums>();
        }

        return user.UserGroup.GetPermissionEnums();
    }

    public async Task<bool> IsUserInRoleAsync(Guid userId, string roleName)
    {
        var user = await _context.Users
            .Include(u => u.UserGroup)
            .FirstOrDefaultAsync(u => u.Id == userId);

        if (user == null)
        {
            return false;
        }

        return user.UserGroup.Name == roleName;
    }

    #endregion

    #region Helper Methods

    private async Task<UserResponse> MapToUserResponseAsync(User user)
    {
        return new UserResponse
        {
            Id = user.Id,
            FirstName = user.FirstName,
            LastName = user.LastName,
            Email = user.Email,
            PhoneNumber = user.PhoneNumber,
            NationalCode = user.NationalCode,
            FullName = user.GetFullName(),
            CreatedAt = user.CreatedAt,
            UpdatedAt = user.UpdatedAt,
            LastLoginAt = user.LastLoginAt,
            IsActive = user.IsActive,
            EmailConfirmed = user.EmailConfirmed,
            TwoFactorEnabled = user.TwoFactorEnabled,
            UserType = user.UserType,
            ProfileImageUrl = user.ProfileImageUrl,
            UserGroup = new UserGroupDto
            {
                Id = user.UserGroup.Id,
                Name = user.UserGroup.Name,
                Description = user.UserGroup.Description,
                CreatedAt = user.UserGroup.CreatedAt,
                UpdatedAt = user.UserGroup.UpdatedAt,
                IsActive = user.UserGroup.IsActive,
                Permissions = user.UserGroup.Permissions.Select(p => new PermissionDto
                {
                    Id = p.Id,
                    Name = p.Name,
                    Code = p.Code,
                    Description = p.Description,
                    PermissionEnum = p.PermissionEnum,
                    CreatedAt = p.CreatedAt,
                    UpdatedAt = p.UpdatedAt,
                    IsActive = p.IsActive
                }).ToList()
            },
            Roles = new List<string> { user.UserGroup.Name },
            Permissions = user.UserGroup.Permissions.Select(p => new PermissionDto
            {
                Id = p.Id,
                Name = p.Name,
                Code = p.Code,
                Description = p.Description,
                PermissionEnum = p.PermissionEnum,
                CreatedAt = p.CreatedAt,
                UpdatedAt = p.UpdatedAt,
                IsActive = p.IsActive
            }).ToList(),
            
            // Individual User Specific Fields
            BirthDate = user.BirthDate,
            EducationLevel = user.EducationLevel,
            Expertise = user.Expertise,
            ResumeUrl = user.ResumeUrl,
            Interests = user.GetInterestsList(),
            SkillLevel = user.SkillLevel,
            ResearchGateLink = user.ResearchGateLink,
            OrcidLink = user.OrcidLink,
            GoogleScholarLink = user.GoogleScholarLink,
            SavedInterests = user.GetSavedInterestsList(),
            
            // Corporate User Specific Fields
            CompanyName = user.CompanyName,
            CompanyNationalId = user.CompanyNationalId,
            RegistrationNumber = user.RegistrationNumber,
            ActivityField = user.ActivityField,
            CompanyPhone = user.CompanyPhone,
            CompanyDescription = user.CompanyDescription,
            Website = user.Website,
            LogoUrl = user.LogoUrl,
            RepresentativeName = user.RepresentativeName,
            RepresentativeEmail = user.RepresentativeEmail,
            RepresentativeNationalId = user.RepresentativeNationalId,
            RepresentativePhone = user.RepresentativePhone,
            FullAddress = user.FullAddress,
            OfficialDocumentsUrl = user.OfficialDocumentsUrl,
            ShowPublicProfile = user.ShowPublicProfile
        };
    }

    private string GenerateRandomPassword()
    {
        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789!@#$%^&*";
        var random = new Random();
        return new string(Enumerable.Repeat(chars, 12).Select(s => s[random.Next(s.Length)]).ToArray());
    }

    private string? GetClientIpAddress()
    {
        // This would be implemented based on your HTTP context
        return null;
    }

    private string? GetUserAgent()
    {
        // This would be implemented based on your HTTP context
        return null;
    }

    #endregion
} 