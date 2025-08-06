namespace LibraryApp.Models;

public class Department
{
    public int Id { get; set; }
    public required string Name { get; set; }
    public string? Description { get; set; }
    
    // Navigation properties
    public ICollection<Student> Students { get; set; } = new List<Student>();
    public ICollection<Supervisor> Supervisors { get; set; } = new List<Supervisor>();
}