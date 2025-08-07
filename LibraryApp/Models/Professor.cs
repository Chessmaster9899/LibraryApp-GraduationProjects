using System.ComponentModel.DataAnnotations;

namespace LibraryApp.Models;

public enum ProfessorRole
{
    Supervisor,
    Evaluator,
    Both
}

public class Professor
{
    public int Id { get; set; }
    
    [Display(Name = "Professor ID")]
    [Required]
    public required string ProfessorId { get; set; }
    
    [Display(Name = "First Name")]
    [Required]
    public required string FirstName { get; set; }
    
    [Display(Name = "Last Name")]
    [Required]
    public required string LastName { get; set; }
    
    [Required]
    [EmailAddress]
    public required string Email { get; set; }
    
    [Phone]
    public string? Phone { get; set; }
    
    [Required]
    public required string Title { get; set; } // Dr., Prof., etc.
    
    [Display(Name = "Department")]
    [Required]
    public int DepartmentId { get; set; }
    
    public string? Specialization { get; set; }
    
    [Display(Name = "Role")]
    public ProfessorRole Role { get; set; } = ProfessorRole.Both;
    
    // Authentication properties
    public string? Password { get; set; } // Hashed password
    
    [Display(Name = "Password Must Be Changed")]
    public bool MustChangePassword { get; set; } = true;
    
    [Display(Name = "Last Login")]
    public DateTime? LastLogin { get; set; }
    
    // Navigation properties
    public Department Department { get; set; } = null!;
    public ICollection<Project> SupervisedProjects { get; set; } = new List<Project>();
    public ICollection<Project> EvaluatedProjects { get; set; } = new List<Project>();
    
    // Computed properties
    public string FullName => $"{FirstName} {LastName}";
    public string DisplayName => $"{Title} {FullName}";
}