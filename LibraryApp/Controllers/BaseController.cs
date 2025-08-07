using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using LibraryApp.Services;
using LibraryApp.Models;

namespace LibraryApp.Controllers
{
    public class BaseController : Controller
    {
        protected readonly IUniversitySettingsService _universitySettings;
        protected readonly ISessionService _sessionService;

        public BaseController(IUniversitySettingsService universitySettings, ISessionService sessionService)
        {
            _universitySettings = universitySettings;
            _sessionService = sessionService;
        }

        // Simplified session-based user information using SessionService
        protected string? CurrentUserId => _sessionService.GetUserId(HttpContext);
        protected string? CurrentUserRole => _sessionService.GetUserRole(HttpContext);
        protected UserRole? CurrentUserRoleEnum => Enum.TryParse<UserRole>(CurrentUserRole, out var role) ? role : null;
        protected int? CurrentEntityId => int.TryParse(HttpContext.Session.GetString("EntityId"), out var id) ? id : null;
        protected bool IsAuthenticated => _sessionService.IsAuthenticated(HttpContext);
        protected bool IsStudent => _sessionService.IsStudent(HttpContext);
        protected bool IsProfessor => _sessionService.IsProfessor(HttpContext);
        protected bool IsAdmin => _sessionService.IsAdmin(HttpContext);
        protected SessionUserInfo? CurrentUser => _sessionService.GetCurrentUser(HttpContext);

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            ViewBag.UniversitySettings = _universitySettings.GetSettings();
            base.OnActionExecuting(context);
        }
    }
}