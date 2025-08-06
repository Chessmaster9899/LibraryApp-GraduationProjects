using System.ComponentModel.DataAnnotations;

namespace LibraryApp.Models;

public class Supervisor
{
    public int Id { get; set; }
    
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
    
    // Navigation properties
    public Department Department { get; set; } = null!;
    public ICollection<Project> Projects { get; set; } = new List<Project>();
}