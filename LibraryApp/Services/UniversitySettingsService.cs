using LibraryApp.Models;

namespace LibraryApp.Services
{
    public interface IUniversitySettingsService
    {
        UniversitySettings GetSettings();
    }

    public class UniversitySettingsService : IUniversitySettingsService
    {
        private readonly IConfiguration _configuration;

        public UniversitySettingsService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public UniversitySettings GetSettings()
        {
            var settings = new UniversitySettings();
            _configuration.GetSection("UniversitySettings").Bind(settings);
            
            // Apply fallbacks for missing settings
            ValidateAndApplyFallbacks(settings);
            
            return settings;
        }
        
        private void ValidateAndApplyFallbacks(UniversitySettings settings)
        {
            // Apply fallbacks for essential settings
            if (string.IsNullOrWhiteSpace(settings.Name))
                settings.Name = "University";
                
            if (string.IsNullOrWhiteSpace(settings.ShortName))
                settings.ShortName = "UNI";
                
            if (string.IsNullOrWhiteSpace(settings.ApplicationTitle))
                settings.ApplicationTitle = "Graduation Projects Library";
                
            if (string.IsNullOrWhiteSpace(settings.TagLine))
                settings.TagLine = "Manage and track graduation projects, students, and supervisors";
                
            if (string.IsNullOrWhiteSpace(settings.LogoPath))
                settings.LogoPath = "/images/defaults/logo.svg";
                
            if (string.IsNullOrWhiteSpace(settings.FaviconPath))
                settings.FaviconPath = "/images/defaults/favicon.svg";
                
            if (string.IsNullOrWhiteSpace(settings.FooterText))
                settings.FooterText = $"{settings.Name} Library System";
                
            if (string.IsNullOrWhiteSpace(settings.ContactEmail))
                settings.ContactEmail = "library@university.edu";
                
            if (string.IsNullOrWhiteSpace(settings.ContactPhone))
                settings.ContactPhone = "+1-555-000-0000";
                
            // Apply color fallbacks
            if (settings.Colors == null)
                settings.Colors = new ThemeColors();
                
            if (string.IsNullOrWhiteSpace(settings.Colors.Primary))
                settings.Colors.Primary = "#007bff";
                
            if (string.IsNullOrWhiteSpace(settings.Colors.Secondary))
                settings.Colors.Secondary = "#6c757d";
                
            if (string.IsNullOrWhiteSpace(settings.Colors.Success))
                settings.Colors.Success = "#28a745";
                
            if (string.IsNullOrWhiteSpace(settings.Colors.Info))
                settings.Colors.Info = "#17a2b8";
                
            if (string.IsNullOrWhiteSpace(settings.Colors.Warning))
                settings.Colors.Warning = "#ffc107";
                
            if (string.IsNullOrWhiteSpace(settings.Colors.Danger))
                settings.Colors.Danger = "#dc3545";
                
            if (string.IsNullOrWhiteSpace(settings.Colors.Light))
                settings.Colors.Light = "#f8f9fa";
                
            if (string.IsNullOrWhiteSpace(settings.Colors.Dark))
                settings.Colors.Dark = "#343a40";
                
            if (string.IsNullOrWhiteSpace(settings.Colors.NavbarBg))
                settings.Colors.NavbarBg = "#ffffff";
                
            if (string.IsNullOrWhiteSpace(settings.Colors.NavbarText))
                settings.Colors.NavbarText = "#333333";
        }
    }
}