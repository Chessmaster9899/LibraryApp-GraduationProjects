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
            return settings;
        }
    }
}