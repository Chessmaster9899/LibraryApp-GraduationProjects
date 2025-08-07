# 🚀 Quick Start Guide - Run LibraryApp Locally

This guide helps you get the LibraryApp running on your local machine in just a few minutes.

## ✅ Prerequisites

- **✅ .NET 8.0 SDK** ([Download here](https://dotnet.microsoft.com/download/dotnet/8.0))
- **✅ Git** (for cloning the repository)
- **✅ A code editor** (Visual Studio, VS Code, or any IDE you prefer)

## 🏁 Super Quick Start (3 Steps)

### 1. Clone & Navigate
```bash
git clone https://github.com/Chessmaster9899/LibraryApp-GraduationProjects.git
cd LibraryApp-GraduationProjects
```

### 2. Run Setup Script
**On Windows:**
```cmd
setup-local.bat
```

**On Linux/Mac:**
```bash
chmod +x setup-local.sh
./setup-local.sh
```

### 3. Start the App
```bash
cd LibraryApp
dotnet run
```

**🎉 Done!** Open your browser to: `https://localhost:7105`

## 📋 Manual Setup (Alternative)

If you prefer manual setup:

```bash
# 1. Navigate to app directory
cd LibraryApp

# 2. Restore packages
dotnet restore

# 3. Build the application
dotnet build

# 4. Run the application
dotnet run
```

## 🌐 Access Your Application

Once running, access the application at:
- **Primary (HTTPS)**: `https://localhost:7105`
- **Alternative (HTTP)**: `http://localhost:5105`

## 🎯 What You'll See

The application comes pre-loaded with sample data and enhanced features:
- **1 Sample Project**: AI-Based Student Performance Prediction System
- **1 Sample Student**: John Smith
- **3 Sample Supervisors**: Dr. Sarah Johnson, Prof. Michael Brown, Dr. Emily Davis
- **Ready-to-use Features**: File upload, search, CRUD operations
- **Role-based Authentication**: Secure login with personalized dashboards
- **Enhanced Navigation**: User dropdown with sign-out functionality
- **Complete Dashboard System**: Different interfaces for Students, Professors, and Admins

### Test the Authentication System
- **Student Login**: Use student ID and default password (first 2 letters of name + last 4 digits of ID)
- **Professor Login**: Use professor ID and default password
- **Admin Access**: Contact system administrator for credentials
- **Sign-Out**: Available from the user dropdown on any page

## 🎨 Customize for Your University

### Quick Branding (5 minutes)
1. **Edit Settings**: Open `appsettings.json` and update the `UniversitySettings` section
2. **Add Your Logo**: Save your logo as `wwwroot/images/university/logo.png` 
3. **Add Favicon**: Save your favicon as `wwwroot/images/university/favicon.ico`
4. **Restart**: Run `dotnet run` again

### Example Configuration
```json
{
  "UniversitySettings": {
    "Name": "Your University Name",
    "ShortName": "YUN",
    "ApplicationTitle": "Graduation Projects Library",
    "TagLine": "Excellence in Education and Research",
    "LogoPath": "/images/university/logo.png",
    "FaviconPath": "/images/university/favicon.ico",
    "FooterText": "Your University Library System",
    "ContactEmail": "library@youruniversity.edu",
    "ContactPhone": "+1-555-123-4567",
    "Colors": {
      "Primary": "#003366",
      "Secondary": "#0066cc",
      "NavbarBg": "#ffffff",
      "NavbarText": "#333333"
    }
  }
}
```

## 🔧 Development Commands

```bash
# Start development server
dotnet run

# Build the application
dotnet build

# Clean build artifacts
dotnet clean

# Update database (after model changes)
dotnet ef database update

# Create new migration
dotnet ef migrations add MigrationName
```

## 🗂️ Key Directories

```
LibraryApp/
├── Controllers/          # MVC Controllers
├── Data/                # Database context
├── Models/              # Data models
├── Views/               # Razor templates
├── wwwroot/             # Static files
│   ├── css/             # Stylesheets
│   ├── images/          # Images
│   │   └── university/  # 👈 Add your assets here
│   └── documents/       # 👈 Uploaded files stored here
└── appsettings.json     # 👈 Configure university settings here
```

## 🛠️ Troubleshooting

### Common Issues

**Port Already in Use?**
```bash
# Use different ports
dotnet run --urls="https://localhost:7106;http://localhost:5106"
```

**Database Issues?**
```bash
# Reset database (also resets authentication data)
rm library.db library.db-shm library.db-wal
dotnet run  # Database will be recreated with default users
```

**Build Errors?**
```bash
# Clean and rebuild
dotnet clean
dotnet restore
dotnet build
```

## 🚀 Production Deployment

When ready for production:
```bash
dotnet publish -c Release -o ./publish
```

For complete deployment instructions, see [UNIVERSITY-THEMING-GUIDE.md](UNIVERSITY-THEMING-GUIDE.md).

## 📞 Need Help?

- **Full Documentation**: See [README.md](README.md)
- **Theming Guide**: See [UNIVERSITY-THEMING-GUIDE.md](UNIVERSITY-THEMING-GUIDE.md)
- **Issues**: Report on GitHub

---

**🎓 That's it! You now have a fully functional university graduation projects management system with enhanced authentication, role-based dashboards, and complete user management running locally.**