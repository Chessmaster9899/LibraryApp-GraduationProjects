# University Theming & Customization Guide

This LibraryApp has been designed to be fully customizable for any university. You can easily brand the application with your university's logo, colors, contact information, and styling.

## 🎨 Quick Start

### 1. Update University Information
Edit `appsettings.json` in the LibraryApp directory:

```json
{
  "UniversitySettings": {
    "Name": "Your University Name",
    "ShortName": "YUN",
    "ApplicationTitle": "Graduation Projects Library", 
    "TagLine": "Your custom tagline here",
    "LogoPath": "/images/university/logo.png",
    "FaviconPath": "/images/university/favicon.ico",
    "FooterText": "Your University Library System",
    "ContactEmail": "library@youruniversity.edu",
    "ContactPhone": "+1-555-123-4567",
    "Colors": {
      "Primary": "#007bff",
      "Secondary": "#6c757d",
      "NavbarBg": "#ffffff",
      "NavbarText": "#333333"
    }
  }
}
```

### 2. Add Your University Assets
Place your files in `wwwroot/images/university/`:

**Required Files:**
- `logo.png` or `logo.svg` - University logo (recommended: SVG for scalability or 200x60px PNG with transparent background)
- `favicon.ico` or `favicon.svg` - Browser favicon (16x16px or 32x32px)

**Optional Files:**
- `hero-image.jpg` - Dashboard background image
- `university-building.jpg` - University building photo
- `seal.png` - University seal/emblem

**📁 Default Fallbacks:**
If your logo files are missing, the system automatically falls back to default placeholder logos located in `/images/defaults/`. This ensures the application always works even if university assets aren't available yet.

### 3. Restart the Application
After making changes to `appsettings.json`, restart the application to see your changes.

## 🎨 Color Customization

### Setting Your University Colors
The theme system uses CSS custom properties that are automatically generated from your settings. Update the `Colors` section in `appsettings.json`:

```json
"Colors": {
  "Primary": "#your-primary-color",      // Main brand color
  "Secondary": "#your-secondary-color",  // Secondary accent color
  "Success": "#your-success-color",      // Success/positive actions
  "Info": "#your-info-color",           // Information/neutral actions
  "Warning": "#your-warning-color",      // Warning/caution actions
  "Danger": "#your-danger-color",       // Error/delete actions
  "Light": "#your-light-color",         // Light backgrounds
  "Dark": "#your-dark-color",           // Dark text/elements
  "NavbarBg": "#your-navbar-bg",        // Navigation bar background
  "NavbarText": "#your-navbar-text"     // Navigation bar text color
}
```

### Popular University Color Schemes

#### Traditional Academic Blue
```json
"Colors": {
  "Primary": "#003366",
  "Secondary": "#0066cc",
  "Success": "#006633",
  "Info": "#0099cc", 
  "Warning": "#cc9900",
  "Danger": "#cc0000",
  "NavbarBg": "#003366",
  "NavbarText": "#ffffff"
}
```

#### Classic Harvard Red
```json
"Colors": {
  "Primary": "#8b0000",
  "Secondary": "#b8860b",
  "Success": "#228b22",
  "Info": "#4682b4",
  "Warning": "#daa520", 
  "Danger": "#dc143c",
  "NavbarBg": "#f8f9fa",
  "NavbarText": "#8b0000"
}
```

#### Modern Tech Green
```json
"Colors": {
  "Primary": "#004225",
  "Secondary": "#2e7d32",
  "Success": "#388e3c",
  "Info": "#0288d1",
  "Warning": "#f57c00",
  "Danger": "#d32f2f",
  "NavbarBg": "#e8f5e8",
  "NavbarText": "#004225"
}
```

#### Purple University
```json
"Colors": {
  "Primary": "#4a148c",
  "Secondary": "#7b1fa2",
  "Success": "#388e3c",
  "Info": "#1976d2",
  "Warning": "#f57c00",
  "Danger": "#d32f2f",
  "NavbarBg": "#f3e5f5",
  "NavbarText": "#4a148c"
}
```

## 🖼️ Logo & Asset Guidelines

### Logo Requirements
- **Format**: PNG preferred (supports transparency)
- **Size**: 200x60px recommended for optimal navbar display
- **Background**: Transparent preferred for professional appearance
- **File size**: Keep under 100KB for fast loading

### Favicon Requirements
- **Format**: ICO file preferred (best browser support)
- **Size**: 16x16px or 32x32px
- **File size**: Keep under 50KB

### Optional Images
- **Hero Image**: 1920x600px, JPG format, university campus or building
- **Building Photo**: 800x400px, JPG format, iconic university building
- **Seal/Emblem**: PNG format with transparency, any reasonable size

