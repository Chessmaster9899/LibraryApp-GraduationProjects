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

// Project Workflow ViewModels
public class ProjectWorkflowViewModel
{
    public Dictionary<ProjectStatus, List<Project>> ProjectsByStatus { get; set; } = new();
    public Dictionary<ProjectStatus, int> StatusCounts { get; set; } = new();
    public List<ProjectActivityLog> RecentActivity { get; set; } = new();
}

public class ProjectWorkflowDetailsViewModel
{
    public Project Project { get; set; } = null!;
    public List<string> AvailableActions { get; set; } = new();
    public List<ProjectStatus> NextStatuses { get; set; } = new();
    public List<ProjectActivityLog> ActivityLog { get; set; } = new();
}

// Dashboard ViewModels
public class StudentDashboardViewModel
{
    public Student Student { get; set; } = null!;
    public int TotalProjects { get; set; }
    public int CompletedProjects { get; set; }
    public int InProgressProjects { get; set; }
    public List<Project> RecentProjects { get; set; } = new();
    public UniversitySettings UniversitySettings { get; set; } = null!;
}

public class ProfessorDashboardViewModel
{
    public Professor Professor { get; set; } = null!;
    public int TotalSupervisedProjects { get; set; }
    public int TotalEvaluatedProjects { get; set; }
    public int CompletedSupervisedProjects { get; set; }
    public int CompletedEvaluatedProjects { get; set; }
    public List<Project> RecentSupervisedProjects { get; set; } = new();
    public List<Project> RecentEvaluatedProjects { get; set; } = new();
    public UniversitySettings UniversitySettings { get; set; } = null!;
}

// Gallery ViewModels
public class EnhancedGalleryViewModel
{
    public List<Project> Projects { get; set; } = new();
    public List<Project> FeaturedProjects { get; set; } = new();
    public List<Project> RecentProjects { get; set; } = new();
    public List<Project> AllProjects { get; set; } = new();
    public List<Department> Departments { get; set; } = new();
    public List<Professor> Supervisors { get; set; } = new();
    public GalleryStatsViewModel Stats { get; set; } = new();
    public Dictionary<string, int> ProjectsByDepartment { get; set; } = new();
    public Dictionary<ProjectStatus, int> ProjectsByStatus { get; set; } = new();
    
    // Filter properties
    public string? SelectedDepartment { get; set; }
    public string? SelectedSupervisor { get; set; }
    public string? SelectedStatus { get; set; }
    public string? SearchQuery { get; set; }
    
    // Current state properties for controller compatibility
    public string? CurrentDepartment { get; set; }
    public string? CurrentSearch { get; set; }
    public string? CurrentSort { get; set; }
    public int CurrentPage { get; set; } = 1;
    public int TotalPages { get; set; }
    public int TotalProjects { get; set; }
    public bool IsAdmin { get; set; }
}

public class GalleryStatsViewModel
{
    public int TotalProjects { get; set; }
    public int PublishedProjects { get; set; }
    public int TotalStudents { get; set; }
    public int TotalSupervisors { get; set; }
    public int TotalDepartments { get; set; }
    public Dictionary<string, int> ProjectsByYear { get; set; } = new();
    public Dictionary<string, int> ProjectsByDepartment { get; set; } = new();
    public int RecentProjects { get; set; }
}

public class ProjectDetailViewModel
{
    public Project Project { get; set; } = null!;
    public List<Project> RelatedProjects { get; set; } = new();
    public List<Project> SupervisorOtherProjects { get; set; } = new();
    public bool CanViewFiles { get; set; }
    public bool CanEdit { get; set; }
    public bool CanManage { get; set; }
    public bool ShowComments { get; set; }
    public string? UserRole { get; set; }
}

// Search ViewModels
public class SearchResultsViewModel
{
    public string SearchQuery { get; set; } = "";
    public string Query { get; set; } = "";
    public string SearchType { get; set; } = "all";
    public string Department { get; set; } = "";
    public string Status { get; set; } = "";
    public int CurrentPage { get; set; } = 1;
    public int PageSize { get; set; } = 20;
    
    public List<Project> Projects { get; set; } = new();
    public List<Student> Students { get; set; } = new();
    public List<Professor> Professors { get; set; } = new();
    
    public int TotalProjects { get; set; }
    public int TotalStudents { get; set; }
    public int TotalProfessors { get; set; }
    public int TotalResults => TotalProjects + TotalStudents + TotalProfessors;
    
    public List<Department> Departments { get; set; } = new();
    public List<object> ProjectStatuses { get; set; } = new();
    public Dictionary<string, int> FilterCounts { get; set; } = new();
    public List<string> SearchSuggestions { get; set; } = new();
}

// Utility Classes
public class StudentEqualityComparer : IEqualityComparer<Student>
{
    public bool Equals(Student? x, Student? y)
    {
        if (x == null || y == null) return false;
        return x.Id == y.Id;
    }

    public int GetHashCode(Student obj)
    {
        return obj.Id.GetHashCode();
    }
}