# Karat Flow - Deployment Guide

## 🚀 Deployment Options

### 1. Windows Deployment

#### Windows Forms (Recommended for Windows)
```bash
# Build Release
cd KaratFlowWinForms
dotnet publish -c Release -r win-x64 --self-contained

# Output: bin\Release\net9.0-windows\win-x64\publish\
```

#### WinUI 3 (Microsoft Store)
```bash
cd "Karat Flow"
dotnet publish -c Release -r win-x64
```

#### Avalonia (Cross-platform)
```bash
cd KaratFlowAvalonia
dotnet publish -c Release -r win-x64 --self-contained
```

### 2. macOS Deployment

#### Avalonia for macOS
```bash
cd KaratFlowAvalonia
dotnet publish -c Release -r osx-x64 --self-contained
```

#### Create macOS App Bundle
```bash
# Install required tools
dotnet tool install --global Avalonia.DotnetRuntimeGenerator

# Generate runtime
avalonia-dotnet-runtime-generator -r osx-x64
```

### 3. Linux Deployment

#### Avalonia for Linux
```bash
cd KaratFlowAvalonia
dotnet publish -c Release -r linux-x64 --self-contained
```

#### Create AppImage
```bash
# Install AppImageTool
wget https://github.com/AppImage/AppImageKit/releases/download/continuous/appimagetool-x86_64.AppImage
chmod +x appimagetool-x86_64.AppImage

# Create AppImage
./appimagetool-x86_64.AppImage KaratFlow.AppDir/
```

## 📱 NFC Hardware Requirements

### Windows NFC Support
- **Built-in NFC**: Windows 10/11 devices with NFC
- **USB Readers**: ACR122U, SCM SCL3711, etc.
- **Required Drivers**: Windows Smart Card drivers

### macOS NFC Support
- **iPhone/iPad**: Core NFC framework (iOS 13+)
- **MacBooks with NFC**: Limited support
- **External Readers**: ACS ACR122U via USB

### Linux NFC Support
- **PC/SC Readers**: Most USB NFC readers
- **Required Packages**: pcscd, libnfc
- **Installation**: `sudo apt-get install pcscd libnfc-dev`

## 🔧 Hardware Setup

### 1. Windows NFC Setup
```powershell
# Install Windows Smart Card service
Get-Service -Name "SCardSvr" | Set-Service -StartupType Automatic
Start-Service -Name "SCardSvr"

# Test NFC reader
Get-PnpDevice -Class SmartCardReader
```

### 2. macOS NFC Setup
```bash
# Check for NFC hardware
system_profiler SPUSBDataType | grep -i nfc

# Install Homebrew (if not installed)
/bin/bash -c "$(curl -fsSL https://raw.githubusercontent.com/Homebrew/install/HEAD/install.sh)"

# Install required packages
brew install pcsctools
```

### 3. Linux NFC Setup
```bash
# Install PC/SC and NFC packages
sudo apt-get update
sudo apt-get install pcscd pcsc-tools libnfc-dev libnfc-bin

# Start PC/SC daemon
sudo systemctl start pcscd
sudo systemctl enable pcscd

# Test NFC reader
pcsc_scan
```

## 🗄️ Database Setup

### SQL Server LocalDB (Development)
```sql
-- Create database
CREATE DATABASE KaratFlowDB;

-- Run migrations
dotnet ef database update
```

### Production SQL Server
```sql
-- Create database
CREATE DATABASE KaratFlowDB;

-- Create user
CREATE LOGIN karatflow WITH PASSWORD = 'YourStrongPassword!';
CREATE USER karatflow FOR LOGIN karatflow;
ALTER ROLE db_owner ADD MEMBER karatflow;
```

### PostgreSQL Alternative
```bash
# Install Npgsql.EntityFrameworkCore
dotnet add package Npgsql.EntityFrameworkCore

# Update connection string in appsettings.json
```

## 🔒 Security Configuration

### 1. Environment Variables
```bash
# Database Connection
export KARATFLOW_DB_CONNECTION="Server=prod-server;Database=KaratFlowDB;User Id=karatflow;Password=YourPassword;"

# NFC Settings
export KARATFLOW_NFC_ENABLED="true"
export KARATFLOW_NFC_READER_TYPE="ACR122U"

# Payment Provider
export KARATFLOW_PAYMENT_PROVIDER="stripe"
export KARATFLOW_STRIPE_API_KEY="sk_live_..."
```

