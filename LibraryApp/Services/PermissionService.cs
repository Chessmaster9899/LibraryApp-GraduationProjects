using Microsoft.EntityFrameworkCore;
using LibraryApp.Data;
using LibraryApp.Models;

namespace LibraryApp.Services;

public interface IPermissionService
{
    Task<bool> HasPermissionAsync(int userId, PermissionType permission);
    Task<bool> HasPermissionAsync(string userIdString, UserRole userRole, PermissionType permission);
    Task<List<Permission>> GetUserPermissionsAsync(int userId);
    Task<List<Role>> GetUserRolesAsync(int userId);
    Task GrantPermissionAsync(int userId, PermissionType permission, string grantedBy);
    Task RevokePermissionAsync(int userId, PermissionType permission, string revokedBy);
    Task AssignRoleAsync(int userId, int roleId, string assignedBy);
    Task RemoveRoleAsync(int userId, int roleId, string removedBy);
    Task<List<Permission>> GetAllPermissionsAsync();
    Task<List<Role>> GetAllRolesAsync();
    Task<Role> CreateRoleAsync(string name, string description, List<PermissionType> permissions, string createdBy);
    Task UpdateRolePermissionsAsync(int roleId, List<PermissionType> permissions, string updatedBy);
    Task InitializeDefaultRolesAndPermissionsAsync();
}

public class PermissionService : IPermissionService
{
    private readonly LibraryContext _context;

    public PermissionService(LibraryContext context)
    {
        _context = context;
    }

    public async Task<bool> HasPermissionAsync(int userId, PermissionType permission)
    {
        // Check direct user permissions first
        var directPermission = await _context.UserPermissions
            .Include(up => up.Permission)
            .FirstOrDefaultAsync(up => up.UserId == userId && 
                                      up.Permission.Type == permission &&
                                      (up.ExpiresAt == null || up.ExpiresAt > DateTime.UtcNow));

        if (directPermission != null)
        {
            return directPermission.IsGranted;
        }

        // Check permissions through roles
        var hasRolePermission = await _context.UserRoles
            .Include(ur => ur.Role)
                .ThenInclude(r => r.RolePermissions)
                    .ThenInclude(rp => rp.Permission)
            .Where(ur => ur.UserId == userId && 
                        ur.Role.IsActive &&
                        (ur.ExpiresAt == null || ur.ExpiresAt > DateTime.UtcNow))
            .SelectMany(ur => ur.Role.RolePermissions)
            .AnyAsync(rp => rp.Permission.Type == permission && rp.Permission.IsActive);

        return hasRolePermission;
    }

    public async Task<bool> HasPermissionAsync(string userIdString, UserRole userRole, PermissionType permission)
    {
        // For backward compatibility with the current system
        // Get the actual user ID from the database
        User? user = null;
        
        switch (userRole)
        {
            case UserRole.Student:
                var student = await _context.Students.FirstOrDefaultAsync(s => s.StudentNumber == userIdString);
                if (student != null)
                {
                    user = await _context.Users.FirstOrDefaultAsync(u => u.StudentId == student.Id);
                }
                break;
            case UserRole.Professor:
                var professor = await _context.Professors.FirstOrDefaultAsync(p => p.ProfessorId == userIdString);
                if (professor != null)
                {
                    user = await _context.Users.FirstOrDefaultAsync(u => u.ProfessorId == professor.Id);
                }
                break;
            case UserRole.Admin:
                user = await _context.Users.FirstOrDefaultAsync(u => u.UserId == userIdString && u.Role == UserRole.Admin);
                break;
        }

        if (user == null)
        {
            return false;
        }

        return await HasPermissionAsync(user.Id, permission);
    }

    public async Task<List<Permission>> GetUserPermissionsAsync(int userId)
    {
        var permissions = new HashSet<Permission>();

        // Get direct permissions
        var directPermissions = await _context.UserPermissions
            .Include(up => up.Permission)
            .Where(up => up.UserId == userId && 
                        up.IsGranted &&
                        (up.ExpiresAt == null || up.ExpiresAt > DateTime.UtcNow))
            .Select(up => up.Permission)
            .ToListAsync();

        foreach (var permission in directPermissions)
        {
            permissions.Add(permission);
        }

        // Get permissions through roles
        var rolePermissions = await _context.UserRoles
            .Include(ur => ur.Role)
                .ThenInclude(r => r.RolePermissions)
                    .ThenInclude(rp => rp.Permission)
            .Where(ur => ur.UserId == userId && 
                        ur.Role.IsActive &&
                        (ur.ExpiresAt == null || ur.ExpiresAt > DateTime.UtcNow))
            .SelectMany(ur => ur.Role.RolePermissions)
            .Select(rp => rp.Permission)
            .Where(p => p.IsActive)
            .ToListAsync();

        foreach (var permission in rolePermissions)
        {
            permissions.Add(permission);
        }

        return permissions.ToList();
    }

