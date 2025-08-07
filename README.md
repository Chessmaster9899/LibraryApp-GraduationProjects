# LibraryApp - University Graduation Projects Management System

A comprehensive ASP.NET Core MVC application for managing university graduation projects, students, supervisors, and academic workflows. Features complete file upload functionality, advanced search capabilities, full university branding customization, and enhanced user authentication with role-based dashboards.

## 🚀 Quick Start - Local Development Setup

### Prerequisites
- .NET 8.0 SDK or later ([Download here](https://dotnet.microsoft.com/download/dotnet/8.0))
- A code editor (Visual Studio, VS Code, or any preferred IDE)
- Git (for cloning the repository)

### Setup Instructions

#### 1. Clone the Repository
```bash
git clone https://github.com/Chessmaster9899/LibraryApp-GraduationProjects.git
cd LibraryApp-GraduationProjects
```

#### 2. Navigate to the Application Directory
```bash
cd LibraryApp
```

#### 3. Restore Dependencies
```bash
dotnet restore
```

#### 4. Run the Application
```bash
dotnet run
```

The application will start and be available at:
- **HTTPS**: `https://localhost:7105`
- **HTTP**: `http://localhost:5105`

#### 5. Access the Application
Open your web browser and navigate to `https://localhost:7105` to start using the LibraryApp.

### That's It! 🎉
The application comes with:
- ✅ **Pre-configured SQLite database** with sample data and automatic migration
- ✅ **Working file upload system** for project documents
- ✅ **Complete CRUD operations** for projects, students, and supervisors
- ✅ **Enhanced authentication system** with role-based navigation and sign-out functionality
- ✅ **Comprehensive dashboards** for Students, Professors, and Administrators
- ✅ **University branding system** ready for customization
- ✅ **Mobile-responsive design** with improved text visibility
- ✅ **Database migration safety** ensuring proper startup sequence

## 🎨 Customize for Your University

### Quick Branding Setup
1. **Edit University Settings**: Update `appsettings.json` with your university information
2. **Add Your Logo**: Place your logo as `wwwroot/images/university/logo.png` or `logo.svg`
3. **Add Favicon**: Place your favicon as `wwwroot/images/university/favicon.ico`
4. **Restart Application**: Run `dotnet run` again to see your changes

For complete customization instructions, see [UNIVERSITY-THEMING-GUIDE.md](UNIVERSITY-THEMING-GUIDE.md).

## 🔧 Features

### Core Functionality
- **Project Management**: Complete CRUD operations for graduation projects
- **Student Management**: Student profiles with academic information and personalized dashboards
- **Supervisor Management**: Faculty supervisor profiles with project supervision and evaluation tools
- **File Upload System**: Secure document upload for project files (PDF, Word, Text)
- **Advanced Search**: Title, abstract, keyword, and supervisor filtering
- **Status Tracking**: Project status management (In Progress, Completed, etc.)
- **Role-Based Dashboards**: Customized interfaces for Students, Professors, and Administrators

### Authentication & Navigation
- **Enhanced User Authentication**: Role-aware login system with secure session management
- **Smart Navigation Dropdown**: User identification with role display and quick access to:
  - Role-specific dashboards
  - Profile management and password change
  - **Universal Sign-Out button** accessible from all interfaces
- **Role-Based Access Control**: Different interfaces and permissions for Students, Professors, and Admins

### Dashboard Features
#### Student Dashboard
- **Projects Overview**: Visual project cards with filtering and statistics
- **Profile Management**: Editable profile information with account statistics
- **Project Tracking**: View assigned projects with supervisor information and status

#### Professor Dashboard  
- **Advanced Project Management**: Filtering by status with evaluation tools
- **Supervision Tools**: Separate views for supervised vs. evaluated projects
- **Student Contact Information**: Easy access to student details and communication

#### Administrator Dashboard
- **System Overview**: Complete statistics on projects, students, professors, and departments
- **User Management**: Full CRUD operations for all user types
- **Quick Actions**: Streamlined creation of new projects, students, and professors

### Technical Features
- **Responsive Design**: Mobile-first, works on all device sizes
- **University Branding**: Complete customization system for any university
- **Secure File Handling**: Automatic file cleanup and secure storage
- **Database Management**: SQLite with Entity Framework Core and automatic migration
- **Clean Architecture**: MVC pattern with service layer separation
- **Improved Text Visibility**: Enhanced CSS for better readability on background images

## 🏗️ Project Structure

```
LibraryApp/
├── Controllers/          # MVC Controllers
├── Data/                # Database context and migrations
├── Models/              # Data models and view models
├── Services/            # Business logic services
├── Views/               # Razor view templates
├── wwwroot/             # Static files (CSS, JS, images)
│   ├── css/             # Stylesheets
│   ├── images/          # Application images
│   │   ├── defaults/    # Default fallback assets
│   │   └── university/  # Your university assets
│   └── documents/       # Uploaded project documents
├── appsettings.json     # Configuration settings
└── Program.cs           # Application startup
```

## 🗄️ Database

The application uses SQLite for easy local development with enhanced startup reliability:
- **Database File**: `library.db` (automatically created)
- **Sample Data**: Pre-populated with example projects, students, and supervisors
- **Automatic Migration**: Database schema is automatically updated before seeding data
- **Startup Safety**: Ensures proper migration sequence to prevent startup errors

### Database Reset (if needed)
```bash
# Delete the database file to reset
rm library.db library.db-shm library.db-wal

# Run the application - database will be recreated with sample data
dotnet run
```

## 🌐 Production Deployment

### Prerequisites for Production
- Server with .NET 8.0 runtime
- Reverse proxy (IIS, Nginx, or Apache)
- SSL certificate for HTTPS

### Build for Production
```bash
dotnet publish -c Release -o ./publish
```

For detailed deployment instructions, see the deployment section in [UNIVERSITY-THEMING-GUIDE.md](UNIVERSITY-THEMING-GUIDE.md).

## 🛠️ Development

### Available Commands
```bash
# Run in development mode with hot reload
dotnet run

# Build the application
dotnet build

# Run tests (if any)
dotnet test

# Update database (after model changes)
dotnet ef database update

# Create new migration
dotnet ef migrations add MigrationName
```

### Adding New Features
1. Models go in `Models/`
2. Controllers go in `Controllers/`
3. Views go in `Views/ControllerName/`
4. Services go in `Services/`
5. Static files go in `wwwroot/`

## 📁 File Upload Configuration

The application supports uploading graduation project documents:
- **Supported Formats**: PDF, DOC, DOCX, TXT
- **Storage Location**: `wwwroot/documents/`
- **Security**: Files are renamed with unique identifiers
- **Cleanup**: Old files are automatically removed when projects are updated

## 🎯 Default Sample Data & Authentication

The application includes sample data for immediate testing:
- **1 Sample Project**: "AI-Based Student Performance Prediction System"
- **1 Sample Student**: John Smith (CS2025001)
- **3 Sample Supervisors**: Dr. Sarah Johnson, Prof. Michael Brown, Dr. Emily Davis
- **3 Sample Departments**: Computer Science, Electrical Engineering, Mechanical Engineering

### Default Login Credentials
Access the system with these pre-configured accounts:

**For Students & Professors:**
- Use your ID number and password
- **First-time login**: Password is first 2 letters of your name + last 4 digits of your ID

**For Admins:**
- Contact system administrator for credentials

### Authentication Features
- **Role-based dashboards** with personalized content
- **Secure session management** with automatic logout
- **User dropdown navigation** showing current user and role
- **Universal sign-out access** from all pages
- **Profile management** with password change functionality

## 🔧 Troubleshooting

### Common Issues

#### "Database is locked" Error
```bash
# Stop the application and delete lock files
rm library.db-shm library.db-wal
dotnet run
```

#### Port Already in Use
```bash
# Kill process using the port
sudo lsof -t -i tcp:5105 | xargs kill -9
# Or run on different port
dotnet run --urls="https://localhost:7106;http://localhost:5106"
```

#### Missing Dependencies
```bash
# Clean and restore
dotnet clean
dotnet restore
dotnet build
```

#### Authentication Issues
```bash
# Reset database to restore default users
rm library.db library.db-shm library.db-wal
dotnet run
```

#### Text Visibility Issues
The application includes enhanced CSS for text visibility on background images. If you experience readability issues:
- Check that custom CSS hasn't overridden the visibility fixes
- Ensure `site.css` includes the text shadow and color rules for non-card elements

## 📞 Support

- **Documentation**: See [UNIVERSITY-THEMING-GUIDE.md](UNIVERSITY-THEMING-GUIDE.md) for theming
- **Issues**: Report issues on the GitHub repository
- **Configuration**: All settings are in `appsettings.json`

## 🎓 Ready for Your University

This application is production-ready and can be immediately deployed at any university. The theming system makes it easy to customize with your university's branding, the comprehensive file management system handles all graduation project workflows, and the enhanced authentication provides secure, role-based access for all users.

### Key Benefits
- **Complete Project Lifecycle Management** from submission to completion
- **Role-Based Access Control** ensuring appropriate permissions for each user type
- **Enhanced User Experience** with intuitive dashboards and navigation
- **Robust Database Management** with automatic migrations and error prevention
- **Professional UI/UX** with improved text visibility and responsive design
- **Universal Sign-Out Access** ensuring secure session management
- **Comprehensive Documentation** for easy deployment and customization

**Start your local development now**: `dotnet run` and visit `https://localhost:7105`
