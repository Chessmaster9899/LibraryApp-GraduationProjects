using System.ComponentModel.DataAnnotations;

namespace LibraryApp.Models;

public enum UserRole
{
    Admin,
    Student,
    Professor,
    Guest
}

public class User
{
    public int Id { get; set; }
    
    [Required]
    public required string UserId { get; set; } // Student ID, Professor ID, or Admin username
    
    [Required]
    public required string Password { get; set; } // Hashed password
    
    [Required]
    public UserRole Role { get; set; }
    
    public bool MustChangePassword { get; set; } = true;
    
    public DateTime? LastLogin { get; set; }
    
    public bool IsActive { get; set; } = true;
    
    // Navigation properties for linking to actual entities
    public int? StudentId { get; set; }
    public Student? Student { get; set; }
    
    public int? ProfessorId { get; set; }
    public Professor? Professor { get; set; }
}

public class Admin
{
    public int Id { get; set; }
    
    [Required]
    public required string Username { get; set; }
    
    [Required]
    public required string Password { get; set; } // Hashed password
    
    [Display(Name = "First Name")]
    public string? FirstName { get; set; }
    
    [Display(Name = "Last Name")]
    public string? LastName { get; set; }
    
    [EmailAddress]
    public string? Email { get; set; }
    
    public DateTime? LastLogin { get; set; }
    
    public bool IsActive { get; set; } = true;
}