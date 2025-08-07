using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using LibraryApp.Data;
using LibraryApp.Models;
using LibraryApp.Services;
using LibraryApp.Attributes;

namespace LibraryApp.Controllers
{
    public class ProjectsController : BaseController
    {
        private readonly LibraryContext _context;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public ProjectsController(LibraryContext context, IUniversitySettingsService universitySettings, IWebHostEnvironment webHostEnvironment, ISessionService sessionService) : base(universitySettings, sessionService)
        {
            _context = context;
            _webHostEnvironment = webHostEnvironment;
        }

        // GET: Projects
        public async Task<IActionResult> Index(string searchTitle, string statusFilter, string supervisorFilter)
        {
            var projectsQuery = _context.Projects
                .Include(p => p.Student)
                .Include(p => p.Supervisor)
                .AsQueryable();

            // Apply search filters
            if (!string.IsNullOrEmpty(searchTitle))
            {
                projectsQuery = projectsQuery.Where(p => p.Title.Contains(searchTitle) || 
                                                   (p.Abstract != null && p.Abstract.Contains(searchTitle)) ||
                                                   (p.Keywords != null && p.Keywords.Contains(searchTitle)));
                ViewData["SearchTitle"] = searchTitle;
            }

            if (!string.IsNullOrEmpty(statusFilter) && Enum.TryParse<ProjectStatus>(statusFilter, out var status))
            {
                projectsQuery = projectsQuery.Where(p => p.Status == status);
                ViewData["StatusFilter"] = statusFilter;
            }

            if (!string.IsNullOrEmpty(supervisorFilter) && int.TryParse(supervisorFilter, out var professorId))
            {
                projectsQuery = projectsQuery.Where(p => p.SupervisorId == professorId);
                ViewData["SupervisorFilter"] = supervisorFilter;
            }

            var projects = await projectsQuery
                .OrderByDescending(p => p.SubmissionDate)
                .ToListAsync();

            // Populate supervisor dropdown
            var professors = await _context.Professors
                .OrderBy(s => s.FirstName)
                .ThenBy(s => s.LastName)
                .Select(s => new { 
                    Id = s.Id, 
                    DisplayText = s.Title + " " + s.FirstName + " " + s.LastName 
                })
                .ToListAsync();
            ViewData["Professors"] = professors;

            return View(projects);
        }

        // GET: Projects/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var project = await _context.Projects
                .Include(p => p.Student)
                .ThenInclude(s => s.Department)
                .Include(p => p.Supervisor)
                .ThenInclude(s => s.Department)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (project == null)
            {
                return NotFound();
            }

            return View(project);
        }

        // GET: Projects/Create
        [AdminOnly]
        public IActionResult Create()
        {
            var students = _context.Students.Include(s => s.Department)
                .Select(s => new { 
                    Id = s.Id, 
                    DisplayText = s.FirstName + " " + s.LastName + " (" + s.StudentNumber + ")"
                }).ToList();
                
            var professors = _context.Professors.Include(s => s.Department)
                .Select(s => new { 
                    Id = s.Id, 
                    DisplayText = s.Title + " " + s.FirstName + " " + s.LastName + " - " + s.Department.Name
                }).ToList();
                
            ViewData["StudentId"] = new SelectList(students, "Id", "DisplayText");
            ViewData["SupervisorId"] = new SelectList(professors, "Id", "DisplayText");
            return View();
        }

        // POST: Projects/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        [AdminOnly]
        public async Task<IActionResult> Create([Bind("Id,Title,Abstract,Keywords,Status,SubmissionDate,DefenseDate,Grade,StudentId,SupervisorId")] Project project, IFormFile? documentFile, IFormFile? posterFile)
        {
            // Handle document file upload
            if (documentFile != null && documentFile.Length > 0)
            {
                var uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "documents");
                Directory.CreateDirectory(uploadsFolder);
                
                var uniqueFileName = Guid.NewGuid().ToString() + "_" + documentFile.FileName;
                var filePath = Path.Combine(uploadsFolder, uniqueFileName);
                
                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await documentFile.CopyToAsync(fileStream);
                }
                
