using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using LibraryApp.Data;
using LibraryApp.Models;
using Microsoft.EntityFrameworkCore;

namespace LibraryApp.Services
{
    public interface IDocumentGenerationService
    {
        Task<byte[]> GenerateProjectReportAsync(int projectId);
        Task<byte[]> GenerateStudentTranscriptAsync(int studentId);
        Task<byte[]> GenerateAnalyticsReportAsync(DateTime? startDate = null, DateTime? endDate = null);
        Task<byte[]> GenerateDepartmentReportAsync(int departmentId);
        Task<byte[]> GenerateProjectCertificateAsync(int projectId);
    }

    public class DocumentGenerationService : IDocumentGenerationService
    {
        private readonly LibraryContext _context;
        private readonly IUniversitySettingsService _universitySettings;

        public DocumentGenerationService(LibraryContext context, IUniversitySettingsService universitySettings)
        {
            _context = context;
            _universitySettings = universitySettings;
        }

        public async Task<byte[]> GenerateProjectReportAsync(int projectId)
        {
            var project = await _context.Projects
                .Include(p => p.ProjectStudents)
                    .ThenInclude(ps => ps.Student)
                        .ThenInclude(s => s.Department)
                .Include(p => p.Supervisor)
                    .ThenInclude(s => s.Department)
                .Include(p => p.Evaluator)
                .FirstOrDefaultAsync(p => p.Id == projectId);

            if (project == null)
                throw new ArgumentException("Project not found");

            var settings = _universitySettings.GetSettings();

            return Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(2, Unit.Centimetre);
                    page.PageColor(Colors.White);
                    page.DefaultTextStyle(x => x.FontSize(10).FontFamily(Fonts.Arial));

                    page.Header()
                        .Row(row =>
                        {
                            row.RelativeItem().Column(column =>
                            {
                                column.Item().Text(settings.Name)
                                    .FontSize(16).SemiBold().FontColor(Colors.Blue.Medium);
                                column.Item().Text(settings.ApplicationTitle)
                                    .FontSize(12).FontColor(Colors.Grey.Darken2);
                                column.Item().Text("Project Report")
                                    .FontSize(14).SemiBold().FontColor(Colors.Black);
                            });

                            row.ConstantItem(100).Height(60).Placeholder();
                        });

                    page.Content()
                        .PaddingVertical(1, Unit.Centimetre)
                        .Column(column =>
                        {
                            // Project Information Section
                            column.Item().Element(container => CreateSectionHeader(container, "Project Information"));
                            
                            column.Item().PaddingBottom(20).Table(table =>
                            {
                                table.ColumnsDefinition(columns =>
                                {
                                    columns.ConstantColumn(120);
                                    columns.RelativeColumn();
                                });

                                table.Cell().Element(CellStyle).Text("Title:").SemiBold();
                                table.Cell().Element(CellStyle).Text(project.Title);

                                table.Cell().Element(CellStyle).Text("Student(s):").SemiBold();
                                table.Cell().Element(CellStyle).Text(string.Join(", ", project.ProjectStudents.Select(ps => ps.Student.FullName)));

                                table.Cell().Element(CellStyle).Text("Department:").SemiBold();
                                table.Cell().Element(CellStyle).Text(project.ProjectStudents.FirstOrDefault()?.Student.Department?.Name ?? "N/A");

                                table.Cell().Element(CellStyle).Text("Supervisor:").SemiBold();
                                table.Cell().Element(CellStyle).Text(project.Supervisor?.DisplayName ?? "N/A");

                                if (project.Evaluator != null)
                                {
                                    table.Cell().Element(CellStyle).Text("Evaluator:").SemiBold();
                                    table.Cell().Element(CellStyle).Text(project.Evaluator.DisplayName);
                                }

                                table.Cell().Element(CellStyle).Text("Status:").SemiBold();
                                table.Cell().Element(CellStyle).Text(project.Status.ToString());

                                table.Cell().Element(CellStyle).Text("Submission Date:").SemiBold();
                                table.Cell().Element(CellStyle).Text(project.SubmissionDate.ToString("yyyy-MM-dd"));

                                if (project.DefenseDate.HasValue)
                                {
                                    table.Cell().Element(CellStyle).Text("Defense Date:").SemiBold();
                                    table.Cell().Element(CellStyle).Text(project.DefenseDate.Value.ToString("yyyy-MM-dd"));
                                }

                                if (!string.IsNullOrEmpty(project.Grade))
                                {
                                    table.Cell().Element(CellStyle).Text("Final Grade:").SemiBold();
                                    table.Cell().Element(CellStyle).Text(project.Grade);
                                }
                            });

                            // Abstract Section
                            if (!string.IsNullOrEmpty(project.Abstract))
                            {
                                column.Item().Element(container => CreateSectionHeader(container, "Abstract"));
                                column.Item().PaddingBottom(20).Text(project.Abstract).Justify();
                            }

                            // Keywords Section
                            if (!string.IsNullOrEmpty(project.Keywords))
                            {
                                column.Item().Element(container => CreateSectionHeader(container, "Keywords"));
                                column.Item().PaddingBottom(20).Text(project.Keywords);
                            }

                            // Note: Comments section could be added here if needed
                            // by querying ProjectComments separately
                        });

                    page.Footer()
                        .AlignCenter()
                        .Text(text =>
                        {
                            text.Span("Generated on ");
                            text.Span(DateTime.Now.ToString("yyyy-MM-dd HH:mm")).SemiBold();
                            text.Span(" - Page ");
                            text.CurrentPageNumber();
                            text.Span(" of ");
                            text.TotalPages();
                        });
                });
            }).GeneratePdf();
        }

        public async Task<byte[]> GenerateStudentTranscriptAsync(int studentId)
        {
            var student = await _context.Students
                .Include(s => s.Department)
                .Include(s => s.ProjectStudents)
                    .ThenInclude(ps => ps.Project)
                        .ThenInclude(p => p.Supervisor)
                .FirstOrDefaultAsync(s => s.Id == studentId);

            if (student == null)
                throw new ArgumentException("Student not found");

            var settings = _universitySettings.GetSettings();

            return Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(2, Unit.Centimetre);
                    page.PageColor(Colors.White);
                    page.DefaultTextStyle(x => x.FontSize(10).FontFamily(Fonts.Arial));

                    page.Header()
                        .Column(column =>
                        {
                            column.Item().AlignCenter().Text(settings.Name)
                                .FontSize(18).SemiBold().FontColor(Colors.Blue.Medium);
                            column.Item().AlignCenter().Text("Official Academic Transcript")
                                .FontSize(14).SemiBold();
                        });

                    page.Content()
                        .PaddingVertical(1, Unit.Centimetre)
                        .Column(column =>
                        {
                            // Student Information
                            column.Item().Element(container => CreateSectionHeader(container, "Student Information"));
                            
                            column.Item().PaddingBottom(20).Table(table =>
                            {
                                table.ColumnsDefinition(columns =>
                                {
                                    columns.ConstantColumn(120);
                                    columns.RelativeColumn();
                                });

                                table.Cell().Element(CellStyle).Text("Full Name:").SemiBold();
                                table.Cell().Element(CellStyle).Text(student.FullName);

                                table.Cell().Element(CellStyle).Text("Student Number:").SemiBold();
                                table.Cell().Element(CellStyle).Text(student.StudentNumber);

                                table.Cell().Element(CellStyle).Text("Department:").SemiBold();
                                table.Cell().Element(CellStyle).Text(student.Department?.Name ?? "N/A");

                                table.Cell().Element(CellStyle).Text("Email:").SemiBold();
                                table.Cell().Element(CellStyle).Text(student.Email);

                                table.Cell().Element(CellStyle).Text("Phone:").SemiBold();
                                table.Cell().Element(CellStyle).Text(student.Phone ?? "N/A");
                            });

                            // Projects Section
                            if (student.ProjectStudents.Any())
                            {
                                column.Item().Element(container => CreateSectionHeader(container, "Project History"));
                                
                                column.Item().Table(table =>
                                {
                                    table.ColumnsDefinition(columns =>
                                    {
                                        columns.RelativeColumn(3);
                                        columns.RelativeColumn(2);
                                        columns.RelativeColumn(1);
                                        columns.RelativeColumn(1);
                                    });

                                    // Headers
                                    table.Cell().Element(HeaderCellStyle).Text("Project Title");
                                    table.Cell().Element(HeaderCellStyle).Text("Supervisor");
                                    table.Cell().Element(HeaderCellStyle).Text("Status");
                                    table.Cell().Element(HeaderCellStyle).Text("Grade");

                                    foreach (var projectStudent in student.ProjectStudents)
                                    {
                                        var project = projectStudent.Project;
                                        
                                        table.Cell().Element(CellStyle).Text(project.Title);
                                        table.Cell().Element(CellStyle).Text(project.Supervisor?.DisplayName ?? "N/A");
                                        table.Cell().Element(CellStyle).Text(project.Status.ToString());
                                        table.Cell().Element(CellStyle).Text(project.Grade ?? "N/A");
                                    }
                                });
                            }
                        });

                    page.Footer()
                        .AlignCenter()
                        .Text(text =>
                        {
                            text.Span("Generated on ");
                            text.Span(DateTime.Now.ToString("yyyy-MM-dd HH:mm")).SemiBold();
                            text.Span(" - Page ");
                            text.CurrentPageNumber();
                            text.Span(" of ");
                            text.TotalPages();
                        });
                });
            }).GeneratePdf();
        }

        public async Task<byte[]> GenerateAnalyticsReportAsync(DateTime? startDate = null, DateTime? endDate = null)
        {
            startDate ??= DateTime.Now.AddYears(-1);
            endDate ??= DateTime.Now;

            var projects = await _context.Projects
                .Include(p => p.ProjectStudents)
                    .ThenInclude(ps => ps.Student)
                        .ThenInclude(s => s.Department)
                .Include(p => p.Supervisor)
                    .ThenInclude(s => s.Department)
                .Where(p => p.SubmissionDate >= startDate && p.SubmissionDate <= endDate)
                .ToListAsync();

            var settings = _universitySettings.GetSettings();

            return Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(2, Unit.Centimetre);
                    page.PageColor(Colors.White);
                    page.DefaultTextStyle(x => x.FontSize(10).FontFamily(Fonts.Arial));

                    page.Header()
                        .Column(column =>
                        {
                            column.Item().AlignCenter().Text(settings.Name)
                                .FontSize(18).SemiBold().FontColor(Colors.Blue.Medium);
                            column.Item().AlignCenter().Text("Analytics Report")
                                .FontSize(14).SemiBold();
                            column.Item().AlignCenter().Text($"Period: {startDate:yyyy-MM-dd} to {endDate:yyyy-MM-dd}")
                                .FontSize(10).FontColor(Colors.Grey.Darken1);
                        });

                    page.Content()
                        .PaddingVertical(1, Unit.Centimetre)
                        .Column(column =>
                        {
                            // Summary Statistics
                            column.Item().Element(container => CreateSectionHeader(container, "Summary Statistics"));
                            
                            column.Item().PaddingBottom(20).Row(row =>
                            {
                                row.RelativeItem().Border(1).Padding(10).Column(statColumn =>
                                {
                                    statColumn.Item().AlignCenter().Text("Total Projects").SemiBold();
                                    statColumn.Item().AlignCenter().Text(projects.Count.ToString())
                                        .FontSize(24).FontColor(Colors.Blue.Medium);
                                });

                                row.Spacing(10);

                                row.RelativeItem().Border(1).Padding(10).Column(statColumn =>
                                {
                                    statColumn.Item().AlignCenter().Text("Completed").SemiBold();
                                    statColumn.Item().AlignCenter().Text(projects.Count(p => p.Status == ProjectStatus.ReviewApproved).ToString())
                                        .FontSize(24).FontColor(Colors.Green.Medium);
                                });

                                row.Spacing(10);

                                row.RelativeItem().Border(1).Padding(10).Column(statColumn =>
                                {
                                    statColumn.Item().AlignCenter().Text("In Progress").SemiBold();
                                    statColumn.Item().AlignCenter().Text(projects.Count(p => p.Status == ProjectStatus.InProgress).ToString())
                                        .FontSize(24).FontColor(Colors.Orange.Medium);
                                });

                                row.Spacing(10);

                                row.RelativeItem().Border(1).Padding(10).Column(statColumn =>
                                {
                                    statColumn.Item().AlignCenter().Text("Projects with Grades").SemiBold();
                                    var gradedCount = projects.Count(p => !string.IsNullOrEmpty(p.Grade));
                                    statColumn.Item().AlignCenter().Text(gradedCount.ToString())
                                        .FontSize(24).FontColor(Colors.Purple.Medium);
                                });
                            });

                            // Projects by Department
                            column.Item().Element(container => CreateSectionHeader(container, "Projects by Department"));
                            
                            var departmentStats = projects.GroupBy(p => p.ProjectStudents.FirstOrDefault()?.Student.Department?.Name ?? "Unknown")
                                .OrderByDescending(g => g.Count())
                                .Take(10);

                            column.Item().PaddingBottom(20).Table(table =>
                            {
                                table.ColumnsDefinition(columns =>
                                {
                                    columns.RelativeColumn(3);
                                    columns.RelativeColumn(1);
                                    columns.RelativeColumn(1);
                                });

                                table.Cell().Element(HeaderCellStyle).Text("Department");
                                table.Cell().Element(HeaderCellStyle).Text("Projects");
                                table.Cell().Element(HeaderCellStyle).Text("Percentage");

                                foreach (var dept in departmentStats)
                                {
                                    var percentage = (dept.Count() * 100.0 / projects.Count);
                                    
                                    table.Cell().Element(CellStyle).Text(dept.Key);
                                    table.Cell().Element(CellStyle).Text(dept.Count().ToString());
                                    table.Cell().Element(CellStyle).Text($"{percentage:F1}%");
                                }
                            });

                            // Projects by Status
                            column.Item().Element(container => CreateSectionHeader(container, "Projects by Status"));
                            
                            var statusStats = projects.GroupBy(p => p.Status)
                                .OrderByDescending(g => g.Count());

                            column.Item().Table(table =>
                            {
                                table.ColumnsDefinition(columns =>
                                {
                                    columns.RelativeColumn(2);
                                    columns.RelativeColumn(1);
                                    columns.RelativeColumn(1);
                                });

                                table.Cell().Element(HeaderCellStyle).Text("Status");
                                table.Cell().Element(HeaderCellStyle).Text("Count");
                                table.Cell().Element(HeaderCellStyle).Text("Percentage");

                                foreach (var status in statusStats)
                                {
                                    var percentage = (status.Count() * 100.0 / projects.Count);
                                    
                                    table.Cell().Element(CellStyle).Text(status.Key.ToString());
                                    table.Cell().Element(CellStyle).Text(status.Count().ToString());
                                    table.Cell().Element(CellStyle).Text($"{percentage:F1}%");
                                }
                            });
                        });

                    page.Footer()
                        .AlignCenter()
                        .Text(text =>
                        {
                            text.Span("Generated on ");
                            text.Span(DateTime.Now.ToString("yyyy-MM-dd HH:mm")).SemiBold();
                            text.Span(" - Page ");
                            text.CurrentPageNumber();
                            text.Span(" of ");
                            text.TotalPages();
                        });
                });
            }).GeneratePdf();
        }

        public async Task<byte[]> GenerateDepartmentReportAsync(int departmentId)
        {
            var department = await _context.Departments
                .Include(d => d.Students)
                    .ThenInclude(s => s.ProjectStudents)
                        .ThenInclude(ps => ps.Project)
                .Include(d => d.Professors)
                    .ThenInclude(p => p.SupervisedProjects)
                .FirstOrDefaultAsync(d => d.Id == departmentId);

            if (department == null)
                throw new ArgumentException("Department not found");

            var settings = _universitySettings.GetSettings();

            return Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(2, Unit.Centimetre);
                    page.PageColor(Colors.White);
                    page.DefaultTextStyle(x => x.FontSize(10).FontFamily(Fonts.Arial));

                    page.Header()
                        .Column(column =>
                        {
                            column.Item().AlignCenter().Text(settings.Name)
                                .FontSize(18).SemiBold().FontColor(Colors.Blue.Medium);
                            column.Item().AlignCenter().Text("Department Report")
                                .FontSize(14).SemiBold();
                            column.Item().AlignCenter().Text(department.Name)
                                .FontSize(12).FontColor(Colors.Grey.Darken1);
                        });

                    page.Content()
                        .PaddingVertical(1, Unit.Centimetre)
                        .Column(column =>
                        {
                            // Department Information
                            column.Item().Element(container => CreateSectionHeader(container, "Department Overview"));
                            
                            column.Item().PaddingBottom(20).Table(table =>
                            {
                                table.ColumnsDefinition(columns =>
                                {
                                    columns.ConstantColumn(120);
                                    columns.RelativeColumn();
                                });

                                table.Cell().Element(CellStyle).Text("Department:").SemiBold();
                                table.Cell().Element(CellStyle).Text(department.Name);

                                if (!string.IsNullOrEmpty(department.Description))
                                {
                                    table.Cell().Element(CellStyle).Text("Description:").SemiBold();
                                    table.Cell().Element(CellStyle).Text(department.Description);
                                }

                                table.Cell().Element(CellStyle).Text("Total Students:").SemiBold();
                                table.Cell().Element(CellStyle).Text(department.Students.Count.ToString());

                                table.Cell().Element(CellStyle).Text("Total Professors:").SemiBold();
                                table.Cell().Element(CellStyle).Text(department.Professors.Count.ToString());

                                var totalProjects = department.Students.Sum(s => s.ProjectStudents.Count);
                                table.Cell().Element(CellStyle).Text("Total Projects:").SemiBold();
                                table.Cell().Element(CellStyle).Text(totalProjects.ToString());
                            });

                            // Professor Statistics
                            if (department.Professors.Any())
                            {
                                column.Item().Element(container => CreateSectionHeader(container, "Professor Statistics"));
                                
                                column.Item().PaddingBottom(20).Table(table =>
                                {
                                    table.ColumnsDefinition(columns =>
                                    {
                                        columns.RelativeColumn(2);
                                        columns.RelativeColumn(1);
                                        columns.RelativeColumn(1);
                                    });

                                    table.Cell().Element(HeaderCellStyle).Text("Professor");
                                    table.Cell().Element(HeaderCellStyle).Text("Supervised");
                                    table.Cell().Element(HeaderCellStyle).Text("Evaluated");

                                    foreach (var professor in department.Professors.OrderBy(p => p.LastName))
                                    {
                                        table.Cell().Element(CellStyle).Text(professor.DisplayName);
                                        table.Cell().Element(CellStyle).Text(professor.SupervisedProjects.Count.ToString());
                                        table.Cell().Element(CellStyle).Text(professor.EvaluatedProjects.Count.ToString());
                                    }
                                });
                            }

                            // Recent Projects
                            var recentProjects = department.Students
                                .SelectMany(s => s.ProjectStudents.Select(ps => ps.Project))
                                .OrderByDescending(p => p.SubmissionDate)
                                .Take(10);

                            if (recentProjects.Any())
                            {
                                column.Item().Element(container => CreateSectionHeader(container, "Recent Projects"));
                                
                                column.Item().Table(table =>
                                {
                                    table.ColumnsDefinition(columns =>
                                    {
                                        columns.RelativeColumn(3);
                                        columns.RelativeColumn(1);
                                        columns.RelativeColumn(1);
                                    });

                                    table.Cell().Element(HeaderCellStyle).Text("Project Title");
                                    table.Cell().Element(HeaderCellStyle).Text("Status");
                                    table.Cell().Element(HeaderCellStyle).Text("Submission");

                                    foreach (var project in recentProjects)
                                    {
                                        table.Cell().Element(CellStyle).Text(project.Title);
                                        table.Cell().Element(CellStyle).Text(project.Status.ToString());
                                        table.Cell().Element(CellStyle).Text(project.SubmissionDate.ToString("yyyy-MM-dd"));
                                    }
                                });
                            }
                        });

                    page.Footer()
                        .AlignCenter()
                        .Text(text =>
                        {
                            text.Span("Generated on ");
                            text.Span(DateTime.Now.ToString("yyyy-MM-dd HH:mm")).SemiBold();
                            text.Span(" - Page ");
                            text.CurrentPageNumber();
                            text.Span(" of ");
                            text.TotalPages();
                        });
                });
            }).GeneratePdf();
        }

        public async Task<byte[]> GenerateProjectCertificateAsync(int projectId)
        {
            var project = await _context.Projects
                .Include(p => p.ProjectStudents)
                    .ThenInclude(ps => ps.Student)
                        .ThenInclude(s => s.Department)
                .Include(p => p.Supervisor)
                .FirstOrDefaultAsync(p => p.Id == projectId);

            if (project == null || project.Status != ProjectStatus.ReviewApproved)
                throw new ArgumentException("Project not found or not approved");

            var settings = _universitySettings.GetSettings();

            return Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(2, Unit.Centimetre);
                    page.PageColor(Colors.White);
                    page.DefaultTextStyle(x => x.FontSize(12).FontFamily(Fonts.Arial));

                    page.Content()
                        .PaddingVertical(3, Unit.Centimetre)
                        .Column(column =>
                        {
                            // Certificate Header
                            column.Item().AlignCenter().Text(settings.Name)
                                .FontSize(24).SemiBold().FontColor(Colors.Blue.Medium);
                            
                            column.Item().PaddingTop(20).AlignCenter().Text("CERTIFICATE OF COMPLETION")
                                .FontSize(18).SemiBold();

                            column.Item().PaddingTop(40).AlignCenter().Text("This is to certify that")
                                .FontSize(14);

                            column.Item().PaddingTop(20).AlignCenter().Text(string.Join(", ", project.ProjectStudents.Select(ps => ps.Student.FullName)))
                                .FontSize(20).SemiBold().FontColor(Colors.Blue.Darken1);

                            column.Item().PaddingTop(20).AlignCenter().Text("has successfully completed the graduation project entitled")
                                .FontSize(14);

                            column.Item().PaddingTop(20).AlignCenter().Text(project.Title)
                                .FontSize(16).SemiBold().Italic();

                            column.Item().PaddingTop(20).AlignCenter().Text($"under the supervision of {project.Supervisor?.DisplayName}")
                                .FontSize(14);

                            column.Item().PaddingTop(20).AlignCenter().Text($"in the {project.ProjectStudents.FirstOrDefault()?.Student.Department?.Name} Department")
                                .FontSize(14);

                            if (!string.IsNullOrEmpty(project.Grade))
                            {
                                column.Item().PaddingTop(20).AlignCenter().Text($"with a final grade of {project.Grade}")
                                    .FontSize(14).SemiBold();
                            }

                            if (project.DefenseDate.HasValue)
                            {
                                column.Item().PaddingTop(40).AlignCenter().Text($"Date of Defense: {project.DefenseDate.Value:MMMM dd, yyyy}")
                                    .FontSize(12);
                            }

                            column.Item().PaddingTop(60).Row(row =>
                            {
                                row.RelativeItem().AlignCenter().Column(signColumn =>
                                {
                                    signColumn.Item().Text("_____________________");
                                    signColumn.Item().PaddingTop(5).Text("Department Head").FontSize(10);
                                });

                                row.RelativeItem().AlignCenter().Column(signColumn =>
                                {
                                    signColumn.Item().Text("_____________________");
                                    signColumn.Item().PaddingTop(5).Text("Academic Affairs").FontSize(10);
                                });
                            });
                        });

                    page.Footer()
                        .AlignCenter()
                        .Text($"Generated on {DateTime.Now:MMMM dd, yyyy}")
                        .FontSize(10).FontColor(Colors.Grey.Darken1);
                });
            }).GeneratePdf();
        }

        private static void CreateSectionHeader(IContainer container, string title)
        {
            container.PaddingBottom(10).BorderBottom(2).BorderColor(Colors.Blue.Medium)
                .Text(title).FontSize(14).SemiBold().FontColor(Colors.Blue.Medium);
        }

        private static IContainer CellStyle(IContainer container)
        {
            return container.BorderBottom(1).BorderColor(Colors.Grey.Lighten2).PaddingVertical(5);
        }

        private static IContainer HeaderCellStyle(IContainer container)
        {
            return container.DefaultTextStyle(x => x.SemiBold()).PaddingVertical(5)
                .BorderBottom(2).BorderColor(Colors.Blue.Medium).Background(Colors.Grey.Lighten3);
        }
    }
}