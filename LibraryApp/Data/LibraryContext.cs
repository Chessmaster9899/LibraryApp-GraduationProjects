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
    public DbSet<ProjectSubmission> ProjectSubmissions { get; set; }
    public DbSet<Notification> Notifications { get; set; }
    public DbSet<SystemAuditLog> SystemAuditLogs { get; set; }
    public DbSet<Announcement> Announcements { get; set; }
    
    // Permission System
    public DbSet<Permission> Permissions { get; set; }
    public DbSet<Role> Roles { get; set; }
    public DbSet<RolePermission> RolePermissions { get; set; }
    public DbSet<UserRoleAssignment> UserRoles { get; set; }
    public DbSet<UserPermission> UserPermissions { get; set; }

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

        // Configure ProjectSubmission relationships
        modelBuilder.Entity<ProjectSubmission>()
            .HasOne(ps => ps.Project)
            .WithMany()
            .HasForeignKey(ps => ps.ProjectId)
            .OnDelete(DeleteBehavior.Cascade);

        // Configure indexes for new models
        modelBuilder.Entity<Notification>()
            .HasIndex(n => new { n.UserId, n.UserRole, n.IsRead });

        modelBuilder.Entity<SystemAuditLog>()
            .HasIndex(a => new { a.UserId, a.Timestamp });

        modelBuilder.Entity<Announcement>()
            .HasIndex(a => new { a.IsActive, a.CreatedDate });

        modelBuilder.Entity<ProjectSubmission>()
            .HasIndex(ps => new { ps.ProjectId, ps.Status });

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

        // Configure Permission System relationships
        modelBuilder.Entity<RolePermission>()
            .HasOne(rp => rp.Role)
            .WithMany(r => r.RolePermissions)
            .HasForeignKey(rp => rp.RoleId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<RolePermission>()
            .HasOne(rp => rp.Permission)
            .WithMany(p => p.RolePermissions)
            .HasForeignKey(rp => rp.PermissionId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<UserRoleAssignment>()
            .HasOne(ur => ur.User)
            .WithMany()
            .HasForeignKey(ur => ur.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<UserRoleAssignment>()
            .HasOne(ur => ur.Role)
            .WithMany(r => r.UserRoles)
            .HasForeignKey(ur => ur.RoleId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<UserPermission>()
            .HasOne(up => up.User)
            .WithMany()
            .HasForeignKey(up => up.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<UserPermission>()
            .HasOne(up => up.Permission)
            .WithMany(p => p.UserPermissions)
            .HasForeignKey(up => up.PermissionId)
            .OnDelete(DeleteBehavior.Cascade);

        // Configure indexes for permission system
        modelBuilder.Entity<RolePermission>()
            .HasIndex(rp => new { rp.RoleId, rp.PermissionId })
            .IsUnique();

        modelBuilder.Entity<UserRoleAssignment>()
            .HasIndex(ur => new { ur.UserId, ur.RoleId })
            .IsUnique();

        modelBuilder.Entity<UserPermission>()
            .HasIndex(up => new { up.UserId, up.PermissionId })
            .IsUnique();

        modelBuilder.Entity<Permission>()
            .HasIndex(p => p.Type)
            .IsUnique();
    }
}