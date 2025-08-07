using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using LibraryApp.Services;
using LibraryApp.Models;

namespace LibraryApp.Controllers
{
    public class BaseController : Controller
    {
        protected readonly IUniversitySettingsService _universitySettings;

        public BaseController(IUniversitySettingsService universitySettings)
        {
            _universitySettings = universitySettings;
        }

        // Helper properties for session-based user information
        protected string? CurrentUserId => HttpContext.Session.GetString("UserId");
        protected string? CurrentUserName => HttpContext.Session.GetString("UserName");
        protected UserRole? CurrentUserRole => Enum.TryParse<UserRole>(HttpContext.Session.GetString("UserRole"), out var role) ? role : null;
        protected int? CurrentEntityId => int.TryParse(HttpContext.Session.GetString("EntityId"), out var id) ? id : null;
        protected bool IsAuthenticated => !string.IsNullOrEmpty(CurrentUserId);

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            ViewBag.UniversitySettings = _universitySettings.GetSettings();
            base.OnActionExecuting(context);
        }
    }
}