    public async Task<List<Role>> GetUserRolesAsync(int userId)
    {
        return await _context.UserRoles
            .Include(ur => ur.Role)
            .Where(ur => ur.UserId == userId && 
                        ur.Role.IsActive &&
                        (ur.ExpiresAt == null || ur.ExpiresAt > DateTime.UtcNow))
            .Select(ur => ur.Role)
            .ToListAsync();
    }

    public async Task GrantPermissionAsync(int userId, PermissionType permission, string grantedBy)
    {
        var permissionEntity = await _context.Permissions.FirstOrDefaultAsync(p => p.Type == permission);
        if (permissionEntity == null) return;

        var existingPermission = await _context.UserPermissions
            .FirstOrDefaultAsync(up => up.UserId == userId && up.PermissionId == permissionEntity.Id);

        if (existingPermission != null)
        {
            existingPermission.IsGranted = true;
            existingPermission.CreatedAt = DateTime.UtcNow;
            existingPermission.CreatedBy = grantedBy;
        }
        else
        {
            _context.UserPermissions.Add(new UserPermission
            {
                UserId = userId,
                PermissionId = permissionEntity.Id,
                IsGranted = true,
                CreatedBy = grantedBy
            });
        }

        await _context.SaveChangesAsync();
    }

    public async Task RevokePermissionAsync(int userId, PermissionType permission, string revokedBy)
    {
        var permissionEntity = await _context.Permissions.FirstOrDefaultAsync(p => p.Type == permission);
        if (permissionEntity == null) return;

        var existingPermission = await _context.UserPermissions
            .FirstOrDefaultAsync(up => up.UserId == userId && up.PermissionId == permissionEntity.Id);

        if (existingPermission != null)
        {
            existingPermission.IsGranted = false;
            existingPermission.CreatedAt = DateTime.UtcNow;
            existingPermission.CreatedBy = revokedBy;
        }
        else
        {
            _context.UserPermissions.Add(new UserPermission
            {
                UserId = userId,
                PermissionId = permissionEntity.Id,
                IsGranted = false,
                CreatedBy = revokedBy
            });
        }

        await _context.SaveChangesAsync();
    }

    public async Task AssignRoleAsync(int userId, int roleId, string assignedBy)
    {
        var existingAssignment = await _context.UserRoles
            .FirstOrDefaultAsync(ur => ur.UserId == userId && ur.RoleId == roleId);

        if (existingAssignment == null)
        {
            _context.UserRoles.Add(new UserRoleAssignment
            {
                UserId = userId,
                RoleId = roleId,
                AssignedBy = assignedBy
            });

            await _context.SaveChangesAsync();
        }
    }

    public async Task RemoveRoleAsync(int userId, int roleId, string removedBy)
    {
        var existingAssignment = await _context.UserRoles
            .FirstOrDefaultAsync(ur => ur.UserId == userId && ur.RoleId == roleId);

        if (existingAssignment != null)
        {
            _context.UserRoles.Remove(existingAssignment);
            await _context.SaveChangesAsync();
        }
    }

    public async Task<List<Permission>> GetAllPermissionsAsync()
    {
        return await _context.Permissions
            .Where(p => p.IsActive)
            .OrderBy(p => p.Category)
            .ThenBy(p => p.Name)
            .ToListAsync();
    }

    public async Task<List<Role>> GetAllRolesAsync()
    {
        return await _context.Roles
            .Where(r => r.IsActive)
            .OrderBy(r => r.Name)
            .ToListAsync();
    }

    public async Task<Role> CreateRoleAsync(string name, string description, List<PermissionType> permissions, string createdBy)
    {
        var role = new Role
        {
            Name = name,
            Description = description,
            IsSystemRole = false
        };

        _context.Roles.Add(role);
        await _context.SaveChangesAsync();

        // Add permissions to the role
        await UpdateRolePermissionsAsync(role.Id, permissions, createdBy);

        return role;
    }

    public async Task UpdateRolePermissionsAsync(int roleId, List<PermissionType> permissions, string updatedBy)
    {
        // Remove existing permissions
        var existingPermissions = await _context.RolePermissions
            .Where(rp => rp.RoleId == roleId)
            .ToListAsync();

        _context.RolePermissions.RemoveRange(existingPermissions);

        // Add new permissions
        foreach (var permissionType in permissions)
        {
            var permission = await _context.Permissions.FirstOrDefaultAsync(p => p.Type == permissionType);
            if (permission != null)
            {
                _context.RolePermissions.Add(new RolePermission
                {
                    RoleId = roleId,
                    PermissionId = permission.Id,
                    GrantedBy = updatedBy
                });
            }
        }

        await _context.SaveChangesAsync();
    }

