using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using LibraryApp.Data;
using LibraryApp.Models;
using LibraryApp.Services;

namespace LibraryApp.Controllers
{
    public class ProfessorController : BaseController
    {
        private readonly LibraryContext _context;

        public ProfessorController(LibraryContext context, IUniversitySettingsService universitySettings) : base(universitySettings)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            // Check if user is logged in as professor
            var userId = HttpContext.Session.GetString("UserId");
            var userRole = HttpContext.Session.GetString("UserRole");
            
            if (string.IsNullOrEmpty(userId) || userRole != "Professor")
            {
                return RedirectToAction("Login", "Auth");
            }

            // Get professor data
            var professor = await _context.Professors
                .Include(p => p.Department)
                .Include(p => p.SupervisedProjects)
                .ThenInclude(proj => proj.Student)
                .Include(p => p.EvaluatedProjects)
                .ThenInclude(proj => proj.Student)
                .FirstOrDefaultAsync(p => p.ProfessorId == userId);

            if (professor == null)
            {
                return RedirectToAction("Login", "Auth");
            }

            var dashboardData = new ProfessorDashboardViewModel
            {
                Professor = professor,
                TotalSupervisedProjects = professor.SupervisedProjects.Count,
                TotalEvaluatedProjects = professor.EvaluatedProjects.Count,
                CompletedSupervisedProjects = professor.SupervisedProjects.Count(p => p.Status == ProjectStatus.Completed || p.Status == ProjectStatus.Defended),
                CompletedEvaluatedProjects = professor.EvaluatedProjects.Count(p => p.Status == ProjectStatus.Completed || p.Status == ProjectStatus.Defended),
                RecentSupervisedProjects = professor.SupervisedProjects.OrderByDescending(p => p.SubmissionDate).Take(5).ToList(),
                RecentEvaluatedProjects = professor.EvaluatedProjects.OrderByDescending(p => p.SubmissionDate).Take(5).ToList(),
                UniversitySettings = _universitySettings.GetSettings()
            };

            return View(dashboardData);
        }

        public async Task<IActionResult> SupervisedProjects()
        {
            var userId = HttpContext.Session.GetString("UserId");
            var userRole = HttpContext.Session.GetString("UserRole");
            
            if (string.IsNullOrEmpty(userId) || userRole != "Professor")
            {
                return RedirectToAction("Login", "Auth");
            }

            var professor = await _context.Professors
                .Include(p => p.SupervisedProjects)
                .ThenInclude(proj => proj.Student)
                .ThenInclude(s => s.Department)
                .FirstOrDefaultAsync(p => p.ProfessorId == userId);

            if (professor == null)
            {
                return RedirectToAction("Login", "Auth");
            }

            ViewBag.PageTitle = "Projects I Supervise";
            return View("ProjectsList", professor.SupervisedProjects.OrderByDescending(p => p.SubmissionDate));
        }

        public async Task<IActionResult> EvaluatedProjects()
        {
            var userId = HttpContext.Session.GetString("UserId");
            var userRole = HttpContext.Session.GetString("UserRole");
            
            if (string.IsNullOrEmpty(userId) || userRole != "Professor")
            {
                return RedirectToAction("Login", "Auth");
            }

            var professor = await _context.Professors
                .Include(p => p.EvaluatedProjects)
                .ThenInclude(proj => proj.Student)
                .ThenInclude(s => s.Department)
                .FirstOrDefaultAsync(p => p.ProfessorId == userId);

            if (professor == null)
            {
                return RedirectToAction("Login", "Auth");
            }

            ViewBag.PageTitle = "Projects I Evaluate";
            return View("ProjectsList", professor.EvaluatedProjects.OrderByDescending(p => p.SubmissionDate));
        }

        public async Task<IActionResult> Profile()
        {
            var userId = HttpContext.Session.GetString("UserId");
            var userRole = HttpContext.Session.GetString("UserRole");
            
            if (string.IsNullOrEmpty(userId) || userRole != "Professor")
            {
                return RedirectToAction("Login", "Auth");
            }

            var professor = await _context.Professors
                .Include(p => p.Department)
                .FirstOrDefaultAsync(p => p.ProfessorId == userId);

            if (professor == null)
            {
                return RedirectToAction("Login", "Auth");
            }

            return View(professor);
        }
    }

    public class ProfessorDashboardViewModel
    {
        public Professor Professor { get; set; } = null!;
        public int TotalSupervisedProjects { get; set; }
        public int TotalEvaluatedProjects { get; set; }
        public int CompletedSupervisedProjects { get; set; }
        public int CompletedEvaluatedProjects { get; set; }
        public List<Project> RecentSupervisedProjects { get; set; } = new();
        public List<Project> RecentEvaluatedProjects { get; set; } = new();
        public UniversitySettings UniversitySettings { get; set; } = null!;
    }
}