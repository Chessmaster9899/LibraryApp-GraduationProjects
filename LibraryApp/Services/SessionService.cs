using LibraryApp.Models;

namespace LibraryApp.Services
{
    public interface ISessionService
    {
        string? GetUserId(HttpContext context);
        string? GetUserRole(HttpContext context);
        bool IsAuthenticated(HttpContext context);
        bool IsStudent(HttpContext context);
        bool IsProfessor(HttpContext context);
        bool IsAdmin(HttpContext context);
        void SetUserSession(HttpContext context, string userId, string userRole);
        void ClearUserSession(HttpContext context);
        SessionUserInfo? GetCurrentUser(HttpContext context);
    }

    public class SessionUserInfo
    {
        public string UserId { get; set; } = string.Empty;
        public string UserRole { get; set; } = string.Empty;
        public bool IsAuthenticated => !string.IsNullOrEmpty(UserId);
    }

    public class SessionService : ISessionService
    {
        private const string USER_ID_KEY = "UserId";
        private const string USER_ROLE_KEY = "UserRole";

        public string? GetUserId(HttpContext context)
        {
            return context.Session.GetString(USER_ID_KEY);
        }

        public string? GetUserRole(HttpContext context)
        {
            return context.Session.GetString(USER_ROLE_KEY);
        }

        public bool IsAuthenticated(HttpContext context)
        {
            return !string.IsNullOrEmpty(GetUserId(context));
        }

        public bool IsStudent(HttpContext context)
        {
            return GetUserRole(context) == "Student";
        }

        public bool IsProfessor(HttpContext context)
        {
            return GetUserRole(context) == "Professor";
        }

        public bool IsAdmin(HttpContext context)
        {
            return GetUserRole(context) == "Admin";
        }

        public void SetUserSession(HttpContext context, string userId, string userRole)
        {
            context.Session.SetString(USER_ID_KEY, userId);
            context.Session.SetString(USER_ROLE_KEY, userRole);
        }

        public void ClearUserSession(HttpContext context)
        {
            context.Session.Remove(USER_ID_KEY);
            context.Session.Remove(USER_ROLE_KEY);
        }

        public SessionUserInfo? GetCurrentUser(HttpContext context)
        {
            var userId = GetUserId(context);
            var userRole = GetUserRole(context);

            if (string.IsNullOrEmpty(userId))
                return null;

            return new SessionUserInfo
            {
                UserId = userId,
                UserRole = userRole ?? string.Empty
            };
        }
    }
}