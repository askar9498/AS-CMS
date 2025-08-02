using System.ComponentModel.DataAnnotations;

namespace AS_CMS.Domain.Entities;

public class Permission
{
    public Guid Id { get; private set; }
    
    [Required]
    [MaxLength(100)]
    public string Name { get; private set; } = string.Empty;
    
    [Required]
    [MaxLength(50)]
    public string Code { get; private set; } = string.Empty;
    
    [MaxLength(500)]
    public string? Description { get; private set; }
    
    public PermissionEnums PermissionEnum { get; private set; }
    
    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }
    
    public bool IsActive { get; private set; } = true;
    
    // Navigation properties
    public virtual ICollection<UserGroup> UserGroups { get; private set; } = new List<UserGroup>();

    // Private constructor for EF Core
    private Permission() { }

    // Factory method for creating new permissions
    public static Permission Create(string name, string code, PermissionEnums permissionEnum, string? description = null)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Name is required", nameof(name));

        if (string.IsNullOrWhiteSpace(code))
            throw new ArgumentException("Code is required", nameof(code));

        return new Permission
        {
            Id = Guid.NewGuid(),
            Name = name.Trim(),
            Code = code.Trim(),
            PermissionEnum = permissionEnum,
            Description = description?.Trim(),
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
    }

    // Update permission
    public void Update(string name, string code, string? description = null)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Name is required", nameof(name));

        if (string.IsNullOrWhiteSpace(code))
            throw new ArgumentException("Code is required", nameof(code));

        Name = name.Trim();
        Code = code.Trim();
        Description = description?.Trim();
        UpdatedAt = DateTime.UtcNow;
    }

    // Deactivate permission
    public void Deactivate()
    {
        IsActive = false;
        UpdatedAt = DateTime.UtcNow;
    }

    // Activate permission
    public void Activate()
    {
        IsActive = true;
        UpdatedAt = DateTime.UtcNow;
    }

    // Override Equals for comparison
    public override bool Equals(object? obj)
    {
        if (obj is not Permission other)
            return false;

        return PermissionEnum == other.PermissionEnum;
    }

    public override int GetHashCode()
    {
        return PermissionEnum.GetHashCode();
    }

    public static bool operator ==(Permission? left, Permission? right)
    {
        return EqualityComparer<Permission>.Default.Equals(left, right);
    }

    public static bool operator !=(Permission? left, Permission? right)
    {
        return !(left == right);
    }
}

public enum PermissionEnums
{
    // User Management
    GetUser = 0,
    RegisterUser = 1,
    GetRoles = 2,
    GetPermissionsOfUser = 3,
    ResetPassword = 4,
    SetRoleToUser = 5,
    DeleteUser = 6,
    UpdateUser = 7,
    GetUsersByFilter = 8,
    GetUsers = 9,
    GetPermissions = 10,
    GetUserByEmail = 11,
    SetAccuracyToUser = 12,
    SearchUser = 13,
    GetUserLoginLogs = 48,
    SetUserPermissions = 46,
    AddRole = 47,
    
    // Content Management
    GetAllPosts = 15,
    RegisterPosts = 16,
    SearchPost = 17,
    GetPostByTitle = 18,
    GetAllPostTypes = 19,
    CreatePost = 20,
    UpdatePost = 21,
    DeletePost = 22,
    GetPostsByCategory = 23,
    GetAllCategories = 24,
    GetAllTags = 25,
    GetAllPostStatus = 26,
    UpdateCategories = 27,
    
    // Innovation Management
    GetAllInnovations = 29,
    CreateInnovation = 30,
    UpdateInnovation = 31,
    DeleteInnovation = 32,
    SetIdeaSubmissionAccuracy = 14,
    
    // Dynamic Pages
    GetAllDynamicPages = 33,
    GetDynamicPage = 34,
    CreateDynamicPage = 35,
    UpdateDynamicPage = 36,
    DeleteDynamicPage = 37,
    SearchDynamicPages = 38,
    
    // Menu Management
    GetAllMenuItems = 39,
    GetMenuItem = 40,
    GetRootMenuItems = 41,
    GetChildMenuItems = 42,
    CreateMenuItem = 43,
    UpdateMenuItem = 44,
    DeleteMenuItem = 45,
    
    // Contact and Support
    GetAllAdminTickets = 49,
    GetUserTickets = 50,
    GetAllContactUs = 51,
    CreateContactUs = 52,
    UpdateContactUs = 53,
    DeleteContactUs = 54,
    SearchContactUs = 55,
    
    // Menu Access Permissions
    ViewCompleteProfileMenu = 2001,
    ViewUserManageMenu = 2002,
    ViewIdeaSubmissionsMenu = 2003,
    ViewMenuManageMenu = 2004,
    ViewDynamicPagesMenu = 2005,
    ViewBlogManageMenu = 2006,
    ViewInnovationsMenu = 2007,
    ViewContactUsMenu = 2008,
    ViewTicketGroupsMenu = 2009,
    ViewSupportMenu = 2010,
    ViewUserSupportMenu = 2011,
    
    // Ticket Groups
    CreateTicketGroupButton = 4001,
    EditTicketGroupButton = 4002,
    DeleteTicketGroupButton = 4003,
    
    // Admin Tickets
    SendAdminMessageButton = 4101,
    CloseTicketButton = 4102,
    DownloadAttachmentButton = 4103,
    GetUserPermissions = 4104
} 