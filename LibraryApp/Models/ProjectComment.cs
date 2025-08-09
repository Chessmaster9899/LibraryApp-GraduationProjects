using System.ComponentModel.DataAnnotations;

namespace LibraryApp.Models;

public enum CommentType
{
    General,
    ReviewComment,
    SupervisorNote,
    EvaluatorComment,
    StudentResponse
}

public class ProjectComment
{
    public int Id { get; set; }
    
    [Required]
    public int ProjectId { get; set; }
    
    [Required]
    [StringLength(500)]
    public required string AuthorId { get; set; } // User ID (Student Number, Professor ID, etc.)
    
    [Required]
    public UserRole AuthorRole { get; set; }
    
    [Required]
    [StringLength(2000)]
    public required string Content { get; set; }
    
    public CommentType Type { get; set; } = CommentType.General;
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    public DateTime? UpdatedAt { get; set; }
    
    public bool IsEdited { get; set; } = false;
    
    public bool IsVisible { get; set; } = true;
    
    // Optional reference to another comment (for replies)
    public int? ParentCommentId { get; set; }
    
    // For tracking approval workflow
    public bool RequiresStudentAcknowledgment { get; set; } = false;
    public DateTime? StudentAcknowledgedAt { get; set; }
    
    // Navigation properties
    public Project Project { get; set; } = null!;
    public ProjectComment? ParentComment { get; set; }
    public ICollection<ProjectComment> Replies { get; set; } = new List<ProjectComment>();
}

// View Models for Comments
public class ProjectCommentsViewModel
{
    public Project Project { get; set; } = null!;
    public List<ProjectCommentDisplayModel> Comments { get; set; } = new();
    public bool CanAddComments { get; set; }
    public bool CanModerateComments { get; set; }
    public string CurrentUserId { get; set; } = string.Empty;
    public UserRole CurrentUserRole { get; set; }
}

public class ProjectCommentDisplayModel
{
    public int Id { get; set; }
    public string Content { get; set; } = string.Empty;
    public CommentType Type { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public bool IsEdited { get; set; }
    public string AuthorName { get; set; } = string.Empty;
    public UserRole AuthorRole { get; set; }
    public bool IsOwnComment { get; set; }
    public bool CanEdit { get; set; }
    public bool CanDelete { get; set; }
    public int? ParentCommentId { get; set; }
    public List<ProjectCommentDisplayModel> Replies { get; set; } = new();
    public bool RequiresStudentAcknowledgment { get; set; }
    public bool IsAcknowledged { get; set; }
}

public class AddCommentViewModel
{
    [Required]
    public int ProjectId { get; set; }
    
    [Required]
    [StringLength(2000)]
    public required string Content { get; set; }
    
    public CommentType Type { get; set; } = CommentType.General;
    
    public int? ParentCommentId { get; set; }
    
    public bool RequiresStudentAcknowledgment { get; set; } = false;
}