### 2. Configuration Files
```json
// appsettings.Production.json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=prod-server;Database=KaratFlowDB;..."
  },
  "NFC": {
    "Enabled": true,
    "ReaderType": "ACR122U",
    "Timeout": 30000
  },
  "Payment": {
    "Provider": "stripe",
    "Stripe": {
      "ApiKey": "sk_live_...",
      "WebhookSecret": "whsec_..."
    }
  }
}
```

## 📦 Package Distribution

### Windows Installer (WiX)
```xml
<!-- Product.wxs -->
<Product Id="*" Name="Karat Flow" Language="1033" Version="1.0.0.0">
  <Package InstallerVersion="200" Compressed="yes" />
  
  <Directory Id="TARGETDIR" Name="SourceDir">
    <Directory Id="ProgramFilesFolder">
      <Directory Id="INSTALLFOLDER" Name="Karat Flow">
        <Component Id="MainExecutable">
          <File Id="KaratFlowExe" Source="KaratFlow.exe" />
        </Component>
      </Directory>
    </Directory>
  </Directory>
</Product>
```

### macOS DMG
```bash
# Create DMG
hdiutil create -volname "Karat Flow" -srcfolder dist/ KaratFlow.dmg

# Sign DMG
codesign --force --sign "Developer ID Application: Your Name" KaratFlow.dmg

# Notarize
xcrun altool --notarize-app --primary-bundle-id "com.karatflow.app" \
  --username "your@email.com" --password "@keychain:AC_PASSWORD" \
  --file KaratFlow.dmg
```

### Linux AppImage
```bash
# Create AppImage structure
mkdir KaratFlow.AppDir
cp KaratFlow KaratFlow.AppDir/
cp karatflow.desktop KaratFlow.AppDir/
cp karatflow.png KaratFlow.AppDir/

# Create AppImage
appimagetool KaratFlow.AppDir/
```

## 🌐 Web API Deployment

### Docker Deployment
```dockerfile
# Dockerfile
FROM mcr.microsoft.com/dotnet/aspnet:9.0
WORKDIR /app
COPY . .
EXPOSE 80
ENTRYPOINT ["dotnet", "KaratFlow.API.dll"]
```

```bash
# Build and run
docker build -t karatflow .
docker run -p 80:80 karatflow
```

### Kubernetes Deployment
```yaml
# k8s-deployment.yaml
apiVersion: apps/v1
kind: Deployment
metadata:
  name: karatflow
spec:
  replicas: 3
  selector:
    matchLabels:
      app: karatflow
  template:
    metadata:
      labels:
        app: karatflow
    spec:
      containers:
      - name: karatflow
        image: karatflow:latest
        ports:
        - containerPort: 80
```

## 📋 Deployment Checklist

### Pre-Deployment
- [ ] Database migrations applied
- [ ] NFC hardware tested
- [ ] Security certificates configured
- [ ] Environment variables set
- [ ] Backup strategy in place
- [ ] Monitoring configured

### Post-Deployment
- [ ] Verify NFC functionality
- [ ] Test payment processing
- [ ] Check database connectivity
- [ ] Monitor error logs
- [ ] Performance testing
- [ ] User acceptance testing

## 🚨 Troubleshooting

### Common Issues

#### NFC Not Working
```bash
# Check if service is running
sudo systemctl status pcscd

# Check reader detection
pcsc_scan

# Check permissions
ls -l /dev/pcsc*
```

#### Database Connection Issues
```bash
# Test connection
sqlcmd -S server-name -U username -P password

# Check firewall
sudo ufw status
```

#### Performance Issues
```bash
# Monitor resources
top
htop
iotop

# Check logs
journalctl -u karatflow
```

## 📞 Support

### Hardware Support
- **ACR122U**: Most common USB NFC reader
- **SCM SCL3711**: Alternative USB reader
- **iPhone**: Use Core NFC framework
- **Android**: Use NFC API

### Software Support
- **Windows 10/11**: Native support
- **macOS**: Limited support
- **Linux**: PC/SC required
- **Mobile**: Platform-specific APIs

### Community Resources
- **GitHub**: Issues and discussions
- **Discord**: Real-time support
- **Documentation**: API references
- **Tutorials**: Setup guides
