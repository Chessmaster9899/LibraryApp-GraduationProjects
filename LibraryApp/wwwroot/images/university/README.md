# University Theme Customization Guide

## Overview
This LibraryApp supports full university branding customization. You can easily customize the application with your university's logo, colors, and branding information.

## Quick Setup

### 1. Update University Settings
Edit `appsettings.json` and update the `UniversitySettings` section:

```json
"UniversitySettings": {
  "Name": "Your University Name",
  "ShortName": "YUN", 
  "ApplicationTitle": "Graduation Projects Library",
  "TagLine": "Your custom tagline here",
  "LogoPath": "/images/university/logo.png",
  "FaviconPath": "/images/university/favicon.ico",
  "FooterText": "Your University Library System",
  "ContactEmail": "library@youruniversity.edu",
  "ContactPhone": "+1-555-123-4567"
}
```

### 2. Add Your University Assets
Place your university assets in `wwwroot/images/university/`:

**Required Files:**
- `logo.png` - Your university logo (recommended: 200x60px, transparent background)
- `favicon.ico` - Your university favicon (16x16px or 32x32px)

**Optional Files:**
- `hero-image.jpg` - Background image for dashboard (1920x600px recommended)
- `university-building.jpg` - Building photo for about sections
- `seal.png` - University seal/emblem

### 3. Customize Colors
Update the theme colors in `appsettings.json`:

```json
"Colors": {
  "Primary": "#your-primary-color",
  "Secondary": "#your-secondary-color",
  "NavbarBg": "#navbar-background",
  "NavbarText": "#navbar-text-color"
}
```

### 4. Additional Customization
For advanced styling, you can:
- Edit `wwwroot/css/site.css` for custom CSS
- Update view templates in `Views/` folders
- Modify the layout in `Views/Shared/_Layout.cshtml`

## Color Scheme Examples

### Traditional University Blue
```json
"Colors": {
  "Primary": "#003366",
  "Secondary": "#0066cc", 
  "NavbarBg": "#003366",
  "NavbarText": "#ffffff"
}
```

### Modern Tech University
```json
"Colors": {
  "Primary": "#2c3e50",
  "Secondary": "#3498db",
  "NavbarBg": "#ecf0f1", 
  "NavbarText": "#2c3e50"
}
```

### Classic Academic
```json
"Colors": {
  "Primary": "#8b0000",
  "Secondary": "#b8860b",
  "NavbarBg": "#f8f9fa",
  "NavbarText": "#8b0000"
}
```

## File Structure
```
wwwroot/
├── images/
│   └── university/
│       ├── logo.png          # Main logo
│       ├── favicon.ico       # Browser favicon
│       ├── hero-image.jpg    # Optional hero image
│       └── university-building.jpg  # Optional building photo
├── css/
│   └── site.css             # Custom styles
└── ...
```

## Tips
- Use high-quality images (PNG for logos, JPG for photos)
- Ensure logos have transparent backgrounds
- Test colors for accessibility (proper contrast)
- Keep logo file sizes reasonable (<100KB)
- Use consistent branding across all assets

## Support
After customization, restart the application to see your changes. All branding will automatically update throughout the application.