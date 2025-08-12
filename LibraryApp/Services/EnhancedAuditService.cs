using LibraryApp.Data;
using LibraryApp.Models;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace LibraryApp.Services
{
    public interface IEnhancedAuditService
    {
        Task LogActionAsync(string userId, UserRole userRole, string action, string entityType, int? entityId = null, object? oldValues = null, object? newValues = null, string? details = null);
        Task LogLoginAsync(string userId, UserRole userRole, string ipAddress, string userAgent);
        Task LogLogoutAsync(string userId, UserRole userRole);
        Task LogFailedLoginAsync(string userId, string ipAddress, string reason);
        Task LogDataAccessAsync(string userId, UserRole userRole, string dataType, string operation, string details = null);
        Task LogSystemEventAsync(string eventType, string description, object? metadata = null);
        Task<List<SystemAuditLog>> GetAuditLogsAsync(int page = 1, int pageSize = 50, string? userId = null, string? action = null, DateTime? startDate = null, DateTime? endDate = null);
        Task<Dictionary<string, object>> GetAuditStatisticsAsync(DateTime? startDate = null, DateTime? endDate = null);
        Task<List<SystemAuditLog>> GetUserActivityAsync(string userId, int days = 30);
        Task CleanupOldLogsAsync(int daysToKeep = 365);
    }

    public class EnhancedAuditService : IEnhancedAuditService
    {
        private readonly LibraryContext _context;
        private readonly ILogger<EnhancedAuditService> _logger;

        public EnhancedAuditService(LibraryContext context, ILogger<EnhancedAuditService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task LogActionAsync(string userId, UserRole userRole, string action, string entityType, int? entityId = null, object? oldValues = null, object? newValues = null, string? details = null)
        {
            try
            {
                var auditLog = new SystemAuditLog
                {
                    UserId = userId,
                    UserRole = userRole,
                    Action = action,
                    EntityType = entityType,
                    EntityId = entityId,
                    Details = CombineDetailsWithValues(details, oldValues, newValues),
                    Timestamp = DateTime.UtcNow,
                    IPAddress = GetCurrentIpAddress(),
                    UserAgent = GetCurrentUserAgent()
                };

                _context.SystemAuditLogs.Add(auditLog);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to log audit action for user {UserId}, action {Action}", userId, action);
            }
        }

        public async Task LogLoginAsync(string userId, UserRole userRole, string ipAddress, string userAgent)
        {
            await LogActionAsync(userId, userRole, "Login", "Authentication", null, null, new { IpAddress = ipAddress, UserAgent = userAgent }, "User logged in successfully");
        }

        public async Task LogLogoutAsync(string userId, UserRole userRole)
        {
            await LogActionAsync(userId, userRole, "Logout", "Authentication", null, null, null, "User logged out");
        }

        public async Task LogFailedLoginAsync(string userId, string ipAddress, string reason)
        {
            await LogActionAsync(userId, UserRole.Guest, "FailedLogin", "Authentication", null, null, new { IpAddress = ipAddress, Reason = reason }, $"Failed login attempt: {reason}");
        }

        public async Task LogDataAccessAsync(string userId, UserRole userRole, string dataType, string operation, string details = null)
        {
            await LogActionAsync(userId, userRole, $"DataAccess_{operation}", dataType, null, null, null, details ?? $"Accessed {dataType} data via {operation}");
        }

        public async Task LogSystemEventAsync(string eventType, string description, object? metadata = null)
        {
            await LogActionAsync("SYSTEM", UserRole.Admin, eventType, "System", null, null, metadata, description);
        }

        public async Task<List<SystemAuditLog>> GetAuditLogsAsync(int page = 1, int pageSize = 50, string? userId = null, string? action = null, DateTime? startDate = null, DateTime? endDate = null)
        {
            var query = _context.SystemAuditLogs.AsQueryable();

            if (!string.IsNullOrEmpty(userId))
            {
                query = query.Where(log => log.UserId.Contains(userId));
            }

            if (!string.IsNullOrEmpty(action))
            {
                query = query.Where(log => log.Action.Contains(action));
            }

            if (startDate.HasValue)
            {
                query = query.Where(log => log.Timestamp >= startDate.Value);
            }

            if (endDate.HasValue)
            {
                query = query.Where(log => log.Timestamp <= endDate.Value);
            }

            return await query
                .OrderByDescending(log => log.Timestamp)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        public async Task<Dictionary<string, object>> GetAuditStatisticsAsync(DateTime? startDate = null, DateTime? endDate = null)
        {
            startDate ??= DateTime.UtcNow.AddDays(-30);
            endDate ??= DateTime.UtcNow;

            var query = _context.SystemAuditLogs
                .Where(log => log.Timestamp >= startDate && log.Timestamp <= endDate);

            var totalLogs = await query.CountAsync();
            
            var actionStats = await query
                .GroupBy(log => log.Action)
                .Select(g => new { Action = g.Key, Count = g.Count() })
                .OrderByDescending(x => x.Count)
                .Take(10)
                .ToDictionaryAsync(x => x.Action, x => x.Count);

            var userRoleStats = await query
                .GroupBy(log => log.UserRole)
                .Select(g => new { Role = g.Key.ToString(), Count = g.Count() })
                .ToDictionaryAsync(x => x.Role, x => x.Count);

            var entityTypeStats = await query
                .Where(log => !string.IsNullOrEmpty(log.EntityType))
                .GroupBy(log => log.EntityType)
                .Select(g => new { EntityType = g.Key, Count = g.Count() })
                .OrderByDescending(x => x.Count)
                .Take(10)
                .ToDictionaryAsync(x => x.EntityType, x => x.Count);

            var dailyActivity = await query
                .GroupBy(log => log.Timestamp.Date)
                .Select(g => new { Date = g.Key, Count = g.Count() })
                .OrderBy(x => x.Date)
                .ToDictionaryAsync(x => x.Date.ToString("yyyy-MM-dd"), x => x.Count);

            var hourlyActivity = await query
                .GroupBy(log => log.Timestamp.Hour)
                .Select(g => new { Hour = g.Key, Count = g.Count() })
                .OrderBy(x => x.Hour)
                .ToDictionaryAsync(x => x.Hour.ToString(), x => x.Count);

            var topUsers = await query
                .Where(log => log.UserRole != UserRole.Guest)
                .GroupBy(log => log.UserId)
                .Select(g => new { UserId = g.Key, Count = g.Count() })
                .OrderByDescending(x => x.Count)
                .Take(10)
                .ToDictionaryAsync(x => x.UserId, x => x.Count);

            var failedLogins = await query
                .Where(log => log.Action == "FailedLogin")
                .CountAsync();

            var successfulLogins = await query
                .Where(log => log.Action == "Login")
                .CountAsync();

            return new Dictionary<string, object>
            {
                ["TotalLogs"] = totalLogs,
                ["ActionStatistics"] = actionStats,
                ["UserRoleStatistics"] = userRoleStats,
                ["EntityTypeStatistics"] = entityTypeStats,
                ["DailyActivity"] = dailyActivity,
                ["HourlyActivity"] = hourlyActivity,
                ["TopUsers"] = topUsers,
                ["FailedLogins"] = failedLogins,
                ["SuccessfulLogins"] = successfulLogins,
                ["LoginSuccessRate"] = successfulLogins + failedLogins > 0 ? 
                    (double)successfulLogins / (successfulLogins + failedLogins) * 100 : 0
            };
        }

        public async Task<List<SystemAuditLog>> GetUserActivityAsync(string userId, int days = 30)
        {
            var startDate = DateTime.UtcNow.AddDays(-days);
            
            return await _context.SystemAuditLogs
                .Where(log => log.UserId == userId && log.Timestamp >= startDate)
                .OrderByDescending(log => log.Timestamp)
                .ToListAsync();
        }

        public async Task CleanupOldLogsAsync(int daysToKeep = 365)
        {
            var cutoffDate = DateTime.UtcNow.AddDays(-daysToKeep);
            
            var oldLogs = _context.SystemAuditLogs
                .Where(log => log.Timestamp < cutoffDate);

            var count = await oldLogs.CountAsync();
            
            if (count > 0)
            {
                _context.SystemAuditLogs.RemoveRange(oldLogs);
                await _context.SaveChangesAsync();
                
                _logger.LogInformation("Cleaned up {Count} old audit logs older than {CutoffDate}", count, cutoffDate);
                
                // Log the cleanup action
                await LogSystemEventAsync("AuditCleanup", $"Cleaned up {count} audit logs older than {cutoffDate:yyyy-MM-dd}", new { Count = count, CutoffDate = cutoffDate });
            }
        }

        // Security-focused audit methods
        public async Task LogSecurityEventAsync(string userId, UserRole userRole, string eventType, string description, object? metadata = null)
        {
            await LogActionAsync(userId, userRole, $"Security_{eventType}", "Security", null, null, metadata, description);
        }

        public async Task LogDataModificationAsync(string userId, UserRole userRole, string entityType, int entityId, object oldValues, object newValues, string? reason = null)
        {
            await LogActionAsync(userId, userRole, "DataModification", entityType, entityId, oldValues, newValues, reason);
        }

        public async Task LogDataDeletionAsync(string userId, UserRole userRole, string entityType, int entityId, object deletedData, string? reason = null)
        {
            await LogActionAsync(userId, userRole, "DataDeletion", entityType, entityId, deletedData, null, reason ?? "Data deleted");
        }

        public async Task LogPrivilegeEscalationAsync(string userId, UserRole oldRole, UserRole newRole, string performedBy)
        {
            await LogActionAsync(performedBy, UserRole.Admin, "PrivilegeEscalation", "UserRole", null, 
                new { UserId = userId, OldRole = oldRole }, 
                new { UserId = userId, NewRole = newRole }, 
                $"User role changed from {oldRole} to {newRole}");
        }

        public async Task<List<SystemAuditLog>> GetSecurityAlertsAsync(int days = 7)
        {
            var startDate = DateTime.UtcNow.AddDays(-days);
            
            return await _context.SystemAuditLogs
                .Where(log => log.Timestamp >= startDate && 
                    (log.Action.StartsWith("Security_") || 
                     log.Action == "FailedLogin" || 
                     log.Action == "PrivilegeEscalation" ||
                     log.Action.Contains("Unauthorized")))
                .OrderByDescending(log => log.Timestamp)
                .ToListAsync();
        }

        // Performance monitoring
        public async Task LogPerformanceMetricAsync(string metricName, double value, string? details = null)
        {
            await LogSystemEventAsync($"Performance_{metricName}", $"Performance metric: {metricName} = {value}", 
                new { MetricName = metricName, Value = value, Details = details });
        }

        public async Task LogSlowQueryAsync(string queryType, double executionTimeMs, string? query = null)
        {
            if (executionTimeMs > 1000) // Log queries taking more than 1 second
            {
                await LogSystemEventAsync("SlowQuery", $"Slow query detected: {queryType} took {executionTimeMs}ms", 
                    new { QueryType = queryType, ExecutionTime = executionTimeMs, Query = query });
            }
        }

        // Helper methods
        private string GetCurrentIpAddress()
        {
            // This would be implemented to get the actual IP address from HttpContext
            // For now, return a placeholder
            return "127.0.0.1";
        }

        private string GetCurrentUserAgent()
        {
            // This would be implemented to get the actual user agent from HttpContext
            // For now, return a placeholder
            return "Unknown";
        }

        private string CombineDetailsWithValues(string? details, object? oldValues, object? newValues)
        {
            var parts = new List<string>();
            
            if (!string.IsNullOrEmpty(details))
                parts.Add(details);
                
            if (oldValues != null)
                parts.Add($"Old: {JsonSerializer.Serialize(oldValues)}");
                
            if (newValues != null)
                parts.Add($"New: {JsonSerializer.Serialize(newValues)}");
                
            return string.Join(" | ", parts);
        }

        // Export functionality
        public async Task<byte[]> ExportAuditLogsAsync(DateTime startDate, DateTime endDate, string format = "csv")
        {
            var logs = await _context.SystemAuditLogs
                .Where(log => log.Timestamp >= startDate && log.Timestamp <= endDate)
                .OrderByDescending(log => log.Timestamp)
                .ToListAsync();

            if (format.ToLower() == "csv")
            {
                return ExportToCsv(logs);
            }
            else
            {
                return ExportToJson(logs);
            }
        }

        private byte[] ExportToCsv(List<SystemAuditLog> logs)
        {
            var csv = new List<string>
            {
                "Timestamp,UserId,UserRole,Action,EntityType,EntityId,Details,IPAddress"
            };

            foreach (var log in logs)
            {
                csv.Add($"{log.Timestamp:yyyy-MM-dd HH:mm:ss},{log.UserId},{log.UserRole},{log.Action},{log.EntityType},{log.EntityId},\"{log.Details}\",{log.IPAddress}");
            }

            return System.Text.Encoding.UTF8.GetBytes(string.Join("\n", csv));
        }

        private byte[] ExportToJson(List<SystemAuditLog> logs)
        {
            var json = JsonSerializer.Serialize(logs, new JsonSerializerOptions { WriteIndented = true });
            return System.Text.Encoding.UTF8.GetBytes(json);
        }
    }
}