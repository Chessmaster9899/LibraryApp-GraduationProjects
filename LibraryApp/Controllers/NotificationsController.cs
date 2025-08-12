using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using LibraryApp.Data;
using LibraryApp.Models;
using LibraryApp.Services;
using LibraryApp.Attributes;

namespace LibraryApp.Controllers;

[SessionAuthorize(UserRole.Student, UserRole.Professor, UserRole.Admin)]
public class NotificationsController : BaseController
{
    private readonly LibraryContext _context;

    public NotificationsController(LibraryContext context, IUniversitySettingsService universitySettings, ISessionService sessionService) 
        : base(universitySettings, sessionService)
    {
        _context = context;
    }

    // GET: Notifications
    public async Task<IActionResult> Index(int page = 1, int pageSize = 20)
    {
        if (CurrentUserId == null || CurrentUserRoleEnum == null)
        {
            return RedirectToAction("Login", "Auth");
        }

        var notifications = _context.Notifications
            .Where(n => n.UserId == CurrentUserId && n.UserRole == CurrentUserRoleEnum.Value)
            .OrderByDescending(n => n.CreatedDate);

        var totalNotifications = await notifications.CountAsync();
        var paginatedNotifications = await notifications
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        ViewBag.CurrentPage = page;
        ViewBag.TotalPages = (int)Math.Ceiling((double)totalNotifications / pageSize);
        ViewBag.TotalNotifications = totalNotifications;
        ViewBag.UnreadCount = await _context.Notifications
            .CountAsync(n => n.UserId == CurrentUserId && n.UserRole == CurrentUserRoleEnum.Value && !n.IsRead);

        return View(paginatedNotifications);
    }

    // GET: Notifications/Unread
    public async Task<IActionResult> Unread()
    {
        if (CurrentUserId == null || CurrentUserRoleEnum == null)
        {
            return RedirectToAction("Login", "Auth");
        }

        var unreadNotifications = await _context.Notifications
            .Where(n => n.UserId == CurrentUserId && n.UserRole == CurrentUserRoleEnum.Value && !n.IsRead)
            .OrderByDescending(n => n.CreatedDate)
            .ToListAsync();

        return View("Index", unreadNotifications);
    }

    // POST: Notifications/MarkAsRead/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> MarkAsRead(int id)
    {
        if (CurrentUserId == null || CurrentUserRoleEnum == null)
        {
            return BadRequest();
        }

        var notification = await _context.Notifications
            .FirstOrDefaultAsync(n => n.Id == id && n.UserId == CurrentUserId && n.UserRole == CurrentUserRoleEnum.Value);

        if (notification == null)
        {
            return NotFound();
        }

        notification.IsRead = true;
        await _context.SaveChangesAsync();

        if (!string.IsNullOrEmpty(notification.RelatedUrl))
        {
            return Redirect(notification.RelatedUrl);
        }

        return RedirectToAction("Index");
    }

    // POST: Notifications/MarkAllAsRead
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> MarkAllAsRead()
    {
        if (CurrentUserId == null || CurrentUserRoleEnum == null)
        {
            return BadRequest();
        }

        var unreadNotifications = await _context.Notifications
            .Where(n => n.UserId == CurrentUserId && n.UserRole == CurrentUserRoleEnum.Value && !n.IsRead)
            .ToListAsync();

        foreach (var notification in unreadNotifications)
        {
            notification.IsRead = true;
        }

        await _context.SaveChangesAsync();
        
        this.AddSuccess("All Notifications Marked", "All notifications have been marked as read");
        return RedirectToAction("Index");
    }

    // DELETE: Notifications/Delete/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        if (CurrentUserId == null || CurrentUserRoleEnum == null)
        {
            return BadRequest();
        }

        var notification = await _context.Notifications
            .FirstOrDefaultAsync(n => n.Id == id && n.UserId == CurrentUserId && n.UserRole == CurrentUserRoleEnum.Value);

        if (notification == null)
        {
            return NotFound();
        }

        _context.Notifications.Remove(notification);
        await _context.SaveChangesAsync();

