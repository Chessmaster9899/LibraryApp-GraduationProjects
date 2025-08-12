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
            .Include(p => p.ProjectStudents)
                .ThenInclude(ps => ps.Student)
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
            .Include(p => p.ProjectStudents)
                .ThenInclude(ps => ps.Student)
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
                .Include(p => p.ProjectStudents)
                    .ThenInclude(ps => ps.Student)
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
                case ProjectStatus.Submitted:
                    project.SubmissionForReviewDate = DateTime.Now;
                    break;
                case ProjectStatus.SupervisorApproved:
                    project.SupervisorReviewDate = DateTime.Now;
                    project.SupervisorComments = comments;
                    break;
                case ProjectStatus.EvaluatorApproved:
                    project.EvaluatorReviewDate = DateTime.Now;
                    project.EvaluatorComments = comments;
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
            ProjectStatus.Created => new List<string> { "Mark as Submitted" },
            ProjectStatus.Submitted => new List<string> { "Supervisor Approve", "Supervisor Reject" },
            ProjectStatus.SupervisorApproved => new List<string> { "Evaluator Approve", "Evaluator Reject" },
            ProjectStatus.EvaluatorApproved => new List<string> { "Publish Project" },
            ProjectStatus.Published => new List<string> { },
            _ => new List<string>()
        };
    }

    private List<ProjectStatus> GetNextValidStatuses(ProjectStatus currentStatus)
    {
        return currentStatus switch
        {
            ProjectStatus.Created => new List<ProjectStatus> { ProjectStatus.Submitted },
            ProjectStatus.Submitted => new List<ProjectStatus> { ProjectStatus.SupervisorApproved, ProjectStatus.Created },
            ProjectStatus.SupervisorApproved => new List<ProjectStatus> { ProjectStatus.EvaluatorApproved, ProjectStatus.Submitted },
            ProjectStatus.EvaluatorApproved => new List<ProjectStatus> { ProjectStatus.Published },
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