                project.DocumentPath = "/documents/" + uniqueFileName;
            }

            // Handle poster file upload
            if (posterFile != null && posterFile.Length > 0)
            {
                var uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "posters");
                Directory.CreateDirectory(uploadsFolder);
                
                var uniqueFileName = Guid.NewGuid().ToString() + "_" + posterFile.FileName;
                var filePath = Path.Combine(uploadsFolder, uniqueFileName);
                
                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await posterFile.CopyToAsync(fileStream);
                }
                
                project.PosterPath = "/posters/" + uniqueFileName;
            }
            
            if (ModelState.IsValid)
            {
                _context.Add(project);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            
            var students = _context.Students.Include(s => s.Department)
                .Select(s => new { 
                    Id = s.Id, 
                    DisplayText = s.FirstName + " " + s.LastName + " (" + s.StudentNumber + ")"
                }).ToList();
                
            var professors = _context.Professors.Include(s => s.Department)
                .Select(s => new { 
                    Id = s.Id, 
                    DisplayText = s.Title + " " + s.FirstName + " " + s.LastName + " - " + s.Department.Name
                }).ToList();
                
            ViewData["StudentId"] = new SelectList(students, "Id", "DisplayText", project.StudentId);
            ViewData["SupervisorId"] = new SelectList(professors, "Id", "DisplayText", project.SupervisorId);
            return View(project);
        }

        // GET: Projects/Edit/5
        [AdminOnly]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var project = await _context.Projects.FindAsync(id);
            if (project == null)
            {
                return NotFound();
            }
            
            var students = _context.Students.Include(s => s.Department)
                .Select(s => new { 
                    Id = s.Id, 
                    DisplayText = s.FirstName + " " + s.LastName + " (" + s.StudentNumber + ")"
                }).ToList();
                
            var professors = _context.Professors.Include(s => s.Department)
                .Select(s => new { 
                    Id = s.Id, 
                    DisplayText = s.Title + " " + s.FirstName + " " + s.LastName + " - " + s.Department.Name
                }).ToList();
                
            ViewData["StudentId"] = new SelectList(students, "Id", "DisplayText", project.StudentId);
            ViewData["SupervisorId"] = new SelectList(professors, "Id", "DisplayText", project.SupervisorId);
            return View(project);
        }

        // POST: Projects/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [AdminOnly]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Title,Abstract,Keywords,Status,SubmissionDate,DefenseDate,Grade,DocumentPath,PosterPath,StudentId,SupervisorId")] Project project, IFormFile? documentFile, IFormFile? posterFile)
        {
            if (id != project.Id)
            {
                return NotFound();
            }

            // Handle document file upload
            if (documentFile != null && documentFile.Length > 0)
            {
                var uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "documents");
                Directory.CreateDirectory(uploadsFolder);
                
                // Delete old file if it exists
                if (!string.IsNullOrEmpty(project.DocumentPath))
                {
                    var oldFilePath = Path.Combine(_webHostEnvironment.WebRootPath, project.DocumentPath.TrimStart('/'));
                    if (System.IO.File.Exists(oldFilePath))
                    {
                        System.IO.File.Delete(oldFilePath);
                    }
                }
                
                var uniqueFileName = Guid.NewGuid().ToString() + "_" + documentFile.FileName;
                var filePath = Path.Combine(uploadsFolder, uniqueFileName);
                
                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await documentFile.CopyToAsync(fileStream);
                }
                
                project.DocumentPath = "/documents/" + uniqueFileName;
            }

            // Handle poster file upload
            if (posterFile != null && posterFile.Length > 0)
            {
                var uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "posters");
                Directory.CreateDirectory(uploadsFolder);
                
                // Delete old poster if it exists
                if (!string.IsNullOrEmpty(project.PosterPath))
                {
                    var oldFilePath = Path.Combine(_webHostEnvironment.WebRootPath, project.PosterPath.TrimStart('/'));
                    if (System.IO.File.Exists(oldFilePath))
                    {
                        System.IO.File.Delete(oldFilePath);
                    }
                }
                
                var uniqueFileName = Guid.NewGuid().ToString() + "_" + posterFile.FileName;
                var filePath = Path.Combine(uploadsFolder, uniqueFileName);
                
                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await posterFile.CopyToAsync(fileStream);
                }
                
                project.PosterPath = "/posters/" + uniqueFileName;
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(project);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ProjectExists(project.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            ViewData["StudentId"] = new SelectList(_context.Students.Include(s => s.Department), "Id", "FirstName", project.StudentId);
            ViewData["SupervisorId"] = new SelectList(_context.Professors.Include(s => s.Department), "Id", "FirstName", project.SupervisorId);
            return View(project);
        }

        // GET: Projects/Delete/5
        [AdminOnly]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var project = await _context.Projects
                .Include(p => p.Student)
                .Include(p => p.Supervisor)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (project == null)
            {
                return NotFound();
            }

            return View(project);
        }

        // POST: Projects/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [AdminOnly]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var project = await _context.Projects.FindAsync(id);
            if (project != null)
            {
                _context.Projects.Remove(project);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool ProjectExists(int id)
        {
            return _context.Projects.Any(e => e.Id == id);
        }
    }
}