        this.AddSuccess("Notification Deleted", "The notification has been removed");
        return RedirectToAction("Index");
    }

    // GET: Notifications/Count (API endpoint for real-time updates)
    [HttpGet]
    public async Task<IActionResult> GetUnreadCount()
    {
        if (CurrentUserId == null || CurrentUserRoleEnum == null)
        {
            return Json(new { count = 0 });
        }

        var count = await _context.Notifications
            .CountAsync(n => n.UserId == CurrentUserId && n.UserRole == CurrentUserRoleEnum.Value && !n.IsRead);

        return Json(new { count });
    }

    // GET: Notifications/Recent (API endpoint for dropdown)
    [HttpGet]
    public async Task<IActionResult> GetRecent(int limit = 5)
    {
        if (CurrentUserId == null || CurrentUserRoleEnum == null)
        {
            return Json(new List<object>());
        }

        var recentNotifications = await _context.Notifications
            .Where(n => n.UserId == CurrentUserId && n.UserRole == CurrentUserRoleEnum.Value)
            .OrderByDescending(n => n.CreatedDate)
            .Take(limit)
            .Select(n => new {
                n.Id,
                n.Title,
                n.Message,
                n.CreatedDate,
                n.IsRead,
                n.RelatedUrl,
                TimeAgo = GetTimeAgo(n.CreatedDate)
            })
            .ToListAsync();

        return Json(recentNotifications);
    }

    private string GetTimeAgo(DateTime dateTime)
    {
        var timeSpan = DateTime.Now - dateTime;
        
        if (timeSpan.TotalDays >= 1)
            return $"{(int)timeSpan.TotalDays} day{((int)timeSpan.TotalDays != 1 ? "s" : "")} ago";
        
        if (timeSpan.TotalHours >= 1)
            return $"{(int)timeSpan.TotalHours} hour{((int)timeSpan.TotalHours != 1 ? "s" : "")} ago";
        
        if (timeSpan.TotalMinutes >= 1)
            return $"{(int)timeSpan.TotalMinutes} minute{((int)timeSpan.TotalMinutes != 1 ? "s" : "")} ago";
        
        return "Just now";
    }
}

// Notification Helper Service
public interface INotificationService
{
    Task CreateNotificationAsync(string userId, UserRole userRole, string title, string message, 
        string? relatedUrl = null, string? relatedEntityType = null, int? relatedEntityId = null);
    Task CreateBulkNotificationAsync(List<string> userIds, UserRole userRole, string title, string message, 
        string? relatedUrl = null, string? relatedEntityType = null, int? relatedEntityId = null);
    Task NotifyProjectStatusChangeAsync(Project project, string action);
    Task NotifyNewProjectSubmissionAsync(ProjectSubmission submission);
    Task NotifyAdminsAsync(string title, string message, string? relatedUrl = null);
}

public class NotificationService : INotificationService
{
    private readonly LibraryContext _context;

    public NotificationService(LibraryContext context)
    {
        _context = context;
    }

    public async Task CreateNotificationAsync(string userId, UserRole userRole, string title, string message, 
        string? relatedUrl = null, string? relatedEntityType = null, int? relatedEntityId = null)
    {
        var notification = new Notification
        {
            UserId = userId,
            UserRole = userRole,
            Title = title,
            Message = message,
            RelatedUrl = relatedUrl,
            RelatedEntityType = relatedEntityType,
            RelatedEntityId = relatedEntityId
        };

        _context.Notifications.Add(notification);
        await _context.SaveChangesAsync();
    }

    public async Task CreateBulkNotificationAsync(List<string> userIds, UserRole userRole, string title, string message, 
        string? relatedUrl = null, string? relatedEntityType = null, int? relatedEntityId = null)
    {
        var notifications = userIds.Select(userId => new Notification
        {
            UserId = userId,
            UserRole = userRole,
            Title = title,
            Message = message,
            RelatedUrl = relatedUrl,
            RelatedEntityType = relatedEntityType,
            RelatedEntityId = relatedEntityId
        }).ToList();

        _context.Notifications.AddRange(notifications);
        await _context.SaveChangesAsync();
    }

    public async Task NotifyProjectStatusChangeAsync(Project project, string action)
    {
        var student = await _context.Students.FindAsync(project.StudentId);
        var supervisor = await _context.Professors.FindAsync(project.SupervisorId);

        if (student != null)
        {
            await CreateNotificationAsync(
                student.StudentNumber,
                UserRole.Student,
                $"Project {action}",
                $"Your project '{project.Title}' has been {action.ToLower()}.",
                $"/Student/Projects"
            );
        }

        if (supervisor != null)
        {
            await CreateNotificationAsync(
                supervisor.ProfessorId,
                UserRole.Professor,
                $"Project {action}",
                $"Project '{project.Title}' has been {action.ToLower()}.",
                $"/Professor/SupervisedProjects"
            );
        }
    }

    public async Task NotifyNewProjectSubmissionAsync(ProjectSubmission submission)
    {
        var project = await _context.Projects
            .Include(p => p.Student)
            .FirstAsync(p => p.Id == submission.ProjectId);

        await NotifyAdminsAsync(
            "New Project Submission",
            $"Project '{project.Title}' by {project.Student.FullName} has been submitted for review.",
            $"/Admin/ReviewSubmission/{submission.Id}"
        );
    }

    public async Task NotifyAdminsAsync(string title, string message, string? relatedUrl = null)
    {
        var admins = await _context.Admins.Where(a => a.IsActive).ToListAsync();
        var adminUserIds = admins.Select(a => a.Username).ToList();

        await CreateBulkNotificationAsync(adminUserIds, UserRole.Admin, title, message, relatedUrl);
    }
}