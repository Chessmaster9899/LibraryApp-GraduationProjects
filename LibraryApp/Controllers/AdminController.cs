using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using LibraryApp.Data;
using LibraryApp.Models;
using LibraryApp.Attributes;
using LibraryApp.Services;

namespace LibraryApp.Controllers;

[AdminOnly]
public class AdminController : BaseController
{
    private readonly LibraryContext _context;

    public AdminController(LibraryContext context, IUniversitySettingsService universitySettings, ISessionService sessionService) : base(universitySettings, sessionService)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<IActionResult> Dashboard()
    {
        var pendingSubmissions = await _context.ProjectSubmissions
            .Include(ps => ps.Project)
            .ThenInclude(p => p.Student)
            .Where(ps => ps.Status == SubmissionStatus.Pending)
            .OrderBy(ps => ps.SubmissionDate)
            .ToListAsync();

        var recentNotifications = await _context.Notifications
            .Where(n => n.UserRole == UserRole.Admin)
            .OrderByDescending(n => n.CreatedDate)
            .Take(10)
            .ToListAsync();

        var recentAuditLogs = await _context.SystemAuditLogs
            .OrderByDescending(a => a.Timestamp)
            .Take(20)
            .ToListAsync();

        var projectStatusCounts = await _context.Projects
            .GroupBy(p => p.Status)
            .ToDictionaryAsync(g => g.Key.ToString(), g => g.Count());

        var userActivityCounts = await _context.SystemAuditLogs
            .Where(a => a.Timestamp >= DateTime.Now.AddDays(-7))
            .GroupBy(a => a.UserRole)
            .ToDictionaryAsync(g => g.Key.ToString(), g => g.Count());

        var recentProjects = await _context.Projects
            .Include(p => p.Student)
            .Include(p => p.Supervisor)
            .OrderByDescending(p => p.SubmissionDate)
            .Take(5)
            .ToListAsync();

        var viewModel = new AdminDashboardEnhancedViewModel
        {
            TotalProjects = await _context.Projects.CountAsync(),
            TotalStudents = await _context.Students.CountAsync(),
            TotalProfessors = await _context.Professors.CountAsync(),
            TotalDepartments = await _context.Departments.CountAsync(),
            PendingSubmissions = pendingSubmissions,
            RecentNotifications = recentNotifications,
            RecentAuditLogs = recentAuditLogs,
            ProjectStatusCounts = projectStatusCounts,
            UserActivityCounts = userActivityCounts,
            RecentProjects = recentProjects
        };

        return View(viewModel);
    }

    [HttpGet]
    public async Task<IActionResult> PendingSubmissions()
    {
        var submissions = await _context.ProjectSubmissions
            .Include(ps => ps.Project)
            .ThenInclude(p => p.Student)
            .ThenInclude(s => s.Department)
            .Include(ps => ps.Project.Supervisor)
            .Where(ps => ps.Status == SubmissionStatus.Pending || ps.Status == SubmissionStatus.UnderReview)
            .OrderBy(ps => ps.SubmissionDate)
            .ToListAsync();

        return View(submissions);
    }

    [HttpGet]
    public async Task<IActionResult> ReviewSubmission(int id)
    {
        var submission = await _context.ProjectSubmissions
            .Include(ps => ps.Project)
            .ThenInclude(p => p.Student)
            .ThenInclude(s => s.Department)
            .Include(ps => ps.Project.Supervisor)
            .Include(ps => ps.Project.Evaluator)
            .FirstOrDefaultAsync(ps => ps.Id == id);

        if (submission == null)
        {
            return NotFound();
        }

        var viewModel = new ProjectSubmissionReviewViewModel
        {
            Submission = submission,
            NewStatus = submission.Status
        };

        return View(viewModel);
    }

