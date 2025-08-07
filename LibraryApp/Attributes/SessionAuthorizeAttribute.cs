using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using LibraryApp.Models;

namespace LibraryApp.Attributes
{
    public class SessionAuthorizeAttribute : ActionFilterAttribute
    {
        private readonly UserRole[] _allowedRoles;

        public SessionAuthorizeAttribute(params UserRole[] allowedRoles)
        {
            _allowedRoles = allowedRoles;
        }

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            var session = context.HttpContext.Session;
            var userRole = session.GetString("UserRole");
            var userId = session.GetString("UserId");

            if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(userRole))
            {
                context.Result = new RedirectToActionResult("Login", "Auth", null);
                return;
            }

            if (!Enum.TryParse<UserRole>(userRole, out var role) || !_allowedRoles.Contains(role))
            {
                context.Result = new ForbidResult();
                return;
            }

            base.OnActionExecuting(context);
        }
    }

    // Convenience attributes for specific roles
    public class AdminOnlyAttribute : SessionAuthorizeAttribute
    {
        public AdminOnlyAttribute() : base(UserRole.Admin) { }
    }

    public class StudentOnlyAttribute : SessionAuthorizeAttribute
    {
        public StudentOnlyAttribute() : base(UserRole.Student) { }
    }

    public class ProfessorOnlyAttribute : SessionAuthorizeAttribute
    {
        public ProfessorOnlyAttribute() : base(UserRole.Professor) { }
    }

    public class AdminOrProfessorAttribute : SessionAuthorizeAttribute
    {
        public AdminOrProfessorAttribute() : base(UserRole.Admin, UserRole.Professor) { }
    }
}