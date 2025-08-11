using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using LibraryApp.Data;
using LibraryApp.Models;
using LibraryApp.Services;
using LibraryApp.Attributes;
using System.ComponentModel.DataAnnotations;

namespace LibraryApp.Controllers;

[AdminOnly]
public class ProjectWorkflowController : BaseController
{
    private readonly LibraryContext _context;

    public ProjectWorkflowController(
        LibraryContext context, 
        IUniversitySettingsService universitySettings, 
        ISessionService sessionService) 
        : base(universitySettings, sessionService)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var projects = await _context.Projects
            .Include(p => p.Student)
                .ThenInclude(s => s.Department)
            .Include(p => p.Supervisor)
            .Include(p => p.Evaluator)
            .OrderBy(p => p.Status)
            .ThenByDescending(p => p.SubmissionDate)
            .ToListAsync();

        var viewModel = new ProjectWorkflowViewModel
        {
            ProjectsByStatus = projects.GroupBy(p => p.Status).ToDictionary(g => g.Key, g => g.ToList()),
            StatusCounts = projects.GroupBy(p => p.Status).ToDictionary(g => g.Key, g => g.Count()),
            RecentActivity = await GetRecentProjectActivity()
        };

        return View(viewModel);
    }

    [HttpGet]
    public async Task<IActionResult> ProjectDetails(int id)
    {
        var project = await _context.Projects
            .Include(p => p.Student)
                .ThenInclude(s => s.Department)
            .Include(p => p.Supervisor)
                .ThenInclude(s => s.Department)
            .Include(p => p.Evaluator)
            .FirstOrDefaultAsync(p => p.Id == id);

        if (project == null)
        {
            return NotFound();
        }

        var availableActions = GetAvailableActions(project.Status);
        var nextStatuses = GetNextValidStatuses(project.Status);

        var viewModel = new ProjectWorkflowDetailsViewModel
        {
            Project = project,
            AvailableActions = availableActions,
            NextStatuses = nextStatuses,
            ActivityLog = await GetProjectActivityLog(id)
        };

        return View(viewModel);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateProjectStatus(int projectId, ProjectStatus newStatus, string? comments = null)
    {
        try
        {
            var project = await _context.Projects
                .Include(p => p.Student)
                .Include(p => p.Supervisor)
                .FirstOrDefaultAsync(p => p.Id == projectId);

            if (project == null)
            {
                return Json(new { success = false, message = "Project not found" });
            }

            // Validate status transition
            if (!IsValidStatusTransition(project.Status, newStatus))
            {
                return Json(new { success = false, message = "Invalid status transition" });
            }

            var oldStatus = project.Status;
            project.Status = newStatus;

            // Set status-specific fields
            switch (newStatus)
            {
                case ProjectStatus.SubmittedForReview:
                    project.SubmissionForReviewDate = DateTime.Now;
                    break;
                case ProjectStatus.ReviewApproved:
                    project.ReviewDate = DateTime.Now;
                    project.ReviewedBy = CurrentUserId;
                    project.ReviewComments = comments;
                    break;
                case ProjectStatus.ReviewRejected:
                    project.ReviewDate = DateTime.Now;
                    project.ReviewedBy = CurrentUserId;
                    project.ReviewComments = comments;
                    break;
                case ProjectStatus.Defended:
                    if (project.DefenseDate == null)
                    {
                        project.DefenseDate = DateTime.Now;
                    }
                    break;
                case ProjectStatus.Published:
                    project.IsPubliclyVisible = true;
                    break;
            }

            await _context.SaveChangesAsync();

            // Log the status change
            await LogProjectActivity(projectId, $"Status changed from {oldStatus} to {newStatus}", comments);

            return Json(new { 
                success = true, 
                message = $"Project status updated to {newStatus}",
                newStatus = newStatus.ToString()
            });
        }
        catch (Exception ex)
        {
            return Json(new { success = false, message = ex.Message });
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ScheduleDefense(int projectId, DateTime defenseDate, string? location = null)
    {
        try
        {
            var project = await _context.Projects.FindAsync(projectId);
            if (project == null)
            {
                return Json(new { success = false, message = "Project not found" });
            }

            project.DefenseDate = defenseDate;
            
            await _context.SaveChangesAsync();

            await LogProjectActivity(projectId, $"Defense scheduled for {defenseDate:yyyy-MM-dd HH:mm}", location);

            return Json(new { 
                success = true, 
                message = "Defense date scheduled successfully",
                defenseDate = defenseDate.ToString("yyyy-MM-dd HH:mm")
            });
        }
        catch (Exception ex)
        {
            return Json(new { success = false, message = ex.Message });
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AssignGrade(int projectId, string grade, string? feedback = null)
    {
        try
        {
            var project = await _context.Projects.FindAsync(projectId);
            if (project == null)
            {
                return Json(new { success = false, message = "Project not found" });
            }

            project.Grade = grade;
            
            if (project.Status == ProjectStatus.Defended && !string.IsNullOrEmpty(grade))
            {
                project.Status = ProjectStatus.Published;
                project.IsPubliclyVisible = true;
            }

            await _context.SaveChangesAsync();

            await LogProjectActivity(projectId, $"Grade assigned: {grade}", feedback);

            return Json(new { 
                success = true, 
                message = "Grade assigned successfully",
                grade = grade,
                newStatus = project.Status.ToString()
            });
        }
        catch (Exception ex)
        {
            return Json(new { success = false, message = ex.Message });
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AssignEvaluator(int projectId, int evaluatorId)
    {
        try
        {
            var project = await _context.Projects.FindAsync(projectId);
            var evaluator = await _context.Professors.FindAsync(evaluatorId);

            if (project == null || evaluator == null)
            {
                return Json(new { success = false, message = "Project or evaluator not found" });
            }

            project.EvaluatorId = evaluatorId;
            await _context.SaveChangesAsync();

            await LogProjectActivity(projectId, $"Evaluator assigned: {evaluator.Title} {evaluator.FirstName} {evaluator.LastName}");

            return Json(new { 
                success = true, 
                message = "Evaluator assigned successfully",
                evaluatorName = $"{evaluator.Title} {evaluator.FirstName} {evaluator.LastName}"
            });
        }
        catch (Exception ex)
        {
            return Json(new { success = false, message = ex.Message });
        }
    }

    [HttpGet]
    public async Task<IActionResult> GetAvailableProfessors()
    {
        var professors = await _context.Professors
            .Include(p => p.Department)
            .Where(p => p.Role == ProfessorRole.Both || p.Role == ProfessorRole.Evaluator)
            .OrderBy(p => p.LastName)
            .Select(p => new { 
                Id = p.Id, 
                Name = $"{p.Title} {p.FirstName} {p.LastName}",
                Department = p.Department.Name
            })
            .ToListAsync();

        return Json(professors);
    }

    private List<string> GetAvailableActions(ProjectStatus status)
    {
        return status switch
        {
            ProjectStatus.InProgress => new List<string> { "Mark as Completed", "Assign Evaluator" },
            ProjectStatus.Completed => new List<string> { "Submit for Review", "Assign Evaluator" },
            ProjectStatus.SubmittedForReview => new List<string> { "Approve Review", "Reject Review", "Assign Evaluator" },
            ProjectStatus.ReviewApproved => new List<string> { "Schedule Defense", "Assign Evaluator" },
            ProjectStatus.ReviewRejected => new List<string> { "Resubmit for Review" },
            ProjectStatus.Defended => new List<string> { "Assign Grade", "Publish Project" },
            ProjectStatus.Published => new List<string> { "Archive Project" },
            _ => new List<string>()
        };
    }

    private List<ProjectStatus> GetNextValidStatuses(ProjectStatus currentStatus)
    {
        return currentStatus switch
        {
            ProjectStatus.InProgress => new List<ProjectStatus> { ProjectStatus.Completed },
            ProjectStatus.Completed => new List<ProjectStatus> { ProjectStatus.SubmittedForReview },
            ProjectStatus.SubmittedForReview => new List<ProjectStatus> { ProjectStatus.ReviewApproved, ProjectStatus.ReviewRejected },
            ProjectStatus.ReviewApproved => new List<ProjectStatus> { ProjectStatus.Defended },
            ProjectStatus.ReviewRejected => new List<ProjectStatus> { ProjectStatus.SubmittedForReview },
            ProjectStatus.Defended => new List<ProjectStatus> { ProjectStatus.Published },
            _ => new List<ProjectStatus>()
        };
    }

    private bool IsValidStatusTransition(ProjectStatus from, ProjectStatus to)
    {
        var validTransitions = GetNextValidStatuses(from);
        return validTransitions.Contains(to);
    }

    private async Task LogProjectActivity(int projectId, string activity, string? details = null)
    {
        var activityLog = new ProjectActivityLog
        {
            ProjectId = projectId,
            Activity = activity,
            Details = details,
            PerformedBy = CurrentUserId ?? "System",
            PerformedAt = DateTime.Now
        };

        _context.ProjectActivityLogs.Add(activityLog);
        await _context.SaveChangesAsync();
    }

    private async Task<List<ProjectActivityLog>> GetProjectActivityLog(int projectId)
    {
        return await _context.ProjectActivityLogs
            .Where(al => al.ProjectId == projectId)
            .OrderByDescending(al => al.PerformedAt)
            .Take(20)
            .ToListAsync();
    }

    private async Task<List<ProjectActivityLog>> GetRecentProjectActivity()
    {
        return await _context.ProjectActivityLogs
            .Include(al => al.Project)
            .OrderByDescending(al => al.PerformedAt)
            .Take(10)
            .ToListAsync();
    }
}