namespace LibraryApp.Models;

public class Supervisor
{
    public int Id { get; set; }
    public required string FirstName { get; set; }
    public required string LastName { get; set; }
    public required string Email { get; set; }
    public string? Phone { get; set; }
    public required string Title { get; set; } // Dr., Prof., etc.
    public int DepartmentId { get; set; }
    public string? Specialization { get; set; }
    
    // Navigation properties
    public Department Department { get; set; } = null!;
    public ICollection<Project> Projects { get; set; } = new List<Project>();
}