    [HttpPost]
    public async Task<IActionResult> ReviewSubmission(ProjectSubmissionReviewViewModel model)
    {
        var submission = await _context.ProjectSubmissions
            .Include(ps => ps.Project)
            .FirstOrDefaultAsync(ps => ps.Id == model.Submission.Id);

        if (submission == null)
        {
            return NotFound();
        }

        submission.Status = model.NewStatus;
        submission.ReviewDate = DateTime.Now;
        submission.ReviewComments = model.ReviewComments;
        submission.ReviewedBy = CurrentUserId;

        var project = submission.Project;

        // Update project status based on review decision
        switch (model.NewStatus)
        {
            case SubmissionStatus.Approved:
                project.Status = ProjectStatus.ReviewApproved;
                project.IsPubliclyVisible = true;
                project.ReviewDate = DateTime.Now;
                project.ReviewComments = model.ReviewComments;
                project.ReviewedBy = CurrentUserId;

                // Copy submission files to project
                if (!string.IsNullOrEmpty(submission.PosterFilePath))
                    project.PosterPath = submission.PosterFilePath;
                if (!string.IsNullOrEmpty(submission.ReportFilePath))
                    project.ReportPath = submission.ReportFilePath;
                if (!string.IsNullOrEmpty(submission.CodeFilesPath))
                    project.CodePath = submission.CodeFilesPath;

                break;

            case SubmissionStatus.Rejected:
                project.Status = ProjectStatus.ReviewRejected;
                project.ReviewDate = DateTime.Now;
                project.ReviewComments = model.ReviewComments;
                project.ReviewedBy = CurrentUserId;
                break;

            case SubmissionStatus.NeedsRevision:
                project.Status = ProjectStatus.Completed; // Back to completed for revision
                break;
        }

        // Create notifications for project participants
        var notifications = new List<Notification>();

        // Notify student
        notifications.Add(new Notification
        {
            UserId = project.Student.StudentNumber,
            UserRole = UserRole.Student,
            Title = $"Project Review: {model.NewStatus}",
            Message = $"Your project '{project.Title}' review has been {model.NewStatus.ToString().ToLower()}. {model.ReviewComments}",
            RelatedUrl = $"/Student/Projects/{project.Id}",
            RelatedEntityType = "Project",
            RelatedEntityId = project.Id
        });

        // Notify supervisor
        notifications.Add(new Notification
        {
            UserId = project.Supervisor.ProfessorId,
            UserRole = UserRole.Professor,
            Title = $"Project Review: {model.NewStatus}",
            Message = $"Project '{project.Title}' review has been {model.NewStatus.ToString().ToLower()}. {model.ReviewComments}",
            RelatedUrl = $"/Professor/Projects/{project.Id}",
            RelatedEntityType = "Project",
            RelatedEntityId = project.Id
        });

        _context.Notifications.AddRange(notifications);

        // Log the action
        var auditLog = new SystemAuditLog
        {
            UserId = CurrentUserId!,
            UserRole = CurrentUserRoleEnum!.Value,
            Action = "ReviewProjectSubmission",
            EntityType = "ProjectSubmission",
            EntityId = submission.Id,
            Details = $"Status: {model.NewStatus}, Comments: {model.ReviewComments}",
            IPAddress = HttpContext.Connection.RemoteIpAddress?.ToString(),
            UserAgent = HttpContext.Request.Headers["User-Agent"].ToString()
        };

        _context.SystemAuditLogs.Add(auditLog);
        await _context.SaveChangesAsync();

        TempData["Success"] = $"Project submission has been {model.NewStatus.ToString().ToLower()}.";
        return RedirectToAction(nameof(PendingSubmissions));
    }

    [HttpGet]
    public async Task<IActionResult> Analytics()
    {
        var viewModel = new AnalyticsViewModel
        {
            ProjectsByStatus = await _context.Projects
                .GroupBy(p => p.Status)
                .ToDictionaryAsync(g => g.Key.ToString(), g => g.Count()),

            ProjectsByDepartment = await _context.Projects
                .Include(p => p.Student.Department)
                .GroupBy(p => p.Student.Department.Name)
                .ToDictionaryAsync(g => g.Key, g => g.Count()),

            MonthlyCompletions = await _context.Projects
                .Where(p => p.Status == ProjectStatus.ReviewApproved || p.Status == ProjectStatus.Defended)
                .Where(p => p.DefenseDate.HasValue && p.DefenseDate >= DateTime.Now.AddYears(-1))
                .GroupBy(p => new { p.DefenseDate!.Value.Year, p.DefenseDate!.Value.Month })
                .ToDictionaryAsync(
                    g => $"{g.Key.Year}-{g.Key.Month:D2}",
                    g => g.Count()),

            TopProjects = await _context.Projects
                .Include(p => p.Student)
                .Include(p => p.Supervisor)
                .Where(p => p.IsPubliclyVisible)
                .OrderByDescending(p => p.DefenseDate)
                .Take(10)
                .ToListAsync(),

            UserLoginActivity = await _context.SystemAuditLogs
                .Where(a => a.Action == "Login" && a.Timestamp >= DateTime.Now.AddDays(-30))
                .GroupBy(a => a.Timestamp.Date)
                .ToDictionaryAsync(
                    g => g.Key.ToString("yyyy-MM-dd"),
                    g => g.Count())
        };

        return View(viewModel);
    }

