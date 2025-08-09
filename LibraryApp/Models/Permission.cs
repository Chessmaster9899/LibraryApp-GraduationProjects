using System.ComponentModel.DataAnnotations;

namespace LibraryApp.Models;

public enum PermissionType
{
    // Project Management
    ViewAllProjects,
    CreateProject,
    EditProject,
    DeleteProject,
    ApproveProject,
    RejectProject,
    
    // Student Management
    ViewAllStudents,
    CreateStudent,
    EditStudent,
    DeleteStudent,
    
    // Professor Management
    ViewAllProfessors,
    CreateProfessor,
    EditProfessor,
    DeleteProfessor,
    
    // Department Management
    ViewAllDepartments,
    ManageDepartments,
    
    // Gallery Management
    ManageGallery,
    CustomizeGallery,
    
    // Comment System
    AddComments,
    ModerateComments,
    
    // System Administration
    ManageUsers,
    ManageRoles,
    ViewSystemLogs,
    ManageSettings
}

public class Permission
{
    public int Id { get; set; }
    
    [Required]
    public PermissionType Type { get; set; }
    
    [Required]
    [StringLength(100)]
    public required string Name { get; set; }
    
    [StringLength(500)]
    public string? Description { get; set; }
    
    [Required]
    [StringLength(50)]
    public required string Category { get; set; }
    
    public bool IsActive { get; set; } = true;
    
    // Navigation properties
    public ICollection<RolePermission> RolePermissions { get; set; } = new List<RolePermission>();
    public ICollection<UserPermission> UserPermissions { get; set; } = new List<UserPermission>();
}

public class Role
{
    public int Id { get; set; }
    
    [Required]
    [StringLength(50)]
    public required string Name { get; set; }
    
    [StringLength(500)]
    public string? Description { get; set; }
    
    public bool IsSystemRole { get; set; } = false; // Admin, Student, Professor are system roles
    
    public bool IsActive { get; set; } = true;
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    // Navigation properties
    public ICollection<RolePermission> RolePermissions { get; set; } = new List<RolePermission>();
    public ICollection<UserRoleAssignment> UserRoles { get; set; } = new List<UserRoleAssignment>();
}

public class RolePermission
{
    public int Id { get; set; }
    
    [Required]
    public int RoleId { get; set; }
    
    [Required]
    public int PermissionId { get; set; }
    
    public DateTime GrantedAt { get; set; } = DateTime.UtcNow;
    
    public string? GrantedBy { get; set; } // User ID who granted this permission
    
    // Navigation properties
    public Role Role { get; set; } = null!;
    public Permission Permission { get; set; } = null!;
}

public class UserRoleAssignment
{
    public int Id { get; set; }
    
    [Required]
    public int UserId { get; set; }
    
    [Required]
    public int RoleId { get; set; }
    
    public DateTime AssignedAt { get; set; } = DateTime.UtcNow;
    
    public string? AssignedBy { get; set; } // User ID who assigned this role
    
    public DateTime? ExpiresAt { get; set; } // Optional expiration date
    
    // Navigation properties
    public User User { get; set; } = null!;
    public Role Role { get; set; } = null!;
}

public class UserPermission
{
    public int Id { get; set; }
    
    [Required]
    public int UserId { get; set; }
    
    [Required]
    public int PermissionId { get; set; }
    
    public bool IsGranted { get; set; } = true; // false means explicitly denied
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    public string? CreatedBy { get; set; } // User ID who granted/denied this permission
    
    public DateTime? ExpiresAt { get; set; } // Optional expiration date
    
    // Navigation properties
    public User User { get; set; } = null!;
    public Permission Permission { get; set; } = null!;
}

// View Models for Role Management
public class RoleManagementViewModel
{
    public List<Role> Roles { get; set; } = new();
    public List<Permission> AllPermissions { get; set; } = new();
    public Dictionary<int, List<Permission>> RolePermissions { get; set; } = new();
}

public class UserRoleAssignmentViewModel
{
    public User User { get; set; } = null!;
    public List<Role> AvailableRoles { get; set; } = new();
    public List<Role> AssignedRoles { get; set; } = new();
    public List<Permission> DirectPermissions { get; set; } = new();
    public List<Permission> AllPermissions { get; set; } = new();
}