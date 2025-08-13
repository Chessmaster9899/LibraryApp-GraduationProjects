using Microsoft.AspNetCore.SignalR;
using LibraryApp.Services;

namespace LibraryApp.Hubs
{
    public class NotificationHub : Hub
    {
        private readonly ISessionService _sessionService;

        public NotificationHub(ISessionService sessionService)
        {
            _sessionService = sessionService;
        }

        public override async Task OnConnectedAsync()
        {
            var httpContext = Context.GetHttpContext();
            if (httpContext == null || !_sessionService.IsAuthenticated(httpContext))
            {
                // Disconnect if not authenticated
                Context.Abort();
                return;
            }

            var userId = _sessionService.GetUserId(httpContext);
            var userRole = _sessionService.GetUserRole(httpContext);

            if (!string.IsNullOrEmpty(userId))
            {
                // Add user to their specific group
                await Groups.AddToGroupAsync(Context.ConnectionId, $"User_{userId}");
                
                // Add user to role-based groups
                if (!string.IsNullOrEmpty(userRole))
                {
                    await Groups.AddToGroupAsync(Context.ConnectionId, $"Role_{userRole}");
                }

                // Notify user they're connected
                await Clients.Caller.SendAsync("Connected", new
                {
                    message = "Real-time notifications enabled",
                    timestamp = DateTime.UtcNow
                });
            }

            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            var httpContext = Context.GetHttpContext();
            if (httpContext != null)
            {
                var userId = _sessionService.GetUserId(httpContext);
                var userRole = _sessionService.GetUserRole(httpContext);

                if (!string.IsNullOrEmpty(userId))
                {
                    await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"User_{userId}");
                    
                    if (!string.IsNullOrEmpty(userRole))
                    {
                        await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"Role_{userRole}");
                    }
                }
            }

            await base.OnDisconnectedAsync(exception);
        }

        // Method for users to join specific project groups
        public async Task JoinProjectGroup(int projectId)
        {
            var httpContext = Context.GetHttpContext();
            if (httpContext == null || !_sessionService.IsAuthenticated(httpContext))
            {
                return;
            }

            await Groups.AddToGroupAsync(Context.ConnectionId, $"Project_{projectId}");
        }

        public async Task LeaveProjectGroup(int projectId)
        {
            var httpContext = Context.GetHttpContext();
            if (httpContext == null || !_sessionService.IsAuthenticated(httpContext))
            {
                return;
            }

            await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"Project_{projectId}");
        }

        // Method for real-time status updates
        public async Task UpdateUserStatus(string status)
        {
            var httpContext = Context.GetHttpContext();
            if (httpContext == null || !_sessionService.IsAuthenticated(httpContext))
            {
                return;
            }

            var userId = _sessionService.GetUserId(httpContext);
            if (!string.IsNullOrEmpty(userId))
            {
                await Clients.Others.SendAsync("UserStatusChanged", new
                {
                    userId = userId,
                    status = status,
                    timestamp = DateTime.UtcNow
                });
            }
        }
    }
}