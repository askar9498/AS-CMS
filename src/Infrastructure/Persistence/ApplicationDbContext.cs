using Microsoft.EntityFrameworkCore;
using AS_CMS.Domain.Entities;

namespace AS_CMS.Infrastructure.Persistence;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
    }

    public DbSet<User> Users { get; set; }
    public DbSet<UserGroup> UserGroups { get; set; }
    public DbSet<Permission> Permissions { get; set; }
    public DbSet<UserLoginLog> UserLoginLogs { get; set; }
    public DbSet<RefreshToken> RefreshTokens { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // User configuration
        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Email).IsRequired().HasMaxLength(255);
            entity.HasIndex(e => e.Email).IsUnique();
            entity.Property(e => e.FirstName).IsRequired().HasMaxLength(100);
            entity.Property(e => e.LastName).IsRequired().HasMaxLength(100);
            entity.Property(e => e.PasswordHash).IsRequired().HasMaxLength(255);
            entity.Property(e => e.PhoneNumber).HasMaxLength(20);
            entity.Property(e => e.NationalCode).HasMaxLength(50);
            entity.Property(e => e.CreatedAt).IsRequired();
            entity.Property(e => e.UpdatedAt).IsRequired();
            entity.Property(e => e.LastLoginAt);
            entity.Property(e => e.IsActive).IsRequired();
            entity.Property(e => e.EmailConfirmed).IsRequired();
            entity.Property(e => e.TwoFactorEnabled).IsRequired();
            entity.Property(e => e.UserType).IsRequired();
            entity.Property(e => e.ProfileImageUrl).HasMaxLength(500);
            
            // Individual user specific fields
            entity.Property(e => e.BirthDate);
            entity.Property(e => e.EducationLevel).HasMaxLength(100);
            entity.Property(e => e.Expertise).HasMaxLength(255);
            entity.Property(e => e.ResumeUrl).HasMaxLength(500);
            entity.Property(e => e.Interests); // JSON string
            entity.Property(e => e.SkillLevel).HasMaxLength(100);
            entity.Property(e => e.ResearchGateLink).HasMaxLength(255);
            entity.Property(e => e.OrcidLink).HasMaxLength(255);
            entity.Property(e => e.GoogleScholarLink).HasMaxLength(255);
            entity.Property(e => e.SavedInterests); // JSON string
            
            // Corporate user specific fields
            entity.Property(e => e.CompanyName).HasMaxLength(255);
            entity.Property(e => e.CompanyNationalId).HasMaxLength(50);
            entity.Property(e => e.RegistrationNumber).HasMaxLength(50);
            entity.Property(e => e.ActivityField).HasMaxLength(255);
            entity.Property(e => e.CompanyPhone).HasMaxLength(255);
            entity.Property(e => e.CompanyDescription).HasMaxLength(255);
            entity.Property(e => e.Website).HasMaxLength(255);
            entity.Property(e => e.LogoUrl).HasMaxLength(255);
            entity.Property(e => e.RepresentativeName).HasMaxLength(255);
            entity.Property(e => e.RepresentativeEmail).HasMaxLength(255);
            entity.Property(e => e.RepresentativeNationalId).HasMaxLength(50);
            entity.Property(e => e.RepresentativePhone).HasMaxLength(20);
            entity.Property(e => e.FullAddress).HasMaxLength(500);
            entity.Property(e => e.OfficialDocumentsUrl).HasMaxLength(500);
            entity.Property(e => e.ShowPublicProfile).IsRequired();
            
            // Relationships
            entity.HasOne(e => e.UserGroup)
                .WithMany(e => e.Users)
                .HasForeignKey(e => e.UserGroupId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // UserGroup configuration
        modelBuilder.Entity<UserGroup>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
            entity.HasIndex(e => e.Name).IsUnique();
            entity.Property(e => e.Description).HasMaxLength(500);
            entity.Property(e => e.CreatedAt).IsRequired();
            entity.Property(e => e.UpdatedAt).IsRequired();
            entity.Property(e => e.IsActive).IsRequired();
        });

        // Permission configuration
        modelBuilder.Entity<Permission>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Code).IsRequired().HasMaxLength(50);
            entity.Property(e => e.Description).HasMaxLength(500);
            entity.Property(e => e.PermissionEnum).IsRequired();
            entity.Property(e => e.CreatedAt).IsRequired();
            entity.Property(e => e.UpdatedAt).IsRequired();
            entity.Property(e => e.IsActive).IsRequired();
            
            entity.HasIndex(e => e.PermissionEnum).IsUnique();
        });

        // UserGroup-Permission many-to-many relationship
        modelBuilder.Entity<UserGroup>()
            .HasMany(ug => ug.Permissions)
            .WithMany(p => p.UserGroups)
            .UsingEntity(j => j.ToTable("UserGroupPermissions"));

        // UserLoginLog configuration
        modelBuilder.Entity<UserLoginLog>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.LoginTime).IsRequired();
            entity.Property(e => e.LogoutTime);
            entity.Property(e => e.IpAddress).HasMaxLength(45);
            entity.Property(e => e.UserAgent).HasMaxLength(500);
            entity.Property(e => e.DeviceType).HasMaxLength(100);
            entity.Property(e => e.Browser).HasMaxLength(100);
            entity.Property(e => e.OperatingSystem).HasMaxLength(100);
            entity.Property(e => e.IsSuccessful).IsRequired();
            entity.Property(e => e.FailureReason).HasMaxLength(500);
            entity.Property(e => e.CreatedAt).IsRequired();
            
            entity.HasOne(e => e.User)
                .WithMany(e => e.LoginLogs)
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // RefreshToken configuration
        modelBuilder.Entity<RefreshToken>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Token).IsRequired().HasMaxLength(255);
            entity.HasIndex(e => e.Token).IsUnique();
            entity.Property(e => e.ExpiresAt).IsRequired();
            entity.Property(e => e.CreatedAt).IsRequired();
            entity.Property(e => e.RevokedBy).HasMaxLength(255);
            entity.Property(e => e.ReplacedByToken).HasMaxLength(255);
            
            entity.HasOne(e => e.User)
                .WithMany(e => e.RefreshTokens)
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // Seed default data
        SeedDefaultData(modelBuilder);
    }

    private static void SeedDefaultData(ModelBuilder modelBuilder)
    {
        // Seed default user groups
        var adminGroupId = Guid.NewGuid();
        var individualGroupId = Guid.NewGuid();
        var corporateGroupId = Guid.NewGuid();

        var userGroups = new[]
        {
            new { Id = adminGroupId, Name = "Admin", Description = "Administrator with full access", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow, IsActive = true },
            new { Id = individualGroupId, Name = "Individual", Description = "Default group for individual users", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow, IsActive = true },
            new { Id = corporateGroupId, Name = "Corporate", Description = "Default group for corporate users", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow, IsActive = true }
        };

        modelBuilder.Entity<UserGroup>().HasData(userGroups);

        // Seed default permissions
        var permissions = new[]
        {
            new { Id = Guid.NewGuid(), Name = "Get User", Code = "GET_USER", Description = "Can view user details", PermissionEnum = PermissionEnums.GetUser, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow, IsActive = true },
            new { Id = Guid.NewGuid(), Name = "Register User", Code = "REGISTER_USER", Description = "Can register new users", PermissionEnum = PermissionEnums.RegisterUser, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow, IsActive = true },
            new { Id = Guid.NewGuid(), Name = "Get Roles", Code = "GET_ROLES", Description = "Can view roles", PermissionEnum = PermissionEnums.GetRoles, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow, IsActive = true },
            new { Id = Guid.NewGuid(), Name = "Get Permissions", Code = "GET_PERMISSIONS", Description = "Can view permissions", PermissionEnum = PermissionEnums.GetPermissions, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow, IsActive = true },
            new { Id = Guid.NewGuid(), Name = "Get Users", Code = "GET_USERS", Description = "Can view all users", PermissionEnum = PermissionEnums.GetUsers, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow, IsActive = true },
            new { Id = Guid.NewGuid(), Name = "Update User", Code = "UPDATE_USER", Description = "Can update user information", PermissionEnum = PermissionEnums.UpdateUser, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow, IsActive = true },
            new { Id = Guid.NewGuid(), Name = "Delete User", Code = "DELETE_USER", Description = "Can delete users", PermissionEnum = PermissionEnums.DeleteUser, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow, IsActive = true },
            new { Id = Guid.NewGuid(), Name = "Search User", Code = "SEARCH_USER", Description = "Can search users", PermissionEnum = PermissionEnums.SearchUser, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow, IsActive = true },
            new { Id = Guid.NewGuid(), Name = "Reset Password", Code = "RESET_PASSWORD", Description = "Can reset user passwords", PermissionEnum = PermissionEnums.ResetPassword, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow, IsActive = true },
            new { Id = Guid.NewGuid(), Name = "Set Role", Code = "SET_ROLE", Description = "Can assign roles to users", PermissionEnum = PermissionEnums.SetRoleToUser, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow, IsActive = true },
            new { Id = Guid.NewGuid(), Name = "Add Role", Code = "ADD_ROLE", Description = "Can create new roles", PermissionEnum = PermissionEnums.AddRole, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow, IsActive = true },
            new { Id = Guid.NewGuid(), Name = "Get User Permissions", Code = "GET_USER_PERMISSIONS", Description = "Can view user permissions", PermissionEnum = PermissionEnums.GetUserPermissions, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow, IsActive = true },
            new { Id = Guid.NewGuid(), Name = "Set User Permissions", Code = "SET_USER_PERMISSIONS", Description = "Can set user permissions", PermissionEnum = PermissionEnums.SetUserPermissions, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow, IsActive = true },
            new { Id = Guid.NewGuid(), Name = "Get User Login Logs", Code = "GET_USER_LOGIN_LOGS", Description = "Can view user login logs", PermissionEnum = PermissionEnums.GetUserLoginLogs, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow, IsActive = true },
            new { Id = Guid.NewGuid(), Name = "Set Accuracy", Code = "SET_ACCURACY", Description = "Can set user accuracy", PermissionEnum = PermissionEnums.SetAccuracyToUser, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow, IsActive = true }
        };

        modelBuilder.Entity<Permission>().HasData(permissions);
    }
} 