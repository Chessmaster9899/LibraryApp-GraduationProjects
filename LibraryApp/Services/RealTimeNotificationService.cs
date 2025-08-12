using Microsoft.AspNetCore.SignalR;
using LibraryApp.Hubs;

namespace LibraryApp.Services
{
    public interface IRealTimeNotificationService
    {
        Task SendNotificationToUserAsync(string userId, string title, string message, string type = "info");
        Task SendNotificationToRoleAsync(string role, string title, string message, string type = "info");
        Task SendProjectUpdateAsync(int projectId, string title, string message, string type = "info");
        Task SendBroadcastNotificationAsync(string title, string message, string type = "info");
        Task NotifyProjectStatusChangeAsync(int projectId, string newStatus, string studentName);
        Task NotifyNewProjectSubmissionAsync(int projectId, string projectTitle, string studentName);
        Task NotifyCommentAddedAsync(int projectId, string commenterName, string commentText);
    }

    public class RealTimeNotificationService : IRealTimeNotificationService
    {
        private readonly IHubContext<NotificationHub> _hubContext;

        public RealTimeNotificationService(IHubContext<NotificationHub> hubContext)
        {
            _hubContext = hubContext;
        }

        public async Task SendNotificationToUserAsync(string userId, string title, string message, string type = "info")
        {
            await _hubContext.Clients.Group($"User_{userId}").SendAsync("ReceiveNotification", new
            {
                title = title,
                message = message,
                type = type,
                timestamp = DateTime.UtcNow,
                id = Guid.NewGuid().ToString()
            });
        }

        public async Task SendNotificationToRoleAsync(string role, string title, string message, string type = "info")
        {
            await _hubContext.Clients.Group($"Role_{role}").SendAsync("ReceiveNotification", new
            {
                title = title,
                message = message,
                type = type,
                timestamp = DateTime.UtcNow,
                id = Guid.NewGuid().ToString()
            });
        }

        public async Task SendProjectUpdateAsync(int projectId, string title, string message, string type = "info")
        {
            await _hubContext.Clients.Group($"Project_{projectId}").SendAsync("ReceiveProjectUpdate", new
            {
                projectId = projectId,
                title = title,
                message = message,
                type = type,
                timestamp = DateTime.UtcNow,
                id = Guid.NewGuid().ToString()
            });
        }

        public async Task SendBroadcastNotificationAsync(string title, string message, string type = "info")
        {
            await _hubContext.Clients.All.SendAsync("ReceiveNotification", new
            {
                title = title,
                message = message,
                type = type,
                timestamp = DateTime.UtcNow,
                id = Guid.NewGuid().ToString(),
                isBroadcast = true
            });
        }

        public async Task NotifyProjectStatusChangeAsync(int projectId, string newStatus, string studentName)
        {
            var title = "Project Status Updated";
            var message = $"{studentName}'s project status has been changed to {newStatus}";
            
            // Notify admins and professors
            await SendNotificationToRoleAsync("Admin", title, message, "info");
            await SendNotificationToRoleAsync("Professor", title, message, "info");
            
            // Notify project watchers
            await SendProjectUpdateAsync(projectId, title, message, "info");
        }

        public async Task NotifyNewProjectSubmissionAsync(int projectId, string projectTitle, string studentName)
        {
            var title = "New Project Submitted";
            var message = $"{studentName} has submitted a new project: {projectTitle}";
            
            // Notify admins and professors
            await SendNotificationToRoleAsync("Admin", title, message, "success");
            await SendNotificationToRoleAsync("Professor", title, message, "success");
        }

        public async Task NotifyCommentAddedAsync(int projectId, string commenterName, string commentText)
        {
            var title = "New Comment Added";
            var message = $"{commenterName} added a comment: {commentText.Substring(0, Math.Min(100, commentText.Length))}...";
            
            // Notify project watchers
            await SendProjectUpdateAsync(projectId, title, message, "info");
        }
    }
}