using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using LibraryApp.Data;
using LibraryApp.Models;
using LibraryApp.Services;
using LibraryApp.Attributes;

namespace LibraryApp.Controllers;

public class SearchController : BaseController
{
    private readonly LibraryContext _context;

    public SearchController(LibraryContext context, IUniversitySettingsService universitySettings, ISessionService sessionService) 
        : base(universitySettings, sessionService)
    {
        _context = context;
    }

    // GET: Search
    public async Task<IActionResult> Index(string query = "", string type = "all", string department = "", 
        string status = "", int page = 1, int pageSize = 20)
    {
        var searchResults = new SearchResultsViewModel
        {
            Query = query,
            SearchType = type,
            Department = department,
            Status = status,
            CurrentPage = page,
            PageSize = pageSize
        };

        if (!string.IsNullOrWhiteSpace(query))
        {
            switch (type.ToLower())
            {
                case "projects":
                    searchResults.Projects = await SearchProjects(query, department, status, page, pageSize);
                    searchResults.TotalProjects = await CountProjects(query, department, status);
                    break;
                case "students":
                    if (CurrentUserRoleEnum == UserRole.Admin || CurrentUserRoleEnum == UserRole.Professor)
                    {
                        searchResults.Students = await SearchStudents(query, department, page, pageSize);
                        searchResults.TotalStudents = await CountStudents(query, department);
                    }
                    break;
                case "professors":
                    if (CurrentUserRoleEnum == UserRole.Admin)
                    {
                        searchResults.Professors = await SearchProfessors(query, department, page, pageSize);
                        searchResults.TotalProfessors = await CountProfessors(query, department);
                    }
                    break;
                default: // "all"
                    searchResults.Projects = await SearchProjects(query, department, status, page, Math.Min(pageSize / 2, 10));
                    searchResults.TotalProjects = await CountProjects(query, department, status);
                    
                    if (CurrentUserRoleEnum == UserRole.Admin || CurrentUserRoleEnum == UserRole.Professor)
                    {
                        searchResults.Students = await SearchStudents(query, department, page, Math.Min(pageSize / 4, 5));
                        searchResults.TotalStudents = await CountStudents(query, department);
                    }
                    
                    if (CurrentUserRoleEnum == UserRole.Admin)
                    {
                        searchResults.Professors = await SearchProfessors(query, department, page, Math.Min(pageSize / 4, 5));
                        searchResults.TotalProfessors = await CountProfessors(query, department);
                    }
                    break;
            }
        }

        // Get filter options
        searchResults.Departments = await _context.Departments
            .OrderBy(d => d.Name)
            .ToListAsync();

        searchResults.ProjectStatuses = Enum.GetValues<ProjectStatus>()
            .Select(s => new { Value = s.ToString(), Text = s.ToString().Replace("_", " ") })
            .Cast<object>()
            .ToList();

        return View(searchResults);
    }

    // GET: Search/Suggestions
    [HttpGet]
    public async Task<IActionResult> GetSuggestions(string query, int limit = 10)
    {
        if (string.IsNullOrWhiteSpace(query) || query.Length < 2)
        {
            return Json(new List<object>());
        }

        var suggestions = new List<object>();

        // Project suggestions
        var projectSuggestions = await _context.Projects
            .Where(p => p.IsPubliclyVisible || CurrentUserRoleEnum != UserRole.Guest)
            .Where(p => p.Title.Contains(query) || p.Abstract.Contains(query) || p.Keywords.Contains(query))
            .Take(limit / 2)
            .Select(p => new {
                type = "project",
                title = p.Title,
                subtitle = $"by {p.Student.FullName}",
                url = Url.Action("Details", "Projects", new { id = p.Id })
            })
            .ToListAsync();

        suggestions.AddRange(projectSuggestions);

        // Student suggestions (for admins and professors)
        if (CurrentUserRoleEnum == UserRole.Admin || CurrentUserRoleEnum == UserRole.Professor)
        {
            var studentSuggestions = await _context.Students
                .Where(s => s.FirstName.Contains(query) || s.LastName.Contains(query) || s.StudentNumber.Contains(query))
                .Take(limit / 4)
                .Select(s => new {
                    type = "student",
                    title = s.FullName,
                    subtitle = $"{s.StudentNumber} - {s.Department.Name}",
                    url = Url.Action("Details", "Students", new { id = s.Id })
                })
                .ToListAsync();

            suggestions.AddRange(studentSuggestions);
        }

        // Professor suggestions (for admins)
        if (CurrentUserRoleEnum == UserRole.Admin)
        {
            var professorSuggestions = await _context.Professors
                .Where(p => p.FirstName.Contains(query) || p.LastName.Contains(query) || p.ProfessorId.Contains(query))
                .Take(limit / 4)
                .Select(p => new {
                    type = "professor",
                    title = p.DisplayName,
                    subtitle = $"{p.ProfessorId} - {p.Department.Name}",
                    url = Url.Action("Details", "Professors", new { id = p.Id })
                })
                .ToListAsync();

            suggestions.AddRange(professorSuggestions);
        }

        return Json(suggestions.Take(limit));
    }

