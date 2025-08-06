namespace LibraryApp.Models;

public class Student
{
    public int Id { get; set; }
    public required string StudentNumber { get; set; }
    public required string FirstName { get; set; }
    public required string LastName { get; set; }
    public required string Email { get; set; }
    public string? Phone { get; set; }
    public int DepartmentId { get; set; }
    public DateTime EnrollmentDate { get; set; }
    
    // Navigation properties
    public Department Department { get; set; } = null!;
    public ICollection<Project> Projects { get; set; } = new List<Project>();
}