using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using LibraryApp.Models;
using LibraryApp.Services;

namespace LibraryApp.Attributes;

public class RequirePermissionAttribute : Attribute, IAsyncAuthorizationFilter
{
    private readonly PermissionType _permission;

    public RequirePermissionAttribute(PermissionType permission)
    {
        _permission = permission;
    }

    public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
    {
        var permissionService = context.HttpContext.RequestServices.GetService<IPermissionService>();
        var sessionService = context.HttpContext.RequestServices.GetService<ISessionService>();

        if (permissionService == null || sessionService == null)
        {
            context.Result = new ForbidResult();
            return;
        }

        // Check if user is authenticated
        if (!sessionService.IsAuthenticated(context.HttpContext))
        {
            context.Result = new RedirectToActionResult("Login", "Auth", null);
            return;
        }

        var userId = sessionService.GetUserId(context.HttpContext);
        var userRole = sessionService.GetUserRole(context.HttpContext);

        if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(userRole))
        {
            context.Result = new RedirectToActionResult("Login", "Auth", null);
            return;
        }

        // Parse user role
        if (!Enum.TryParse<UserRole>(userRole, out var userRoleEnum))
        {
            context.Result = new ForbidResult();
            return;
        }

        // Check permission
        var hasPermission = await permissionService.HasPermissionAsync(userId, userRoleEnum, _permission);

        if (!hasPermission)
        {
            context.Result = new RedirectToActionResult("AccessDenied", "Error", null);
            return;
        }
    }
}

// Multiple permissions attribute
public class RequireAnyPermissionAttribute : Attribute, IAsyncAuthorizationFilter
{
    private readonly PermissionType[] _permissions;

    public RequireAnyPermissionAttribute(params PermissionType[] permissions)
    {
        _permissions = permissions;
    }

    public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
    {
        var permissionService = context.HttpContext.RequestServices.GetService<IPermissionService>();
        var sessionService = context.HttpContext.RequestServices.GetService<ISessionService>();

        if (permissionService == null || sessionService == null)
        {
            context.Result = new ForbidResult();
            return;
        }

        // Check if user is authenticated
        if (!sessionService.IsAuthenticated(context.HttpContext))
        {
            context.Result = new RedirectToActionResult("Login", "Auth", null);
            return;
        }

        var userId = sessionService.GetUserId(context.HttpContext);
        var userRole = sessionService.GetUserRole(context.HttpContext);

        if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(userRole))
        {
            context.Result = new RedirectToActionResult("Login", "Auth", null);
            return;
        }

        // Parse user role
        if (!Enum.TryParse<UserRole>(userRole, out var userRoleEnum))
        {
            context.Result = new ForbidResult();
            return;
        }

        // Check if user has any of the required permissions
        bool hasAnyPermission = false;
        foreach (var permission in _permissions)
        {
            if (await permissionService.HasPermissionAsync(userId, userRoleEnum, permission))
            {
                hasAnyPermission = true;
                break;
            }
        }

        if (!hasAnyPermission)
        {
            context.Result = new RedirectToActionResult("AccessDenied", "Error", null);
            return;
        }
    }
}