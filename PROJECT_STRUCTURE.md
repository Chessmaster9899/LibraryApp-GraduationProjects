# 🏗️ Project Structure Guide

This document provides a comprehensive overview of the LibraryApp project structure, helping developers understand the codebase organization and architectural decisions.

## 📁 Root Directory Structure

```
LibraryApp-GraduationProjects/
├── LibraryApp/                    # Main application directory
├── LibraryApp.sln                 # Visual Studio solution file
├── README.md                      # Main documentation
├── QUICK-START.md                 # Quick setup guide
├── PROJECT_STRUCTURE.md           # This file - project architecture
├── REQUIREMENTS.md                # Technical requirements
├── UNIVERSITY-THEMING-GUIDE.md    # Branding customization guide
├── appsettings.json              # Application configuration
├── setup-local.bat               # Windows setup script
├── setup-local.sh                # Linux/Mac setup script
└── .gitignore                    # Git ignore rules
```

## 🎯 Application Architecture (LibraryApp/)

### Core Application Structure

```
LibraryApp/
├── Controllers/                   # MVC Controllers - Request handling
│   ├── AdminController.cs         # Admin panel functionality
│   ├── HomeController.cs          # Public pages and authentication
│   ├── ProfessorController.cs     # Professor dashboard and management
│   ├── ProjectsController.cs      # Project CRUD operations
│   ├── StudentsController.cs      # Student management
│   ├── StudentController.cs       # Student dashboard
│   └── SupervisorsController.cs   # Supervisor management
├── Data/                         # Database layer
│   ├── ApplicationDbContext.cs   # Entity Framework context
│   └── Migrations/               # Database schema migrations
├── Models/                       # Data models and ViewModels
│   ├── Project.cs               # Project entity
│   ├── Student.cs               # Student entity
│   ├── Supervisor.cs            # Supervisor entity
│   ├── Department.cs            # Department entity
│   └── ViewModels/              # ViewModels for complex views
├── Services/                     # Business logic layer
│   ├── AuthService.cs           # Authentication and authorization
│   ├── SeedDataService.cs       # Database seeding
│   └── FileService.cs           # File upload management
├── Views/                       # Razor view templates
│   ├── Shared/                  # Shared layouts and partials
│   │   ├── _Layout.cshtml       # Main application layout
│   │   ├── _LoginPartial.cshtml # Authentication UI components
│   │   └── Error.cshtml         # Error pages
│   ├── Home/                    # Public pages
│   ├── Admin/                   # Administrator interface
│   ├── Student/                 # Student dashboard views
│   ├── Professor/               # Professor dashboard views
│   ├── Projects/                # Project management views
│   ├── Students/                # Student management views
│   └── Supervisors/             # Supervisor management views
├── wwwroot/                     # Static files and assets
│   ├── css/                     # Stylesheets
│   │   ├── site.css             # Main application styles
│   │   └── bootstrap.min.css    # Bootstrap framework
│   ├── js/                      # JavaScript files
│   │   ├── site.js              # Custom JavaScript
│   │   └── bootstrap.bundle.min.js # Bootstrap components
│   ├── images/                  # Application images
│   │   ├── defaults/            # Default fallback assets
│   │   │   ├── logo.png         # Default university logo
│   │   │   ├── favicon.ico      # Default favicon
│   │   │   └── background.png   # Default background image
│   │   └── university/          # Custom university assets
│   │       ├── logo.png         # Your university logo
│   │       ├── favicon.ico      # Your university favicon
│   │       └── background.png   # Custom background (optional)
│   └── documents/               # Uploaded project documents
├── appsettings.json             # Application configuration
├── appsettings.Development.json # Development-specific settings
├── Program.cs                   # Application entry point and configuration
├── LibraryApp.csproj           # Project file with dependencies
└── library.db                  # SQLite database file (auto-created)
```

## 🔧 Architecture Patterns

### MVC Pattern Implementation

**Controllers** (`Controllers/`)
- Handle HTTP requests and user input
- Coordinate between Models and Views
- Implement authentication and authorization
- Return appropriate Views or API responses

**Models** (`Models/`)
- Define data structures and business entities
- Include validation attributes
- Represent database tables through Entity Framework

**Views** (`Views/`)
- Razor templates for HTML generation
- Strongly-typed views using ViewModels
- Shared layouts for consistent UI
- Role-based view rendering

### Service Layer Pattern

**Services** (`Services/`)
- `AuthService.cs`: Handles authentication, authorization, and user management
- `SeedDataService.cs`: Manages database initialization and sample data
- `FileService.cs`: Handles secure file uploads and management

### Repository Pattern (via Entity Framework)

