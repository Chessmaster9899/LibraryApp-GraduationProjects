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

            // Create sample project
            var project1 = new Project
            {
                Title = "AI-Based Student Performance Prediction System",
                Abstract = "This project develops a machine learning system to predict student academic performance using various data points including attendance, assignment scores, and engagement metrics.",
                Keywords = "Machine Learning, AI, Education, Prediction",
                Status = ProjectStatus.InProgress,
                SubmissionDate = new DateTime(2025, 8, 1),
                StudentId = student1.Id,
                SupervisorId = prof1.Id,
                EvaluatorId = prof3.Id
            };

            context.Projects.Add(project1);
            await context.SaveChangesAsync();
        }
    }
}