# LibraryApp - University Graduation Projects Management System

A comprehensive, elegant ASP.NET Core MVC application for managing university graduation projects, students, supervisors, and academic workflows. Features complete file upload functionality, advanced search capabilities, full university branding customization, enhanced user authentication with role-based dashboards, comprehensive error handling, and modern UI/UX design.

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
- ✅ **Working file upload system** for project documents and posters
- ✅ **Complete CRUD operations** for projects, students, and supervisors
- ✅ **Enhanced authentication system** with role-based navigation and sign-out functionality
- ✅ **Comprehensive dashboards** for Students, Professors, and Administrators
- ✅ **University branding system** ready for customization
- ✅ **Mobile-responsive design** with improved text visibility
- ✅ **Database migration safety** ensuring proper startup sequence
- ✅ **Elegant error handling** with user-friendly error pages
- ✅ **Modern UI/UX** with professional typography and animations

## 🎨 Customize for Your University

### Quick Branding Setup
1. **Edit University Settings**: Update `appsettings.json` with your university information
2. **Add Your Logo**: Place your logo as `wwwroot/images/university/logo.png` or `logo.svg`
3. **Add Favicon**: Place your favicon as `wwwroot/images/university/favicon.ico`
4. **Restart Application**: Run `dotnet run` again to see your changes

For complete customization instructions, see [UNIVERSITY-THEMING-GUIDE.md](UNIVERSITY-THEMING-GUIDE.md).

## 🔧 Features Overview

### Core Functionality
- **Project Management**: Complete CRUD operations for graduation projects with status tracking
- **Student Management**: Student profiles with academic information and personalized dashboards
- **Supervisor Management**: Faculty supervisor profiles with project supervision and evaluation tools
- **File Upload System**: Secure document and poster upload for project files (PDF, Word, Text, Images)
- **Advanced Search**: Title, abstract, keyword, and supervisor filtering with real-time results
- **Status Tracking**: Comprehensive project status management (In Progress, Completed, Under Review, etc.)
- **Role-Based Dashboards**: Customized interfaces for Students, Professors, and Administrators

### Authentication & Security
- **Enhanced User Authentication**: Role-aware login system with secure session management
- **Smart Navigation**: Role-specific navigation with user identification and quick access
- **Role-Based Access Control**: Different interfaces and permissions for Students, Professors, and Admins
- **Session Security**: Automatic logout and secure session handling
- **Password Management**: Change password functionality with validation
- **Access Control**: Prevents unauthorized access to restricted resources

### Dashboard Features

#### Student Dashboard
- **Projects Overview**: Visual project cards with filtering and statistics
- **My Projects**: Comprehensive project management with file upload capabilities
- **Profile Management**: Editable profile information with account statistics
- **Project Tracking**: View assigned projects with supervisor information and status
- **File Management**: Upload and manage project documents and posters

#### Professor Dashboard  
- **Advanced Project Management**: Filtering by status with evaluation tools
- **Supervision Tools**: Separate views for supervised vs. evaluated projects
- **Student Contact Information**: Easy access to student details and communication
- **Project Editing**: Edit and manage projects for supervised students
- **File Management**: Upload and manage files for student projects

#### Administrator Dashboard
- **System Overview**: Complete statistics on projects, students, professors, and departments
- **User Management**: Full CRUD operations for all user types
- **Quick Actions**: Streamlined creation of new projects, students, and professors
- **System Monitoring**: View application usage and status

### Error Handling & User Experience
- **Elegant Error Pages**: Custom-designed error pages for different scenarios (404, 403, 500, etc.)
- **User-Friendly Messages**: Clear, helpful error messages and guidance
- **Global Error Handling**: Comprehensive error catching and logging
- **Toast Notifications**: Real-time success, warning, and error notifications
- **Form Validation**: Client and server-side validation with elegant feedback
- **Recovery Options**: Multiple ways to recover from errors

### Modern UI/UX Design
- **Professional Typography**: Google Fonts (Inter & Poppins) for elegant readability
- **Modern Color System**: Sophisticated color palette with CSS custom properties
- **Responsive Design**: Mobile-first approach that works on all device sizes
- **Smooth Animations**: Subtle transitions and hover effects for better user experience
- **Accessibility Features**: High contrast support, focus indicators, and screen reader compatibility
- **Loading States**: Visual feedback during operations
- **Interactive Components**: Enhanced buttons, forms, cards, and navigation

