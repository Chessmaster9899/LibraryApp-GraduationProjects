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

        public StudentController(LibraryContext context, IUniversitySettingsService universitySettings, ISessionService sessionService) : base(universitySettings, sessionService)
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

        // Workflow Actions for Students
        [HttpPost]
        public async Task<IActionResult> SubmitProject(int projectId, string? comments = null)
        {
            var project = await _context.Projects
                .Include(p => p.Supervisor)
                .FirstOrDefaultAsync(p => p.Id == projectId);

            if (project == null)
            {
                this.AddError("Error", "Project not found");
                return RedirectToAction("Index");
            }

            // Check permission - only the assigned student can submit
            var studentId = int.Parse(CurrentUserId!);
            if (project.StudentId != studentId)
            {
                this.AddError("Permission Denied", "You can only submit your own projects");
                return RedirectToAction("Index");
            }

            if (project.Status == ProjectStatus.Approved || project.Status == ProjectStatus.InProgress)
            {
                project.Status = ProjectStatus.SubmittedForReview;
                project.SubmissionForReviewDate = DateTime.Now;
                project.ReviewComments = comments;

                await _context.SaveChangesAsync();
                this.AddSuccess("Project Submitted", $"Project '{project.Title}' has been submitted for review");
            }
            else
            {
                this.AddWarning("Invalid Status", "Only approved or in-progress projects can be submitted for review");
            }

            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> StartProject(int projectId)
        {
            var project = await _context.Projects
                .Include(p => p.Supervisor)
                .FirstOrDefaultAsync(p => p.Id == projectId);

            if (project == null)
            {
                this.AddError("Error", "Project not found");
                return RedirectToAction("Index");
            }

            // Check permission - only the assigned student can start
            var studentId = int.Parse(CurrentUserId!);
            if (project.StudentId != studentId)
            {
                this.AddError("Permission Denied", "You can only start your own projects");
                return RedirectToAction("Index");
            }

            if (project.Status == ProjectStatus.Approved)
            {
                project.Status = ProjectStatus.InProgress;
                await _context.SaveChangesAsync();
                this.AddSuccess("Project Started", $"You have started working on '{project.Title}'");
            }
            else
            {
                this.AddWarning("Invalid Status", "Only approved projects can be started");
            }

            return RedirectToAction("Index");
        }

        public async Task<IActionResult> Projects()
        {
            var studentId = int.Parse(CurrentUserId!);
            var projects = await _context.Projects
                .Include(p => p.Supervisor)
                .Include(p => p.Evaluator)
                .Include(p => p.Student)
                .ThenInclude(s => s.Department)
                .Where(p => p.StudentId == studentId)
                .OrderByDescending(p => p.SubmissionDate)
                .ToListAsync();

            return View(projects);
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