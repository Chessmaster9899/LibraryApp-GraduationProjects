using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using LibraryApp.Data;
using LibraryApp.Models;
using LibraryApp.Services;
using LibraryApp.Attributes;

namespace LibraryApp.Controllers
{
    [StudentOnly]
    public class StudentController : BaseController
    {
        private readonly LibraryContext _context;

        public StudentController(LibraryContext context, IUniversitySettingsService universitySettings) : base(universitySettings)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            // Check if user is logged in as student
            var userId = HttpContext.Session.GetString("UserId");
            var userRole = HttpContext.Session.GetString("UserRole");
            
            if (string.IsNullOrEmpty(userId) || userRole != "Student")
            {
                return RedirectToAction("Login", "Auth");
            }

            // Get student data
            var student = await _context.Students
                .Include(s => s.Department)
                .Include(s => s.Projects)
                .ThenInclude(p => p.Supervisor)
                .FirstOrDefaultAsync(s => s.StudentNumber == userId);

            if (student == null)
            {
                return RedirectToAction("Login", "Auth");
            }

            var dashboardData = new StudentDashboardViewModel
            {
                Student = student,
                TotalProjects = student.Projects.Count,
                CompletedProjects = student.Projects.Count(p => p.Status == ProjectStatus.Completed || p.Status == ProjectStatus.Defended),
                InProgressProjects = student.Projects.Count(p => p.Status == ProjectStatus.InProgress || p.Status == ProjectStatus.Approved),
                RecentProjects = student.Projects.OrderByDescending(p => p.SubmissionDate).Take(5).ToList(),
                UniversitySettings = _universitySettings.GetSettings()
            };

            return View(dashboardData);
        }

        public async Task<IActionResult> Projects()
        {
            var userId = HttpContext.Session.GetString("UserId");
            var userRole = HttpContext.Session.GetString("UserRole");
            
            if (string.IsNullOrEmpty(userId) || userRole != "Student")
            {
                return RedirectToAction("Login", "Auth");
            }

            var student = await _context.Students
                .Include(s => s.Projects)
                .ThenInclude(p => p.Supervisor)
                .ThenInclude(s => s.Department)
                .FirstOrDefaultAsync(s => s.StudentNumber == userId);

            if (student == null)
            {
                return RedirectToAction("Login", "Auth");
            }

            return View(student.Projects.OrderByDescending(p => p.SubmissionDate));
        }

        public async Task<IActionResult> Profile()
        {
            var userId = HttpContext.Session.GetString("UserId");
            var userRole = HttpContext.Session.GetString("UserRole");
            
            if (string.IsNullOrEmpty(userId) || userRole != "Student")
            {
                return RedirectToAction("Login", "Auth");
            }

            var student = await _context.Students
                .Include(s => s.Department)
                .Include(s => s.Projects)
                .FirstOrDefaultAsync(s => s.StudentNumber == userId);

            if (student == null)
            {
                return RedirectToAction("Login", "Auth");
            }

            return View(student);
        }

        [HttpPost]
        public async Task<IActionResult> UpdateProfile(Student model)
        {
            var userId = HttpContext.Session.GetString("UserId");
            var userRole = HttpContext.Session.GetString("UserRole");
            
            if (string.IsNullOrEmpty(userId) || userRole != "Student")
            {
                this.AddError("Access Denied", "Please log in to update your profile");
                return RedirectToAction("Login", "Auth");
            }

            var student = await _context.Students
                .Include(s => s.Department)
                .FirstOrDefaultAsync(s => s.StudentNumber == userId);

            if (student == null)
            {
                this.AddError("Student Not Found", "Could not find your student record");
                return RedirectToAction("Login", "Auth");
            }

            // Only allow updating certain fields
            student.FirstName = model.FirstName;
            student.LastName = model.LastName;
            student.Email = model.Email;
            student.Phone = model.Phone;

            try
            {
                await _context.SaveChangesAsync();
                this.AddSuccess("Profile Updated", "Your profile has been successfully updated");
            }
            catch (Exception)
            {
                this.AddError("Update Failed", "Failed to update your profile. Please try again");
            }

            return RedirectToAction("Profile");
        }
    }

    public class StudentDashboardViewModel
    {
        public Student Student { get; set; } = null!;
        public int TotalProjects { get; set; }
        public int CompletedProjects { get; set; }
        public int InProgressProjects { get; set; }
        public List<Project> RecentProjects { get; set; } = new();
        public UniversitySettings UniversitySettings { get; set; } = null!;
    }
}