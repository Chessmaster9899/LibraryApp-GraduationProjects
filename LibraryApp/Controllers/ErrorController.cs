using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Diagnostics;
using LibraryApp.Models;
using LibraryApp.Services;
using System.Diagnostics;

namespace LibraryApp.Controllers
{
    public class ErrorController : BaseController
    {
        public ErrorController(IUniversitySettingsService universitySettings, ISessionService sessionService) 
            : base(universitySettings, sessionService)
        {
        }

        [Route("Error")]
        [Route("Error/Index")]
        public IActionResult Index()
        {
            var exceptionHandlerPathFeature = HttpContext.Features.Get<IExceptionHandlerPathFeature>();
            var statusCodeFeature = HttpContext.Features.Get<IStatusCodeReExecuteFeature>();
            
            var model = new ErrorViewModel
            {
                RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier
            };

            // Get status code
            var statusCode = HttpContext.Response.StatusCode;
            if (statusCodeFeature?.OriginalStatusCode != null)
            {
                statusCode = statusCodeFeature.OriginalStatusCode;
            }

            ViewBag.StatusCode = statusCode;
            ViewBag.ErrorMessage = GetFriendlyErrorMessage(statusCode, exceptionHandlerPathFeature?.Error);

            Response.StatusCode = statusCode;
            return View(model);
        }

        [Route("Error/NotFound")]
        [Route("Error/404")]
        public new IActionResult NotFound()
        {
            Response.StatusCode = 404;
            ViewBag.StatusCode = 404;
            return View();
        }

        [Route("Error/AccessDenied")]
        [Route("Error/403")]
        public IActionResult AccessDenied()
        {
            Response.StatusCode = 403;
            ViewBag.StatusCode = 403;
            return View();
        }

        [Route("Error/Unauthorized")]
        [Route("Error/401")]
        public new IActionResult Unauthorized()
        {
            Response.StatusCode = 401;
            ViewBag.StatusCode = 401;
            
            // If user is not authenticated, redirect to login
            if (User.Identity?.IsAuthenticated != true)
            {
                return RedirectToAction("Login", "Auth");
            }
            
            // Otherwise show access denied
            return RedirectToAction("AccessDenied");
        }

        [Route("Error/ServerError")]
        [Route("Error/500")]
        public IActionResult ServerError()
        {
            Response.StatusCode = 500;
            ViewBag.StatusCode = 500;
            return View("Index");
        }

        private static string GetFriendlyErrorMessage(int statusCode, Exception? exception)
        {
            return statusCode switch
            {
                400 => "The request could not be understood by the server due to malformed syntax.",
                401 => "You are not authorized to access this resource. Please log in.",
                403 => "You don't have permission to access this resource.",
                404 => "The requested resource could not be found.",
                408 => "The request timed out. Please try again.",
                429 => "Too many requests. Please wait a moment and try again.",
                500 => "An internal server error occurred. Please try again later.",
                502 => "Bad gateway. The server is temporarily unavailable.",
                503 => "Service unavailable. Please try again later.",
                504 => "Gateway timeout. The request took too long to process.",
                _ => exception?.Message ?? "An unexpected error occurred. Please try again."
            };
        }
    }
}