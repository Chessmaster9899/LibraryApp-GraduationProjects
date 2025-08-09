using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using LibraryApp.Data;
using LibraryApp.Models;
using LibraryApp.Services;
using System.Text.Json;

namespace LibraryApp.Controllers.Api;

[ApiController]
[Route("api/[controller]")]
public class ProjectsController : ControllerBase
{
    private readonly LibraryContext _context;
    private readonly ILogger<ProjectsController> _logger;

    public ProjectsController(LibraryContext context, ILogger<ProjectsController> logger)
    {
        _context = context;
        _logger = logger;
    }

    // GET: api/projects
    [HttpGet]
    public async Task<ActionResult<ApiResponse<PagedResult<ProjectDto>>>> GetProjects(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string? search = null,
        [FromQuery] string? department = null,
        [FromQuery] string? status = null,
        [FromQuery] bool publicOnly = true)
    {
        try
        {
            var query = _context.Projects
                .Include(p => p.Student)
                .Include(p => p.Supervisor)
                .Include(p => p.Student.Department)
                .AsQueryable();

            // Apply public visibility filter
            if (publicOnly)
            {
                query = query.Where(p => p.IsPubliclyVisible);
            }

            // Apply search filter
            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(p => 
                    p.Title.Contains(search) || 
                    (p.Abstract ?? string.Empty).Contains(search) || 
                    (p.Keywords ?? string.Empty).Contains(search) ||
                    p.Student.FirstName.Contains(search) ||
                    p.Student.LastName.Contains(search));
            }

            // Apply department filter
            if (!string.IsNullOrWhiteSpace(department))
            {
                query = query.Where(p => p.Student.Department.Name == department);
            }

            // Apply status filter
            if (!string.IsNullOrWhiteSpace(status) && Enum.TryParse<ProjectStatus>(status, out var statusEnum))
            {
                query = query.Where(p => p.Status == statusEnum);
            }

            var totalCount = await query.CountAsync();
            var projects = await query
                .OrderByDescending(p => p.SubmissionDate)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(p => new ProjectDto
                {
                    Id = p.Id,
                    Title = p.Title,
                    Abstract = p.Abstract,
                    Keywords = p.Keywords,
                    Status = p.Status.ToString(),
                    SubmissionDate = p.SubmissionDate,
                    DefenseDate = p.DefenseDate,
                    Grade = p.Grade,
                    Student = new StudentDto
                    {
                        Id = p.Student.Id,
                        FullName = p.Student.FullName,
                        StudentNumber = p.Student.StudentNumber,
                        Department = p.Student.Department.Name
                    },
                    Supervisor = new ProfessorDto
                    {
                        Id = p.Supervisor.Id,
                        FullName = p.Supervisor.DisplayName,
                        Department = p.Supervisor.Department.Name,
                        Specialization = p.Supervisor.Specialization
                    }
                })
                .ToListAsync();

            var result = new PagedResult<ProjectDto>
            {
                Data = projects,
                TotalCount = totalCount,
                Page = page,
                PageSize = pageSize,
                TotalPages = (int)Math.Ceiling((double)totalCount / pageSize)
            };

            return Ok(new ApiResponse<PagedResult<ProjectDto>>
            {
                Success = true,
                Data = result,
                Message = "Projects retrieved successfully"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving projects");
            return StatusCode(500, new ApiResponse<PagedResult<ProjectDto>>
            {
                Success = false,
                Message = "An error occurred while retrieving projects"
            });
        }
    }

    // GET: api/projects/{id}
    [HttpGet("{id}")]
    public async Task<ActionResult<ApiResponse<ProjectDto>>> GetProject(int id)
    {
        try
        {
            var project = await _context.Projects
                .Include(p => p.Student)
                .Include(p => p.Supervisor)
                .Include(p => p.Evaluator)
                .Include(p => p.Student.Department)
                .Include(p => p.Supervisor.Department)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (project == null)
            {
                return NotFound(new ApiResponse<ProjectDto>
                {
                    Success = false,
                    Message = "Project not found"
                });
            }

            var projectDto = new ProjectDto
            {
                Id = project.Id,
                Title = project.Title,
                Abstract = project.Abstract,
                Keywords = project.Keywords,
                Status = project.Status.ToString(),
                SubmissionDate = project.SubmissionDate,
                DefenseDate = project.DefenseDate,
                Grade = project.Grade,
                ReviewComments = project.ReviewComments,
                Student = new StudentDto
                {
                    Id = project.Student.Id,
                    FullName = project.Student.FullName,
                    StudentNumber = project.Student.StudentNumber,
                    Department = project.Student.Department.Name,
                    Email = project.Student.Email
                },
                Supervisor = new ProfessorDto
                {
                    Id = project.Supervisor.Id,
                    FullName = project.Supervisor.DisplayName,
                    Department = project.Supervisor.Department.Name,
                    Specialization = project.Supervisor.Specialization
                },
                Evaluator = project.Evaluator != null ? new ProfessorDto
                {
                    Id = project.Evaluator.Id,
                    FullName = project.Evaluator.DisplayName,
                    Department = project.Evaluator.Department.Name,
                    Specialization = project.Evaluator.Specialization
                } : null
            };

            return Ok(new ApiResponse<ProjectDto>
            {
                Success = true,
                Data = projectDto,
                Message = "Project retrieved successfully"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving project {ProjectId}", id);
            return StatusCode(500, new ApiResponse<ProjectDto>
            {
                Success = false,
                Message = "An error occurred while retrieving the project"
            });
        }
    }

    // GET: api/projects/statistics
    [HttpGet("statistics")]
    public async Task<ActionResult<ApiResponse<ProjectStatisticsDto>>> GetStatistics()
    {
        try
        {
            var statistics = new ProjectStatisticsDto
            {
                TotalProjects = await _context.Projects.CountAsync(),
                PublishedProjects = await _context.Projects.CountAsync(p => p.IsPubliclyVisible),
                CompletedProjects = await _context.Projects.CountAsync(p => 
                    p.Status == ProjectStatus.Completed || p.Status == ProjectStatus.Defended),
                ProjectsByDepartment = await _context.Projects
                    .Include(p => p.Student.Department)
                    .GroupBy(p => p.Student.Department.Name)
                    .ToDictionaryAsync(g => g.Key, g => g.Count()),
                ProjectsByStatus = await _context.Projects
                    .GroupBy(p => p.Status)
                    .ToDictionaryAsync(g => g.Key.ToString(), g => g.Count()),
                RecentProjects = await _context.Projects
                    .Where(p => p.IsPubliclyVisible)
                    .OrderByDescending(p => p.SubmissionDate)
                    .Take(10)
                    .Select(p => new ProjectSummaryDto
                    {
                        Id = p.Id,
                        Title = p.Title,
                        StudentName = p.Student.FullName,
                        Department = p.Student.Department.Name,
                        SubmissionDate = p.SubmissionDate
                    })
                    .ToListAsync()
            };

            return Ok(new ApiResponse<ProjectStatisticsDto>
            {
                Success = true,
                Data = statistics,
                Message = "Statistics retrieved successfully"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving project statistics");
            return StatusCode(500, new ApiResponse<ProjectStatisticsDto>
            {
                Success = false,
                Message = "An error occurred while retrieving statistics"
            });
        }
    }
}

// DTOs for API responses
public class ApiResponse<T>
{
    public bool Success { get; set; }
    public T? Data { get; set; }
    public string Message { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}

public class PagedResult<T>
{
    public List<T> Data { get; set; } = new();
    public int TotalCount { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalPages { get; set; }
    public bool HasNextPage => Page < TotalPages;
    public bool HasPreviousPage => Page > 1;
}

public class ProjectDto
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Abstract { get; set; }
    public string? Keywords { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime SubmissionDate { get; set; }
    public DateTime? DefenseDate { get; set; }
    public string? Grade { get; set; }
    public string? ReviewComments { get; set; }
    public StudentDto Student { get; set; } = null!;
    public ProfessorDto Supervisor { get; set; } = null!;
    public ProfessorDto? Evaluator { get; set; }
}

public class StudentDto
{
    public int Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string StudentNumber { get; set; } = string.Empty;
    public string Department { get; set; } = string.Empty;
    public string? Email { get; set; }
}

public class ProfessorDto
{
    public int Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Department { get; set; } = string.Empty;
    public string? Specialization { get; set; }
}

public class ProjectStatisticsDto
{
    public int TotalProjects { get; set; }
    public int PublishedProjects { get; set; }
    public int CompletedProjects { get; set; }
    public Dictionary<string, int> ProjectsByDepartment { get; set; } = new();
    public Dictionary<string, int> ProjectsByStatus { get; set; } = new();
    public List<ProjectSummaryDto> RecentProjects { get; set; } = new();
}

public class ProjectSummaryDto
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string StudentName { get; set; } = string.Empty;
    public string Department { get; set; } = string.Empty;
    public DateTime SubmissionDate { get; set; }
}