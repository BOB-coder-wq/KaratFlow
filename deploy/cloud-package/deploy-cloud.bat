@echo off
echo 🚀 Karat Flow Cloud Deployment Script
echo ===================================

echo.
echo 📋 Step 1: Deploy API Backend
echo -------------------------------
cd KaratFlowAPI
echo Building API...
dotnet publish -c Release -o ./publish
echo API built successfully!
echo.

echo 📋 Step 2: Deploy Web Frontend  
echo --------------------------------
cd ../KaratFlowWeb
echo Installing dependencies...
npm install
echo Building web app...
npm run build
echo Web app built successfully!
echo.

echo 📋 Step 3: Create Deployment Package
echo -----------------------------------
cd ..
mkdir deploy\cloud-package 2>nul
copy KaratFlowAPI\publish\* deploy\cloud-package\api\ /Y
copy KaratFlowWeb\dist\* deploy\cloud-package\web\ /Y
copy CLOUD-DEPLOYMENT.md deploy\cloud-package\ /Y
copy deploy-cloud.bat deploy\cloud-package\ /Y

echo 📋 Deployment package created!
echo Location: deploy\cloud-package\
echo.

echo 📋 Next Steps:
echo =============
echo 1. Upload API folder to Azure/Render
echo 2. Upload Web folder to Vercel  
echo 3. Update MongoDB Atlas connection string
echo 4. Configure environment variables
echo 5. Test deployment
echo.

echo 🎉 Ready for cloud deployment!
echo.
pause