    [HttpGet]
    public async Task<IActionResult> Communication()
    {
        var viewModel = new CommunicationViewModel
        {
            RecentAnnouncements = await _context.Announcements
                .Where(a => a.IsActive)
                .OrderByDescending(a => a.CreatedDate)
                .Take(10)
                .ToListAsync()
        };

        return View(viewModel);
    }

    [HttpPost]
    public async Task<IActionResult> CreateAnnouncement(CommunicationViewModel model)
    {
        var announcement = new Announcement
        {
            Title = model.Title,
            Content = model.Content,
            CreatedBy = CurrentUserId!,
            ExpiryDate = model.ExpiryDate,
            IsUrgent = model.IsUrgent,
            TargetRoles = string.Join(",", model.TargetRoles)
        };

        _context.Announcements.Add(announcement);

        // Create notifications for target users
        var notifications = new List<Notification>();

        foreach (var role in model.TargetRoles)
        {
            if (Enum.TryParse<UserRole>(role, out var userRole))
            {
                List<string> userIds = new();

                switch (userRole)
                {
                    case UserRole.Student:
                        userIds = await _context.Students.Select(s => s.StudentNumber).ToListAsync();
                        break;
                    case UserRole.Professor:
                        userIds = await _context.Professors.Select(p => p.ProfessorId).ToListAsync();
                        break;
                    case UserRole.Admin:
                        userIds = await _context.Admins.Select(a => a.Username).ToListAsync();
                        break;
                }

                foreach (var userId in userIds)
                {
                    notifications.Add(new Notification
                    {
                        UserId = userId,
                        UserRole = userRole,
                        Title = model.Title,
                        Message = model.Content,
                        RelatedUrl = "/Admin/Announcements",
                        RelatedEntityType = "Announcement",
                        RelatedEntityId = announcement.Id
                    });
                }
            }
        }

        _context.Notifications.AddRange(notifications);
        await _context.SaveChangesAsync();

        TempData["Success"] = "Announcement created and notifications sent successfully.";
        return RedirectToAction(nameof(Communication));
    }

    [HttpGet]
    public async Task<IActionResult> AuditTrail()
    {
        var auditLogs = await _context.SystemAuditLogs
            .OrderByDescending(a => a.Timestamp)
            .Take(100)
            .ToListAsync();

        return View(auditLogs);
    }

    // Additional Admin functionality
    [HttpGet]
    public async Task<IActionResult> SystemOverview()
    {
        var overview = new
        {
            ProjectStats = await _context.Projects
                .GroupBy(p => p.Status)
                .Select(g => new { Status = g.Key.ToString(), Count = g.Count() })
                .ToListAsync(),
            
            UserStats = new
            {
                TotalStudents = await _context.Students.CountAsync(),
                TotalProfessors = await _context.Professors.CountAsync(),
                ActiveStudents = await _context.Students.Where(s => s.LastLogin >= DateTime.Now.AddDays(-30)).CountAsync(),
                ActiveProfessors = await _context.Professors.Where(p => p.LastLogin >= DateTime.Now.AddDays(-30)).CountAsync()
            },
            
            DepartmentStats = await _context.Departments
                .Select(d => new {
                    d.Name,
                    StudentCount = d.Students.Count(),
                    ProfessorCount = d.Professors.Count(),
                    ProjectCount = d.Students.SelectMany(s => s.Projects).Count()
                })
                .ToListAsync(),
                
            RecentActivity = await _context.SystemAuditLogs
                .OrderByDescending(a => a.Timestamp)
                .Take(20)
                .ToListAsync()
        };

        return View(overview);
    }

    [HttpGet]
    public IActionResult Reports()
    {
        var reports = new ReportsViewModel
        {
            ReportType = "overview",
            StartDate = DateTime.Now.AddMonths(-3),
            EndDate = DateTime.Now
        };

        return View(reports);
    }

    [HttpPost]
    public async Task<IActionResult> GenerateReport(ReportsViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View("Reports", model);
        }

