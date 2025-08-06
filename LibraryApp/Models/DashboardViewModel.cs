namespace LibraryApp.Models;

public class DashboardViewModel
{
    public int TotalProjects { get; set; }
    public int TotalStudents { get; set; }
    public int TotalSupervisors { get; set; }
    public int TotalDepartments { get; set; }
    
    public List<ProjectStatusSummary> ProjectsByStatus { get; set; } = new List<ProjectStatusSummary>();
    public List<Project> RecentProjects { get; set; } = new List<Project>();
    public UniversitySettings UniversitySettings { get; set; } = new UniversitySettings();
}

public class ProjectStatusSummary
{
    public ProjectStatus Status { get; set; }
    public int Count { get; set; }
}