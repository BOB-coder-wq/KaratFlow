# 🚀 Vercel + Render Deployment (Recommended)

## Why This Combination?
- **Vercel**: Best for React frontend (free, fast, CDN)
- **Render**: Best for .NET API backend (free tier available)
- **Both**: Easy to use, no credit card required

## Step 1: Setup MongoDB Atlas
1. Go to [MongoDB Atlas](https://www.mongodb.com/atlas)
2. Create free M0 cluster
3. Create user `karatflow` with password
4. Add IP access: `0.0.0.0/0`
5. Copy connection string

## Step 2: Deploy API to Render
1. Go to [Render](https://render.com)
2. Sign up with GitHub
3. Click "New" → "Web Service"
4. Connect your GitHub repo
5. Settings:
   - **Name**: karatflow-api
   - **Runtime**: .NET 9
   - **Root Directory**: KaratFlowAPI
   - **Build Command**: `dotnet publish -c Release -o ./publish`
   - **Start Command**: `dotnet ./publish/KaratFlowAPI.dll`
   - **Instance Type**: Free

6. Add Environment Variables:
   ```
   MongoDB__ConnectionString=mongodb+srv://karatflow:YOUR_PASSWORD@karatflow.xxxxx.mongodb.net/?retryWrites=true&w=majority
   Jwt__Key=YourSecretJWTKeyHere123456789
   ```

## Step 3: Deploy Frontend to Vercel
1. Go to [Vercel](https://vercel.com)
2. Sign up with GitHub
3. Click "New Project"
4. Import your GitHub repo
5. Settings:
   - **Framework Preset**: React
   - **Root Directory**: KaratFlowWeb
   - **Build Command**: `npm install && npm run build`
   - **Output Directory**: dist

## Step 4: Update Frontend API URL
Edit `src/App.jsx`:
```javascript
const API_BASE = 'https://karatflow-api.onrender.com/api';
const SOCKET_URL = 'https://karatflow-api.onrender.com';
```

## Step 5: Test Everything
1. **API Health**: https://karatflow-api.onrender.com/health
2. **Web App**: https://your-app.vercel.app
3. **Test Login**: Use admin/password123
4. **Test Payment**: Send 100 Karats to alice

## URLs After Deployment:
- **API**: https://karatflow-api.onrender.com
- **Web**: https://karatflow-web.vercel.app
- **API Docs**: https://karatflow-api.onrender.com/swagger

## Benefits:
- ✅ Completely free (no credit card needed)
- ✅ Global CDN with Vercel
- ✅ Automatic SSL certificates
- ✅ GitHub integration
- ✅ Easy updates and redeployments
- ✅ Professional URLs

## Troubleshooting:
- If API doesn't start, check Render logs
- If web app can't connect, verify CORS settings
- Update connection string if MongoDB fails
