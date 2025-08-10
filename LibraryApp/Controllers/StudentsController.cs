using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using LibraryApp.Data;
using LibraryApp.Models;
using LibraryApp.Services;
using LibraryApp.Attributes;

namespace LibraryApp.Controllers
{
    [AdminOnly]
    public class StudentsController : BaseController
    {
        private readonly LibraryContext _context;

        public StudentsController(LibraryContext context, IUniversitySettingsService universitySettings, ISessionService sessionService) : base(universitySettings, sessionService)
        {
            _context = context;
        }

        // GET: Students
        public async Task<IActionResult> Index()
        {
            var students = _context.Students.Include(s => s.Department);
            return View(await students.ToListAsync());
        }

        // GET: Students/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var student = await _context.Students
                .Include(s => s.Department)
                .Include(s => s.Projects)
                .ThenInclude(p => p.Supervisor)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (student == null)
            {
                return NotFound();
            }

            return View(student);
        }

        // GET: Students/Create
        public IActionResult Create()
        {
            ViewData["DepartmentId"] = new SelectList(_context.Departments, "Id", "Name");
            return View();
        }

        // POST: Students/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,StudentNumber,FirstName,LastName,Email,Phone,DepartmentId,EnrollmentDate")] Student student)
        {
            if (ModelState.IsValid)
            {
                // Generate default password
                var authService = new AuthenticationService(_context);
                var defaultPassword = authService.GenerateDefaultPassword(student.FirstName, student.StudentNumber);
                student.Password = authService.HashPassword(defaultPassword);
                student.MustChangePassword = true;
                
                _context.Add(student);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            
            ViewData["DepartmentId"] = new SelectList(_context.Departments, "Id", "Name", student.DepartmentId);
            return View(student);
        }

        // GET: Students/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var student = await _context.Students.FindAsync(id);
            if (student == null)
            {
                return NotFound();
            }
            ViewData["DepartmentId"] = new SelectList(_context.Departments, "Id", "Name", student.DepartmentId);
            return View(student);
        }

        // POST: Students/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,StudentNumber,FirstName,LastName,Email,Phone,DepartmentId,EnrollmentDate")] Student student)
        {
            if (id != student.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var existingStudent = await _context.Students.AsNoTracking().FirstAsync(s => s.Id == id);
                    student.Password = existingStudent.Password; // Keep existing password
                    student.MustChangePassword = existingStudent.MustChangePassword;
                    student.LastLogin = existingStudent.LastLogin;
                    
                    _context.Update(student);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!StudentExists(student.Id))
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
            ViewData["DepartmentId"] = new SelectList(_context.Departments, "Id", "Name", student.DepartmentId);
            return View(student);
        }

        // GET: Students/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var student = await _context.Students
                .Include(s => s.Department)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (student == null)
            {
                return NotFound();
            }

            return View(student);
        }

        // POST: Students/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var student = await _context.Students.FindAsync(id);
            if (student != null)
            {
                _context.Students.Remove(student);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool StudentExists(int id)
        {
            return _context.Students.Any(e => e.Id == id);
        }

        // Export Students to CSV
        [HttpGet]
        public async Task<IActionResult> ExportStudents()
        {
            var students = await _context.Students
                .Include(s => s.Department)
                .Include(s => s.Projects)
                .OrderBy(s => s.LastName)
                .ToListAsync();

            var csv = new System.Text.StringBuilder();
            csv.AppendLine("Student Number,First Name,Last Name,Email,Phone,Department,Enrollment Date,Last Login,Projects Count");

            foreach (var student in students)
            {
                csv.AppendLine($"{student.StudentNumber},{student.FirstName},{student.LastName},{student.Email},{student.Phone},{student.Department?.Name},{student.EnrollmentDate:yyyy-MM-dd},{student.LastLogin:yyyy-MM-dd},{student.Projects?.Count ?? 0}");
            }

            var bytes = System.Text.Encoding.UTF8.GetBytes(csv.ToString());
            return File(bytes, "text/csv", $"students_export_{DateTime.Now:yyyyMMdd}.csv");
        }

        // Get Student Activities (AJAX endpoint)
        [HttpGet]
        public async Task<IActionResult> GetStudentActivities()
        {
            var activities = await _context.SystemAuditLogs
                .Where(a => a.UserRole == UserRole.Student)
                .OrderByDescending(a => a.Timestamp)
                .Take(50)
                .Select(a => new
                {
                    a.UserId,
                    a.Action,
                    a.EntityType,
                    a.Timestamp,
                    a.Details
                })
                .ToListAsync();

            return Json(activities);
        }

        // Student Analytics (AJAX endpoint)
        [HttpGet]
        public async Task<IActionResult> GetStudentAnalytics()
        {
            var analytics = new
            {
                TotalStudents = await _context.Students.CountAsync(),
                ActiveStudents = await _context.Students.Where(s => s.LastLogin >= DateTime.Now.AddDays(-30)).CountAsync(),
                NewStudentsThisMonth = await _context.Students.Where(s => s.EnrollmentDate >= DateTime.Now.AddDays(-30)).CountAsync(),
                DepartmentDistribution = await _context.Students
                    .Include(s => s.Department)
                    .GroupBy(s => s.Department.Name)
                    .Select(g => new { Department = g.Key, Count = g.Count() })
                    .ToListAsync(),
                EnrollmentTrend = await _context.Students
                    .Where(s => s.EnrollmentDate >= DateTime.Now.AddMonths(-12))
                    .GroupBy(s => new { s.EnrollmentDate.Year, s.EnrollmentDate.Month })
                    .Select(g => new { 
                        Period = $"{g.Key.Year}-{g.Key.Month:D2}", 
                        Count = g.Count() 
                    })
                    .ToListAsync()
            };

            return Json(analytics);
        }
    }
}