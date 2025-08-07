using System.ComponentModel.DataAnnotations;

namespace LibraryApp.Models;

public enum SubmissionStatus
{
    Pending,
    UnderReview,
    Approved,
    Rejected,
    NeedsRevision
}

public class ProjectSubmission
{
    public int Id { get; set; }
    
    [Required]
    public int ProjectId { get; set; }
    
    [Required]
    [Display(Name = "Submission Date")]
    public DateTime SubmissionDate { get; set; } = DateTime.Now;
    
    [Required]
    [StringLength(1000)]
    [Display(Name = "Submission Comments")]
    public required string SubmissionComments { get; set; }
    
    [Required]
    public SubmissionStatus Status { get; set; } = SubmissionStatus.Pending;
    
    [Display(Name = "Review Date")]
    public DateTime? ReviewDate { get; set; }
    
    [StringLength(2000)]
    [Display(Name = "Review Comments")]
    public string? ReviewComments { get; set; }
    
    [Display(Name = "Reviewed By")]
    public string? ReviewedBy { get; set; }
    
    [Display(Name = "Poster File")]
    public string? PosterFilePath { get; set; }
    
    [Display(Name = "Report File")]
    public string? ReportFilePath { get; set; }
    
    [Display(Name = "Code Files")]
    public string? CodeFilesPath { get; set; }
    
    // Navigation properties
    public Project Project { get; set; } = null!;
}

public class Notification
{
    public int Id { get; set; }
    
    [Required]
    public required string UserId { get; set; }
    
    [Required]
    public UserRole UserRole { get; set; }
    
    [Required]
    [StringLength(200)]
    public required string Title { get; set; }
    
    [Required]
    [StringLength(1000)]
    public required string Message { get; set; }
    
    [Required]
    public DateTime CreatedDate { get; set; } = DateTime.Now;
    
    public bool IsRead { get; set; } = false;
    
    public string? RelatedUrl { get; set; }
    
    public string? RelatedEntityType { get; set; }
    
    public int? RelatedEntityId { get; set; }
}

public class SystemAuditLog
{
    public int Id { get; set; }
    
    [Required]
    public required string UserId { get; set; }
    
    [Required]
    public UserRole UserRole { get; set; }
    
    [Required]
    [StringLength(100)]
    public required string Action { get; set; }
    
    [Required]
    [StringLength(100)]
    public required string EntityType { get; set; }
    
    public int? EntityId { get; set; }
    
    [StringLength(1000)]
    public string? Details { get; set; }
    
    [Required]
    public DateTime Timestamp { get; set; } = DateTime.Now;
    
    [StringLength(50)]
    public string? IPAddress { get; set; }
    
    [StringLength(200)]
    public string? UserAgent { get; set; }
}

public class Announcement
{
    public int Id { get; set; }
    
    [Required]
    [StringLength(200)]
    public required string Title { get; set; }
    
    [Required]
    [StringLength(2000)]
    public required string Content { get; set; }
    
    [Required]
    public DateTime CreatedDate { get; set; } = DateTime.Now;
    
    [Required]
    public required string CreatedBy { get; set; }
    
    public DateTime? ExpiryDate { get; set; }
    
    public bool IsActive { get; set; } = true;
    
    public bool IsUrgent { get; set; } = false;
    
    [StringLength(500)]
    public string? TargetRoles { get; set; } // Comma-separated roles
}