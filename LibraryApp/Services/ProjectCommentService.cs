using Microsoft.EntityFrameworkCore;
using LibraryApp.Data;
using LibraryApp.Models;

namespace LibraryApp.Services;

public interface IProjectCommentService
{
    Task<List<ProjectCommentDisplayModel>> GetProjectCommentsAsync(int projectId, string currentUserId, UserRole currentUserRole);
    Task<ProjectComment> AddCommentAsync(int projectId, string authorId, UserRole authorRole, string content, CommentType type = CommentType.General, int? parentCommentId = null, bool requiresAcknowledgment = false);
    Task<bool> UpdateCommentAsync(int commentId, string authorId, string newContent);
    Task<bool> DeleteCommentAsync(int commentId, string userId, UserRole userRole);
    Task<bool> AcknowledgeCommentAsync(int commentId, string studentId);
    Task<bool> CanUserCommentOnProjectAsync(int projectId, string userId, UserRole userRole);
    Task<bool> CanUserModerateCommentsAsync(int projectId, string userId, UserRole userRole);
}

public class ProjectCommentService : IProjectCommentService
{
    private readonly LibraryContext _context;

    public ProjectCommentService(LibraryContext context)
    {
        _context = context;
    }

    public async Task<List<ProjectCommentDisplayModel>> GetProjectCommentsAsync(int projectId, string currentUserId, UserRole currentUserRole)
    {
        var comments = await _context.ProjectComments
            .Where(c => c.ProjectId == projectId && c.IsVisible && c.ParentCommentId == null)
            .OrderBy(c => c.CreatedAt)
            .ToListAsync();

        var commentDisplayModels = new List<ProjectCommentDisplayModel>();

        foreach (var comment in comments)
        {
            var displayModel = await CreateCommentDisplayModel(comment, currentUserId, currentUserRole);
            
            // Load replies
            displayModel.Replies = await GetCommentRepliesAsync(comment.Id, currentUserId, currentUserRole);
            
            commentDisplayModels.Add(displayModel);
        }

        return commentDisplayModels;
    }

    private async Task<List<ProjectCommentDisplayModel>> GetCommentRepliesAsync(int parentCommentId, string currentUserId, UserRole currentUserRole)
    {
        var replies = await _context.ProjectComments
            .Where(c => c.ParentCommentId == parentCommentId && c.IsVisible)
            .OrderBy(c => c.CreatedAt)
            .ToListAsync();

        var replyDisplayModels = new List<ProjectCommentDisplayModel>();

        foreach (var reply in replies)
        {
            var displayModel = await CreateCommentDisplayModel(reply, currentUserId, currentUserRole);
            replyDisplayModels.Add(displayModel);
        }

        return replyDisplayModels;
    }

    private async Task<ProjectCommentDisplayModel> CreateCommentDisplayModel(ProjectComment comment, string currentUserId, UserRole currentUserRole)
    {
        var authorName = await GetAuthorNameAsync(comment.AuthorId, comment.AuthorRole);
        var isOwnComment = comment.AuthorId == currentUserId && comment.AuthorRole == currentUserRole;
        var canEdit = isOwnComment || currentUserRole == UserRole.Admin;
        var canDelete = isOwnComment || currentUserRole == UserRole.Admin || 
                       (currentUserRole == UserRole.Professor && await CanProfessorModerateProject(comment.ProjectId, currentUserId));

        return new ProjectCommentDisplayModel
        {
            Id = comment.Id,
            Content = comment.Content,
            Type = comment.Type,
            CreatedAt = comment.CreatedAt,
            UpdatedAt = comment.UpdatedAt,
            IsEdited = comment.IsEdited,
            AuthorName = authorName,
            AuthorRole = comment.AuthorRole,
            IsOwnComment = isOwnComment,
            CanEdit = canEdit,
            CanDelete = canDelete,
            ParentCommentId = comment.ParentCommentId,
            RequiresStudentAcknowledgment = comment.RequiresStudentAcknowledgment,
            IsAcknowledged = comment.StudentAcknowledgedAt.HasValue
        };
    }

