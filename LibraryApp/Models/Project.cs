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
    public required string Title { get; set; }
    public string? Abstract { get; set; }
    public string? Keywords { get; set; }
    public ProjectStatus Status { get; set; } = ProjectStatus.Proposed;
    public DateTime SubmissionDate { get; set; }
    public DateTime? DefenseDate { get; set; }
    public string? Grade { get; set; }
    public string? DocumentPath { get; set; } // Path to PDF or document file
    
    // Foreign keys
    public int StudentId { get; set; }
    public int SupervisorId { get; set; }
    
    // Navigation properties
    public Student Student { get; set; } = null!;
    public Supervisor Supervisor { get; set; } = null!;
}