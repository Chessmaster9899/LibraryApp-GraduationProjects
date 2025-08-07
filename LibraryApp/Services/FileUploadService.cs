using System.Text.RegularExpressions;
using LibraryApp.Models;

namespace LibraryApp.Services;

public interface IFileUploadService
{
    Task<FileUploadResult> UploadFileAsync(IFormFile file, string category, int? entityId = null);
    Task<bool> DeleteFileAsync(string filePath);
    Task<FileValidationResult> ValidateFileAsync(IFormFile file, string category);
    string GetFileUrl(string filePath);
    Task<List<FileInfo>> GetFilesForEntityAsync(string category, int entityId);
}

public class FileUploadResult
{
    public bool Success { get; set; }
    public string? FilePath { get; set; }
    public string? FileName { get; set; }
    public string? ErrorMessage { get; set; }
    public long FileSize { get; set; }
}

public class FileValidationResult
{
    public bool IsValid { get; set; }
    public List<string> Errors { get; set; } = new();
}

public class FileUploadService : IFileUploadService
{
    private readonly IWebHostEnvironment _environment;
    private readonly ILogger<FileUploadService> _logger;

    // File type configurations
    private readonly Dictionary<string, FileTypeConfig> _fileTypeConfigs = new()
    {
        ["poster"] = new FileTypeConfig
        {
            MaxSizeBytes = 10 * 1024 * 1024, // 10MB
            AllowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".pdf" },
            AllowedMimeTypes = new[] { "image/jpeg", "image/png", "application/pdf" },
            Directory = "posters"
        },
        ["report"] = new FileTypeConfig
        {
            MaxSizeBytes = 50 * 1024 * 1024, // 50MB
            AllowedExtensions = new[] { ".pdf", ".doc", ".docx" },
            AllowedMimeTypes = new[] { "application/pdf", "application/msword", "application/vnd.openxmlformats-officedocument.wordprocessingml.document" },
            Directory = "reports"
        },
        ["code"] = new FileTypeConfig
        {
            MaxSizeBytes = 100 * 1024 * 1024, // 100MB
            AllowedExtensions = new[] { ".zip", ".rar", ".7z", ".tar", ".gz" },
            AllowedMimeTypes = new[] { "application/zip", "application/x-rar-compressed", "application/x-7z-compressed", "application/x-tar", "application/gzip" },
            Directory = "code"
        },
        ["document"] = new FileTypeConfig
        {
            MaxSizeBytes = 25 * 1024 * 1024, // 25MB
            AllowedExtensions = new[] { ".pdf", ".doc", ".docx", ".txt", ".md" },
            AllowedMimeTypes = new[] { "application/pdf", "application/msword", "application/vnd.openxmlformats-officedocument.wordprocessingml.document", "text/plain", "text/markdown" },
            Directory = "documents"
        }
    };

    public FileUploadService(IWebHostEnvironment environment, ILogger<FileUploadService> logger)
    {
        _environment = environment;
        _logger = logger;
    }

    public async Task<FileUploadResult> UploadFileAsync(IFormFile file, string category, int? entityId = null)
    {
        try
        {
            // Validate file
            var validation = await ValidateFileAsync(file, category);
            if (!validation.IsValid)
            {
                return new FileUploadResult
                {
                    Success = false,
                    ErrorMessage = string.Join(", ", validation.Errors)
                };
            }

            var config = _fileTypeConfigs[category.ToLower()];
            var uploadsPath = Path.Combine(_environment.WebRootPath, "uploads", config.Directory);
            
            // Create directory if it doesn't exist
            Directory.CreateDirectory(uploadsPath);

            // Generate unique filename
            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
            var safeFileName = SanitizeFileName(Path.GetFileNameWithoutExtension(file.FileName));
            var uniqueFileName = entityId.HasValue 
                ? $"{category}_{entityId}_{DateTime.Now:yyyyMMdd_HHmmss}_{safeFileName}{extension}"
                : $"{category}_{DateTime.Now:yyyyMMdd_HHmmss}_{safeFileName}{extension}";

            var filePath = Path.Combine(uploadsPath, uniqueFileName);
            var relativeFilePath = $"/uploads/{config.Directory}/{uniqueFileName}";

            // Save file
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            _logger.LogInformation($"File uploaded successfully: {relativeFilePath}");

            return new FileUploadResult
            {
                Success = true,
                FilePath = relativeFilePath,
                FileName = uniqueFileName,
                FileSize = file.Length
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error uploading file: {file.FileName}");
            return new FileUploadResult
            {
                Success = false,
                ErrorMessage = "An error occurred while uploading the file."
            };
        }
    }

    public async Task<bool> DeleteFileAsync(string filePath)
    {
        try
        {
            if (string.IsNullOrEmpty(filePath))
                return false;

            var physicalPath = Path.Combine(_environment.WebRootPath, filePath.TrimStart('/'));
            
            if (File.Exists(physicalPath))
            {
                await Task.Run(() => File.Delete(physicalPath));
                _logger.LogInformation($"File deleted successfully: {filePath}");
                return true;
            }

            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error deleting file: {filePath}");
            return false;
        }
    }

    public async Task<FileValidationResult> ValidateFileAsync(IFormFile file, string category)
    {
        var result = new FileValidationResult { IsValid = true };

        if (file == null || file.Length == 0)
        {
            result.IsValid = false;
            result.Errors.Add("File is required.");
            return result;
        }

        var categoryLower = category.ToLower();
        if (!_fileTypeConfigs.ContainsKey(categoryLower))
        {
            result.IsValid = false;
            result.Errors.Add("Invalid file category.");
            return result;
        }

        var config = _fileTypeConfigs[categoryLower];

        // Check file size
        if (file.Length > config.MaxSizeBytes)
        {
            result.IsValid = false;
            result.Errors.Add($"File size exceeds the maximum allowed size of {config.MaxSizeBytes / (1024 * 1024)}MB.");
        }

        // Check file extension
        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (!config.AllowedExtensions.Contains(extension))
        {
            result.IsValid = false;
            result.Errors.Add($"File type '{extension}' is not allowed. Allowed types: {string.Join(", ", config.AllowedExtensions)}");
        }

        // Check MIME type
        if (!config.AllowedMimeTypes.Contains(file.ContentType.ToLowerInvariant()))
        {
            result.IsValid = false;
            result.Errors.Add($"File content type '{file.ContentType}' is not allowed.");
        }

        // Additional security checks
        var fileName = file.FileName.ToLowerInvariant();
        var dangerousPatterns = new[] { ".exe", ".bat", ".cmd", ".scr", ".js", ".vbs", ".ps1" };
        if (dangerousPatterns.Any(pattern => fileName.Contains(pattern)))
        {
            result.IsValid = false;
            result.Errors.Add("File contains potentially dangerous content.");
        }

        return await Task.FromResult(result);
    }

    public string GetFileUrl(string filePath)
    {
        return filePath?.StartsWith("/") == true ? filePath : $"/{filePath}";
    }

    public async Task<List<FileInfo>> GetFilesForEntityAsync(string category, int entityId)
    {
        try
        {
            var config = _fileTypeConfigs[category.ToLower()];
            var directoryPath = Path.Combine(_environment.WebRootPath, "uploads", config.Directory);
            
            if (!Directory.Exists(directoryPath))
                return new List<FileInfo>();

            var pattern = $"{category}_{entityId}_*";
            var files = Directory.GetFiles(directoryPath, pattern)
                .Select(f => new FileInfo(f))
                .OrderByDescending(f => f.CreationTime)
                .ToList();

            return await Task.FromResult(files);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error getting files for entity {entityId} in category {category}");
            return new List<FileInfo>();
        }
    }

    private string SanitizeFileName(string fileName)
    {
        // Remove invalid characters
        var invalidChars = Path.GetInvalidFileNameChars();
        var sanitized = new string(fileName.Where(c => !invalidChars.Contains(c)).ToArray());
        
        // Remove extra spaces and replace with underscores
        sanitized = Regex.Replace(sanitized, @"\s+", "_");
        
        // Limit length
        if (sanitized.Length > 50)
            sanitized = sanitized.Substring(0, 50);
            
        return sanitized;
    }
}

public class FileTypeConfig
{
    public long MaxSizeBytes { get; set; }
    public string[] AllowedExtensions { get; set; } = Array.Empty<string>();
    public string[] AllowedMimeTypes { get; set; } = Array.Empty<string>();
    public string Directory { get; set; } = string.Empty;
}

// Enhanced project models for file management
public class ProjectFile
{
    public int Id { get; set; }
    public int ProjectId { get; set; }
    public string FileName { get; set; } = string.Empty;
    public string FilePath { get; set; } = string.Empty;
    public string FileType { get; set; } = string.Empty; // poster, report, code, document
    public long FileSize { get; set; }
    public DateTime UploadDate { get; set; } = DateTime.Now;
    public string UploadedBy { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
    
    // Navigation property
    public Project Project { get; set; } = null!;
}