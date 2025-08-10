using LibraryApp.Models;
using System.ComponentModel.DataAnnotations;

namespace LibraryApp.Models;

public class GuestDashboardViewModel
{
    public List<Project> CompletedProjects { get; set; } = new();
    public List<Project> FeaturedProjects { get; set; } = new();
    public int TotalCompletedProjects { get; set; }
    public List<Department> Departments { get; set; } = new();
    public Dictionary<string, int> ProjectsByDepartment { get; set; } = new();
}

public class AdminDashboardEnhancedViewModel : DashboardViewModel
{
    public List<ProjectSubmission> PendingSubmissions { get; set; } = new();
    public List<Notification> RecentNotifications { get; set; } = new();
    public List<SystemAuditLog> RecentAuditLogs { get; set; } = new();
    public Dictionary<string, int> ProjectStatusCounts { get; set; } = new();
    public Dictionary<string, int> UserActivityCounts { get; set; } = new();
    public new List<Project> RecentProjects { get; set; } = new();
}

public class StudentDashboardEnhancedViewModel : DashboardViewModel
{
    public List<Project> MyProjects { get; set; } = new();
    public List<Notification> MyNotifications { get; set; } = new();
    public Student? StudentInfo { get; set; }
    public Dictionary<string, object> ProgressMetrics { get; set; } = new();
    public List<Project> AvailableProjects { get; set; } = new();
}

public class ProfessorDashboardEnhancedViewModel : DashboardViewModel
{
    public List<Project> SupervisedProjects { get; set; } = new();
    public List<Project> EvaluatedProjects { get; set; } = new();
    public List<Notification> MyNotifications { get; set; } = new();
    public Professor? ProfessorInfo { get; set; }
    public Dictionary<string, int> WorkloadMetrics { get; set; } = new();
    public List<Student> MyStudents { get; set; } = new();
}

public class ProjectSubmissionViewModel
{
    public Project Project { get; set; } = null!;
    public string SubmissionComments { get; set; } = string.Empty;
    public IFormFile? PosterFile { get; set; }
    public IFormFile? ReportFile { get; set; }
    public IFormFile? CodeFiles { get; set; }
}

public class ProjectSubmissionReviewViewModel
{
    public ProjectSubmission Submission { get; set; } = null!;
    public SubmissionStatus NewStatus { get; set; }
    public string ReviewComments { get; set; } = string.Empty;
}

public class UserManagementViewModel
{
    public List<Student> Students { get; set; } = new();
    public List<Professor> Professors { get; set; } = new();
    public List<Admin> Admins { get; set; } = new();
    public Dictionary<string, int> UserStatistics { get; set; } = new();
}

public class AnalyticsViewModel
{
    public Dictionary<string, int> ProjectsByStatus { get; set; } = new();
    public Dictionary<string, int> ProjectsByDepartment { get; set; } = new();
    public Dictionary<string, int> MonthlyCompletions { get; set; } = new();
    public Dictionary<string, double> AverageGrades { get; set; } = new();
    public List<Project> TopProjects { get; set; } = new();
    public Dictionary<string, int> UserLoginActivity { get; set; } = new();
}

public class ReportsViewModel
{
    public string ReportType { get; set; } = string.Empty;
    public DateTime StartDate { get; set; } = DateTime.Now.AddMonths(-1);
    public DateTime EndDate { get; set; } = DateTime.Now;
    public List<string> SelectedDepartments { get; set; } = new();
    public List<string> SelectedStatuses { get; set; } = new();
    public List<object> ReportData { get; set; } = new();
}

public class CommunicationViewModel
{
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public List<string> TargetRoles { get; set; } = new();
    public DateTime? ExpiryDate { get; set; }
    public bool IsUrgent { get; set; }
    public List<Announcement> RecentAnnouncements { get; set; } = new();
}

// Role Management ViewModels
public class UserRoleManagementViewModel
{
    public List<User> Users { get; set; } = new();
    public List<Role> AllRoles { get; set; } = new();
    public Dictionary<int, List<Role>> UserRoleAssignments { get; set; } = new();
}

public class CreateRoleViewModel
{
    [Required]
    [StringLength(50)]
    public string Name { get; set; } = "";
    
    [StringLength(500)]
    public string? Description { get; set; }
    
    public List<Permission> AllPermissions { get; set; } = new();
    public List<PermissionType> SelectedPermissions { get; set; } = new();
}

public class EditRoleViewModel
{
    public int Id { get; set; }
    
    [Required]
    [StringLength(50)]
    public string Name { get; set; } = "";
    
    [StringLength(500)]
    public string? Description { get; set; }
    
    public List<Permission> AllPermissions { get; set; } = new();
    public List<PermissionType> SelectedPermissions { get; set; } = new();
    public bool IsSystemRole { get; set; }
}

public class GalleryAdminSettingsViewModel
{
    public int TotalProjects { get; set; }
    public int PublicProjects { get; set; }
    public int FeaturedProjects { get; set; }
    public int RecentlyAdded { get; set; }
}