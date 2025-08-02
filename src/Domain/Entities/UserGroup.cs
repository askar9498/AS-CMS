using System.ComponentModel.DataAnnotations;

namespace AS_CMS.Domain.Entities;

public class UserGroup
{
    public Guid Id { get; private set; }
    
    [Required]
    [MaxLength(100)]
    public string Name { get; private set; } = string.Empty;
    
    [MaxLength(500)]
    public string? Description { get; private set; }
    
    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }
    
    public bool IsActive { get; private set; } = true;
    
    // Navigation properties
    public virtual ICollection<User> Users { get; private set; } = new List<User>();
    public virtual ICollection<Permission> Permissions { get; private set; } = new List<Permission>();

    // Private constructor for EF Core
    private UserGroup() { }

    // Factory method for creating new user groups
    public static UserGroup Create(string name, string? description = null)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Name is required", nameof(name));

        return new UserGroup
        {
            Id = Guid.NewGuid(),
            Name = name.Trim(),
            Description = description?.Trim(),
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
    }

    // Update user group
    public void Update(string name, string? description = null)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Name is required", nameof(name));

        Name = name.Trim();
        Description = description?.Trim();
        UpdatedAt = DateTime.UtcNow;
    }

    // Add user to group
    public void AddUser(User user)
    {
        if (user == null)
            throw new ArgumentNullException(nameof(user));

        if (!Users.Any(u => u.Id == user.Id))
        {
            Users.Add(user);
            UpdatedAt = DateTime.UtcNow;
        }
    }

    // Remove user from group
    public void RemoveUser(User user)
    {
        if (user == null)
            throw new ArgumentNullException(nameof(user));

        var userToRemove = Users.FirstOrDefault(u => u.Id == user.Id);
        if (userToRemove != null)
        {
            Users.Remove(userToRemove);
            UpdatedAt = DateTime.UtcNow;
        }
    }

    // Add permission to group
    public void AddPermission(Permission permission)
    {
        if (permission == null)
            throw new ArgumentNullException(nameof(permission));

        if (!Permissions.Any(p => p.Id == permission.Id))
        {
            Permissions.Add(permission);
            UpdatedAt = DateTime.UtcNow;
        }
    }

    // Remove permission from group
    public void RemovePermission(Permission permission)
    {
        if (permission == null)
            throw new ArgumentNullException(nameof(permission));

        var permissionToRemove = Permissions.FirstOrDefault(p => p.Id == permission.Id);
        if (permissionToRemove != null)
        {
            Permissions.Remove(permissionToRemove);
            UpdatedAt = DateTime.UtcNow;
        }
    }

    // Get all permission enums for this group
    public List<PermissionEnums> GetPermissionEnums()
    {
        return Permissions.Select(p => p.PermissionEnum).ToList();
    }

    // Check if group has specific permission
    public bool HasPermission(PermissionEnums permissionEnum)
    {
        return Permissions.Any(p => p.PermissionEnum == permissionEnum);
    }

    // Deactivate group
    public void Deactivate()
    {
        IsActive = false;
        UpdatedAt = DateTime.UtcNow;
    }

    // Activate group
    public void Activate()
    {
        IsActive = true;
        UpdatedAt = DateTime.UtcNow;
    }
} 