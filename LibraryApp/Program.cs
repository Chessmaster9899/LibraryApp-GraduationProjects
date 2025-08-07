using Microsoft.EntityFrameworkCore;
using LibraryApp.Data;
using LibraryApp.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

// Add Session support
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

// Add Entity Framework
builder.Services.AddDbContext<LibraryContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

// Add University Settings Service
builder.Services.AddScoped<IUniversitySettingsService, UniversitySettingsService>();

// Add Authentication Service
builder.Services.AddScoped<IAuthenticationService, AuthenticationService>();

var app = builder.Build();

// Migrate database and seed sample data
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<LibraryContext>();
    var authService = scope.ServiceProvider.GetRequiredService<IAuthenticationService>();
    
    // Ensure the database is created and all migrations are applied
    await context.Database.MigrateAsync();
    
    // Seed sample data
    await SeedDataService.SeedAsync(context, authService);
}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();
app.UseSession();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Auth}/{action=Login}/{id?}");

app.Run();
