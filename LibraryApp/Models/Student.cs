using System.ComponentModel.DataAnnotations;

namespace LibraryApp.Models;

public class Student
{
    public int Id { get; set; }
    
    [Display(Name = "Student Number")]
    [Required]
    public required string StudentNumber { get; set; }
    
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
    
    [Display(Name = "Department")]
    [Required]
    public int DepartmentId { get; set; }
    
    [Display(Name = "Enrollment Date")]
    [Required]
    [DataType(DataType.Date)]
    public DateTime EnrollmentDate { get; set; }
    
    // Navigation properties
    public Department Department { get; set; } = null!;
    public ICollection<Project> Projects { get; set; } = new List<Project>();
}