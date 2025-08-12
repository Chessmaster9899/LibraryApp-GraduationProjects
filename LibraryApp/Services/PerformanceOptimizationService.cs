using Microsoft.Extensions.Caching.Memory;
using LibraryApp.Data;
using LibraryApp.Models;
using Microsoft.EntityFrameworkCore;

namespace LibraryApp.Services
{
    public interface IPerformanceOptimizationService
    {
        Task<T> GetCachedDataAsync<T>(string key, Func<Task<T>> factory, TimeSpan? expiration = null);
        void ClearCache(string key);
        void ClearCacheByPattern(string pattern);
        Task WarmupCacheAsync();
        Task<List<Project>> GetCachedProjectsAsync(int pageSize = 20, int page = 1);
        Task<Dictionary<string, int>> GetCachedDashboardStatsAsync();
        Task<List<Department>> GetCachedDepartmentsAsync();
        void InvalidateProjectCaches();
        void InvalidateStudentCaches();
        void InvalidateProfessorCaches();
        void InvalidateDepartmentCaches();
    }

    public class PerformanceOptimizationService : IPerformanceOptimizationService
    {
        private readonly IMemoryCache _cache;
        private readonly LibraryContext _context;
        private readonly ILogger<PerformanceOptimizationService> _logger;
        private readonly Dictionary<string, DateTime> _cacheKeys;
        private readonly SemaphoreSlim _semaphore;

        public PerformanceOptimizationService(
            IMemoryCache cache, 
            LibraryContext context, 
            ILogger<PerformanceOptimizationService> logger)
        {
            _cache = cache;
            _context = context;
            _logger = logger;
            _cacheKeys = new Dictionary<string, DateTime>();
            _semaphore = new SemaphoreSlim(1, 1);
        }

        public async Task<T> GetCachedDataAsync<T>(string key, Func<Task<T>> factory, TimeSpan? expiration = null)
        {
            if (_cache.TryGetValue(key, out T cachedValue))
            {
                return cachedValue;
            }

            await _semaphore.WaitAsync();
            try
            {
                // Double-check locking pattern
                if (_cache.TryGetValue(key, out cachedValue))
                {
                    return cachedValue;
                }

                var options = new MemoryCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = expiration ?? TimeSpan.FromMinutes(30),
                    SlidingExpiration = TimeSpan.FromMinutes(5),
                    Priority = CacheItemPriority.Normal
                };

                var data = await factory();
                _cache.Set(key, data, options);
                
                // Track cache keys for pattern clearing
                _cacheKeys[key] = DateTime.UtcNow;
                
                _logger.LogDebug("Cached data with key: {CacheKey}", key);
                return data;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error caching data with key: {CacheKey}", key);
                return await factory(); // Fallback to direct call
            }
            finally
            {
                _semaphore.Release();
            }
        }

        public void ClearCache(string key)
        {
            _cache.Remove(key);
            _cacheKeys.Remove(key);
            _logger.LogDebug("Cleared cache key: {CacheKey}", key);
        }

        public void ClearCacheByPattern(string pattern)
        {
            var keysToRemove = _cacheKeys.Keys.Where(k => k.Contains(pattern)).ToList();
            
            foreach (var key in keysToRemove)
            {
                _cache.Remove(key);
                _cacheKeys.Remove(key);
            }
            
            _logger.LogDebug("Cleared {Count} cache keys with pattern: {Pattern}", keysToRemove.Count, pattern);
        }

        public async Task WarmupCacheAsync()
        {
            _logger.LogInformation("Starting cache warmup...");
            
            try
            {
                // Warmup critical data
                await GetCachedDashboardStatsAsync();
                await GetCachedDepartmentsAsync();
                await GetCachedProjectsAsync(20, 1);
                
                // Warmup search data
                await GetCachedDataAsync("search:recent_projects", async () =>
                {
                    return await _context.Projects
                        .Include(p => p.ProjectStudents)
                            .ThenInclude(ps => ps.Student)
                        .Include(p => p.Supervisor)
                        .Where(p => p.IsPubliclyVisible)
                        .OrderByDescending(p => p.SubmissionDate)
                        .Take(100)
                        .ToListAsync();
                }, TimeSpan.FromHours(1));

                // Warmup analytics data
                await GetCachedDataAsync("analytics:monthly_completions", async () =>
                {
                    return await _context.Projects
                        .Where(p => p.Status == ProjectStatus.ReviewApproved || p.Status == ProjectStatus.Defended)
                        .Where(p => p.DefenseDate.HasValue && p.DefenseDate >= DateTime.Now.AddYears(-1))
                        .GroupBy(p => new { p.DefenseDate!.Value.Year, p.DefenseDate!.Value.Month })
                        .ToDictionaryAsync(
                            g => $"{g.Key.Year}-{g.Key.Month:D2}",
                            g => g.Count());
                }, TimeSpan.FromHours(6));

                _logger.LogInformation("Cache warmup completed successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during cache warmup");
            }
        }

