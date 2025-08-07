using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using LibraryApp.Data;
using LibraryApp.Models;
using LibraryApp.Services;

namespace LibraryApp.Controllers
{
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
                .FirstOrDefaultAsync(s => s.StudentNumber == userId);

            if (student == null)
            {
                return RedirectToAction("Login", "Auth");
            }

            return View(student);
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