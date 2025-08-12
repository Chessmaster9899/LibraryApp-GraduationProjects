using System.ComponentModel.DataAnnotations;

namespace LibraryApp.Models;

public class ProjectStudent
{
    public int ProjectId { get; set; }
    public int StudentId { get; set; }

    // Navigation properties
    public Project Project { get; set; } = null!;
    public Student Student { get; set; } = null!;

    [Display(Name = "Join Date")]
    public DateTime JoinDate { get; set; } = DateTime.Now;
}