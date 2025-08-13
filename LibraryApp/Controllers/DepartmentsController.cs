using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using LibraryApp.Data;
using LibraryApp.Models;
using LibraryApp.Services;
using LibraryApp.Attributes;

namespace LibraryApp.Controllers
{
    [AdminOnly]
    public class DepartmentsController : BaseController
    {
        private readonly LibraryContext _context;

        public DepartmentsController(LibraryContext context, IUniversitySettingsService universitySettings, ISessionService sessionService) : base(universitySettings, sessionService)
        {
            _context = context;
        }

        // GET: Departments
        public async Task<IActionResult> Index()
        {
            var departments = await _context.Departments
                .Include(d => d.Students)
                    .ThenInclude(s => s.ProjectStudents)
                        .ThenInclude(ps => ps.Project)
                .Include(d => d.Professors)
                .ToListAsync();
            return View(departments);
        }

        // GET: Departments/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var department = await _context.Departments
                .Include(d => d.Students)
                .Include(d => d.Professors)
                .FirstOrDefaultAsync(m => m.Id == id);
            
            if (department == null)
            {
                return NotFound();
            }

            return View(department);
        }

        // GET: Departments/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Departments/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Name,Description")] Department department)
        {
            if (ModelState.IsValid)
            {
                _context.Add(department);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(department);
        }

        // GET: Departments/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var department = await _context.Departments.FindAsync(id);
            if (department == null)
            {
                return NotFound();
            }
            return View(department);
        }

        // POST: Departments/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,Description")] Department department)
        {
            if (id != department.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(department);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!DepartmentExists(department.Id))
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
            return View(department);
        }

        // GET: Departments/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var department = await _context.Departments
                .Include(d => d.Students)
                .Include(d => d.Professors)
                .FirstOrDefaultAsync(m => m.Id == id);
            
            if (department == null)
            {
                return NotFound();
            }

            return View(department);
        }

        // POST: Departments/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var department = await _context.Departments.FindAsync(id);
            if (department != null)
            {
                _context.Departments.Remove(department);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool DepartmentExists(int id)
        {
            return _context.Departments.Any(e => e.Id == id);
        }

        // Export Departments to CSV
        [HttpGet]
        public async Task<IActionResult> Export()
        {
            var departments = await _context.Departments
                .Include(d => d.Students)
                .Include(d => d.Professors)
                .OrderBy(d => d.Name)
                .ToListAsync();

            var csv = new System.Text.StringBuilder();
            csv.AppendLine("Department Name,Description,Student Count,Professor Count,Projects Count,Student/Professor Ratio");

            foreach (var dept in departments)
            {
                var studentCount = dept.Students.Count;
                var professorCount = dept.Professors.Count;
                var projectCount = dept.Students.SelectMany(s => s.Projects).Count();
                var ratio = professorCount > 0 ? (double)studentCount / professorCount : 0;

                csv.AppendLine($"{dept.Name},{dept.Description},{studentCount},{professorCount},{projectCount},{ratio:F1}");
            }

            var bytes = System.Text.Encoding.UTF8.GetBytes(csv.ToString());
            return File(bytes, "text/csv", $"departments_export_{DateTime.Now:yyyyMMdd}.csv");
        }

        [HttpGet]
        public async Task<IActionResult> GenerateSummary()
        {
            var departments = await _context.Departments
                .Include(d => d.Students)
                    .ThenInclude(s => s.Projects)
                .Include(d => d.Professors)
                    .ThenInclude(p => p.SupervisedProjects)
                .ToListAsync();

            var summary = new {
                success = true,
                message = "Department summary generated successfully",
                data = new {
                    totalDepartments = departments.Count,
                    totalStudents = departments.Sum(d => d.Students.Count),
                    totalProfessors = departments.Sum(d => d.Professors.Count),
                    totalProjects = departments.Sum(d => d.Students.SelectMany(s => s.Projects).Count()),
                    departmentStats = departments.Select(d => new {
                        name = d.Name,
                        studentCount = d.Students.Count,
                        professorCount = d.Professors.Count,
                        projectCount = d.Students.SelectMany(s => s.Projects).Count(),
                        ratio = d.Professors.Count > 0 ? (double)d.Students.Count / d.Professors.Count : 0
                    }).OrderByDescending(d => d.studentCount).ToList()
                }
            };

            return Json(summary);
        }

        [HttpGet]
        public async Task<IActionResult> ValidateStructure()
        {
            var departments = await _context.Departments
                .Include(d => d.Students)
                .Include(d => d.Professors)
                .ToListAsync();

            var validationIssues = new List<object>();

            foreach (var dept in departments)
            {
                // Check for departments with no professors
                if (dept.Professors.Count == 0)
                {
                    validationIssues.Add(new {
                        type = "warning",
                        department = dept.Name,
                        issue = "No professors assigned",
                        description = "Department has no professors assigned to supervise students"
                    });
                }

                // Check for departments with too many students per professor
                if (dept.Professors.Count > 0)
                {
                    var ratio = (double)dept.Students.Count / dept.Professors.Count;
                    if (ratio > 20) // Arbitrary threshold
                    {
                        validationIssues.Add(new {
                            type = "error",
                            department = dept.Name,
                            issue = "High student-to-professor ratio",
                            description = $"Ratio of {ratio:F1} students per professor exceeds recommended threshold"
                        });
                    }
                }

                // Check for departments with no students
                if (dept.Students.Count == 0)
                {
                    validationIssues.Add(new {
                        type = "info",
                        department = dept.Name,
                        issue = "No students enrolled",
                        description = "Department has no students currently enrolled"
                    });
                }

                // Check for missing department description
                if (string.IsNullOrWhiteSpace(dept.Description))
                {
                    validationIssues.Add(new {
                        type = "warning",
                        department = dept.Name,
                        issue = "Missing description",
                        description = "Department description is empty or missing"
                    });
                }
            }

            var validation = new {
                success = true,
                message = "Department structure validation completed",
                data = new {
                    totalIssues = validationIssues.Count,
                    errorCount = validationIssues.Count(i => i.GetType().GetProperty("type")?.GetValue(i)?.ToString() == "error"),
                    warningCount = validationIssues.Count(i => i.GetType().GetProperty("type")?.GetValue(i)?.ToString() == "warning"),
                    infoCount = validationIssues.Count(i => i.GetType().GetProperty("type")?.GetValue(i)?.ToString() == "info"),
                    issues = validationIssues
                }
            };

            return Json(validation);
        }
    }
}