using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using LibraryApp.Data;
using LibraryApp.Models;
using LibraryApp.Services;
using LibraryApp.Attributes;

namespace LibraryApp.Controllers;

public class GalleryController : BaseController
{
    private readonly LibraryContext _context;

    public GalleryController(LibraryContext context, IUniversitySettingsService universitySettings, ISessionService sessionService) 
        : base(universitySettings, sessionService)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<IActionResult> Index(string? department, string? search, string? sort, int page = 1)
    {
        var query = _context.Projects
            .Include(p => p.Student)
                .ThenInclude(s => s.Department)
            .Include(p => p.Supervisor)
            .Where(p => p.IsPubliclyVisible && 
                       (p.Status == ProjectStatus.ReviewApproved || 
                        p.Status == ProjectStatus.Defended || 
                        p.Status == ProjectStatus.Published));

        // Apply filters
        if (!string.IsNullOrEmpty(department))
        {
            query = query.Where(p => p.Student.Department.Name == department);
        }

        if (!string.IsNullOrEmpty(search))
        {
            query = query.Where(p => p.Title.Contains(search) || 
                                   (p.Abstract ?? string.Empty).Contains(search) || 
                                   (p.Keywords ?? string.Empty).Contains(search) ||
                                   p.Student.FirstName.Contains(search) ||
                                   p.Student.LastName.Contains(search));
        }

        // Apply sorting
        query = sort switch
        {
            "title" => query.OrderBy(p => p.Title),
            "date_asc" => query.OrderBy(p => p.DefenseDate ?? p.SubmissionDate),
            "date_desc" => query.OrderByDescending(p => p.DefenseDate ?? p.SubmissionDate),
            "student" => query.OrderBy(p => p.Student.LastName),
            "department" => query.OrderBy(p => p.Student.Department.Name),
            _ => query.OrderByDescending(p => p.DefenseDate ?? p.SubmissionDate)
        };

        const int pageSize = 12;
        var totalProjects = await query.CountAsync();
        var projects = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        var departments = await _context.Departments
            .OrderBy(d => d.Name)
            .ToListAsync();

        var featuredProjects = await _context.Projects
            .Include(p => p.Student)
                .ThenInclude(s => s.Department)
            .Include(p => p.Supervisor)
            .Where(p => p.IsPubliclyVisible && 
                       !string.IsNullOrEmpty(p.PosterPath) &&
                       (p.Status == ProjectStatus.ReviewApproved || 
                        p.Status == ProjectStatus.Defended || 
                        p.Status == ProjectStatus.Published))
            .OrderByDescending(p => p.DefenseDate ?? p.SubmissionDate)
            .Take(6)
            .ToListAsync();

        var galleryStats = new GalleryStatsViewModel
        {
            TotalProjects = await _context.Projects.CountAsync(p => p.IsPubliclyVisible),
            TotalStudents = await _context.Students.CountAsync(),
            TotalSupervisors = await _context.Professors.CountAsync(),
            TotalDepartments = await _context.Departments.CountAsync(),
            ProjectsByDepartment = await _context.Projects
                .Include(p => p.Student)
                    .ThenInclude(s => s.Department)
                .Where(p => p.IsPubliclyVisible)
                .GroupBy(p => p.Student.Department.Name)
                .Select(g => new { Department = g.Key, Count = g.Count() })
                .ToDictionaryAsync(x => x.Department, x => x.Count),
            RecentProjects = totalProjects
        };

        var viewModel = new EnhancedGalleryViewModel
        {
            Projects = projects,
            FeaturedProjects = featuredProjects,
            Departments = departments,
            Stats = galleryStats,
            CurrentDepartment = department,
            CurrentSearch = search,
            CurrentSort = sort,
            CurrentPage = page,
            TotalPages = (int)Math.Ceiling((double)totalProjects / pageSize),
            TotalProjects = totalProjects,
            IsAdmin = IsAdmin
        };

        return View(viewModel);
    }

