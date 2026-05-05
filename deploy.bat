@echo off
echo 🚀 Karat Flow - Windows Deployment Script
echo =====================================

echo.
echo Building Windows Forms version...
cd KaratFlowWinForms
dotnet publish -c Release -r win-x64 --self-contained -p:PublishSingleFile=true --output ..\deploy\win-forms

echo.
echo Building Avalonia version...
cd ..\KaratFlowAvalonia
dotnet publish -c Release -r win-x64 --self-contained -p:PublishSingleFile=true --output ..\deploy\win-avalonia

echo.
echo Building cross-platform versions...
dotnet publish -c Release -r linux-x64 --self-contained -p:PublishSingleFile=true --output ..\deploy\linux
dotnet publish -c Release -r osx-x64 --self-contained -p:PublishSingleFile=true --output ..\deploy\macos

echo.
echo Creating installer packages...
cd ..\deploy

echo.
echo ✅ Deployment complete!
echo.
echo Available builds:
echo - Windows Forms: win-forms\KaratFlowWinForms.exe
echo - Avalonia Windows: win-avalonia\KaratFlowAvalonia.exe
echo - Linux: linux\KaratFlowAvalonia
echo - macOS: macos\KaratFlowAvalonia
echo.
echo Press any key to exit...
pause >nul
