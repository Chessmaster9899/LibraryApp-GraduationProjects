using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using LibraryApp.Services;

namespace LibraryApp.Controllers
{
    public class BaseController : Controller
    {
        protected readonly IUniversitySettingsService _universitySettings;

        public BaseController(IUniversitySettingsService universitySettings)
        {
            _universitySettings = universitySettings;
        }

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            ViewBag.UniversitySettings = _universitySettings.GetSettings();
            base.OnActionExecuting(context);
        }
    }
}