## 🎨 Advanced Customization

### Custom CSS
For additional styling, you can edit `wwwroot/css/site.css`. The file includes CSS custom properties that automatically use your university colors:

```css
/* Your custom styles using university colors */
.custom-element {
  background-color: var(--primary-color);
  color: var(--navbar-text);
  border: 2px solid var(--secondary-color);
}
```

### Available CSS Variables
- `--primary-color`
- `--secondary-color`
- `--success-color`
- `--info-color`
- `--warning-color`
- `--danger-color`
- `--light-color`
- `--dark-color`
- `--navbar-bg`
- `--navbar-text`

## 📱 Mobile Responsiveness

The application is fully responsive and automatically adapts to your university branding on all devices:
- Mobile phones (320px+)
- Tablets (768px+)
- Desktop computers (1024px+)
- Large screens (1200px+)

## 🚀 Deployment Tips

### Before Deployment
1. ✅ Test all color combinations for accessibility (proper contrast)
2. ✅ Verify logo displays correctly on different screen sizes
3. ✅ Check contact information is correct
4. ✅ Test favicon appears in browser tabs
5. ✅ Validate all links work properly

### Production Checklist
- [ ] University name and contact information updated
- [ ] Logo and favicon uploaded
- [ ] Color scheme matches university brand guidelines
- [ ] All text content reviewed and approved
- [ ] Mobile responsiveness tested
- [ ] Accessibility compliance verified

## 🎓 Complete Example

Here's a complete configuration for "Greenfield University":

```json
{
  "UniversitySettings": {
    "Name": "Greenfield University",
    "ShortName": "GFU",
    "ApplicationTitle": "Graduation Projects Library",
    "TagLine": "Excellence in Research and Innovation",
    "LogoPath": "/images/university/gfu-logo.png",
    "FaviconPath": "/images/university/gfu-favicon.ico",
    "FooterText": "Greenfield University Library System",
    "ContactEmail": "library@greenfield.edu",
    "ContactPhone": "+1-555-GFU-INFO",
    "Colors": {
      "Primary": "#004225",
      "Secondary": "#2e7d32",
      "Success": "#388e3c",
      "Info": "#0288d1",
      "Warning": "#f57c00",
      "Danger": "#d32f2f",
      "Light": "#f8f9fa",
      "Dark": "#343a40",
      "NavbarBg": "#e8f5e8",
      "NavbarText": "#004225"
    }
  }
}
```

## 📄 File Upload System

### Project Document Management
The application now includes a comprehensive file upload system for project documents:

**Supported File Types:**
- PDF documents (`.pdf`)
- Microsoft Word documents (`.doc`, `.docx`)
- Text files (`.txt`)

**Features:**
- ✅ **Secure Upload**: Files are automatically renamed with unique identifiers to prevent conflicts
- ✅ **Storage Management**: Documents are stored in `/wwwroot/documents/` with proper organization
- ✅ **Update Handling**: When editing projects, old files are automatically removed when new ones are uploaded
- ✅ **Download Links**: Direct links to view/download documents from project details
- ✅ **Fallback Display**: Shows "No document uploaded" message when no file is present

**Usage:**
1. **Creating Projects**: Use the file upload field in the "Create Project" form
2. **Editing Projects**: Upload new documents or keep existing ones in the "Edit Project" form
3. **Viewing Documents**: Click the "View Document" button in project details

**Technical Details:**
- Files are stored in: `/wwwroot/documents/`
- Naming convention: `{GUID}_{original-filename}`
- Maximum file size: Controlled by ASP.NET Core settings
- Direct web access via: `/documents/{filename}`

## 🔧 Troubleshooting

### Logo Not Showing
- Check file path in `LogoPath` setting
- Verify logo file exists in `wwwroot/images/university/`
- Ensure file permissions allow web access
- Check browser console for 404 errors

### Colors Not Updating
- Restart the application after changing `appsettings.json`
- Clear browser cache (Ctrl+F5 or Cmd+Shift+R)
- Check for typos in color hex codes

### Mobile Display Issues
- Test on actual mobile devices, not just browser dev tools
- Verify images are appropriately sized
- Check contrast ratios for accessibility

## 📞 Support

After customization, your university branding will automatically appear throughout the entire application:
- Navigation bar with logo
- Dashboard with university colors
- All buttons and interactive elements
- Footer with contact information
- Page titles and metadata

The application maintains its professional appearance while reflecting your university's unique identity.

---

*This theming system was designed to be university-friendly and requires no programming knowledge to customize.*