#!/bin/bash

echo "🚀 Karat Flow - Cross-Platform Deployment Script"
echo "==============================================="

# Create deployment directory
mkdir -p deploy

echo ""
echo "Building Avalonia for Linux..."
cd KaratFlowAvalonia
dotnet publish -c Release -r linux-x64 --self-contained -p:PublishSingleFile=true -o ../deploy/linux

echo ""
echo "Building Avalonia for macOS..."
dotnet publish -c Release -r osx-x64 --self-contained -p:PublishSingleFile=true -o ../deploy/macos

echo ""
echo "Building Avalonia for Windows..."
dotnet publish -c Release -r win-x64 --self-contained -p:PublishSingleFile=true -o ../deploy/windows

echo ""
echo "Building for ARM64 (Apple Silicon)..."
dotnet publish -c Release -r osx-arm64 --self-contained -p:PublishSingleFile=true -o ../deploy/macos-arm64

echo ""
echo "Creating AppImage for Linux..."
cd ../deploy/linux
mkdir -p KaratFlow.AppDir/usr/bin
cp KaratFlowAvalonia KaratFlow.AppDir/usr/bin/

# Create desktop file
cat > KaratFlow.AppDir/karatflow.desktop << EOF
[Desktop Entry]
Name=Karat Flow
Comment=Digital Currency System
Exec=usr/bin/KaratFlowAvalonia
Icon=karatflow
Type=Application
Categories=Office;Finance;
EOF

# Download AppImageTool (simplified)
echo "Note: Install AppImageTool manually to create .AppImage package"

echo ""
echo "Creating macOS app bundle..."
cd ../macos
mkdir -p KaratFlow.app/Contents/MacOS
mkdir -p KaratFlow.app/Contents/Resources
cp KaratFlowAvalonia KaratFlow.app/Contents/MacOS/

# Create Info.plist
cat > KaratFlow.app/Contents/Info.plist << EOF
<?xml version="1.0" encoding="UTF-8"?>
<!DOCTYPE plist PUBLIC "-//Apple//DTD PLIST 1.0//EN" "http://www.apple.com/DTDs/PropertyList-1.0.dtd">
<plist version="1.0">
<dict>
    <key>CFBundleExecutable</key>
    <string>KaratFlowAvalonia</string>
    <key>CFBundleIdentifier</key>
    <string>com.karatflow.app</string>
    <key>CFBundleName</key>
    <string>Karat Flow</string>
    <key>CFBundleVersion</key>
    <string>1.0</string>
    <key>CFBundleShortVersionString</key>
    <string>1.0</string>
    <key>CFBundlePackageType</key>
    <string>APPL</string>
</dict>
</plist>
EOF

echo ""
echo "✅ Deployment complete!"
echo ""
echo "Available builds:"
echo "- Linux: deploy/linux/KaratFlowAvalonia"
echo "- macOS: deploy/macos/KaratFlow.app"
echo "- Windows: deploy/windows/KaratFlowAvalonia.exe"
echo "- macOS ARM64: deploy/macos-arm64/KaratFlowAvalonia"
echo ""
echo "To create installers:"
echo "- Linux: Use AppImageTool or create .deb/.rpm packages"
echo "- macOS: Use create-dmg or Xcode"
echo "- Windows: Use WiX or Inno Setup"
