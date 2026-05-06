# 🚀 Render Deployment Guide

## Step 1: Setup Render Account
1. Go to [Render](https://render.com)
2. Sign up with GitHub (free)
3. You get $300 free credits per month

## Step 2: Deploy API Backend
1. Click "New" → "Web Service"
2. Connect your GitHub repository
3. Settings:
   - **Name**: karatflow-api
   - **Runtime**: .NET 9
   - **Root Directory**: KaratFlowAPI
   - **Build Command**: `dotnet publish -c Release -o ./publish`
   - **Start Command**: `dotnet ./publish/KaratFlowAPI.dll`
   - **Instance Type**: Free ($0/month)

## Step 3: Add Environment Variables
In Render dashboard → Environment Variables:
```
MongoDB__ConnectionString=mongodb+srv://karatflow:YOUR_PASSWORD@karatflow.xxxxx.mongodb.net/?retryWrites=true&w=majority
Jwt__Key=YourSecretJWTKeyHere123456789
ASPNETCORE_ENVIRONMENT=Production
```

## Step 4: Deploy Web Frontend
1. Click "New" → "Static Site"
2. Connect same GitHub repository
3. Settings:
   - **Name**: karatflow-web
   - **Root Directory**: KaratFlowWeb
   - **Build Command**: `npm install && npm run build`
   - **Publish Directory**: dist
   - **Node Version**: 18

## Step 5: Update Web App API URL
Edit `src/App.jsx`:
```javascript
const API_BASE = 'https://karatflow-api.onrender.com/api';
const SOCKET_URL = 'https://karatflow-api.onrender.com';
```

## Step 6: Test Deployment
1. API: https://karatflow-api.onrender.com/health
2. Web: https://karatflow-web.onrender.com
3. Test login and payments

## Benefits of Render:
- ✅ Free tier available
- ✅ Easy GitHub integration
- ✅ Automatic deployments
- ✅ SSL certificates included
- ✅ No credit card required for free tier
