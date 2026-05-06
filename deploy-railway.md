# 🚀 Railway Deployment Guide

## Step 1: Setup Railway Account
1. Go to [Railway](https://railway.app)
2. Sign up with GitHub (free)
3. You get $5 free credits per month (enough for small apps)

## Step 2: Install Railway CLI
```bash
npm install -g @railway/cli
```

## Step 3: Deploy API Backend
```bash
cd KaratFlowAPI
railway login
railway init
railway up
```

## Step 4: Configure API
In Railway dashboard → Settings → Variables:
```
MongoDB__ConnectionString=mongodb+srv://karatflow:YOUR_PASSWORD@karatflow.xxxxx.mongodb.net/?retryWrites=true&w=majority
Jwt__Key=YourSecretJWTKeyHere123456789
PORT=8080
```

## Step 5: Deploy Web Frontend
```bash
cd KaratFlowWeb
railway login
railway init
railway up
```

## Step 6: Update API URL
Edit `src/App.jsx`:
```javascript
const API_BASE = 'https://your-app-name.railway.app/api';
const SOCKET_URL = 'https://your-app-name.railway.app';
```

## Benefits of Railway:
- ✅ Free tier available
- ✅ Simple CLI deployment
- ✅ GitHub integration
- ✅ Automatic SSL
- ✅ Easy configuration