    public async Task<ProjectComment> AddCommentAsync(int projectId, string authorId, UserRole authorRole, string content, CommentType type = CommentType.General, int? parentCommentId = null, bool requiresAcknowledgment = false)
    {
        var comment = new ProjectComment
        {
            ProjectId = projectId,
            AuthorId = authorId,
            AuthorRole = authorRole,
            Content = content,
            Type = type,
            ParentCommentId = parentCommentId,
            RequiresStudentAcknowledgment = requiresAcknowledgment
        };

        _context.ProjectComments.Add(comment);
        await _context.SaveChangesAsync();

        return comment;
    }

    public async Task<bool> UpdateCommentAsync(int commentId, string authorId, string newContent)
    {
        var comment = await _context.ProjectComments.FindAsync(commentId);
        
        if (comment == null || comment.AuthorId != authorId)
        {
            return false;
        }

        comment.Content = newContent;
        comment.UpdatedAt = DateTime.UtcNow;
        comment.IsEdited = true;

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeleteCommentAsync(int commentId, string userId, UserRole userRole)
    {
        var comment = await _context.ProjectComments
            .Include(c => c.Project)
            .FirstOrDefaultAsync(c => c.Id == commentId);

        if (comment == null)
        {
            return false;
        }

        // Check permissions
        var canDelete = comment.AuthorId == userId && comment.AuthorRole == userRole;
        canDelete = canDelete || userRole == UserRole.Admin;
        canDelete = canDelete || (userRole == UserRole.Professor && await CanProfessorModerateProject(comment.ProjectId, userId));

        if (!canDelete)
        {
            return false;
        }

        // Soft delete by marking as invisible
        comment.IsVisible = false;
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> AcknowledgeCommentAsync(int commentId, string studentId)
    {
        var comment = await _context.ProjectComments
            .Include(c => c.Project)
            .FirstOrDefaultAsync(c => c.Id == commentId);

        if (comment == null || !comment.RequiresStudentAcknowledgment)
        {
            return false;
        }

        // Verify the student is the project owner
        var project = await _context.Projects
            .Include(p => p.Student)
            .FirstOrDefaultAsync(p => p.Id == comment.ProjectId);

        if (project == null || project.Student.StudentNumber != studentId)
        {
            return false;
        }

        comment.StudentAcknowledgedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> CanUserCommentOnProjectAsync(int projectId, string userId, UserRole userRole)
    {
        var project = await _context.Projects
            .Include(p => p.Student)
            .Include(p => p.Supervisor)
            .Include(p => p.Evaluator)
            .FirstOrDefaultAsync(p => p.Id == projectId);

        if (project == null)
        {
            return false;
        }

        return userRole switch
        {
            UserRole.Admin => true,
            UserRole.Student => project.Student.StudentNumber == userId,
            UserRole.Professor => project.Supervisor.ProfessorId == userId || 
                                 (project.Evaluator != null && project.Evaluator.ProfessorId == userId),
            _ => false
        };
    }

    public async Task<bool> CanUserModerateCommentsAsync(int projectId, string userId, UserRole userRole)
    {
        if (userRole == UserRole.Admin)
        {
            return true;
        }

        if (userRole == UserRole.Professor)
        {
            return await CanProfessorModerateProject(projectId, userId);
        }

        return false;
    }

    private async Task<bool> CanProfessorModerateProject(int projectId, string professorId)
    {
        var project = await _context.Projects
            .Include(p => p.Supervisor)
            .Include(p => p.Evaluator)
            .FirstOrDefaultAsync(p => p.Id == projectId);

        if (project == null)
        {
            return false;
        }

        return project.Supervisor.ProfessorId == professorId || 
               (project.Evaluator != null && project.Evaluator.ProfessorId == professorId);
    }

    private async Task<string> GetAuthorNameAsync(string authorId, UserRole authorRole)
    {
        return authorRole switch
        {
            UserRole.Student => await GetStudentNameAsync(authorId),
            UserRole.Professor => await GetProfessorNameAsync(authorId),
            UserRole.Admin => "Administrator",
            _ => "Unknown User"
        };
    }

    private async Task<string> GetStudentNameAsync(string studentNumber)
    {
        var student = await _context.Students
            .FirstOrDefaultAsync(s => s.StudentNumber == studentNumber);
        
        return student?.FullName ?? "Unknown Student";
    }

    private async Task<string> GetProfessorNameAsync(string professorId)
    {
        var professor = await _context.Professors
            .FirstOrDefaultAsync(p => p.ProfessorId == professorId);
        
        return professor?.DisplayName ?? "Unknown Professor";
    }
}