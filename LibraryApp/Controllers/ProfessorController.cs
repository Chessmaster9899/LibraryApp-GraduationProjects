using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using LibraryApp.Data;
using LibraryApp.Models;
using LibraryApp.Services;
using LibraryApp.Attributes;

namespace LibraryApp.Controllers
{
    [ProfessorOnly]
    public class ProfessorController : BaseController
    {
        private readonly LibraryContext _context;

        public ProfessorController(LibraryContext context, IUniversitySettingsService universitySettings, ISessionService sessionService) : base(universitySettings, sessionService)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            // Check if user is logged in as professor
            var userId = HttpContext.Session.GetString("UserId");
            var userRole = HttpContext.Session.GetString("UserRole");
            
            if (string.IsNullOrEmpty(userId) || userRole != "Professor")
            {
                return RedirectToAction("Login", "Auth");
            }

            // Get professor data
            var professor = await _context.Professors
                .Include(p => p.Department)
                .Include(p => p.SupervisedProjects)
                .ThenInclude(proj => proj.Student)
                .Include(p => p.EvaluatedProjects)
                .ThenInclude(proj => proj.Student)
                .FirstOrDefaultAsync(p => p.ProfessorId == userId);

            if (professor == null)
            {
                return RedirectToAction("Login", "Auth");
            }

            var dashboardData = new ProfessorDashboardViewModel
            {
                Professor = professor,
                TotalSupervisedProjects = professor.SupervisedProjects.Count,
                TotalEvaluatedProjects = professor.EvaluatedProjects.Count,
                CompletedSupervisedProjects = professor.SupervisedProjects.Count(p => p.Status == ProjectStatus.Completed || p.Status == ProjectStatus.Defended),
                CompletedEvaluatedProjects = professor.EvaluatedProjects.Count(p => p.Status == ProjectStatus.Completed || p.Status == ProjectStatus.Defended),
                RecentSupervisedProjects = professor.SupervisedProjects.OrderByDescending(p => p.SubmissionDate).Take(5).ToList(),
                RecentEvaluatedProjects = professor.EvaluatedProjects.OrderByDescending(p => p.SubmissionDate).Take(5).ToList(),
                UniversitySettings = _universitySettings.GetSettings()
            };

            return View(dashboardData);
        }

        public async Task<IActionResult> SupervisedProjects()
        {
            var userId = HttpContext.Session.GetString("UserId");
            var userRole = HttpContext.Session.GetString("UserRole");
            
            if (string.IsNullOrEmpty(userId) || userRole != "Professor")
            {
                return RedirectToAction("Login", "Auth");
            }

            var professor = await _context.Professors
                .Include(p => p.SupervisedProjects)
                .ThenInclude(proj => proj.Student)
                .ThenInclude(s => s.Department)
                .FirstOrDefaultAsync(p => p.ProfessorId == userId);

            if (professor == null)
            {
                return RedirectToAction("Login", "Auth");
            }

            ViewBag.PageTitle = "Projects I Supervise";
            return View("ProjectsList", professor.SupervisedProjects.OrderByDescending(p => p.SubmissionDate));
        }

        public async Task<IActionResult> EvaluatedProjects()
        {
            var userId = HttpContext.Session.GetString("UserId");
            var userRole = HttpContext.Session.GetString("UserRole");
            
            if (string.IsNullOrEmpty(userId) || userRole != "Professor")
            {
                return RedirectToAction("Login", "Auth");
            }

            var professor = await _context.Professors
                .Include(p => p.EvaluatedProjects)
                .ThenInclude(proj => proj.Student)
                .ThenInclude(s => s.Department)
                .FirstOrDefaultAsync(p => p.ProfessorId == userId);

            if (professor == null)
            {
                return RedirectToAction("Login", "Auth");
            }

            ViewBag.PageTitle = "Projects I Evaluate";
            return View("ProjectsList", professor.EvaluatedProjects.OrderByDescending(p => p.SubmissionDate));
        }

