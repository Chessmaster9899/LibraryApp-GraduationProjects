using System.ComponentModel.DataAnnotations;

namespace LibraryApp.Models;

public enum ProjectStatus
{
    Proposed,
    Approved,
    InProgress,
    Completed,
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
    
    // Foreign keys
    [Display(Name = "Student")]
    [Required]
    public int StudentId { get; set; }
    
    [Display(Name = "Supervisor")]
    [Required]
    public int SupervisorId { get; set; }
    
    // Navigation properties
    public Student Student { get; set; } = null!;
    public Supervisor Supervisor { get; set; } = null!;
}