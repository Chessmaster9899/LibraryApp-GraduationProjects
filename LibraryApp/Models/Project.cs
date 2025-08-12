using System.ComponentModel.DataAnnotations;

namespace LibraryApp.Models;

public enum ProjectStatus
{
    Created,          // Admin created the project
    Proposed,         // Project proposed
    InProgress,       // Project in progress
    Submitted,        // Students submitted their work
    SubmittedForReview, // Submitted for review
    SupervisorApproved, // Supervisor approved the submission
    EvaluatorApproved,  // Evaluator approved (final approval)
    Approved,         // General approved status
    Completed,        // Project completed
    Defended,         // Project defended
    Published,        // Admin published to gallery
    ReviewApproved,   // Review approved by admin
    ReviewRejected    // Review rejected by admin
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
    
    public ProjectStatus Status { get; set; } = ProjectStatus.Created;
    
    [Display(Name = "Submission Date")]
    [Required]
    [DataType(DataType.Date)]
    public DateTime SubmissionDate { get; set; }
    
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
    
    [Display(Name = "Supervisor Review Date")]
    public DateTime? SupervisorReviewDate { get; set; }
    
    [Display(Name = "Supervisor Comments")]
    public string? SupervisorComments { get; set; }
    
    [Display(Name = "Evaluator Review Date")]
    public DateTime? EvaluatorReviewDate { get; set; }
    
    [Display(Name = "Evaluator Comments")]
    public string? EvaluatorComments { get; set; }
    
    [Display(Name = "Defense Date")]
    public DateTime? DefenseDate { get; set; }
    
    [Display(Name = "Grade")]
    public string? Grade { get; set; }
    
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
    public ICollection<ProjectStudent> ProjectStudents { get; set; } = new List<ProjectStudent>();
}