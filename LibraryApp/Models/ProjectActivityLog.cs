using System.ComponentModel.DataAnnotations;

namespace LibraryApp.Models;

public class ProjectActivityLog
{
    public int Id { get; set; }
    
    [Required]
    public int ProjectId { get; set; }
    
    [Required]
    [StringLength(500)]
    public string Activity { get; set; } = "";
    
    [StringLength(1000)]
    public string? Details { get; set; }
    
    [Required]
    [StringLength(100)]
    public string PerformedBy { get; set; } = "";
    
    [Required]
    public DateTime PerformedAt { get; set; } = DateTime.UtcNow;
    
    // Navigation properties
    public Project Project { get; set; } = null!;
}