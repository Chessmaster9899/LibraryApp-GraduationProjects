#!/bin/bash

echo "🎓 LibraryApp - Local Development Setup"
echo "======================================"
echo

# Check if .NET is installed
echo "🔍 Checking .NET installation..."
if command -v dotnet &> /dev/null; then
    DOTNET_VERSION=$(dotnet --version)
    echo "✅ .NET SDK found: $DOTNET_VERSION"
    
    # Check if .NET 8.0 is available
    if dotnet --list-sdks | grep -q "8\."; then
        echo "✅ .NET 8.0 SDK is available"
    else
        echo "⚠️  .NET 8.0 SDK not found. Please install .NET 8.0 or later."
        echo "   Download from: https://dotnet.microsoft.com/download/dotnet/8.0"
        exit 1
    fi
else
    echo "❌ .NET SDK not found. Please install .NET 8.0 or later."
    echo "   Download from: https://dotnet.microsoft.com/download/dotnet/8.0"
    exit 1
fi

echo

# Navigate to LibraryApp directory
echo "📁 Navigating to LibraryApp directory..."
cd LibraryApp || {
    echo "❌ LibraryApp directory not found. Make sure you're in the root of the project."
    exit 1
}
echo "✅ In LibraryApp directory"

echo

# Restore packages
echo "📦 Restoring NuGet packages..."
if dotnet restore; then
    echo "✅ Packages restored successfully"
else
    echo "❌ Failed to restore packages"
    exit 1
fi

echo

# Build the application
echo "🔨 Building the application..."
if dotnet build; then
    echo "✅ Application built successfully"
else
    echo "❌ Build failed"
    exit 1
fi

echo

# Check database
echo "🗄️  Checking database setup..."
if [ -f "library.db" ]; then
    echo "✅ Database file exists"
else
    echo "ℹ️  Database will be created on first run"
fi

echo

# Check required directories
echo "📂 Checking required directories..."
if [ -d "wwwroot/documents" ]; then
    echo "✅ Documents directory exists"
else
    echo "📁 Creating documents directory..."
    mkdir -p wwwroot/documents
    echo "✅ Documents directory created"
fi

if [ -d "wwwroot/images/university" ]; then
    echo "✅ University images directory exists"
else
    echo "📁 Creating university images directory..."
    mkdir -p wwwroot/images/university
    echo "✅ University images directory created"
fi

echo

# Final setup complete
echo "🎉 Setup Complete!"
echo "=================="
echo
echo "🚀 To start the application:"
echo "   dotnet run"
echo
echo "🌐 The application will be available at:"
echo "   • HTTPS: https://localhost:7105"
echo "   • HTTP:  http://localhost:5105"
echo
echo "🎨 To customize for your university:"
echo "   1. Edit appsettings.json with your university details"
echo "   2. Add your logo to wwwroot/images/university/logo.png"
echo "   3. Add your favicon to wwwroot/images/university/favicon.ico"
echo "   4. Restart the application"
echo
echo "📚 For detailed customization instructions:"
echo "   See UNIVERSITY-THEMING-GUIDE.md"
echo
echo "Happy coding! 🎓"