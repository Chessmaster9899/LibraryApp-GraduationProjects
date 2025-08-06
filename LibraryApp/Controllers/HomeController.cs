using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using LibraryApp.Models;
using LibraryApp.Data;

namespace LibraryApp.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly LibraryContext _context;

    public HomeController(ILogger<HomeController> logger, LibraryContext context)
    {
        _logger = logger;
        _context = context;
    }

    public async Task<IActionResult> Index()
    {
        var dashboardData = new DashboardViewModel
        {
            TotalProjects = await _context.Projects.CountAsync(),
            TotalStudents = await _context.Students.CountAsync(),
            TotalSupervisors = await _context.Supervisors.CountAsync(),
            TotalDepartments = await _context.Departments.CountAsync(),
            
            ProjectsByStatus = await _context.Projects
                .GroupBy(p => p.Status)
                .Select(g => new ProjectStatusSummary
                {
                    Status = g.Key,
                    Count = g.Count()
                })
                .ToListAsync(),
                
            RecentProjects = await _context.Projects
                .Include(p => p.Student)
                .Include(p => p.Supervisor)
                .OrderByDescending(p => p.SubmissionDate)
                .Take(5)
                .ToListAsync()
        };

        return View(dashboardData);
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
