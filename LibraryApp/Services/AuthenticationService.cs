using LibraryApp.Models;
using Microsoft.EntityFrameworkCore;

namespace LibraryApp.Services;

public interface IAuthenticationService
{
    Task<AuthenticationResult> AuthenticateAsync(string userId, string password);
    string GenerateDefaultPassword(string firstName, string idNumber);
    string HashPassword(string password);
    bool VerifyPassword(string password, string hash);
    Task<bool> ChangePasswordAsync(string userId, string newPassword);
}

public class AuthenticationResult
{
    public bool Success { get; set; }
    public string? Message { get; set; }
    public UserRole? Role { get; set; }
    public int? EntityId { get; set; } // Student ID, Professor ID, or Admin ID
    public string? DisplayName { get; set; }
    public bool MustChangePassword { get; set; }
}

public class AuthenticationService : IAuthenticationService
{
    private readonly LibraryApp.Data.LibraryContext _context;

    public AuthenticationService(LibraryApp.Data.LibraryContext context)
    {
        _context = context;
    }

    public async Task<AuthenticationResult> AuthenticateAsync(string userId, string password)
    {
        // Check admin first
        if (userId == "Admin")
        {
            var admin = await _context.Admins.FirstOrDefaultAsync(a => a.Username == userId && a.IsActive);
            if (admin != null && VerifyPassword(password, admin.Password))
            {
                admin.LastLogin = DateTime.Now;
                await _context.SaveChangesAsync();
                
                return new AuthenticationResult
                {
                    Success = true,
                    Role = UserRole.Admin,
                    EntityId = admin.Id,
                    DisplayName = admin.FirstName ?? "Administrator",
                    MustChangePassword = false
                };
            }
        }

        // Check for guest access (special case)
        if (userId.ToLower() == "guest")
        {
            return new AuthenticationResult
            {
                Success = true,
                Role = UserRole.Guest,
                EntityId = 0,
                DisplayName = "Guest User",
                MustChangePassword = false
            };
        }

        // Check students
        var student = await _context.Students.FirstOrDefaultAsync(s => s.StudentNumber == userId);
        if (student != null && student.Password != null && VerifyPassword(password, student.Password))
        {
            student.LastLogin = DateTime.Now;
            await _context.SaveChangesAsync();
            
            return new AuthenticationResult
            {
                Success = true,
                Role = UserRole.Student,
                EntityId = student.Id,
                DisplayName = student.FullName,
                MustChangePassword = student.MustChangePassword
            };
        }

        // Check professors
        var professor = await _context.Professors.FirstOrDefaultAsync(p => p.ProfessorId == userId);
        if (professor != null && professor.Password != null && VerifyPassword(password, professor.Password))
        {
            professor.LastLogin = DateTime.Now;
            await _context.SaveChangesAsync();
            
            return new AuthenticationResult
            {
                Success = true,
                Role = UserRole.Professor,
                EntityId = professor.Id,
                DisplayName = professor.DisplayName,
                MustChangePassword = professor.MustChangePassword
            };
        }

        return new AuthenticationResult
        {
            Success = false,
            Message = "Invalid credentials"
        };
    }

    public string GenerateDefaultPassword(string firstName, string idNumber)
    {
        if (string.IsNullOrEmpty(firstName) || string.IsNullOrEmpty(idNumber))
            return string.Empty;

        var firstTwoLetters = firstName.Length >= 2 ? firstName.Substring(0, 2).ToLower() : firstName.ToLower();
        var lastFourDigits = idNumber.Length >= 4 ? idNumber.Substring(idNumber.Length - 4) : idNumber;
        
        return firstTwoLetters + lastFourDigits;
    }

    public string HashPassword(string password)
    {
        return BCrypt.Net.BCrypt.HashPassword(password);
    }

    public bool VerifyPassword(string password, string hash)
    {
        try
        {
            return BCrypt.Net.BCrypt.Verify(password, hash);
        }
        catch
        {
            return false;
        }
    }

    public async Task<bool> ChangePasswordAsync(string userId, string newPassword)
    {
        var hashedPassword = HashPassword(newPassword);

        // Check students
        var student = await _context.Students.FirstOrDefaultAsync(s => s.StudentNumber == userId);
        if (student != null)
        {
            student.Password = hashedPassword;
            student.MustChangePassword = false;
            await _context.SaveChangesAsync();
            return true;
        }

        // Check professors
        var professor = await _context.Professors.FirstOrDefaultAsync(p => p.ProfessorId == userId);
        if (professor != null)
        {
            professor.Password = hashedPassword;
            professor.MustChangePassword = false;
            await _context.SaveChangesAsync();
            return true;
        }

        // Check admin
        var admin = await _context.Admins.FirstOrDefaultAsync(a => a.Username == userId);
        if (admin != null)
        {
            admin.Password = hashedPassword;
            await _context.SaveChangesAsync();
            return true;
        }

        return false;
    }
}