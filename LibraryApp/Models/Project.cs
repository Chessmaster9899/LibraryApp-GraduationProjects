using System.ComponentModel.DataAnnotations;

namespace LibraryApp.Models;

public enum ProjectStatus
{
    Proposed,
    Approved,
    InProgress,
    Completed,
    SubmittedForReview,
    ReviewApproved,
    ReviewRejected,
    Defended,
    Published
}

public class Project
{
    public int Id { get; set; }
    
    [Required]
    [StringLength(200)]
    public required string Title { get; set; }
    
    [StringLength(2000)]
    public string? Abstract { get; set; }
    
    public string? Keywords { get; set; }
    
    public ProjectStatus Status { get; set; } = ProjectStatus.Proposed;
    
    [Display(Name = "Submission Date")]
    [Required]
    [DataType(DataType.Date)]
    public DateTime SubmissionDate { get; set; }
    
    [Display(Name = "Defense Date")]
    [DataType(DataType.Date)]
    public DateTime? DefenseDate { get; set; }
    
    public string? Grade { get; set; }
    
    [Display(Name = "Document Path")]
    public string? DocumentPath { get; set; } // Path to PDF or document file
    
    [Display(Name = "Poster Path")]
    public string? PosterPath { get; set; } // Path to project poster
    
    [Display(Name = "Report Path")]
    public string? ReportPath { get; set; } // Path to final report
    
    [Display(Name = "Code Path")]
    public string? CodePath { get; set; } // Path to source code files
    
    [Display(Name = "Submission Date for Review")]
    public DateTime? SubmissionForReviewDate { get; set; }
    
    [Display(Name = "Review Date")]
    public DateTime? ReviewDate { get; set; }
    
    [Display(Name = "Review Comments")]
    public string? ReviewComments { get; set; }
    
    [Display(Name = "Reviewed By")]
    public string? ReviewedBy { get; set; }
    
    public bool IsPubliclyVisible { get; set; } = false; // For guest viewing
    
    // Foreign keys
    [Display(Name = "Student")]
    [Required]
    public int StudentId { get; set; }
    
    [Display(Name = "Supervisor")]
    [Required]
    public int SupervisorId { get; set; }
    
    [Display(Name = "Evaluator")]
    public int? EvaluatorId { get; set; }
    
    // Navigation properties
    public Student Student { get; set; } = null!;
    public Professor Supervisor { get; set; } = null!;
    public Professor? Evaluator { get; set; }
}