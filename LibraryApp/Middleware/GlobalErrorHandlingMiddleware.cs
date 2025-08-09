using Microsoft.AspNetCore.Diagnostics;
using System.Net;

namespace LibraryApp.Middleware
{
    public class GlobalErrorHandlingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<GlobalErrorHandlingMiddleware> _logger;

        public GlobalErrorHandlingMiddleware(RequestDelegate next, ILogger<GlobalErrorHandlingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An unhandled exception occurred: {Message}", ex.Message);
                await HandleExceptionAsync(context, ex);
            }
        }

        private static async Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            var response = context.Response;
            response.ContentType = "text/html";

            var statusCode = exception switch
            {
                UnauthorizedAccessException => HttpStatusCode.Unauthorized,
                KeyNotFoundException => HttpStatusCode.NotFound,
                ArgumentException => HttpStatusCode.BadRequest,
                InvalidOperationException => HttpStatusCode.BadRequest,
                _ => HttpStatusCode.InternalServerError
            };

            response.StatusCode = (int)statusCode;

            // Store error details for the error page
            var feature = context.Features.Get<IExceptionHandlerFeature>();
            if (feature == null)
            {
                context.Features.Set<IExceptionHandlerFeature>(new ExceptionHandlerFeature
                {
                    Error = exception
                });
            }

            // Redirect to appropriate error page
            var errorPath = statusCode switch
            {
                HttpStatusCode.NotFound => "/Error/NotFound",
                HttpStatusCode.Unauthorized or HttpStatusCode.Forbidden => "/Error/AccessDenied",
                _ => "/Error"
            };

            context.Request.Path = errorPath;
            context.Request.QueryString = QueryString.Empty;

            // Let the error handling controller handle the rest
            await Task.CompletedTask;
        }
    }

    public class ExceptionHandlerFeature : IExceptionHandlerFeature
    {
        public Exception Error { get; set; } = null!;
    }

    public static class GlobalErrorHandlingMiddlewareExtensions
    {
        public static IApplicationBuilder UseGlobalErrorHandling(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<GlobalErrorHandlingMiddleware>();
        }
    }
}