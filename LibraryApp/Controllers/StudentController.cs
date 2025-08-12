using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using LibraryApp.Data;
using LibraryApp.Models;
using LibraryApp.Services;
using LibraryApp.Attributes;

namespace LibraryApp.Controllers
{
    [StudentOnly]
    public class StudentController : BaseController
    {
        private readonly LibraryContext _context;

        public StudentController(LibraryContext context, IUniversitySettingsService universitySettings, ISessionService sessionService) : base(universitySettings, sessionService)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            // Check if user is logged in as student
            var userId = HttpContext.Session.GetString("UserId");
            var userRole = HttpContext.Session.GetString("UserRole");
            
            if (string.IsNullOrEmpty(userId) || userRole != "Student")
            {
                return RedirectToAction("Login", "Auth");
            }

            // Get student data
            var student = await _context.Students
                .Include(s => s.Department)
                .Include(s => s.ProjectStudents)
                    .ThenInclude(ps => ps.Project)
                        .ThenInclude(p => p.Supervisor)
                .FirstOrDefaultAsync(s => s.StudentNumber == userId);

            if (student == null)
            {
                return RedirectToAction("Login", "Auth");
            }

            var dashboardData = new StudentDashboardViewModel
            {
                Student = student,
                TotalProjects = student.ProjectStudents.Count,
                CompletedProjects = student.ProjectStudents.Count(ps => ps.Project.Status == ProjectStatus.EvaluatorApproved || ps.Project.Status == ProjectStatus.Published),
                InProgressProjects = student.ProjectStudents.Count(ps => ps.Project.Status == ProjectStatus.Submitted || ps.Project.Status == ProjectStatus.SupervisorApproved),
                RecentProjects = student.ProjectStudents.OrderByDescending(ps => ps.Project.SubmissionDate).Take(5).Select(ps => ps.Project).ToList(),
                UniversitySettings = _universitySettings.GetSettings()
            };

            return View(dashboardData);
        }

        public async Task<IActionResult> Projects()
        {
            var userId = HttpContext.Session.GetString("UserId");
            var userRole = HttpContext.Session.GetString("UserRole");
            
            if (string.IsNullOrEmpty(userId) || userRole != "Student")
            {
                return RedirectToAction("Login", "Auth");
            }

            var student = await _context.Students
                .Include(s => s.ProjectStudents)
                    .ThenInclude(ps => ps.Project)
                        .ThenInclude(p => p.Supervisor)
                            .ThenInclude(s => s.Department)
                .FirstOrDefaultAsync(s => s.StudentNumber == userId);

            if (student == null)
            {
                return RedirectToAction("Login", "Auth");
            }

            return View(student.Projects.OrderByDescending(p => p.SubmissionDate));
        }