    private async Task<List<Project>> SearchProjects(string query, string department, string status, int page, int pageSize)
    {
        var projectsQuery = _context.Projects
            .Include(p => p.Student)
            .Include(p => p.Supervisor)
            .Include(p => p.Student.Department)
            .AsQueryable();

        // Apply visibility filter
        if (CurrentUserRoleEnum == UserRole.Guest)
        {
            projectsQuery = projectsQuery.Where(p => p.IsPubliclyVisible);
        }

        // Apply text search
        projectsQuery = projectsQuery.Where(p => 
            p.Title.Contains(query) || 
            p.Abstract.Contains(query) || 
            p.Keywords.Contains(query) ||
            p.Student.FirstName.Contains(query) ||
            p.Student.LastName.Contains(query) ||
            p.Supervisor.FirstName.Contains(query) ||
            p.Supervisor.LastName.Contains(query));

        // Apply department filter
        if (!string.IsNullOrEmpty(department))
        {
            projectsQuery = projectsQuery.Where(p => p.Student.Department.Name == department);
        }

        // Apply status filter
        if (!string.IsNullOrEmpty(status) && Enum.TryParse<ProjectStatus>(status, out var statusEnum))
        {
            projectsQuery = projectsQuery.Where(p => p.Status == statusEnum);
        }

        return await projectsQuery
            .OrderByDescending(p => p.SubmissionDate)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
    }

    private async Task<List<Student>> SearchStudents(string query, string department, int page, int pageSize)
    {
        var studentsQuery = _context.Students
            .Include(s => s.Department)
            .Include(s => s.Projects)
            .AsQueryable();

        studentsQuery = studentsQuery.Where(s => 
            s.FirstName.Contains(query) || 
            s.LastName.Contains(query) || 
            s.StudentNumber.Contains(query) ||
            s.Email.Contains(query));

        if (!string.IsNullOrEmpty(department))
        {
            studentsQuery = studentsQuery.Where(s => s.Department.Name == department);
        }

        return await studentsQuery
            .OrderBy(s => s.LastName)
            .ThenBy(s => s.FirstName)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
    }

    private async Task<List<Professor>> SearchProfessors(string query, string department, int page, int pageSize)
    {
        var professorsQuery = _context.Professors
            .Include(p => p.Department)
            .Include(p => p.SupervisedProjects)
            .Include(p => p.EvaluatedProjects)
            .AsQueryable();

        professorsQuery = professorsQuery.Where(p => 
            p.FirstName.Contains(query) || 
            p.LastName.Contains(query) || 
            p.ProfessorId.Contains(query) ||
            p.Email.Contains(query) ||
            p.Specialization.Contains(query));

        if (!string.IsNullOrEmpty(department))
        {
            professorsQuery = professorsQuery.Where(p => p.Department.Name == department);
        }

        return await professorsQuery
            .OrderBy(p => p.LastName)
            .ThenBy(p => p.FirstName)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
    }

    private async Task<int> CountProjects(string query, string department, string status)
    {
        var projectsQuery = _context.Projects.AsQueryable();

        if (CurrentUserRoleEnum == UserRole.Guest)
        {
            projectsQuery = projectsQuery.Where(p => p.IsPubliclyVisible);
        }

        projectsQuery = projectsQuery.Where(p => 
            p.Title.Contains(query) || 
            p.Abstract.Contains(query) || 
            p.Keywords.Contains(query) ||
            p.Student.FirstName.Contains(query) ||
            p.Student.LastName.Contains(query) ||
            p.Supervisor.FirstName.Contains(query) ||
            p.Supervisor.LastName.Contains(query));

        if (!string.IsNullOrEmpty(department))
        {
            projectsQuery = projectsQuery.Where(p => p.Student.Department.Name == department);
        }

        if (!string.IsNullOrEmpty(status) && Enum.TryParse<ProjectStatus>(status, out var statusEnum))
        {
            projectsQuery = projectsQuery.Where(p => p.Status == statusEnum);
        }

        return await projectsQuery.CountAsync();
    }

    private async Task<int> CountStudents(string query, string department)
    {
        var studentsQuery = _context.Students.AsQueryable();

        studentsQuery = studentsQuery.Where(s => 
            s.FirstName.Contains(query) || 
            s.LastName.Contains(query) || 
            s.StudentNumber.Contains(query) ||
            s.Email.Contains(query));

        if (!string.IsNullOrEmpty(department))
        {
            studentsQuery = studentsQuery.Where(s => s.Department.Name == department);
        }

        return await studentsQuery.CountAsync();
    }

    private async Task<int> CountProfessors(string query, string department)
    {
        var professorsQuery = _context.Professors.AsQueryable();

        professorsQuery = professorsQuery.Where(p => 
            p.FirstName.Contains(query) || 
            p.LastName.Contains(query) || 
            p.ProfessorId.Contains(query) ||
            p.Email.Contains(query) ||
            p.Specialization.Contains(query));

        if (!string.IsNullOrEmpty(department))
        {
            professorsQuery = professorsQuery.Where(p => p.Department.Name == department);
        }

        return await professorsQuery.CountAsync();
    }
}

public class SearchResultsViewModel
{
    public string Query { get; set; } = string.Empty;
    public string SearchType { get; set; } = "all";
    public string Department { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public int CurrentPage { get; set; } = 1;
    public int PageSize { get; set; } = 20;

    public List<Project> Projects { get; set; } = new();
    public List<Student> Students { get; set; } = new();
    public List<Professor> Professors { get; set; } = new();

    public int TotalProjects { get; set; }
    public int TotalStudents { get; set; }
    public int TotalProfessors { get; set; }

    public List<Department> Departments { get; set; } = new();
    public List<object> ProjectStatuses { get; set; } = new();

    public int TotalResults => TotalProjects + TotalStudents + TotalProfessors;
}