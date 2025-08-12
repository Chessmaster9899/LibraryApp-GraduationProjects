using Microsoft.EntityFrameworkCore;
using LibraryApp.Data;
using LibraryApp.Services;
using LibraryApp.Middleware;
using LibraryApp.Hubs;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

// Add SignalR
builder.Services.AddSignalR();

// Add Session support
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

// Add Entity Framework
builder.Services.AddDbContext<LibraryContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection"))
        .ConfigureWarnings(warnings => warnings.Ignore(Microsoft.EntityFrameworkCore.Diagnostics.RelationalEventId.PendingModelChangesWarning)));

// Add University Settings Service
builder.Services.AddScoped<IUniversitySettingsService, UniversitySettingsService>();

// Add Authentication Service
builder.Services.AddScoped<IAuthenticationService, AuthenticationService>();

// Add Session Service
builder.Services.AddScoped<ISessionService, SessionService>();

// Add Notification Service
builder.Services.AddScoped<LibraryApp.Controllers.INotificationService, LibraryApp.Controllers.NotificationService>();

// Add Real-Time Notification Service
builder.Services.AddScoped<IRealTimeNotificationService, RealTimeNotificationService>();

// Add File Upload Service
builder.Services.AddScoped<IFileUploadService, FileUploadService>();

// Add Permission Service
builder.Services.AddScoped<IPermissionService, PermissionService>();

// Add Project Comment Service
builder.Services.AddScoped<IProjectCommentService, ProjectCommentService>();

var app = builder.Build();

// Migrate database and seed sample data
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<LibraryContext>();
    var authService = scope.ServiceProvider.GetRequiredService<IAuthenticationService>();
    var permissionService = scope.ServiceProvider.GetRequiredService<IPermissionService>();
    
    // Ensure the database is created and all migrations are applied
    await context.Database.MigrateAsync();
    
    // Initialize permissions and roles
    await permissionService.InitializeDefaultRolesAndPermissionsAsync();
    
    // Seed sample data
    await SeedDataService.SeedAsync(context, authService);
    
    // Ensure User entries exist for the permission system
    await SeedDataService.CreateUserEntriesForPermissionSystem(context);
}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    // Use custom error handling for production
    app.UseStatusCodePagesWithRedirects("/Error/{0}");
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}
else
{
    // Development error handling
    app.UseDeveloperExceptionPage();
    app.UseStatusCodePagesWithRedirects("/Error/{0}");
}

// Add global error handling middleware
app.UseGlobalErrorHandling();

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();
app.UseSession();

app.UseAuthorization();

// Map SignalR Hub
app.MapHub<NotificationHub>("/notificationHub");

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Auth}/{action=Login}/{id?}");

app.Run();
