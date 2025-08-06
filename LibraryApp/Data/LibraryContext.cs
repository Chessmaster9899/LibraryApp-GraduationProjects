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
    public DbSet<Supervisor> Supervisors { get; set; }
    public DbSet<Project> Projects { get; set; }

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

        modelBuilder.Entity<Supervisor>()
            .HasIndex(s => s.Email)
            .IsUnique();

        // Configure foreign key relationships
        modelBuilder.Entity<Student>()
            .HasOne(s => s.Department)
            .WithMany(d => d.Students)
            .HasForeignKey(s => s.DepartmentId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Supervisor>()
            .HasOne(s => s.Department)
            .WithMany(d => d.Supervisors)
            .HasForeignKey(s => s.DepartmentId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Project>()
            .HasOne(p => p.Student)
            .WithMany(s => s.Projects)
            .HasForeignKey(p => p.StudentId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Project>()
            .HasOne(p => p.Supervisor)
            .WithMany(s => s.Projects)
            .HasForeignKey(p => p.SupervisorId)
            .OnDelete(DeleteBehavior.Restrict);

        // Seed some initial data
        modelBuilder.Entity<Department>().HasData(
            new Department { Id = 1, Name = "Computer Science", Description = "Department of Computer Science and Engineering" },
            new Department { Id = 2, Name = "Electrical Engineering", Description = "Department of Electrical and Electronics Engineering" },
            new Department { Id = 3, Name = "Mechanical Engineering", Description = "Department of Mechanical Engineering" }
        );
    }
}