using Microsoft.EntityFrameworkCore;
using LibraryApp.Models;

namespace LibraryApp.Data;

public class LibraryContext : DbContext
{
    public LibraryContext(DbContextOptions<LibraryContext> options) : base(options)
    {
    }

    public DbSet<Department> Departments { get; set; }
    public DbSet<Student> Students { get; set; }
    public DbSet<Professor> Professors { get; set; }
    public DbSet<Project> Projects { get; set; }
    public DbSet<User> Users { get; set; }
    public DbSet<Admin> Admins { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configure relationships and constraints
        modelBuilder.Entity<Student>()
            .HasIndex(s => s.StudentNumber)
            .IsUnique();

        modelBuilder.Entity<Student>()
            .HasIndex(s => s.Email)
            .IsUnique();

        modelBuilder.Entity<Professor>()
            .HasIndex(s => s.ProfessorId)
            .IsUnique();

        modelBuilder.Entity<Professor>()
            .HasIndex(s => s.Email)
            .IsUnique();

        modelBuilder.Entity<User>()
            .HasIndex(u => u.UserId)
            .IsUnique();

        modelBuilder.Entity<Admin>()
            .HasIndex(a => a.Username)
            .IsUnique();

        // Configure foreign key relationships
        modelBuilder.Entity<Student>()
            .HasOne(s => s.Department)
            .WithMany(d => d.Students)
            .HasForeignKey(s => s.DepartmentId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Professor>()
            .HasOne(s => s.Department)
            .WithMany(d => d.Professors)
            .HasForeignKey(s => s.DepartmentId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Project>()
            .HasOne(p => p.Student)
            .WithMany(s => s.Projects)
            .HasForeignKey(p => p.StudentId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Project>()
            .HasOne(p => p.Supervisor)
            .WithMany(s => s.SupervisedProjects)
            .HasForeignKey(p => p.SupervisorId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Project>()
            .HasOne(p => p.Evaluator)
            .WithMany(s => s.EvaluatedProjects)
            .HasForeignKey(p => p.EvaluatorId)
            .OnDelete(DeleteBehavior.SetNull);

        // Configure User relationships
        modelBuilder.Entity<User>()
            .HasOne(u => u.Student)
            .WithOne()
            .HasForeignKey<User>(u => u.StudentId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<User>()
            .HasOne(u => u.Professor)
            .WithOne()
            .HasForeignKey<User>(u => u.ProfessorId)
            .OnDelete(DeleteBehavior.Cascade);

        // Seed some initial data
        modelBuilder.Entity<Department>().HasData(
            new Department { Id = 1, Name = "Computer Science", Description = "Department of Computer Science and Engineering" },
            new Department { Id = 2, Name = "Electrical Engineering", Description = "Department of Electrical and Electronics Engineering" },
            new Department { Id = 3, Name = "Mechanical Engineering", Description = "Department of Mechanical Engineering" }
        );

        // Seed default admin
        modelBuilder.Entity<Admin>().HasData(
            new Admin 
            { 
                Id = 1, 
                Username = "Admin", 
                Password = "$2a$11$APNew6ZOckEPwn6bofQLYO/NE1huf6VExTzyBLC7ZP4I9Oj.1krbO", // Default: "Admin123@"
                FirstName = "System",
                LastName = "Administrator",
                Email = "admin@university.edu",
                IsActive = true
            }
        );
    }
}