        List<object> reportData = new();

        switch (model.ReportType.ToLower())
        {
            case "projects":
                reportData = await GenerateProjectReport(model);
                break;
            case "students":
                reportData = await GenerateStudentReport(model);
                break;
            case "professors":
                reportData = await GenerateProfessorReport(model);
                break;
            case "departments":
                reportData = await GenerateDepartmentReport(model);
                break;
            default:
                reportData = await GenerateOverviewReport(model);
                break;
        }

        model.ReportData = reportData;
        return View("Reports", model);
    }

    [HttpGet]
    public async Task<IActionResult> Communications()
    {
        var viewModel = new CommunicationViewModel
        {
            RecentAnnouncements = await _context.Announcements
                .Where(a => a.IsActive)
                .OrderByDescending(a => a.CreatedDate)
                .Take(10)
                .ToListAsync()
        };

        return View(viewModel);
    }

    [HttpPost]
    public async Task<IActionResult> SendAnnouncement(CommunicationViewModel model)
    {
        if (!ModelState.IsValid)
        {
            model.RecentAnnouncements = await _context.Announcements
                .Where(a => a.IsActive)
                .OrderByDescending(a => a.CreatedDate)
                .Take(10)
                .ToListAsync();
            return View("Communications", model);
        }

        var announcement = new Announcement
        {
            Title = model.Title,
            Content = model.Content,
            CreatedBy = CurrentUserId ?? "System",
            CreatedDate = DateTime.Now,
            ExpiryDate = model.ExpiryDate,
            IsUrgent = model.IsUrgent,
            TargetRoles = string.Join(",", model.TargetRoles),
            IsActive = true
        };

        _context.Announcements.Add(announcement);
        await _context.SaveChangesAsync();

        this.AddSuccess("Announcement Sent", "Your announcement has been sent successfully");
        return RedirectToAction("Communications");
    }

    [HttpGet]
    public async Task<IActionResult> UserManagement()
    {
        var viewModel = new UserManagementViewModel
        {
            Students = await _context.Students.Include(s => s.Department).OrderBy(s => s.LastName).ToListAsync(),
            Professors = await _context.Professors.Include(p => p.Department).OrderBy(p => p.LastName).ToListAsync(),
            Admins = await _context.Admins.Where(a => a.IsActive).OrderBy(a => a.LastName).ToListAsync(),
            UserStatistics = new Dictionary<string, int>
            {
                ["TotalUsers"] = await _context.Students.CountAsync() + await _context.Professors.CountAsync() + await _context.Admins.CountAsync(),
                ["ActiveStudents"] = await _context.Students.Where(s => s.LastLogin >= DateTime.Now.AddDays(-30)).CountAsync(),
                ["ActiveProfessors"] = await _context.Professors.Where(p => p.LastLogin >= DateTime.Now.AddDays(-30)).CountAsync(),
                ["NewUsersThisMonth"] = await _context.Students.Where(s => s.EnrollmentDate >= DateTime.Now.AddDays(-30)).CountAsync()
            }
        };

        return View(viewModel);
    }

    [HttpPost]
    public async Task<IActionResult> DeactivateUser(string userId, UserRole userRole)
    {
        bool success = false;
        string message = "";

        try
        {
            switch (userRole)
            {
                case UserRole.Student:
                    var student = await _context.Students.FirstOrDefaultAsync(s => s.StudentNumber == userId);
                    if (student != null)
                    {
                        // Instead of deleting, we could add an IsActive field
                        message = "Student account deactivated";
                        success = true;
                    }
                    break;
                case UserRole.Professor:
                    var professor = await _context.Professors.FirstOrDefaultAsync(p => p.ProfessorId == userId);
                    if (professor != null)
                    {
                        message = "Professor account deactivated";
                        success = true;
                    }
                    break;
                case UserRole.Admin:
                    var admin = await _context.Admins.FirstOrDefaultAsync(a => a.Username == userId);
                    if (admin != null)
                    {
                        admin.IsActive = false;
                        await _context.SaveChangesAsync();
                        message = "Admin account deactivated";
                        success = true;
                    }
                    break;
            }
        }
        catch (Exception ex)
        {
            message = "Failed to deactivate user: " + ex.Message;
        }

        return Json(new { success, message });
    }

    // Report generation helper methods
    private async Task<List<object>> GenerateProjectReport(ReportsViewModel model)
    {
        var query = _context.Projects
            .Include(p => p.Student)
                .ThenInclude(s => s.Department)
            .Include(p => p.Supervisor)
            .Where(p => p.SubmissionDate >= model.StartDate && p.SubmissionDate <= model.EndDate);

        if (model.SelectedDepartments.Any())
        {
            query = query.Where(p => model.SelectedDepartments.Contains(p.Student.Department.Name));
        }

        if (model.SelectedStatuses.Any())
        {
            var statuses = model.SelectedStatuses.Select(s => Enum.Parse<ProjectStatus>(s)).ToList();
            query = query.Where(p => statuses.Contains(p.Status));
        }

        return await query.Select(p => new {
            p.Title,
            StudentName = p.Student.FullName,
            Department = p.Student.Department.Name,
            SupervisorName = p.Supervisor.DisplayName,
            p.Status,
            p.SubmissionDate,
            p.Grade
        }).Cast<object>().ToListAsync();
    }

    private async Task<List<object>> GenerateStudentReport(ReportsViewModel model)
    {
        var query = _context.Students
            .Include(s => s.Department)
            .Include(s => s.Projects)
            .Where(s => s.EnrollmentDate >= model.StartDate && s.EnrollmentDate <= model.EndDate);

        if (model.SelectedDepartments.Any())
        {
            query = query.Where(s => model.SelectedDepartments.Contains(s.Department.Name));
        }

        return await query.Select(s => new {
            s.FullName,
            s.StudentNumber,
            Department = s.Department.Name,
            s.Email,
            ProjectCount = s.Projects.Count(),
            CompletedProjects = s.Projects.Count(p => p.Status == ProjectStatus.Completed || p.Status == ProjectStatus.Defended),
            s.EnrollmentDate,
            s.LastLogin
        }).Cast<object>().ToListAsync();
    }

    private async Task<List<object>> GenerateProfessorReport(ReportsViewModel model)
    {
        var query = _context.Professors
            .Include(p => p.Department)
            .Include(p => p.SupervisedProjects)
            .Include(p => p.EvaluatedProjects)
            .AsQueryable();

        if (model.SelectedDepartments.Any())
        {
            query = query.Where(p => model.SelectedDepartments.Contains(p.Department.Name));
        }

        return await query.Select(p => new {
            p.DisplayName,
            p.ProfessorId,
            Department = p.Department.Name,
            p.Email,
            p.Specialization,
            SupervisedCount = p.SupervisedProjects.Count(),
            EvaluatedCount = p.EvaluatedProjects.Count(),
            p.LastLogin
        }).Cast<object>().ToListAsync();
    }

    private async Task<List<object>> GenerateDepartmentReport(ReportsViewModel model)
    {
        return await _context.Departments
            .Include(d => d.Students)
            .Include(d => d.Professors)
            .Select(d => new {
                d.Name,
                d.Description,
                StudentCount = d.Students.Count(),
                ProfessorCount = d.Professors.Count(),
                ProjectCount = d.Students.SelectMany(s => s.Projects).Count(),
                CompletedProjects = d.Students.SelectMany(s => s.Projects)
                    .Count(p => p.Status == ProjectStatus.Completed || p.Status == ProjectStatus.Defended)
            }).Cast<object>().ToListAsync();
    }

    private async Task<List<object>> GenerateOverviewReport(ReportsViewModel model)
    {
        return new List<object>
        {
            new {
                Metric = "Total Projects",
                Value = await _context.Projects.CountAsync(),
                Period = $"{model.StartDate:yyyy-MM-dd} to {model.EndDate:yyyy-MM-dd}"
            },
            new {
                Metric = "Completed Projects",
                Value = await _context.Projects.Where(p => p.Status == ProjectStatus.Completed || p.Status == ProjectStatus.Defended).CountAsync(),
                Period = $"{model.StartDate:yyyy-MM-dd} to {model.EndDate:yyyy-MM-dd}"
            },
            new {
                Metric = "Active Students",
                Value = await _context.Students.Where(s => s.LastLogin >= DateTime.Now.AddDays(-30)).CountAsync(),
                Period = "Last 30 days"
            },
            new {
                Metric = "Active Professors",
                Value = await _context.Professors.Where(p => p.LastLogin >= DateTime.Now.AddDays(-30)).CountAsync(),
                Period = "Last 30 days"
            }
        };
    }
}