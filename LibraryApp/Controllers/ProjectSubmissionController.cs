using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using LibraryApp.Data;
using LibraryApp.Models;
using LibraryApp.Attributes;
using LibraryApp.Services;

namespace LibraryApp.Controllers;

[SessionAuthorize(UserRole.Student, UserRole.Professor)]
public class ProjectSubmissionController : BaseController
{
    private readonly LibraryContext _context;

    public ProjectSubmissionController(LibraryContext context, IUniversitySettingsService universitySettings, ISessionService sessionService) : base(universitySettings, sessionService)
    {
        _context = context;
    }

    [HttpGet]
    [SessionAuthorize(UserRole.Student, UserRole.Professor)]
    public async Task<IActionResult> Submit(int projectId)
    {
        var project = await _context.Projects
            .Include(p => p.Student)
            .Include(p => p.Supervisor)
            .FirstOrDefaultAsync(p => p.Id == projectId);

        if (project == null)
        {
            return NotFound();
        }

        // Check if user has permission to submit for this project
        var hasPermission = false;
        if (CurrentUserRoleEnum == UserRole.Student && CurrentEntityId == project.StudentId)
        {
            hasPermission = true;
        }
        else if (CurrentUserRoleEnum == UserRole.Professor && 
                (CurrentEntityId == project.SupervisorId || CurrentEntityId == project.EvaluatorId))
        {
            hasPermission = true;
        }

        if (!hasPermission)
        {
            return Forbid();
        }

        // Check if project is in correct status for submission
        if (project.Status != ProjectStatus.Completed)
        {
            TempData["Error"] = "Project must be marked as completed before submission for review.";
            return RedirectToAction("Details", "Projects", new { id = projectId });
        }

        // Check if there's already a pending submission
        var existingSubmission = await _context.ProjectSubmissions
            .FirstOrDefaultAsync(ps => ps.ProjectId == projectId && 
                                      ps.Status == SubmissionStatus.Pending);

        if (existingSubmission != null)
        {
            TempData["Error"] = "There is already a pending submission for this project.";
            return RedirectToAction("Details", "Projects", new { id = projectId });
        }

        var viewModel = new ProjectSubmissionViewModel
        {
            Project = project
        };

        return View(viewModel);
    }

    [HttpPost]
    [SessionAuthorize(UserRole.Student, UserRole.Professor)]
    public async Task<IActionResult> Submit(ProjectSubmissionViewModel model)
    {
        if (!ModelState.IsValid)
        {
            model.Project = await _context.Projects
                .Include(p => p.Student)
                .Include(p => p.Supervisor)
                .FirstAsync(p => p.Id == model.Project.Id);
            return View(model);
        }

        var project = await _context.Projects.FindAsync(model.Project.Id);
        if (project == null)
        {
            return NotFound();
        }

        // Create uploads directory if it doesn't exist
        var uploadsPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "submissions");
        Directory.CreateDirectory(uploadsPath);

        var submission = new ProjectSubmission
        {
            ProjectId = project.Id,
            SubmissionComments = model.SubmissionComments,
            Status = SubmissionStatus.Pending
        };

        // Handle file uploads
        if (model.PosterFile != null)
        {
            var posterFileName = $"poster_{project.Id}_{Guid.NewGuid()}{Path.GetExtension(model.PosterFile.FileName)}";
            var posterPath = Path.Combine(uploadsPath, posterFileName);
            
            using (var stream = new FileStream(posterPath, FileMode.Create))
            {
                await model.PosterFile.CopyToAsync(stream);
            }
            
            submission.PosterFilePath = $"/uploads/submissions/{posterFileName}";
        }

        if (model.ReportFile != null)
        {
            var reportFileName = $"report_{project.Id}_{Guid.NewGuid()}{Path.GetExtension(model.ReportFile.FileName)}";
            var reportPath = Path.Combine(uploadsPath, reportFileName);
            
            using (var stream = new FileStream(reportPath, FileMode.Create))
            {
                await model.ReportFile.CopyToAsync(stream);
            }
            
            submission.ReportFilePath = $"/uploads/submissions/{reportFileName}";
        }

        if (model.CodeFiles != null)
        {
            var codeFileName = $"code_{project.Id}_{Guid.NewGuid()}{Path.GetExtension(model.CodeFiles.FileName)}";
            var codePath = Path.Combine(uploadsPath, codeFileName);
            
            using (var stream = new FileStream(codePath, FileMode.Create))
            {
                await model.CodeFiles.CopyToAsync(stream);
            }
            
            submission.CodeFilesPath = $"/uploads/submissions/{codeFileName}";
        }

        _context.ProjectSubmissions.Add(submission);
        
        // Update project status
        project.Status = ProjectStatus.SubmittedForReview;
        project.SubmissionForReviewDate = DateTime.Now;

        // Create notification for admins
        var notification = new Notification
        {
            UserId = "Admin",
            UserRole = UserRole.Admin,
            Title = "New Project Submission",
            Message = $"Project '{project.Title}' has been submitted for review.",
            RelatedUrl = $"/Admin/ReviewSubmission/{submission.Id}",
            RelatedEntityType = "ProjectSubmission",
            RelatedEntityId = submission.Id
        };
        
        _context.Notifications.Add(notification);
        await _context.SaveChangesAsync();

        TempData["Success"] = "Project submitted for review successfully.";
        return RedirectToAction("Details", "Projects", new { id = project.Id });
    }

    [HttpGet]
    [SessionAuthorize(UserRole.Student, UserRole.Professor)]
    public async Task<IActionResult> Status(int projectId)
    {
        var submissions = await _context.ProjectSubmissions
            .Include(ps => ps.Project)
            .Where(ps => ps.ProjectId == projectId)
            .OrderByDescending(ps => ps.SubmissionDate)
            .ToListAsync();

        return View(submissions);
    }
}