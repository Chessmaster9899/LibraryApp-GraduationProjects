using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using LibraryApp.Data;
using LibraryApp.Models;

namespace LibraryApp.Controllers
{
    public class ProjectsController : Controller
    {
        private readonly LibraryContext _context;

        public ProjectsController(LibraryContext context)
        {
            _context = context;
        }

        // GET: Projects
        public async Task<IActionResult> Index()
        {
            var projects = _context.Projects
                .Include(p => p.Student)
                .Include(p => p.Supervisor);
            return View(await projects.ToListAsync());
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
        public IActionResult Create()
        {
            var students = _context.Students.Include(s => s.Department)
                .Select(s => new { 
                    Id = s.Id, 
                    DisplayText = s.FirstName + " " + s.LastName + " (" + s.StudentNumber + ")"
                }).ToList();
                
            var supervisors = _context.Supervisors.Include(s => s.Department)
                .Select(s => new { 
                    Id = s.Id, 
                    DisplayText = s.Title + " " + s.FirstName + " " + s.LastName + " - " + s.Department.Name
                }).ToList();
                
            ViewData["StudentId"] = new SelectList(students, "Id", "DisplayText");
            ViewData["SupervisorId"] = new SelectList(supervisors, "Id", "DisplayText");
            return View();
        }

        // POST: Projects/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Title,Abstract,Keywords,Status,SubmissionDate,DefenseDate,Grade,DocumentPath,StudentId,SupervisorId")] Project project)
        {
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
                
            var supervisors = _context.Supervisors.Include(s => s.Department)
                .Select(s => new { 
                    Id = s.Id, 
                    DisplayText = s.Title + " " + s.FirstName + " " + s.LastName + " - " + s.Department.Name
                }).ToList();
                
            ViewData["StudentId"] = new SelectList(students, "Id", "DisplayText", project.StudentId);
            ViewData["SupervisorId"] = new SelectList(supervisors, "Id", "DisplayText", project.SupervisorId);
            return View(project);
        }

        // GET: Projects/Edit/5
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
                
            var supervisors = _context.Supervisors.Include(s => s.Department)
                .Select(s => new { 
                    Id = s.Id, 
                    DisplayText = s.Title + " " + s.FirstName + " " + s.LastName + " - " + s.Department.Name
                }).ToList();
                
            ViewData["StudentId"] = new SelectList(students, "Id", "DisplayText", project.StudentId);
            ViewData["SupervisorId"] = new SelectList(supervisors, "Id", "DisplayText", project.SupervisorId);
            return View(project);
        }

        // POST: Projects/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Title,Abstract,Keywords,Status,SubmissionDate,DefenseDate,Grade,DocumentPath,StudentId,SupervisorId")] Project project)
        {
            if (id != project.Id)
            {
                return NotFound();
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
            ViewData["SupervisorId"] = new SelectList(_context.Supervisors.Include(s => s.Department), "Id", "FirstName", project.SupervisorId);
            return View(project);
        }

        // GET: Projects/Delete/5
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