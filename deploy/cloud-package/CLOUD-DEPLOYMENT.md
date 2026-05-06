# 🚀 Karat Flow Cloud Deployment Guide

## 📋 Prerequisites

Before deploying, you'll need:
- MongoDB Atlas account (free tier available)
- Vercel account (for web frontend)
- Azure/Render account (for API backend)
- Git repository (GitHub/GitLab)

---

## 🗄️ Step 1: Setup MongoDB Atlas

### Create MongoDB Atlas Cluster
1. Go to [MongoDB Atlas](https://www.mongodb.com/atlas)
2. Sign up for free account
3. Create new cluster:
   - **Cloud Provider**: AWS/Azure/GCP
   - **Region**: Choose closest to your users
   - **Tier**: M0 Sandbox (Free)
4. Wait for cluster to create (2-5 minutes)

### Configure Database Access
1. Go to **Database Access** → **Add New User**
2. Create user:
   - **Username**: `karatflow`
   - **Password**: Generate strong password
   - **Database User Privileges**: Read and write to any database
3. Save credentials securely

### Configure Network Access
1. Go to **Network Access** → **Add IP Address**
2. Select **Allow Access from Anywhere** (0.0.0.0/0)
3. Click **Confirm**

### Get Connection String
1. Go to **Database** → **Connect**
2. Select **Connect your application**
3. Copy connection string
4. Replace `<password>` with your actual password

---

## 🔧 Step 2: Deploy API Backend

### Option A: Azure App Service (Recommended)
```bash
# Build the API
cd KaratFlowAPI
dotnet publish -c Release -o ./publish

# Deploy to Azure
# Install Azure CLI
az login
az webapp up --resource-group KaratFlow --name karatflow-api --sku F1 --location "East US"

# Deploy files
az webapp deployment source config-zip --resource-group KaratFlow --name karatflow-api --src ./publish.zip
```

### Option B: Render
1. Go to [Render](https://render.com)
2. Sign up and connect GitHub
3. Create new **Web Service**
4. Settings:
   - **Name**: karatflow-api
   - **Runtime**: .NET 9
   - **Build Command**: `dotnet publish -c Release -o ./publish`
   - **Start Command**: `dotnet ./publish/KaratFlowAPI.dll`
   - **Environment Variables**:
     - `MongoDB__ConnectionString`: Your MongoDB connection string
     - `Jwt__Key`: Your JWT secret key

### Option C: Railway
```bash
# Install Railway CLI
npm install -g @railway/cli

# Login and deploy
railway login
railway init
railway up
```

### Update Configuration
Edit `appsettings.json`:
```json
{
  "MongoDB": {
    "ConnectionString": "mongodb+srv://karatflow:YOUR_PASSWORD@karatflow.xxxxx.mongodb.net/?retryWrites=true&w=majority",
    "DatabaseName": "KaratFlowDB"
  },
  "Jwt": {
    "Key": "YourSecretJWTKeyHere123456789",
    "Issuer": "KaratFlow",
    "Audience": "KaratFlowUsers"
  }
}
```

---

## 🌐 Step 3: Deploy Web Frontend

### Deploy to Vercel (Recommended)
```bash
# Install Vercel CLI
npm install -g vercel

# Deploy frontend
cd KaratFlowWeb
vercel --prod
```

### Update API URL
Edit `src/App.jsx`:
```javascript
const API_BASE = 'https://your-api-url.com/api';
const SOCKET_URL = 'https://your-api-url.com';
```

### Configure Environment
Create `.env.local`:
```env
VITE_API_BASE_URL=https://your-api-url.com/api
VITE_SOCKET_URL=https://your-api-url.com
```

---

## 🔗 Step 4: Configure CORS

### Update API CORS Settings
Edit `Program.cs` in API:
```csharp
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader());
});

app.UseCors("AllowAll");
```

---

## ✅ Step 5: Test Deployment

### Health Checks
1. API Health: `https://your-api-url.com/health`
2. API Docs: `https://your-api-url.com/swagger`
3. Web App: `https://your-app-url.vercel.app`

### Test Functionality
1. **Login**: Use demo accounts (admin/alice/bob/charlie)
2. **Send Payment**: Test between users
3. **Check Balance**: Verify real-time updates
4. **NFC Payment**: Test card simulation

---

## 🌍 Step 6: Domain Setup (Optional)

### Custom Domain for API
1. Go to your DNS provider
2. Add CNAME record: `api.yourdomain.com` → `your-api-url.com`
3. Update CORS settings in API

### Custom Domain for Web
1. In Vercel dashboard → Domains → Add
2. Add domain: `yourdomain.com`
3. Update DNS with provided records

---

## 🔒 Step 7: Security Configuration

### Production Settings
1. **HTTPS**: Ensure all endpoints use HTTPS
2. **JWT Security**: Use strong secret keys
3. **MongoDB Security**: Enable IP whitelisting
4. **Rate Limiting**: Implement API rate limits

### Environment Variables
Set these in production:
```bash
MONGODB_URI=mongodb+srv://...
JWT_KEY=YourStrongSecretKey
NODE_ENV=production
```

---

## 📊 Step 8: Monitoring

### Application Monitoring
1. **Azure Monitor**: If using Azure
2. **Render Metrics**: If using Render
3. **MongoDB Atlas**: Database performance
4. **Vercel Analytics**: Web app performance

### Health Endpoints
- `/health` - API health status
- `/swagger` - API documentation
- `/` - API information

---

## 🚨 Troubleshooting

### Common Issues

#### API Deployment Issues
- **Port**: Ensure API listens on correct port
- **Dependencies**: Check all NuGet packages restore
- **Configuration**: Verify connection strings and JWT keys

#### Database Connection Issues
- **IP Access**: Check MongoDB Atlas IP whitelist
- **Credentials**: Verify username/password in connection string
- **Network**: Check firewall/proxy settings

#### Frontend Issues
- **CORS**: Verify API allows frontend origin
- **API URL**: Update with correct API endpoint
- **Environment**: Check environment variables

#### Payment Issues
- **Real-time**: Verify WebSocket connections
- **Database**: Check transaction persistence
- **Authentication**: Validate JWT tokens

### Debug Commands
```bash
# Check API logs
az webapp log tail --name karatflow-api

# Test API locally
curl -X GET https://your-api-url.com/health

# Check MongoDB connection
mongosh "mongodb+srv://..."
```

---

## 📱 Step 9: Mobile Optimization

### Progressive Web App
The web app is already mobile-optimized:
- Responsive design
- Touch-friendly interface
- PWA capabilities (planned)

### Mobile Testing
1. Test on iOS Safari
2. Test on Android Chrome
3. Test on various screen sizes
4. Verify touch interactions

---

## 🎉 Success Checklist

- [ ] MongoDB Atlas cluster created and configured
- [ ] API backend deployed and healthy
- [ ] Web frontend deployed and accessible
- [ ] CORS properly configured
- [ ] JWT authentication working
- [ ] Real-time payments functioning
- [ ] Database persistence verified
- [ ] Mobile responsiveness confirmed
- [ ] Security settings configured
- [ ] Monitoring and logging enabled

---

## 📞 Support

### Documentation
- **API Docs**: Available at `/swagger`
- **User Guide**: Built into web app
- **Developer Docs**: This deployment guide

### Quick Links
- **MongoDB Atlas**: https://cloud.mongodb.com
- **Vercel**: https://vercel.com
- **Azure Portal**: https://portal.azure.com
- **Render**: https://render.com

---

## 🚀 Ready to Go!

Once deployed, your Karat Flow system will be:
- **Globally accessible** from any device
- **Real-time** with instant payment processing
- **Secure** with JWT authentication
- **Scalable** with cloud infrastructure
- **Mobile-friendly** for all users

Users can now:
1. Access the web app from anywhere
2. Send/receive payments in real-time
3. Use NFC card payments
4. View transaction history
5. Experience modern digital currency

**🎊 Congratulations! Your cloud-based Karat Flow is now live!**
