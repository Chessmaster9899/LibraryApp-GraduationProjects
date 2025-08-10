using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using LibraryApp.Data;
using LibraryApp.Models;
using LibraryApp.Services;
using LibraryApp.Attributes;
using System.ComponentModel.DataAnnotations;

namespace LibraryApp.Controllers;

[AdminOnly]
public class RoleManagementController : BaseController
{
    private readonly LibraryContext _context;
    private readonly IPermissionService _permissionService;

    public RoleManagementController(
        LibraryContext context, 
        IUniversitySettingsService universitySettings, 
        ISessionService sessionService,
        IPermissionService permissionService) 
        : base(universitySettings, sessionService)
    {
        _context = context;
        _permissionService = permissionService;
    }

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var roles = await _context.Roles
            .Include(r => r.RolePermissions)
                .ThenInclude(rp => rp.Permission)
            .OrderBy(r => r.Name)
            .ToListAsync();

        var allPermissions = await _context.Permissions
            .OrderBy(p => p.Category)
            .ThenBy(p => p.Name)
            .ToListAsync();

        var rolePermissions = new Dictionary<int, List<Permission>>();
        foreach (var role in roles)
        {
            rolePermissions[role.Id] = role.RolePermissions
                .Select(rp => rp.Permission)
                .OrderBy(p => p.Category)
                .ThenBy(p => p.Name)
                .ToList();
        }

        var viewModel = new RoleManagementViewModel
        {
            Roles = roles,
            AllPermissions = allPermissions,
            RolePermissions = rolePermissions
        };

