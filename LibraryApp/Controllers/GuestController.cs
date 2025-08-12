using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using LibraryApp.Data;
using LibraryApp.Models;
using LibraryApp.Services;

namespace LibraryApp.Controllers;

[Route("Guest")]
public class GuestController : BaseController
{
    private readonly LibraryContext _context;

    public GuestController(LibraryContext context, IUniversitySettingsService universitySettings, ISessionService sessionService) : base(universitySettings, sessionService)
    {
        _context = context;
    }

    [HttpGet("")]
    [HttpGet("Dashboard")]
    public async Task<IActionResult> Dashboard()
    {
        var completedProjects = await _context.Projects
            .Include(p => p.ProjectStudents)
                .ThenInclude(ps => ps.Student)
                    .ThenInclude(s => s.Department)
            .Include(p => p.Supervisor)
            .Where(p => p.IsPubliclyVisible && 
                       (p.Status == ProjectStatus.EvaluatorApproved || 
                        p.Status == ProjectStatus.Published))
            .OrderByDescending(p => p.SubmissionDate)
            .ToListAsync();

        var departments = await _context.Departments.ToListAsync();

        var viewModel = new GuestDashboardViewModel
        {
            CompletedProjects = completedProjects.Take(10).ToList(),
            FeaturedProjects = completedProjects.Where(p => !string.IsNullOrEmpty(p.PosterPath)).Take(6).ToList(),
            TotalCompletedProjects = completedProjects.Count,
            Departments = departments,
            ProjectsByDepartment = completedProjects
                .Where(p => p.ProjectStudents.Any()) // Only include projects that have students
                .GroupBy(p => p.ProjectStudents.First().Student.Department.Name)
                .ToDictionary(g => g.Key, g => g.Count())
        };

        return View(viewModel);
    }

    [HttpGet("Projects")]
    public async Task<IActionResult> Projects(string? department, string? keyword, int page = 1)
    {
        var query = _context.Projects
            .Include(p => p.ProjectStudents)
                .ThenInclude(ps => ps.Student)
                    .ThenInclude(s => s.Department)
            .Include(p => p.Supervisor)
            .Where(p => p.IsPubliclyVisible && 
                       (p.Status == ProjectStatus.EvaluatorApproved || 
                        p.Status == ProjectStatus.Published));

        if (!string.IsNullOrEmpty(department))
        {
            query = query.Where(p => p.ProjectStudents.Any(ps => ps.Student.Department.Name == department));
        }

        if (!string.IsNullOrEmpty(keyword))
        {
            query = query.Where(p => p.Title.Contains(keyword) || 
                                   p.Abstract!.Contains(keyword) || 
                                   p.Keywords!.Contains(keyword));
        }

        const int pageSize = 12;
        var totalCount = await query.CountAsync();
        var projects = await query
            .OrderByDescending(p => p.SubmissionDate)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        ViewBag.Department = department;
        ViewBag.Keyword = keyword;
        ViewBag.Page = page;
        ViewBag.TotalPages = (int)Math.Ceiling((double)totalCount / pageSize);
        ViewBag.Departments = await _context.Departments.ToListAsync();

        return View(projects);
    }

    [HttpGet("Project/{id}")]
    public async Task<IActionResult> ProjectDetails(int id)
    {
        var project = await _context.Projects
            .Include(p => p.ProjectStudents)
                .ThenInclude(ps => ps.Student)
                    .ThenInclude(s => s.Department)
            .Include(p => p.Supervisor)
            .Include(p => p.Evaluator)
            .FirstOrDefaultAsync(p => p.Id == id && p.IsPubliclyVisible && 
                                    (p.Status == ProjectStatus.EvaluatorApproved || 
                                     p.Status == ProjectStatus.Published));

        if (project == null)
        {
            return NotFound();
        }

        return View(project);
    }

    [HttpGet("Poster/{id}")]
    public async Task<IActionResult> ViewPoster(int id)
    {
        var project = await _context.Projects
            .FirstOrDefaultAsync(p => p.Id == id && p.IsPubliclyVisible && 
                                    !string.IsNullOrEmpty(p.PosterPath));

        if (project == null || string.IsNullOrEmpty(project.PosterPath))
        {
            return NotFound();
        }

        var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", project.PosterPath.TrimStart('/'));
        
        if (!System.IO.File.Exists(filePath))
        {
            return NotFound();
        }

        var fileBytes = await System.IO.File.ReadAllBytesAsync(filePath);
        var contentType = GetContentType(filePath);
        
        return File(fileBytes, contentType);
    }

    [HttpGet("Report/{id}")]
    public async Task<IActionResult> DownloadReport(int id)
    {
        var project = await _context.Projects
            .FirstOrDefaultAsync(p => p.Id == id && p.IsPubliclyVisible && 
                                    !string.IsNullOrEmpty(p.ReportPath));

        if (project == null || string.IsNullOrEmpty(project.ReportPath))
        {
            return NotFound();
        }

        var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", project.ReportPath.TrimStart('/'));
        
        if (!System.IO.File.Exists(filePath))
        {
            return NotFound();
        }

        var fileBytes = await System.IO.File.ReadAllBytesAsync(filePath);
        var fileName = Path.GetFileName(project.ReportPath);
        
        return File(fileBytes, "application/pdf", fileName);
    }

    [HttpGet("Code/{id}")]
    public async Task<IActionResult> DownloadCode(int id)
    {
        var project = await _context.Projects
            .FirstOrDefaultAsync(p => p.Id == id && p.IsPubliclyVisible && 
                                    !string.IsNullOrEmpty(p.CodePath));

        if (project == null || string.IsNullOrEmpty(project.CodePath))
        {
            return NotFound();
        }

        var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", project.CodePath.TrimStart('/'));
        
        if (!System.IO.File.Exists(filePath))
        {
            return NotFound();
        }

        var fileBytes = await System.IO.File.ReadAllBytesAsync(filePath);
        var fileName = Path.GetFileName(project.CodePath);
        
        return File(fileBytes, "application/zip", fileName);
    }

    private static string GetContentType(string path)
    {
        var ext = Path.GetExtension(path).ToLowerInvariant();
        return ext switch
        {
            ".pdf" => "application/pdf",
            ".jpg" or ".jpeg" => "image/jpeg",
            ".png" => "image/png",
            ".gif" => "image/gif",
            ".bmp" => "image/bmp",
            _ => "application/octet-stream"
        };
    }
}