**Data Layer** (`Data/`)
- `ApplicationDbContext.cs`: Central database context
- Entity Framework Code-First approach
- Automatic migrations for schema updates

## 🎨 Frontend Architecture

### CSS Organization

```
wwwroot/css/
├── site.css                     # Main application styles
│   ├── Universal styles         # Base typography and layout
│   ├── Navigation styles        # Header and navigation
│   ├── Authentication styles    # Login forms and user dropdowns
│   ├── Dashboard styles         # Role-specific dashboard layouts
│   ├── Card components          # Project and user cards
│   ├── Form styles             # Input forms and buttons
│   ├── Text visibility fixes   # Background image text contrast
│   └── Responsive design       # Mobile-first responsive rules
└── bootstrap.min.css           # Bootstrap framework
```

### JavaScript Organization

```
wwwroot/js/
├── site.js                     # Custom application JavaScript
│   ├── Form validation         # Client-side validation
│   ├── Dynamic content         # AJAX operations
│   ├── UI interactions         # Dropdowns, modals, etc.
│   └── File upload handling    # Upload progress and validation
└── bootstrap.bundle.min.js     # Bootstrap components and interactions
```

## 🗄️ Database Schema

### Entity Relationships

```
Department (1) ←→ (Many) Student
Department (1) ←→ (Many) Supervisor  
Supervisor (1) ←→ (Many) Project
Student (1) ←→ (Many) Project (as author)
```

### Key Entities

**Project**
- Core entity for graduation projects
- Links to Student (author) and Supervisor
- Includes file upload functionality
- Status tracking and metadata

**Student** 
- Student information and academic details
- Authentication credentials
- Department association

**Supervisor**
- Faculty information and specializations  
- Authentication credentials
- Department association
- Project supervision capacity

**Department**
- Organizational structure
- Links to Students and Supervisors

## 🔐 Authentication Architecture

### Role-Based Access Control

**Student Role**
- Access to personal dashboard
- View all projects (read-only)
- Manage personal profile
- Upload project documents

**Professor Role**  
- Access to professor dashboard
- Manage supervised projects
- Evaluate assigned projects
- View student information

**Admin Role**
- Full system access
- User management (CRUD operations)
- Project management
- System configuration

### Authentication Flow

1. **Login** (`HomeController.Login`)
2. **Role Determination** (`AuthService`)
3. **Dashboard Redirect** (Role-specific)
4. **Session Management** (Automatic)
5. **Sign-Out** (Universal access via dropdown)

## 🚀 Deployment Architecture

### Development Setup
- SQLite database for local development
- Automatic database migration on startup
- Hot reload for development changes
- Debug configurations in `appsettings.Development.json`

### Production Considerations
- Database migration safety checks
- Secure file upload validation
- Environment-specific configurations
- Reverse proxy compatibility (IIS, Nginx, Apache)

## 📝 Configuration Files

### appsettings.json Structure

```json
{
  "UniversitySettings": {
    "Name": "University branding and identity",
    "Colors": "UI color scheme configuration",
    "ContactInfo": "University contact details"
  },
  "ConnectionStrings": {
    "DefaultConnection": "Database connection configuration"
  },
  "Logging": {
    "LogLevel": "Application logging configuration"
  }
}
```

## 🔄 Development Workflow

### Adding New Features

1. **Models**: Define/update entities in `Models/`
2. **Migration**: Create database migration with `dotnet ef migrations add`
3. **Services**: Implement business logic in `Services/`
4. **Controllers**: Add controller actions for HTTP handling
5. **Views**: Create Razor templates in appropriate `Views/` subdirectory
6. **Styles**: Add CSS in `wwwroot/css/site.css`
7. **Testing**: Test functionality across all user roles

### File Organization Best Practices

- **Controllers**: One controller per major entity/feature area
- **Views**: Organize by controller name in subdirectories
- **Models**: Separate entities from ViewModels
- **Services**: Single responsibility per service class
- **Static Assets**: Organize by type (css, js, images)

## 🛠️ Extensibility Points

### Adding New User Roles
1. Update `AuthService.cs` with role definitions
2. Add role-specific controllers and views
3. Update navigation in `_Layout.cshtml`
4. Implement authorization attributes

### Adding New File Types
1. Update file validation in `FileService.cs`
2. Add MIME type handling
3. Update upload UI in relevant views

### Custom Branding
1. Update `appsettings.json` university settings
2. Replace assets in `wwwroot/images/university/`
3. Customize CSS variables in `site.css`
4. Update email templates and text content

This architecture provides a solid foundation for a university graduation projects management system while maintaining flexibility for customization and extension.
