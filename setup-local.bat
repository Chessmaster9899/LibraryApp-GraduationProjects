@echo off
cls

echo 🎓 LibraryApp - Local Development Setup
echo ======================================
echo.

REM Check if .NET is installed
echo 🔍 Checking .NET installation...
dotnet --version >nul 2>&1
if %errorlevel% neq 0 (
    echo ❌ .NET SDK not found. Please install .NET 8.0 or later.
    echo    Download from: https://dotnet.microsoft.com/download/dotnet/8.0
    pause
    exit /b 1
)

for /f "tokens=*" %%i in ('dotnet --version') do set DOTNET_VERSION=%%i
echo ✅ .NET SDK found: %DOTNET_VERSION%

REM Check if .NET 8.0 is available
dotnet --list-sdks | findstr "8\." >nul 2>&1
if %errorlevel% neq 0 (
    echo ⚠️  .NET 8.0 SDK not found. Please install .NET 8.0 or later.
    echo    Download from: https://dotnet.microsoft.com/download/dotnet/8.0
    pause
    exit /b 1
)
echo ✅ .NET 8.0 SDK is available

echo.

REM Navigate to LibraryApp directory
echo 📁 Navigating to LibraryApp directory...
if not exist "LibraryApp" (
    echo ❌ LibraryApp directory not found. Make sure you're in the root of the project.
    pause
    exit /b 1
)
cd LibraryApp
echo ✅ In LibraryApp directory

echo.

REM Restore packages
echo 📦 Restoring NuGet packages...
dotnet restore
if %errorlevel% neq 0 (
    echo ❌ Failed to restore packages
    pause
    exit /b 1
)
echo ✅ Packages restored successfully

echo.

REM Build the application
echo 🔨 Building the application...
dotnet build
if %errorlevel% neq 0 (
    echo ❌ Build failed
    pause
    exit /b 1
)
echo ✅ Application built successfully

echo.

REM Check database
echo 🗄️  Checking database setup...
if exist "library.db" (
    echo ✅ Database file exists
) else (
    echo ℹ️  Database will be created on first run
)

echo.

REM Check required directories
echo 📂 Checking required directories...
if exist "wwwroot\documents" (
    echo ✅ Documents directory exists
) else (
    echo 📁 Creating documents directory...
    mkdir "wwwroot\documents"
    echo ✅ Documents directory created
)

if exist "wwwroot\images\university" (
    echo ✅ University images directory exists
) else (
    echo 📁 Creating university images directory...
    mkdir "wwwroot\images\university"
    echo ✅ University images directory created
)

echo.

REM Final setup complete
echo 🎉 Setup Complete!
echo ==================
echo.
echo 🚀 To start the application:
echo    dotnet run
echo.
echo 🌐 The application will be available at:
echo    • HTTPS: https://localhost:7105
echo    • HTTP:  http://localhost:5105
echo.
echo 🎨 To customize for your university:
echo    1. Edit appsettings.json with your university details
echo    2. Add your logo to wwwroot\images\university\logo.png
echo    3. Add your favicon to wwwroot\images\university\favicon.ico
echo    4. Restart the application
echo.
echo 📚 For detailed customization instructions:
echo    See UNIVERSITY-THEMING-GUIDE.md
echo.
echo Happy coding! 🎓
echo.
pause