        public async Task<IActionResult> Profile()
        {
            var userId = HttpContext.Session.GetString("UserId");
            var userRole = HttpContext.Session.GetString("UserRole");
            
            if (string.IsNullOrEmpty(userId) || userRole != "Professor")
            {
                return RedirectToAction("Login", "Auth");
            }

            var professor = await _context.Professors
                .Include(p => p.Department)
                .Include(p => p.SupervisedProjects)
                .Include(p => p.EvaluatedProjects)
                .FirstOrDefaultAsync(p => p.ProfessorId == userId);

            if (professor == null)
            {
                return RedirectToAction("Login", "Auth");
            }

            return View(professor);
        }

        public async Task<IActionResult> MyStudents()
        {
            var userId = HttpContext.Session.GetString("UserId");
            var userRole = HttpContext.Session.GetString("UserRole");
            
            if (string.IsNullOrEmpty(userId) || userRole != "Professor")
            {
                return RedirectToAction("Login", "Auth");
            }

            var professor = await _context.Professors
                .Include(p => p.SupervisedProjects)
                .ThenInclude(proj => proj.Student)
                .ThenInclude(s => s.Department)
                .Include(p => p.EvaluatedProjects)
                .ThenInclude(proj => proj.Student)
                .ThenInclude(s => s.Department)
                .FirstOrDefaultAsync(p => p.ProfessorId == userId);

            if (professor == null)
            {
                return RedirectToAction("Login", "Auth");
            }

            // Get unique students from both supervised and evaluated projects
            var supervisedStudents = professor.SupervisedProjects.Select(p => p.Student).ToList();
            var evaluatedStudents = professor.EvaluatedProjects.Select(p => p.Student).ToList();
            
            var allStudents = supervisedStudents.Union(evaluatedStudents, new StudentEqualityComparer()).ToList();

            ViewBag.PageTitle = "My Students";
            return View(allStudents);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateProfile(Professor model)
        {
            var userId = HttpContext.Session.GetString("UserId");
            var userRole = HttpContext.Session.GetString("UserRole");
            
            if (string.IsNullOrEmpty(userId) || userRole != "Professor")
            {
                this.AddError("Access Denied", "Please log in to update your profile");
                return RedirectToAction("Login", "Auth");
            }

            var professor = await _context.Professors
                .Include(p => p.Department)
                .FirstOrDefaultAsync(p => p.ProfessorId == userId);

            if (professor == null)
            {
                this.AddError("Professor Not Found", "Could not find your professor record");
                return RedirectToAction("Login", "Auth");
            }

            // Only allow updating certain fields
            professor.Title = model.Title;
            professor.FirstName = model.FirstName;
            professor.LastName = model.LastName;
            professor.Email = model.Email;
            professor.Phone = model.Phone;
            professor.Specialization = model.Specialization;

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

        // Workflow Actions
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ApproveProject(int projectId, string? comments = null)
        {
            var project = await _context.Projects
                .Include(p => p.Student)
                .FirstOrDefaultAsync(p => p.Id == projectId);

            if (project == null)
            {
                this.AddError("Error", "Project not found");
                return RedirectToAction("Index");
            }

            // Check permission - only supervisor can approve
            var currentProfessor = await _context.Professors
                .FirstOrDefaultAsync(p => p.ProfessorId == CurrentUserId);
            if (currentProfessor == null || project.SupervisorId != currentProfessor.Id)
            {
                this.AddError("Permission Denied", "You can only approve projects you supervise");
                return RedirectToAction("Index");
            }

            if (project.Status == ProjectStatus.Proposed)
            {
                project.Status = ProjectStatus.Approved;
                project.ReviewComments = comments;
                project.ReviewDate = DateTime.Now;
                project.ReviewedBy = CurrentUser?.UserId;

                await _context.SaveChangesAsync();
                this.AddSuccess("Project Approved", $"Project '{project.Title}' has been approved successfully");
            }
            else
            {
                this.AddWarning("Invalid Status", "Only proposed projects can be approved");
            }

            return RedirectToAction("Index");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RejectProject(int projectId, string comments)
        {
            if (string.IsNullOrWhiteSpace(comments))
            {
                this.AddError("Comments Required", "Please provide rejection comments");
                return RedirectToAction("Index");
            }

            var project = await _context.Projects
                .Include(p => p.Student)
                .FirstOrDefaultAsync(p => p.Id == projectId);

            if (project == null)
            {
                this.AddError("Error", "Project not found");
                return RedirectToAction("Index");
            }

            // Check permission - only supervisor can reject
            var currentProfessor = await _context.Professors
                .FirstOrDefaultAsync(p => p.ProfessorId == CurrentUserId);
            if (currentProfessor == null || project.SupervisorId != currentProfessor.Id)
            {
                this.AddError("Permission Denied", "You can only reject projects you supervise");
                return RedirectToAction("Index");
            }

            if (project.Status == ProjectStatus.Proposed)
            {
                project.Status = ProjectStatus.ReviewRejected;
                project.ReviewComments = comments;
                project.ReviewDate = DateTime.Now;
                project.ReviewedBy = CurrentUser?.UserId;

                await _context.SaveChangesAsync();
                this.AddInfo("Project Rejected", $"Project '{project.Title}' has been rejected");
            }
            else
            {
                this.AddWarning("Invalid Status", "Only proposed projects can be rejected");
            }

            return RedirectToAction("Index");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MarkCompleted(int projectId, string? comments = null)
        {
            var project = await _context.Projects
                .Include(p => p.Student)
                .FirstOrDefaultAsync(p => p.Id == projectId);

            if (project == null)
            {
                this.AddError("Error", "Project not found");
                return RedirectToAction("Index");
            }

            // Check permission - supervisor or evaluator can mark as completed
            var currentProfessor = await _context.Professors
                .FirstOrDefaultAsync(p => p.ProfessorId == CurrentUserId);
            if (currentProfessor == null || (project.SupervisorId != currentProfessor.Id && project.EvaluatorId != currentProfessor.Id))
            {
                this.AddError("Permission Denied", "You can only mark projects you supervise or evaluate as completed");
                return RedirectToAction("Index");
            }

            if (project.Status == ProjectStatus.InProgress)
            {
                project.Status = ProjectStatus.Completed;
                project.ReviewComments = comments;
                project.ReviewDate = DateTime.Now;
                project.ReviewedBy = CurrentUser?.UserId;

                await _context.SaveChangesAsync();
                this.AddSuccess("Project Completed", $"Project '{project.Title}' has been marked as completed");
            }
            else
            {
                this.AddWarning("Invalid Status", "Only in-progress projects can be marked as completed");
            }

            return RedirectToAction("Index");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ScheduleDefense(int projectId, DateTime defenseDate, string? comments = null)
        {
            var project = await _context.Projects
                .Include(p => p.Student)
                .FirstOrDefaultAsync(p => p.Id == projectId);

            if (project == null)
            {
                this.AddError("Error", "Project not found");
                return RedirectToAction("Index");
            }

            // Check permission - supervisor can schedule defense
            var currentProfessor = await _context.Professors
                .FirstOrDefaultAsync(p => p.ProfessorId == CurrentUserId);
            if (currentProfessor == null || project.SupervisorId != currentProfessor.Id)
            {
                this.AddError("Permission Denied", "You can only schedule defense for projects you supervise");
                return RedirectToAction("Index");
            }

            if (project.Status == ProjectStatus.Completed || project.Status == ProjectStatus.ReviewApproved)
            {
                project.DefenseDate = defenseDate;
                project.ReviewComments = comments;
                project.ReviewDate = DateTime.Now;
                project.ReviewedBy = CurrentUser?.UserId;

                await _context.SaveChangesAsync();
                this.AddSuccess("Defense Scheduled", $"Defense for '{project.Title}' scheduled for {defenseDate:MMM dd, yyyy}");
            }
            else
            {
                this.AddWarning("Invalid Status", "Only completed or review-approved projects can have defense scheduled");
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

            // Check permission - only supervisor can edit
            var currentProfessor = await _context.Professors
                .FirstOrDefaultAsync(p => p.ProfessorId == CurrentUserId);
            if (currentProfessor == null || project.SupervisorId != currentProfessor.Id)
            {
                this.AddError("Permission Denied", "You can only edit projects you supervise");
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

            // Check permission - only supervisor can edit
            var currentProfessor = await _context.Professors
                .FirstOrDefaultAsync(p => p.ProfessorId == CurrentUserId);
            if (currentProfessor == null || project.SupervisorId != currentProfessor.Id)
            {
                this.AddError("Permission Denied", "You can only edit projects you supervise");
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
                this.AddSuccess("Project Updated", "The project has been successfully updated");
                return RedirectToAction("SupervisedProjects");
            }
            catch (Exception)
            {
                this.AddError("Update Failed", "Failed to update the project. Please try again");
                return View(project);
            }
        }

        // Additional Professor functionality
        public async Task<IActionResult> EvaluationTasks()
        {
            var userId = HttpContext.Session.GetString("UserId");
            if (string.IsNullOrEmpty(userId))
            {
                return RedirectToAction("Login", "Auth");
            }

            var professor = await _context.Professors
                .Include(p => p.EvaluatedProjects)
                    .ThenInclude(p => p.Student)
                        .ThenInclude(s => s.Department)
                .Include(p => p.EvaluatedProjects)
                    .ThenInclude(p => p.Supervisor)
                .FirstOrDefaultAsync(p => p.ProfessorId == userId);

            if (professor == null)
            {
                return RedirectToAction("Login", "Auth");
            }

            // Get projects that need evaluation
            var pendingEvaluations = professor.EvaluatedProjects
                .Where(p => p.Status == ProjectStatus.SubmittedForReview || p.Status == ProjectStatus.ReviewApproved)
                .OrderBy(p => p.SubmissionForReviewDate)
                .ToList();

            ViewBag.PageTitle = "Evaluation Tasks";
            return View(pendingEvaluations);
        }

        public async Task<IActionResult> ProjectEvaluation(int projectId)
        {
            var userId = HttpContext.Session.GetString("UserId");
            if (string.IsNullOrEmpty(userId))
            {
                return RedirectToAction("Login", "Auth");
            }

            var project = await _context.Projects
                .Include(p => p.Student)
                    .ThenInclude(s => s.Department)
                .Include(p => p.Supervisor)
                    .ThenInclude(s => s.Department)
                .Include(p => p.Evaluator)
                .FirstOrDefaultAsync(p => p.Id == projectId);

            if (project == null)
            {
                return NotFound();
            }

            var professor = await _context.Professors
                .FirstOrDefaultAsync(p => p.ProfessorId == userId);

            if (professor == null || project.EvaluatorId != professor.Id)
            {
                this.AddError("Access Denied", "You can only evaluate projects assigned to you");
                return RedirectToAction("Index");
            }

            ViewBag.PageTitle = $"Evaluate: {project.Title}";
            return View(project);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SubmitEvaluation(int projectId, string evaluationComments, string grade, bool approve)
        {
            var userId = HttpContext.Session.GetString("UserId");
            if (string.IsNullOrEmpty(userId))
            {
                return Json(new { success = false, message = "Authentication required" });
            }

            var project = await _context.Projects
                .Include(p => p.Student)
                .Include(p => p.Evaluator)
                .FirstOrDefaultAsync(p => p.Id == projectId);

            if (project == null)
            {
                return Json(new { success = false, message = "Project not found" });
            }

            var professor = await _context.Professors
                .FirstOrDefaultAsync(p => p.ProfessorId == userId);

            if (professor == null || project.EvaluatorId != professor.Id)
            {
                return Json(new { success = false, message = "Access denied" });
            }

            project.ReviewComments = evaluationComments;
            project.Grade = grade;
            project.ReviewDate = DateTime.Now;
            project.ReviewedBy = userId;
            project.Status = approve ? ProjectStatus.ReviewApproved : ProjectStatus.ReviewRejected;

            try
            {
                await _context.SaveChangesAsync();
                
                string message = approve ? 
                    $"Project '{project.Title}' has been approved with grade {grade}" :
                    $"Project '{project.Title}' has been rejected";
                    
                return Json(new { success = true, message = message });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Failed to submit evaluation: " + ex.Message });
            }
        }

        public async Task<IActionResult> StudentProgress(int studentId)
        {
            var userId = HttpContext.Session.GetString("UserId");
            if (string.IsNullOrEmpty(userId))
            {
                return RedirectToAction("Login", "Auth");
            }

            var student = await _context.Students
                .Include(s => s.Department)
                .Include(s => s.Projects)
                    .ThenInclude(p => p.Supervisor)
                .FirstOrDefaultAsync(s => s.Id == studentId);

            if (student == null)
            {
                return NotFound();
            }

            var professor = await _context.Professors
                .FirstOrDefaultAsync(p => p.ProfessorId == userId);

            if (professor == null)
            {
                return RedirectToAction("Login", "Auth");
            }

            // Check if professor supervises or evaluates any project for this student
            var hasAccess = student.Projects.Any(p => 
                p.SupervisorId == professor.Id || p.EvaluatorId == professor.Id);

            if (!hasAccess)
            {
                this.AddError("Access Denied", "You can only view progress for students you supervise or evaluate");
                return RedirectToAction("Index");
            }

            ViewBag.PageTitle = $"Progress: {student.FullName}";
            return View(student);
        }

        public async Task<IActionResult> Notifications()
        {
            var userId = HttpContext.Session.GetString("UserId");
            if (string.IsNullOrEmpty(userId))
            {
                return RedirectToAction("Login", "Auth");
            }

            var notifications = await _context.Notifications
                .Where(n => n.UserId == userId && n.UserRole == UserRole.Professor)
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

        [HttpGet]
        public async Task<IActionResult> ExportDashboardData()
        {
            var userId = HttpContext.Session.GetString("UserId");
            if (string.IsNullOrEmpty(userId))
            {
                return RedirectToAction("Login", "Auth");
            }

            var professor = await _context.Professors
                .Include(p => p.Department)
                .Include(p => p.SupervisedProjects)
                .ThenInclude(proj => proj.Student)
                .Include(p => p.EvaluatedProjects)
                .ThenInclude(proj => proj.Student)
                .FirstOrDefaultAsync(p => p.ProfessorId == userId);

            if (professor == null)
            {
                return RedirectToAction("Login", "Auth");
            }

            // Generate CSV content
            var csv = new System.Text.StringBuilder();
            csv.AppendLine("Professor Dashboard Report");
            csv.AppendLine($"Generated on: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
            csv.AppendLine($"Professor: {professor.Title} {professor.FirstName} {professor.LastName}");
            csv.AppendLine($"Department: {professor.Department?.Name}");
            csv.AppendLine($"Specialization: {professor.Specialization}");
            csv.AppendLine();
            
            csv.AppendLine("Supervised Projects");
            csv.AppendLine("Title,Student,Status,Submission Date");
            foreach (var project in professor.SupervisedProjects)
            {
                csv.AppendLine($"\"{project.Title}\",\"{project.Student?.FullName}\",\"{project.Status}\",\"{project.SubmissionDate:yyyy-MM-dd}\"");
            }
            
            csv.AppendLine();
            csv.AppendLine("Evaluated Projects");
            csv.AppendLine("Title,Student,Status,Submission Date");
            foreach (var project in professor.EvaluatedProjects)
            {
                csv.AppendLine($"\"{project.Title}\",\"{project.Student?.FullName}\",\"{project.Status}\",\"{project.SubmissionDate:yyyy-MM-dd}\"");
            }

            var fileName = $"professor_dashboard_{DateTime.Now:yyyyMMdd_HHmmss}.csv";
            var bytes = System.Text.Encoding.UTF8.GetBytes(csv.ToString());
            
            return File(bytes, "text/csv", fileName);
        }
    }
}