        public async Task<List<Project>> GetCachedProjectsAsync(int pageSize = 20, int page = 1)
        {
            var key = $"projects:page_{page}_size_{pageSize}";
            
            return await GetCachedDataAsync(key, async () =>
            {
                return await _context.Projects
                    .Include(p => p.ProjectStudents)
                        .ThenInclude(ps => ps.Student)
                            .ThenInclude(s => s.Department)
                    .Include(p => p.Supervisor)
                    .Where(p => p.IsPubliclyVisible)
                    .OrderByDescending(p => p.SubmissionDate)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();
            }, TimeSpan.FromMinutes(15));
        }

        public async Task<Dictionary<string, int>> GetCachedDashboardStatsAsync()
        {
            const string key = "dashboard:stats";
            
            return await GetCachedDataAsync(key, async () =>
            {
                var stats = new Dictionary<string, int>();
                
                // Run queries in parallel for better performance
                var totalProjectsTask = _context.Projects.CountAsync();
                var totalStudentsTask = _context.Students.CountAsync();
                var totalProfessorsTask = _context.Professors.CountAsync();
                var totalDepartmentsTask = _context.Departments.CountAsync();
                var pendingProjectsTask = _context.Projects.CountAsync(p => p.Status == ProjectStatus.Created);
                var completedProjectsTask = _context.Projects.CountAsync(p => p.Status == ProjectStatus.ReviewApproved);
                var inProgressProjectsTask = _context.Projects.CountAsync(p => p.Status == ProjectStatus.InProgress);

                await Task.WhenAll(
                    totalProjectsTask, totalStudentsTask, totalProfessorsTask, 
                    totalDepartmentsTask, pendingProjectsTask, completedProjectsTask, inProgressProjectsTask);

                stats["TotalProjects"] = await totalProjectsTask;
                stats["TotalStudents"] = await totalStudentsTask;
                stats["TotalProfessors"] = await totalProfessorsTask;
                stats["TotalDepartments"] = await totalDepartmentsTask;
                stats["PendingProjects"] = await pendingProjectsTask;
                stats["CompletedProjects"] = await completedProjectsTask;
                stats["InProgressProjects"] = await inProgressProjectsTask;

                return stats;
            }, TimeSpan.FromMinutes(10));
        }

        public async Task<List<Department>> GetCachedDepartmentsAsync()
        {
            const string key = "departments:all";
            
            return await GetCachedDataAsync(key, async () =>
            {
                return await _context.Departments
                    .OrderBy(d => d.Name)
                    .ToListAsync();
            }, TimeSpan.FromHours(1));
        }

        // Cache invalidation methods for data updates
        public void InvalidateProjectCaches()
        {
            ClearCacheByPattern("projects:");
            ClearCacheByPattern("dashboard:");
            ClearCacheByPattern("analytics:");
            ClearCacheByPattern("search:");
        }

        public void InvalidateStudentCaches()
        {
            ClearCacheByPattern("students:");
            ClearCacheByPattern("dashboard:");
        }

        public void InvalidateProfessorCaches()
        {
            ClearCacheByPattern("professors:");
            ClearCacheByPattern("dashboard:");
        }

        public void InvalidateDepartmentCaches()
        {
            ClearCacheByPattern("departments:");
            ClearCacheByPattern("dashboard:");
        }

        // Performance monitoring
        public Dictionary<string, object> GetCacheStatistics()
        {
            return new Dictionary<string, object>
            {
                ["TotalCacheKeys"] = _cacheKeys.Count,
                ["CacheKeys"] = _cacheKeys.Keys.ToList(),
                ["OldestCacheEntry"] = _cacheKeys.Any() ? _cacheKeys.Values.Min() : (DateTime?)null,
                ["NewestCacheEntry"] = _cacheKeys.Any() ? _cacheKeys.Values.Max() : (DateTime?)null
            };
        }

        // Compression helpers for large data sets
        public async Task<T> GetCompressedCachedDataAsync<T>(string key, Func<Task<T>> factory, TimeSpan? expiration = null)
        {
            // For very large datasets, you could implement compression here
            // This is a placeholder for potential compression implementation
            return await GetCachedDataAsync(key, factory, expiration);
        }

        // Background cache refresh
        public async Task RefreshCacheInBackgroundAsync(string key, Func<Task<object>> factory)
        {
            _ = Task.Run(async () =>
            {
                try
                {
                    var data = await factory();
                    var options = new MemoryCacheEntryOptions
                    {
                        AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(30),
                        SlidingExpiration = TimeSpan.FromMinutes(5),
                        Priority = CacheItemPriority.Normal
                    };
                    
                    _cache.Set(key, data, options);
                    _cacheKeys[key] = DateTime.UtcNow;
                    
                    _logger.LogDebug("Background cache refresh completed for key: {CacheKey}", key);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error during background cache refresh for key: {CacheKey}", key);
                }
            });
        }
    }

    // Extension methods for easy cache invalidation in controllers
    public static class CacheInvalidationExtensions
    {
        public static void InvalidateRelatedCaches(this IPerformanceOptimizationService cacheService, string entityType)
        {
            switch (entityType.ToLower())
            {
                case "project":
                    cacheService.InvalidateProjectCaches();
                    break;
                case "student":
                    cacheService.InvalidateStudentCaches();
                    break;
                case "professor":
                    cacheService.InvalidateProfessorCaches();
                    break;
                case "department":
                    cacheService.InvalidateDepartmentCaches();
                    break;
            }
        }
    }
}