    public async Task InitializeDefaultRolesAndPermissionsAsync()
    {
        // Create permissions if they don't exist
        foreach (PermissionType permissionType in Enum.GetValues<PermissionType>())
        {
            var exists = await _context.Permissions.AnyAsync(p => p.Type == permissionType);
            if (!exists)
            {
                var permission = new Permission
                {
                    Type = permissionType,
                    Name = permissionType.ToString().SplitCamelCase(),
                    Description = GetPermissionDescription(permissionType),
                    Category = GetPermissionCategory(permissionType)
                };

                _context.Permissions.Add(permission);
            }
        }

        // Create default roles if they don't exist
        await CreateDefaultRoleIfNotExists("Student", "Default role for students", new[]
        {
            PermissionType.ViewAllProjects,
            PermissionType.CreateProject,
            PermissionType.EditProject,
            PermissionType.AddComments
        });

        await CreateDefaultRoleIfNotExists("Professor", "Default role for professors", new[]
        {
            PermissionType.ViewAllProjects,
            PermissionType.ViewAllStudents,
            PermissionType.EditProject,
            PermissionType.ApproveProject,
            PermissionType.RejectProject,
            PermissionType.AddComments,
            PermissionType.ModerateComments
        });

        await CreateDefaultRoleIfNotExists("Admin", "System administrator role", Enum.GetValues<PermissionType>());

        await _context.SaveChangesAsync();
    }

    private async Task CreateDefaultRoleIfNotExists(string name, string description, PermissionType[] permissions)
    {
        var exists = await _context.Roles.AnyAsync(r => r.Name == name);
        if (!exists)
        {
            var role = new Role
            {
                Name = name,
                Description = description,
                IsSystemRole = true
            };

            _context.Roles.Add(role);
            await _context.SaveChangesAsync();

            // Add permissions
            foreach (var permissionType in permissions)
            {
                var permission = await _context.Permissions.FirstOrDefaultAsync(p => p.Type == permissionType);
                if (permission != null)
                {
                    _context.RolePermissions.Add(new RolePermission
                    {
                        RoleId = role.Id,
                        PermissionId = permission.Id,
                        GrantedBy = "System"
                    });
                }
            }
        }
    }

    private static string GetPermissionDescription(PermissionType permission)
    {
        return permission switch
        {
            PermissionType.ViewAllProjects => "View all projects in the system",
            PermissionType.CreateProject => "Create new projects",
            PermissionType.EditProject => "Edit existing projects",
            PermissionType.DeleteProject => "Delete projects",
            PermissionType.ApproveProject => "Approve submitted projects",
            PermissionType.RejectProject => "Reject submitted projects",
            PermissionType.ViewAllStudents => "View all students in the system",
            PermissionType.CreateStudent => "Add new students",
            PermissionType.EditStudent => "Edit student information",
            PermissionType.DeleteStudent => "Remove students from the system",
            PermissionType.ViewAllProfessors => "View all professors in the system",
            PermissionType.CreateProfessor => "Add new professors",
            PermissionType.EditProfessor => "Edit professor information",
            PermissionType.DeleteProfessor => "Remove professors from the system",
            PermissionType.ViewAllDepartments => "View all departments",
            PermissionType.ManageDepartments => "Create, edit, and delete departments",
            PermissionType.ManageGallery => "Manage project gallery content",
            PermissionType.CustomizeGallery => "Customize gallery appearance and settings",
            PermissionType.AddComments => "Add comments to projects",
            PermissionType.ModerateComments => "Moderate and manage comments",
            PermissionType.ManageUsers => "Manage user accounts",
            PermissionType.ManageRoles => "Create and manage user roles",
            PermissionType.ViewSystemLogs => "View system audit logs",
            PermissionType.ManageSettings => "Manage system settings",
            _ => permission.ToString()
        };
    }

    private static string GetPermissionCategory(PermissionType permission)
    {
        return permission switch
        {
            PermissionType.ViewAllProjects or 
            PermissionType.CreateProject or 
            PermissionType.EditProject or 
            PermissionType.DeleteProject or 
            PermissionType.ApproveProject or 
            PermissionType.RejectProject => "Project Management",
            
            PermissionType.ViewAllStudents or 
            PermissionType.CreateStudent or 
            PermissionType.EditStudent or 
            PermissionType.DeleteStudent => "Student Management",
            
            PermissionType.ViewAllProfessors or 
            PermissionType.CreateProfessor or 
            PermissionType.EditProfessor or 
            PermissionType.DeleteProfessor => "Professor Management",
            
            PermissionType.ViewAllDepartments or 
            PermissionType.ManageDepartments => "Department Management",
            
            PermissionType.ManageGallery or 
            PermissionType.CustomizeGallery => "Gallery Management",
            
            PermissionType.AddComments or 
            PermissionType.ModerateComments => "Comment System",
            
            PermissionType.ManageUsers or 
            PermissionType.ManageRoles or 
            PermissionType.ViewSystemLogs or 
            PermissionType.ManageSettings => "System Administration",
            
            _ => "General"
        };
    }
}

// Extension method for string utilities
public static class StringExtensions
{
    public static string SplitCamelCase(this string input)
    {
        return System.Text.RegularExpressions.Regex.Replace(input, "([A-Z])", " $1").Trim();
    }
}