### Technical Features
- **Modern CSS Architecture**: CSS custom properties, modern layouts, and responsive design
- **Enhanced Forms**: Floating labels, file upload styling, and validation feedback
- **Toast Notification System**: JavaScript-powered notifications with auto-dismiss
- **University Branding**: Complete customization system for any university
- **Secure File Handling**: Automatic file cleanup and secure storage
- **Database Management**: SQLite with Entity Framework Core and automatic migration
- **Clean Architecture**: MVC pattern with service layer separation
- **Performance Optimized**: Efficient queries and optimized assets

## 🏗️ Project Structure

```
LibraryApp/
├── Attributes/           # Custom validation attributes
├── Controllers/          # MVC Controllers
│   ├── Api/             # API controllers
│   ├── AdminController.cs
│   ├── AuthController.cs
│   ├── ErrorController.cs   # NEW: Error handling
│   ├── StudentController.cs
│   └── ProfessorController.cs
├── Data/                # Database context and migrations
├── Middleware/          # NEW: Global error handling middleware
├── Models/              # Data models and view models
├── Services/            # Business logic services
├── Views/               # Razor view templates
│   ├── Shared/
│   │   ├── Error.cshtml       # Enhanced error page
│   │   ├── NotFound.cshtml    # NEW: 404 error page
│   │   ├── AccessDenied.cshtml # NEW: 403 error page
│   │   └── _Layout.cshtml     # Enhanced navigation
│   ├── Student/         # Student-specific views
│   └── Professor/       # Professor-specific views
├── wwwroot/             # Static files (CSS, JS, images)
│   ├── css/
│   │   └── site.css     # Enhanced modern styling
│   ├── js/
│   │   └── site.js      # Enhanced toast notifications
│   ├── images/          # Application images
│   │   ├── defaults/    # Default fallback assets
│   │   └── university/  # Your university assets
│   └── documents/       # Uploaded project documents
├── appsettings.json     # Configuration settings
└── Program.cs           # Application startup with error handling
```

## 🗄️ Database Schema

The application uses SQLite for easy local development with enhanced startup reliability:
- **Database File**: `library.db` (automatically created)
- **Tables**: Projects, Students, Professors, Departments, ProjectSubmissions
- **Sample Data**: Pre-populated with example projects, students, and supervisors
- **Automatic Migration**: Database schema is automatically updated before seeding data
- **Startup Safety**: Ensures proper migration sequence to prevent startup errors

### Database Entities
- **Students**: Complete academic profiles with authentication
- **Professors**: Faculty information with supervision capabilities
- **Projects**: Comprehensive project data with file attachments
- **Departments**: Academic department organization
- **Project Submissions**: File management for project documents and posters

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
6. Validation attributes go in `Attributes/`

## 📁 File Upload Configuration

The application supports uploading graduation project documents and posters:
- **Document Formats**: PDF, DOC, DOCX, TXT
- **Poster Formats**: JPG, PNG, GIF, PDF
- **Storage Location**: `wwwroot/documents/`
- **Security**: Files are renamed with unique identifiers
- **Cleanup**: Old files are automatically removed when projects are updated
- **Validation**: File type and size validation with user-friendly error messages

## 🎯 Default Sample Data & Authentication

The application includes sample data for immediate testing:
- **2 Sample Projects**: Including AI-based and IoT projects
- **3 Sample Students**: John Smith, Alice Johnson, and Bob Wilson
- **3 Sample Supervisors**: Dr. Sarah Johnson, Prof. Michael Brown, Dr. Emily Davis
- **3 Sample Departments**: Computer Science, Electrical Engineering, Mechanical Engineering

### Default Login Credentials
Access the system with these pre-configured accounts:

**For Students & Professors:**
- Use your ID number and password
- **First-time login**: Password is first 2 letters of your name + last 4 digits of your ID
- **Example**: Student CS2025001 (John Smith) → password: `Jo5001`

**For Admins:**
- Contact system administrator for credentials
- Default admin credentials are provided in the seeded data