        return View(viewModel);
    }

    [HttpGet]
    public async Task<IActionResult> UserRoles()
    {
        var users = await _context.Users
            .Include(u => u.Student)
                .ThenInclude(s => s!.Department)
            .Include(u => u.Professor)
                .ThenInclude(p => p!.Department)
            .OrderBy(u => u.Role)
            .ThenBy(u => u.UserId)
            .ToListAsync();

        var userRoles = await _context.UserRoles
            .Include(ur => ur.Role)
            .Include(ur => ur.User)
            .ToListAsync();

        var allRoles = await _context.Roles
            .Where(r => r.IsActive)
            .OrderBy(r => r.Name)
            .ToListAsync();

        var userRoleAssignments = new Dictionary<int, List<Role>>();
        foreach (var user in users)
        {
            userRoleAssignments[user.Id] = userRoles
                .Where(ur => ur.UserId == user.Id && (ur.ExpiresAt == null || ur.ExpiresAt > DateTime.UtcNow))
                .Select(ur => ur.Role)
                .ToList();
        }

        var viewModel = new UserRoleManagementViewModel
        {
            Users = users,
            AllRoles = allRoles,
            UserRoleAssignments = userRoleAssignments
        };

        return View(viewModel);
    }

    [HttpPost]
    public async Task<IActionResult> AssignRole(int userId, int roleId)
    {
        try
        {
            var user = await _context.Users.FindAsync(userId);
            var role = await _context.Roles.FindAsync(roleId);

            if (user == null || role == null)
            {
                return Json(new { success = false, message = "User or role not found" });
            }

            // Check if assignment already exists
            var existingAssignment = await _context.UserRoles
                .FirstOrDefaultAsync(ur => ur.UserId == userId && ur.RoleId == roleId);

            if (existingAssignment != null)
            {
                return Json(new { success = false, message = "User already has this role" });
            }

            await _permissionService.AssignRoleAsync(userId, roleId, CurrentUserId!);

            return Json(new { 
                success = true, 
                message = $"Role '{role.Name}' assigned to user successfully" 
            });
        }
        catch (Exception ex)
        {
            return Json(new { success = false, message = ex.Message });
        }
    }

    [HttpPost]
    public async Task<IActionResult> RemoveRole(int userId, int roleId)
    {
        try
        {
            var user = await _context.Users.FindAsync(userId);
            var role = await _context.Roles.FindAsync(roleId);

            if (user == null || role == null)
            {
                return Json(new { success = false, message = "User or role not found" });
            }

            await _permissionService.RemoveRoleAsync(userId, roleId, CurrentUserId!);

            return Json(new { 
                success = true, 
                message = $"Role '{role.Name}' removed from user successfully" 
            });
        }
        catch (Exception ex)
        {
            return Json(new { success = false, message = ex.Message });
        }
    }

    [HttpGet]
    public async Task<IActionResult> CreateRole()
    {
        var allPermissions = await _context.Permissions
            .OrderBy(p => p.Category)
            .ThenBy(p => p.Name)
            .ToListAsync();

        var viewModel = new CreateRoleViewModel
        {
            AllPermissions = allPermissions
        };

        return View(viewModel);
    }

    [HttpPost]
    public async Task<IActionResult> CreateRole(CreateRoleViewModel model)
    {
        if (!ModelState.IsValid)
        {
            model.AllPermissions = await _context.Permissions
                .OrderBy(p => p.Category)
                .ThenBy(p => p.Name)
                .ToListAsync();
            return View(model);
        }

        try
        {
            var role = await _permissionService.CreateRoleAsync(
                model.Name, 
                model.Description ?? "", 
                model.SelectedPermissions, 
                CurrentUserId!);

            TempData["Success"] = $"Role '{model.Name}' created successfully";
            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            ModelState.AddModelError("", ex.Message);
            model.AllPermissions = await _context.Permissions
                .OrderBy(p => p.Category)
                .ThenBy(p => p.Name)
                .ToListAsync();
            return View(model);
        }
    }

    [HttpGet]
    public async Task<IActionResult> EditRole(int id)
    {
        var role = await _context.Roles
            .Include(r => r.RolePermissions)
                .ThenInclude(rp => rp.Permission)
            .FirstOrDefaultAsync(r => r.Id == id);

        if (role == null)
        {
            return NotFound();
        }

        var allPermissions = await _context.Permissions
            .OrderBy(p => p.Category)
            .ThenBy(p => p.Name)
            .ToListAsync();

        var currentPermissions = role.RolePermissions
            .Select(rp => rp.Permission.Type)
            .ToList();

        var viewModel = new EditRoleViewModel
        {
            Id = role.Id,
            Name = role.Name,
            Description = role.Description,
            AllPermissions = allPermissions,
            SelectedPermissions = currentPermissions,
            IsSystemRole = role.IsSystemRole
        };

        return View(viewModel);
    }

    [HttpPost]
    public async Task<IActionResult> EditRole(EditRoleViewModel model)
    {
        if (!ModelState.IsValid)
        {
            model.AllPermissions = await _context.Permissions
                .OrderBy(p => p.Category)
                .ThenBy(p => p.Name)
                .ToListAsync();
            return View(model);
        }

        try
        {
            await _permissionService.UpdateRolePermissionsAsync(
                model.Id, 
                model.SelectedPermissions, 
                CurrentUserId!);

            // Update role name and description if not system role
            if (!model.IsSystemRole)
            {
                var role = await _context.Roles.FindAsync(model.Id);
                if (role != null)
                {
                    role.Name = model.Name;
                    role.Description = model.Description;
                    await _context.SaveChangesAsync();
                }
            }

            TempData["Success"] = $"Role '{model.Name}' updated successfully";
            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            ModelState.AddModelError("", ex.Message);
            model.AllPermissions = await _context.Permissions
                .OrderBy(p => p.Category)
                .ThenBy(p => p.Name)
                .ToListAsync();
            return View(model);
        }
    }

    [HttpPost]
    public async Task<IActionResult> DeleteRole(int id)
    {
        try
        {
            var role = await _context.Roles.FindAsync(id);
            if (role == null)
            {
                return Json(new { success = false, message = "Role not found" });
            }

            if (role.IsSystemRole)
            {
                return Json(new { success = false, message = "Cannot delete system roles" });
            }

            // Check if role is assigned to any users
            var userCount = await _context.UserRoles.CountAsync(ur => ur.RoleId == id);
            if (userCount > 0)
            {
                return Json(new { 
                    success = false, 
                    message = $"Cannot delete role. It is assigned to {userCount} user(s)" 
                });
            }

            // Remove role permissions first
            var rolePermissions = await _context.RolePermissions
                .Where(rp => rp.RoleId == id)
                .ToListAsync();
            _context.RolePermissions.RemoveRange(rolePermissions);

            // Remove the role
            _context.Roles.Remove(role);
            await _context.SaveChangesAsync();

            return Json(new { 
                success = true, 
                message = $"Role '{role.Name}' deleted successfully" 
            });
        }
        catch (Exception ex)
        {
            return Json(new { success = false, message = ex.Message });
        }
    }
}