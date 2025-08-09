using Microsoft.AspNetCore.Mvc;
using LibraryApp.Models;
using LibraryApp.Services;
using LibraryApp.Attributes;

namespace LibraryApp.Controllers;

public class CommentsController : BaseController
{
    private readonly IProjectCommentService _commentService;

    public CommentsController(IProjectCommentService commentService, IUniversitySettingsService universitySettings, ISessionService sessionService) 
        : base(universitySettings, sessionService)
    {
        _commentService = commentService;
    }

    [HttpGet]
    public async Task<IActionResult> GetProjectComments(int projectId)
    {
        if (!IsAuthenticated)
        {
            return Json(new { success = false, message = "Authentication required" });
        }

        var canComment = await _commentService.CanUserCommentOnProjectAsync(projectId, CurrentUserId!, CurrentUserRoleEnum!.Value);
        if (!canComment)
        {
            return Json(new { success = false, message = "Access denied" });
        }

        var comments = await _commentService.GetProjectCommentsAsync(projectId, CurrentUserId!, CurrentUserRoleEnum!.Value);
        
        return Json(new { 
            success = true, 
            comments = comments.Select(c => new {
                id = c.Id,
                content = c.Content,
                type = c.Type.ToString(),
                createdAt = c.CreatedAt.ToString("MMM dd, yyyy 'at' h:mm tt"),
                updatedAt = c.UpdatedAt?.ToString("MMM dd, yyyy 'at' h:mm tt"),
                isEdited = c.IsEdited,
                authorName = c.AuthorName,
                authorRole = c.AuthorRole.ToString(),
                isOwnComment = c.IsOwnComment,
                canEdit = c.CanEdit,
                canDelete = c.CanDelete,
                parentCommentId = c.ParentCommentId,
                requiresAcknowledgment = c.RequiresStudentAcknowledgment,
                isAcknowledged = c.IsAcknowledged,
                replies = c.Replies.Select(r => new {
                    id = r.Id,
                    content = r.Content,
                    createdAt = r.CreatedAt.ToString("MMM dd, yyyy 'at' h:mm tt"),
                    authorName = r.AuthorName,
                    authorRole = r.AuthorRole.ToString(),
                    isOwnComment = r.IsOwnComment,
                    canEdit = r.CanEdit,
                    canDelete = r.CanDelete
                })
            })
        });
    }

    [HttpPost]
    [RequireAnyPermission(PermissionType.AddComments)]
    public async Task<IActionResult> AddComment([FromBody] AddCommentViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return Json(new { success = false, message = "Invalid comment data" });
        }

        var canComment = await _commentService.CanUserCommentOnProjectAsync(model.ProjectId, CurrentUserId!, CurrentUserRoleEnum!.Value);
        if (!canComment)
        {
            return Json(new { success = false, message = "You don't have permission to comment on this project" });
        }

        try
        {
            var comment = await _commentService.AddCommentAsync(
                model.ProjectId, 
                CurrentUserId!, 
                CurrentUserRoleEnum!.Value, 
                model.Content, 
                model.Type, 
                model.ParentCommentId,
                model.RequiresStudentAcknowledgment
            );

            return Json(new { 
                success = true, 
                message = "Comment added successfully",
                commentId = comment.Id
            });
        }
        catch (Exception ex)
        {
            return Json(new { success = false, message = "Failed to add comment: " + ex.Message });
        }
    }

    [HttpPost]
    public async Task<IActionResult> UpdateComment([FromBody] UpdateCommentModel model)
    {
        if (!ModelState.IsValid)
        {
            return Json(new { success = false, message = "Invalid comment data" });
        }

        var success = await _commentService.UpdateCommentAsync(model.CommentId, CurrentUserId!, model.Content);
        
        if (success)
        {
            return Json(new { success = true, message = "Comment updated successfully" });
        }
        else
        {
            return Json(new { success = false, message = "Failed to update comment" });
        }
    }

    [HttpPost]
    public async Task<IActionResult> DeleteComment(int commentId)
    {
        var success = await _commentService.DeleteCommentAsync(commentId, CurrentUserId!, CurrentUserRoleEnum!.Value);
        
        if (success)
        {
            return Json(new { success = true, message = "Comment deleted successfully" });
        }
        else
        {
            return Json(new { success = false, message = "Failed to delete comment" });
        }
    }

    [HttpPost]
    public async Task<IActionResult> AcknowledgeComment(int commentId)
    {
        if (CurrentUserRoleEnum != UserRole.Student)
        {
            return Json(new { success = false, message = "Only students can acknowledge comments" });
        }

        var success = await _commentService.AcknowledgeCommentAsync(commentId, CurrentUserId!);
        
        if (success)
        {
            return Json(new { success = true, message = "Comment acknowledged successfully" });
        }
        else
        {
            return Json(new { success = false, message = "Failed to acknowledge comment" });
        }
    }
}

public class UpdateCommentModel
{
    public int CommentId { get; set; }
    public required string Content { get; set; }
}