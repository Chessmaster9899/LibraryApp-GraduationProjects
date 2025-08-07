using LibraryApp.Data;
using LibraryApp.Models;
using LibraryApp.Services;

namespace LibraryApp.Services
{
    public class SeedDataService
    {
        public static async Task SeedAsync(LibraryContext context, IAuthenticationService authService)
        {
            // Only seed if no professors exist
            if (context.Professors.Any())
                return;

            // Create sample professors
            var prof1 = new Professor
            {
                ProfessorId = "P2025001",
                FirstName = "Sarah",
                LastName = "Johnson",
                Email = "sarah.johnson@university.edu",
                Phone = "555-0101",
                Title = "Dr.",
                DepartmentId = 1, // Computer Science
                Specialization = "Artificial Intelligence",
                Role = ProfessorRole.Both,
                Password = authService.HashPassword(authService.GenerateDefaultPassword("Sarah", "P2025001")),
                MustChangePassword = true
            };

            var prof2 = new Professor
            {
                ProfessorId = "P2025002",
                FirstName = "Michael",
                LastName = "Brown",
                Email = "michael.brown@university.edu",
                Phone = "555-0102",
                Title = "Prof.",
                DepartmentId = 2, // Electrical Engineering
                Specialization = "Signal Processing",
                Role = ProfessorRole.Supervisor,
                Password = authService.HashPassword(authService.GenerateDefaultPassword("Michael", "P2025002")),
                MustChangePassword = true
            };

            var prof3 = new Professor
            {
                ProfessorId = "P2025003",
                FirstName = "Emily",
                LastName = "Davis",
                Email = "emily.davis@university.edu",
                Phone = "555-0103",
                Title = "Dr.",
                DepartmentId = 1, // Computer Science
                Specialization = "Software Engineering",
                Role = ProfessorRole.Evaluator,
                Password = authService.HashPassword(authService.GenerateDefaultPassword("Emily", "P2025003")),
                MustChangePassword = true
            };

            context.Professors.AddRange(prof1, prof2, prof3);
            await context.SaveChangesAsync();

            // Create sample student
            var student1 = new Student
            {
                StudentNumber = "CS2025001",
                FirstName = "John",
                LastName = "Smith",
                Email = "john.smith@student.university.edu",
                Phone = "555-0201",
                DepartmentId = 1, // Computer Science
                EnrollmentDate = new DateTime(2021, 9, 1),
                Password = authService.HashPassword(authService.GenerateDefaultPassword("John", "CS2025001")),
                MustChangePassword = true
            };

            context.Students.Add(student1);
            await context.SaveChangesAsync();

            // Create additional sample students for more diverse projects
            var student2 = new Student
            {
                StudentNumber = "EE2025001",
                FirstName = "Maria",
                LastName = "Rodriguez",
                Email = "maria.rodriguez@student.university.edu",
                Phone = "555-0202",
                DepartmentId = 2, // Electrical Engineering
                EnrollmentDate = new DateTime(2021, 9, 1),
                Password = authService.HashPassword(authService.GenerateDefaultPassword("Maria", "EE2025001")),
                MustChangePassword = true
            };

            var student3 = new Student
            {
                StudentNumber = "ME2025001",
                FirstName = "David",
                LastName = "Wilson",
                Email = "david.wilson@student.university.edu",
                Phone = "555-0203",
                DepartmentId = 3, // Mechanical Engineering
                EnrollmentDate = new DateTime(2021, 9, 1),
                Password = authService.HashPassword(authService.GenerateDefaultPassword("David", "ME2025001")),
                MustChangePassword = true
            };

            context.Students.AddRange(student2, student3);
            await context.SaveChangesAsync();

            // Create sample projects with various statuses
            var project1 = new Project
            {
                Title = "AI-Based Student Performance Prediction System",
                Abstract = "This project develops a machine learning system to predict student academic performance using various data points including attendance, assignment scores, and engagement metrics. The system uses neural networks and ensemble methods to achieve high prediction accuracy.",
                Keywords = "Machine Learning, AI, Education, Prediction",
                Status = ProjectStatus.InProgress,
                SubmissionDate = new DateTime(2025, 8, 1),
                StudentId = student1.Id,
                SupervisorId = prof1.Id,
                EvaluatorId = prof3.Id
            };

            // Create a completed and published project for Guest gallery
            var project2 = new Project
            {
                Title = "Smart Home Automation System Using IoT",
                Abstract = "An innovative IoT-based smart home automation system that integrates various sensors and actuators to provide intelligent control over lighting, temperature, security, and energy management. The system features a mobile application for remote monitoring and control.",
                Keywords = "IoT, Smart Home, Automation, Mobile App, Energy Management",
                Status = ProjectStatus.ReviewApproved,
                SubmissionDate = new DateTime(2024, 12, 15),
                DefenseDate = new DateTime(2025, 1, 20),
                Grade = "A",
                StudentId = student2.Id,
                SupervisorId = prof2.Id,
                EvaluatorId = prof1.Id,
                IsPubliclyVisible = true,
                ReviewDate = new DateTime(2025, 1, 25),
                ReviewComments = "Excellent project with innovative implementation and comprehensive documentation. Approved for publication.",
                ReviewedBy = "Admin"
            };

            // Create another completed project
            var project3 = new Project
            {
                Title = "Autonomous Drone Navigation System",
                Abstract = "Development of an autonomous navigation system for drones using computer vision and machine learning algorithms. The system enables drones to navigate complex environments while avoiding obstacles and optimizing flight paths for various mission objectives.",
                Keywords = "Drone Technology, Computer Vision, Autonomous Systems, Machine Learning, Navigation",
                Status = ProjectStatus.ReviewApproved,
                SubmissionDate = new DateTime(2024, 11, 30),
                DefenseDate = new DateTime(2025, 1, 10),
                Grade = "A+",
                StudentId = student3.Id,
                SupervisorId = prof1.Id,
                EvaluatorId = prof2.Id,
                IsPubliclyVisible = true,
                ReviewDate = new DateTime(2025, 1, 15),
                ReviewComments = "Outstanding project demonstrating exceptional technical skills and innovation. Highly recommended for publication and further research.",
                ReviewedBy = "Admin"
            };

            // Create a project pending submission
            var project4 = new Project
            {
                Title = "Blockchain-Based Supply Chain Management",
                Abstract = "A comprehensive blockchain solution for supply chain transparency and traceability. The system tracks products from manufacturing to delivery, ensuring authenticity and reducing fraud in global supply chains.",
                Keywords = "Blockchain, Supply Chain, Transparency, Smart Contracts",
                Status = ProjectStatus.Completed,
                SubmissionDate = new DateTime(2025, 7, 15),
                StudentId = student1.Id,
                SupervisorId = prof3.Id,
                EvaluatorId = prof1.Id
            };

            context.Projects.AddRange(project1, project2, project3, project4);
            await context.SaveChangesAsync();

            // Create sample project submissions for admin review demonstration
            var submission1 = new ProjectSubmission
            {
                ProjectId = project4.Id,
                SubmissionComments = "Completed blockchain supply chain management system with full documentation, working prototype, and comprehensive testing results. Ready for review and publication.",
                Status = SubmissionStatus.Pending,
                SubmissionDate = DateTime.Now.AddDays(-2)
            };

            context.ProjectSubmissions.Add(submission1);

            // Create sample notifications
            var notifications = new List<Notification>
            {
                new Notification
                {
                    UserId = "Admin",
                    UserRole = UserRole.Admin,
                    Title = "New Project Submission",
                    Message = "Project 'Blockchain-Based Supply Chain Management' has been submitted for review.",
                    CreatedDate = DateTime.Now.AddDays(-2),
                    RelatedUrl = "/Admin/ReviewSubmission/" + submission1.Id,
                    RelatedEntityType = "ProjectSubmission",
                    RelatedEntityId = submission1.Id
                },
                new Notification
                {
                    UserId = "CS2025001",
                    UserRole = UserRole.Student,
                    Title = "Project Submission Confirmed",
                    Message = "Your project 'Blockchain-Based Supply Chain Management' has been successfully submitted for review.",
                    CreatedDate = DateTime.Now.AddDays(-2)
                }
            };

            context.Notifications.AddRange(notifications);

            // Create sample announcements
            var announcements = new List<Announcement>
            {
                new Announcement
                {
                    Title = "New Project Gallery Now Available",
                    Content = "We're excited to announce the launch of our new public project gallery! Visitors can now browse completed graduation projects from all departments. This feature showcases the innovative work of our students and faculty.",
                    CreatedBy = "Admin",
                    CreatedDate = DateTime.Now.AddDays(-7),
                    IsActive = true,
                    IsUrgent = false,
                    TargetRoles = "Student,Professor,Admin"
                },
                new Announcement
                {
                    Title = "Project Submission Deadline Reminder",
                    Content = "Final reminder: Project submissions for this semester are due by August 31st. Please ensure all required documents are uploaded through the new submission system.",
                    CreatedBy = "Admin",
                    CreatedDate = DateTime.Now.AddDays(-3),
                    IsActive = true,
                    IsUrgent = true,
                    TargetRoles = "Student,Professor",
                    ExpiryDate = DateTime.Now.AddDays(5)
                }
            };

            context.Announcements.AddRange(announcements);

            // Create sample audit logs
            var auditLogs = new List<SystemAuditLog>
            {
                new SystemAuditLog
                {
                    UserId = "Admin",
                    UserRole = UserRole.Admin,
                    Action = "ReviewProjectSubmission",
                    EntityType = "ProjectSubmission",
                    EntityId = submission1.Id,
                    Details = "Project submission review initiated",
                    Timestamp = DateTime.Now.AddDays(-1),
                    IPAddress = "192.168.1.100"
                },
                new SystemAuditLog
                {
                    UserId = "CS2025001",
                    UserRole = UserRole.Student,
                    Action = "SubmitProject",
                    EntityType = "Project",
                    EntityId = project4.Id,
                    Details = "Project submitted for review with all required files",
                    Timestamp = DateTime.Now.AddDays(-2),
                    IPAddress = "192.168.1.200"
                },
                new SystemAuditLog
                {
                    UserId = "Admin",
                    UserRole = UserRole.Admin,
                    Action = "Login",
                    EntityType = "User",
                    Details = "Administrator login successful",
                    Timestamp = DateTime.Now.AddHours(-2),
                    IPAddress = "192.168.1.100"
                }
            };

            context.SystemAuditLogs.AddRange(auditLogs);
            await context.SaveChangesAsync();
        }
    }
}