### Authentication Features
- **Role-based dashboards** with personalized content and navigation
- **Secure session management** with automatic logout and session validation
- **User dropdown navigation** showing current user and role with quick access menu
- **Universal sign-out access** from all pages with confirmation
- **Profile management** with password change functionality and validation
- **Error handling** for authentication failures with helpful guidance

## 🎨 UI/UX Features

### Modern Design System
- **Typography**: Professional Google Fonts (Inter & Poppins) with optimized readability
- **Color Palette**: Sophisticated blue-based color system with semantic color usage
- **Animations**: Subtle hover effects, transitions, and loading states
- **Responsive Layout**: Mobile-first design that adapts to all screen sizes
- **Accessibility**: WCAG compliance with focus indicators and high contrast support

### Component Library
- **Enhanced Buttons**: Multiple variants with hover effects and loading states
- **Modern Forms**: Floating labels, elegant validation, and file upload styling
- **Professional Cards**: Hover effects, shadows, and organized content layout
- **Navigation**: Role-based menus with icons and smooth transitions
- **Tables**: Sortable, responsive tables with hover effects
- **Alerts & Toasts**: Contextual notifications with auto-dismiss functionality

### Interactive Features
- **Toast Notifications**: Real-time feedback for user actions
- **Loading States**: Visual feedback during operations
- **Form Enhancement**: Client-side validation and interactive elements
- **Progressive Enhancement**: Works with JavaScript disabled

## 🔧 Error Handling System

### Comprehensive Error Coverage
- **Global Error Handling**: Catches all unhandled exceptions
- **Custom Error Pages**: Designed for specific HTTP status codes
- **User-Friendly Messages**: Clear explanations and recovery options
- **Logging**: Detailed error logging for debugging

### Error Page Types
- **404 Not Found**: Custom page with navigation suggestions
- **403 Access Denied**: Role-specific guidance and alternatives
- **500 Server Error**: Professional error page with support contact
- **General Errors**: Fallback page for unexpected issues

### Error Recovery
- **Multiple Navigation Options**: Back button, home link, role-specific dashboards
- **Contact Information**: Support email and phone integration
- **Contextual Guidance**: Specific advice based on error type
- **Request ID Tracking**: For technical support and debugging

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

#### UI/Styling Issues
- Ensure custom CSS hasn't overridden the enhanced styling
- Check that Google Fonts are loading properly
- Verify Bootstrap and FontAwesome are included

#### File Upload Problems
- Check file permissions in `wwwroot/documents/`
- Verify file size limits in configuration
- Ensure supported file types are being used

## 📞 Support

- **Documentation**: See [UNIVERSITY-THEMING-GUIDE.md](UNIVERSITY-THEMING-GUIDE.md) for theming
- **Issues**: Report issues on the GitHub repository
- **Configuration**: All settings are in `appsettings.json`
- **Error Logs**: Check application logs for detailed error information

## 🎓 Ready for Your University

This application is production-ready and can be immediately deployed at any university. The comprehensive theming system makes it easy to customize with your university's branding, the file management system handles all graduation project workflows, the enhanced authentication provides secure role-based access for all users, and the modern UI ensures an excellent user experience.

### Key Benefits
- **Complete Project Lifecycle Management** from submission to completion with file handling
- **Role-Based Access Control** ensuring appropriate permissions for each user type
- **Enhanced User Experience** with intuitive dashboards, elegant error handling, and modern UI
- **Robust Database Management** with automatic migrations and error prevention
- **Professional UI/UX** with modern typography, animations, and responsive design
- **Comprehensive Error Handling** with user-friendly pages and recovery options
- **Universal Sign-Out Access** ensuring secure session management across all interfaces
- **Extensive Documentation** for easy deployment, customization, and maintenance

### Technical Highlights
- **Modern CSS Architecture** with custom properties and responsive design
- **Professional Typography** with Google Fonts integration
- **Elegant Animations** and micro-interactions for better UX
- **Comprehensive Validation** with custom attributes and user-friendly messages
- **Toast Notification System** for real-time user feedback
- **Global Error Handling** with detailed logging and recovery options
- **Accessibility Features** for inclusive design
- **Performance Optimized** with efficient queries and optimized assets

**Start your local development now**: `dotnet run` and visit `https://localhost:7105`
