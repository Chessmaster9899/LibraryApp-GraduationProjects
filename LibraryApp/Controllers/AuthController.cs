using Microsoft.AspNetCore.Mvc;
using LibraryApp.Models;
using LibraryApp.Services;

namespace LibraryApp.Controllers
{
    public class AuthController : Controller
    {
        private readonly IAuthenticationService _authService;
        private readonly IUniversitySettingsService _universitySettings;

        public AuthController(IAuthenticationService authService, IUniversitySettingsService universitySettings)
        {
            _authService = authService;
            _universitySettings = universitySettings;
        }

        public IActionResult Login()
        {
            ViewBag.UniversitySettings = _universitySettings.GetSettings();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(string userId, string password)
        {
            if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(password))
            {
                this.AddError("Login Failed", "Please enter both User ID and Password");
                ViewBag.UniversitySettings = _universitySettings.GetSettings();
                return View();
            }

            var result = await _authService.AuthenticateAsync(userId, password);
            
            if (result.Success)
            {
                // Set session data
                HttpContext.Session.SetString("UserId", userId);
                HttpContext.Session.SetString("UserRole", result.Role.ToString()!);
                HttpContext.Session.SetString("UserName", result.DisplayName!);
                HttpContext.Session.SetInt32("EntityId", result.EntityId!.Value);
                HttpContext.Session.SetString("MustChangePassword", result.MustChangePassword.ToString());

                this.AddSuccess("Welcome!", $"Successfully logged in as {result.DisplayName}");

                if (result.MustChangePassword)
                {
                    this.AddWarning("Password Change Required", "Please update your password for security");
                    return RedirectToAction("ChangePassword");
                }

                // Redirect based on role
                return result.Role switch
                {
                    UserRole.Admin => RedirectToAction("Index", "Home"),
                    UserRole.Student => RedirectToAction("Index", "Student"),
                    UserRole.Professor => RedirectToAction("Index", "Professor"),
                    _ => RedirectToAction("Index", "Home")
                };
            }

            this.AddError("Login Failed", result.Message ?? "Invalid credentials");
            ViewBag.UniversitySettings = _universitySettings.GetSettings();
            return View();
        }

        public IActionResult ChangePassword()
        {
            var userId = HttpContext.Session.GetString("UserId");
            if (string.IsNullOrEmpty(userId))
            {
                this.AddError("Access Denied", "Please log in to change your password");
                return RedirectToAction("Login");
            }
            
            ViewBag.UniversitySettings = _universitySettings.GetSettings();
            ViewBag.UserId = userId;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangePassword(string newPassword, string confirmPassword)
        {
            var userId = HttpContext.Session.GetString("UserId");
            if (string.IsNullOrEmpty(userId))
            {
                this.AddError("Session Expired", "Please log in again");
                return RedirectToAction("Login");
            }

            if (string.IsNullOrEmpty(newPassword) || newPassword != confirmPassword)
            {
                this.AddError("Validation Error", "Passwords do not match or are empty");
                ViewBag.UniversitySettings = _universitySettings.GetSettings();
                ViewBag.UserId = userId;
                return View();
            }

            if (newPassword.Length < 6)
            {
                this.AddError("Validation Error", "Password must be at least 6 characters long");
                ViewBag.UniversitySettings = _universitySettings.GetSettings();
                ViewBag.UserId = userId;
                return View();
            }

            var success = await _authService.ChangePasswordAsync(userId, newPassword);
            if (success)
            {
                HttpContext.Session.SetString("MustChangePassword", "false");
                this.AddSuccess("Password Updated", "Your password has been successfully changed");
                
                // Redirect based on role
                var userRole = HttpContext.Session.GetString("UserRole");
                return userRole switch
                {
                    "Admin" => RedirectToAction("Index", "Home"),
                    "Student" => RedirectToAction("Index", "Student"),
                    "Professor" => RedirectToAction("Index", "Professor"),
                    _ => RedirectToAction("Index", "Home")
                };
            }

            this.AddError("Update Failed", "Failed to change password. Please try again");
            ViewBag.UniversitySettings = _universitySettings.GetSettings();
            ViewBag.UserId = userId;
            return View();
        }

        public IActionResult Logout()
        {
            var userName = HttpContext.Session.GetString("UserName");
            HttpContext.Session.Clear();
            this.AddInfo("Signed Out", $"Goodbye {userName ?? "User"}! You have been successfully signed out");
            return RedirectToAction("Login");
        }
    }
}