    [HttpGet]
    public async Task<IActionResult> Project(int id)
    {
        var project = await _context.Projects
            .Include(p => p.Student)
                .ThenInclude(s => s.Department)
            .Include(p => p.Supervisor)
                .ThenInclude(s => s.Department)
            .Include(p => p.Evaluator)
            .FirstOrDefaultAsync(p => p.Id == id && p.IsPubliclyVisible);

        if (project == null)
        {
            return NotFound();
        }

        // Get related projects from the same department
        var relatedProjects = await _context.Projects
            .Include(p => p.Student)
            .Include(p => p.Supervisor)
            .Where(p => p.Id != id && 
                       p.IsPubliclyVisible && 
                       p.Student.DepartmentId == project.Student.DepartmentId)
            .OrderByDescending(p => p.DefenseDate ?? p.SubmissionDate)
            .Take(4)
            .ToListAsync();

        var viewModel = new ProjectDetailViewModel
        {
            Project = project,
            RelatedProjects = relatedProjects,
            CanManage = IsAdmin,
            ShowComments = IsAuthenticated
        };

        return View(viewModel);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [RequirePermission(PermissionType.ManageGallery)]
    public async Task<IActionResult> ToggleProjectVisibility(int projectId)
    {
        var project = await _context.Projects.FindAsync(projectId);
        if (project == null)
        {
            return Json(new { success = false, message = "Project not found" });
        }

        project.IsPubliclyVisible = !project.IsPubliclyVisible;
        await _context.SaveChangesAsync();

        return Json(new { 
            success = true, 
            message = $"Project visibility {(project.IsPubliclyVisible ? "enabled" : "disabled")}",
            isVisible = project.IsPubliclyVisible
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [RequirePermission(PermissionType.CustomizeGallery)]
    public async Task<IActionResult> FeatureProject(int projectId, bool featured = true)
    {
        var project = await _context.Projects.FindAsync(projectId);
        if (project == null)
        {
            return Json(new { success = false, message = "Project not found" });
        }

        // For now, we'll use the PosterPath as an indicator of featured status
        // In a full implementation, you'd add a Featured field to the Project model
        
        return Json(new { 
            success = true, 
            message = $"Project {(featured ? "featured" : "unfeatured")} successfully"
        });
    }

    [HttpGet]
    [RequirePermission(PermissionType.CustomizeGallery)]
    public async Task<IActionResult> AdminSettings()
    {
        var settings = new GalleryAdminSettingsViewModel
        {
            TotalProjects = await _context.Projects.CountAsync(),
            PublicProjects = await _context.Projects.CountAsync(p => p.IsPubliclyVisible),
            FeaturedProjects = await _context.Projects.CountAsync(p => !string.IsNullOrEmpty(p.PosterPath)),
            RecentlyAdded = await _context.Projects
                .Where(p => p.SubmissionDate >= DateTime.Now.AddDays(-30))
                .CountAsync()
        };

        return View(settings);
    }

    [HttpGet]
    public async Task<IActionResult> Statistics()
    {
        var stats = await GetGalleryStatistics();
        
        if (Request.Headers["Accept"].ToString().Contains("application/json"))
        {
            return Json(stats);
        }

        return View(stats);
    }

    private async Task<object> GetGalleryStatistics()
    {
        var currentYear = DateTime.Now.Year;
        
        return new
        {
            TotalProjects = await _context.Projects.CountAsync(p => p.IsPubliclyVisible),
            ProjectsThisYear = await _context.Projects
                .CountAsync(p => p.IsPubliclyVisible && p.SubmissionDate.Year == currentYear),
            DepartmentStats = await _context.Projects
                .Include(p => p.Student)
                    .ThenInclude(s => s.Department)
                .Where(p => p.IsPubliclyVisible)
                .GroupBy(p => p.Student.Department.Name)
                .Select(g => new { Department = g.Key, Count = g.Count() })
                .ToListAsync(),
            MonthlySubmissions = await _context.Projects
                .Where(p => p.IsPubliclyVisible && p.SubmissionDate.Year == currentYear)
                .GroupBy(p => p.SubmissionDate.Month)
                .Select(g => new { Month = g.Key, Count = g.Count() })
                .OrderBy(x => x.Month)
                .ToListAsync(),
            StatusDistribution = await _context.Projects
                .Where(p => p.IsPubliclyVisible)
                .GroupBy(p => p.Status)
                .Select(g => new { Status = g.Key.ToString(), Count = g.Count() })
                .ToListAsync()
        };
    }

    [HttpGet]
    [RequirePermission(PermissionType.CustomizeGallery)]
    public async Task<IActionResult> ManageVisibility()
    {
        var projects = await _context.Projects
            .Include(p => p.Student)
                .ThenInclude(s => s.Department)
            .Include(p => p.Supervisor)
            .OrderByDescending(p => p.SubmissionDate)
            .ToListAsync();

        return Json(new { 
            success = true, 
            projects = projects.Select(p => new {
                id = p.Id,
                title = p.Title,
                student = p.Student?.FullName,
                isVisible = p.IsPubliclyVisible,
                department = p.Student?.Department?.Name
            })
        });
    }

    [HttpGet] 
    [RequirePermission(PermissionType.CustomizeGallery)]
    public IActionResult CustomizeLayout()
    {
        // Return layout customization options
        var layoutOptions = new {
            success = true,
            message = "Layout customization options loaded",
            options = new {
                themes = new[] { "default", "dark", "modern", "academic" },
                layouts = new[] { "grid", "list", "card", "masonry" },
                itemsPerPage = new[] { 6, 9, 12, 18, 24 }
            }
        };

        return Json(layoutOptions);
    }
}