        public async Task<IActionResult> Profile()
        {
            var userId = HttpContext.Session.GetString("UserId");
            var userRole = HttpContext.Session.GetString("UserRole");
            
            if (string.IsNullOrEmpty(userId) || userRole != "Student")
            {
                return RedirectToAction("Login", "Auth");
            }

            var student = await _context.Students
                .Include(s => s.Department)
                .Include(s => s.Projects)
                .FirstOrDefaultAsync(s => s.StudentNumber == userId);

            if (student == null)
            {
                return RedirectToAction("Login", "Auth");
            }

            return View(student);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateProfile(Student model)
        {
            var userId = HttpContext.Session.GetString("UserId");
            var userRole = HttpContext.Session.GetString("UserRole");
            
            if (string.IsNullOrEmpty(userId) || userRole != "Student")
            {
                this.AddError("Access Denied", "Please log in to update your profile");
                return RedirectToAction("Login", "Auth");
            }

            var student = await _context.Students
                .Include(s => s.Department)
                .FirstOrDefaultAsync(s => s.StudentNumber == userId);

            if (student == null)
            {
                this.AddError("Student Not Found", "Could not find your student record");
                return RedirectToAction("Login", "Auth");
            }

            // Only allow updating certain fields
            student.FirstName = model.FirstName;
            student.LastName = model.LastName;
            student.Email = model.Email;
            student.Phone = model.Phone;

            try
            {
                await _context.SaveChangesAsync();
                this.AddSuccess("Profile Updated", "Your profile has been successfully updated");
            }
            catch (Exception)
            {
                this.AddError("Update Failed", "Failed to update your profile. Please try again");
            }

            return RedirectToAction("Profile");
        }

        // Workflow Actions for Students
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SubmitProject(int projectId, string? comments = null)
        {
            var project = await _context.Projects
                .Include(p => p.Supervisor)
                .FirstOrDefaultAsync(p => p.Id == projectId);

            if (project == null)
            {
                this.AddError("Error", "Project not found");
                return RedirectToAction("Index");
            }

            // Check permission - only the assigned student can submit
            var currentStudent = await _context.Students
                .FirstOrDefaultAsync(s => s.StudentNumber == CurrentUserId);
            if (currentStudent == null || project.StudentId != currentStudent.Id)
            {
                this.AddError("Permission Denied", "You can only submit your own projects");
                return RedirectToAction("Index");
            }

            if (project.Status == ProjectStatus.Approved || project.Status == ProjectStatus.InProgress)
            {
                project.Status = ProjectStatus.SubmittedForReview;
                project.SubmissionForReviewDate = DateTime.Now;
                project.ReviewComments = comments;

                await _context.SaveChangesAsync();
                this.AddSuccess("Project Submitted", $"Project '{project.Title}' has been submitted for review");
            }
            else
            {
                this.AddWarning("Invalid Status", "Only approved or in-progress projects can be submitted for review");
            }

            return RedirectToAction("Index");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> StartProject(int projectId)
        {
            var project = await _context.Projects
                .Include(p => p.Supervisor)
                .FirstOrDefaultAsync(p => p.Id == projectId);

            if (project == null)
            {
                this.AddError("Error", "Project not found");
                return RedirectToAction("Index");
            }

            // Check permission - only the assigned student can start
            var currentStudent = await _context.Students
                .FirstOrDefaultAsync(s => s.StudentNumber == CurrentUserId);
            if (currentStudent == null || project.StudentId != currentStudent.Id)
            {
                this.AddError("Permission Denied", "You can only start your own projects");
                return RedirectToAction("Index");
            }

            if (project.Status == ProjectStatus.Approved)
            {
                project.Status = ProjectStatus.InProgress;
                await _context.SaveChangesAsync();
                this.AddSuccess("Project Started", $"You have started working on '{project.Title}'");
            }
            else
            {
                this.AddWarning("Invalid Status", "Only approved projects can be started");
            }

            return RedirectToAction("Index");
        }

        public async Task<IActionResult> EditProject(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var project = await _context.Projects
                .Include(p => p.Student)
                .Include(p => p.Supervisor)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (project == null)
            {
                return NotFound();
            }

            // Check permission - only the assigned student can edit
            var currentStudent = await _context.Students
                .FirstOrDefaultAsync(s => s.StudentNumber == CurrentUserId);
            if (currentStudent == null || project.StudentId != currentStudent.Id)
            {
                this.AddError("Permission Denied", "You can only edit your own projects");
                return RedirectToAction("Index");
            }

            return View(project);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditProject(int id, [Bind("Id,Title,Abstract,Keywords,DocumentPath,PosterPath")] Project model, IFormFile? documentFile, IFormFile? posterFile)
        {
            if (id != model.Id)
            {
                return NotFound();
            }

            var project = await _context.Projects
                .Include(p => p.Student)
                .Include(p => p.Supervisor)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (project == null)
            {
                return NotFound();
            }

            // Check permission - only the assigned student can edit
            var currentStudent = await _context.Students
                .FirstOrDefaultAsync(s => s.StudentNumber == CurrentUserId);
            if (currentStudent == null || project.StudentId != currentStudent.Id)
            {
                this.AddError("Permission Denied", "You can only edit your own projects");
                return RedirectToAction("Index");
            }

            // Handle document file upload
            if (documentFile != null && documentFile.Length > 0)
            {
                var allowedExtensions = new[] { ".pdf", ".doc", ".docx", ".txt" };
                var extension = Path.GetExtension(documentFile.FileName).ToLowerInvariant();
                
                if (!allowedExtensions.Contains(extension))
                {
                    this.AddError("Invalid File Type", "Only PDF, DOC, DOCX, and TXT files are allowed for documents");
                    return View(project);
                }

                var webRootPath = HttpContext.RequestServices.GetRequiredService<IWebHostEnvironment>().WebRootPath;
                var uploadsFolder = Path.Combine(webRootPath, "documents");
                Directory.CreateDirectory(uploadsFolder);
                
                // Delete old file if it exists
                if (!string.IsNullOrEmpty(project.DocumentPath))
                {
                    var oldFilePath = Path.Combine(webRootPath, project.DocumentPath.TrimStart('/'));
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
                var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".pdf" };
                var extension = Path.GetExtension(posterFile.FileName).ToLowerInvariant();
                
                if (!allowedExtensions.Contains(extension))
                {
                    this.AddError("Invalid File Type", "Only JPG, PNG, GIF, and PDF files are allowed for posters");
                    return View(project);
                }

                var webRootPath = HttpContext.RequestServices.GetRequiredService<IWebHostEnvironment>().WebRootPath;
                var uploadsFolder = Path.Combine(webRootPath, "posters");
                Directory.CreateDirectory(uploadsFolder);
                
                // Delete old poster if it exists
                if (!string.IsNullOrEmpty(project.PosterPath))
                {
                    var oldFilePath = Path.Combine(webRootPath, project.PosterPath.TrimStart('/'));
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

            // Update the allowed fields
            project.Title = model.Title;
            project.Abstract = model.Abstract;
            project.Keywords = model.Keywords;

            try
            {
                await _context.SaveChangesAsync();
                this.AddSuccess("Project Updated", "Your project has been successfully updated");
                return RedirectToAction("Projects");
            }
            catch (Exception)
            {
                this.AddError("Update Failed", "Failed to update your project. Please try again");
                return View(project);
            }
        }

        // Additional Student functionality
        public async Task<IActionResult> ProjectHistory()
        {
            var userId = HttpContext.Session.GetString("UserId");
            if (string.IsNullOrEmpty(userId))
            {
                return RedirectToAction("Login", "Auth");
            }

            var student = await _context.Students
                .Include(s => s.Projects)
                    .ThenInclude(p => p.Supervisor)
                .FirstOrDefaultAsync(s => s.StudentNumber == userId);

            if (student == null)
            {
                return RedirectToAction("Login", "Auth");
            }

            ViewBag.PageTitle = "Project History";
            return View(student.Projects.OrderByDescending(p => p.SubmissionDate));
        }

        public async Task<IActionResult> Notifications()
        {
            var userId = HttpContext.Session.GetString("UserId");
            if (string.IsNullOrEmpty(userId))
            {
                return RedirectToAction("Login", "Auth");
            }

            var notifications = await _context.Notifications
                .Where(n => n.UserId == userId && n.UserRole == UserRole.Student)
                .OrderByDescending(n => n.CreatedDate)
                .Take(50)
                .ToListAsync();

            ViewBag.PageTitle = "My Notifications";
            return View(notifications);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MarkNotificationRead(int notificationId)
        {
            var userId = HttpContext.Session.GetString("UserId");
            if (string.IsNullOrEmpty(userId))
            {
                return Json(new { success = false, message = "Authentication required" });
            }

            var notification = await _context.Notifications
                .FirstOrDefaultAsync(n => n.Id == notificationId && n.UserId == userId);

            if (notification == null)
            {
                return Json(new { success = false, message = "Notification not found" });
            }

            notification.IsRead = true;
            await _context.SaveChangesAsync();

            return Json(new { success = true });
        }

        public async Task<IActionResult> ViewProjectDocument(int projectId, string documentType)
        {
            var userId = HttpContext.Session.GetString("UserId");
            if (string.IsNullOrEmpty(userId))
            {
                return RedirectToAction("Login", "Auth");
            }

            var project = await _context.Projects
                .Include(p => p.Student)
                .FirstOrDefaultAsync(p => p.Id == projectId);

            if (project == null || project.Student.StudentNumber != userId)
            {
                this.AddError("Access Denied", "You can only view your own project documents");
                return RedirectToAction("Index");
            }

            string? filePath = documentType switch
            {
                "document" => project.DocumentPath,
                "poster" => project.PosterPath,
                "report" => project.ReportPath,
                "code" => project.CodePath,
                _ => null
            };

            if (string.IsNullOrEmpty(filePath) || !System.IO.File.Exists(filePath))
            {
                this.AddError("File Not Found", "The requested document could not be found");
                return RedirectToAction("Index");
            }

            var fileName = Path.GetFileName(filePath);
            var fileBytes = await System.IO.File.ReadAllBytesAsync(filePath);
            var contentType = GetContentType(filePath);

            return File(fileBytes, contentType, fileName);
        }

        private string GetContentType(string filePath)
        {
            var extension = Path.GetExtension(filePath).ToLowerInvariant();
            return extension switch
            {
                ".pdf" => "application/pdf",
                ".doc" => "application/msword",
                ".docx" => "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
                ".txt" => "text/plain",
                ".jpg" => "image/jpeg",
                ".jpeg" => "image/jpeg",
                ".png" => "image/png",
                ".gif" => "image/gif",
                _ => "application/octet-stream"
            };
        }
    }
}