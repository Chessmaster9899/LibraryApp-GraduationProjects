namespace LibraryApp.Models
{
    public class UniversitySettings
    {
        public string Name { get; set; } = "University";
        public string ShortName { get; set; } = "UNI";
        public string ApplicationTitle { get; set; } = "Graduation Projects Library";
        public string TagLine { get; set; } = "Manage and track graduation projects, students, and professors";
        public string LogoPath { get; set; } = "/images/university/logo.png";
        public string FaviconPath { get; set; } = "/images/university/favicon.ico";
        public string FooterText { get; set; } = "University Library System";
        public string ContactEmail { get; set; } = "library@university.edu";
        public string ContactPhone { get; set; } = "+1-555-123-4567";
        public ThemeColors Colors { get; set; } = new ThemeColors();
    }

    public class ThemeColors
    {
        public string Primary { get; set; } = "#007bff";
        public string Secondary { get; set; } = "#6c757d";
        public string Success { get; set; } = "#28a745";
        public string Info { get; set; } = "#17a2b8";
        public string Warning { get; set; } = "#ffc107";
        public string Danger { get; set; } = "#dc3545";
        public string Light { get; set; } = "#f8f9fa";
        public string Dark { get; set; } = "#343a40";
        public string NavbarBg { get; set; } = "#ffffff";
        public string NavbarText { get; set; } = "#333333";
    }
}