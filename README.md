# 💎 Karat Flow - Digital Currency System

A modern, cloud-based digital currency system with real-time payments, NFC integration, and multi-platform support.

## 🌟 Features

### 🌐 Cloud-Based System
- **MongoDB Atlas** for global data storage
- **Real-time payments** with WebSocket updates
- **JWT authentication** with secure token management
- **RESTful API** with comprehensive endpoints

### 📱 Modern Web Interface
- **React + Vite** frontend with Tailwind CSS
- **Mobile-responsive** design
- **Real-time notifications** and live updates
- **Beautiful UI** with Heroicons

### 💳 Payment Features
- **User-to-user payments** with instant processing
- **NFC card payments** with daily limits
- **Transaction history** with detailed records
- **Balance management** with real-time updates

### 🔒 Security & Privacy
- **Password hashing** with BCrypt
- **JWT tokens** with expiration
- **HTTPS encryption** for all communications
- **Privacy protection** (recipient balances hidden)

## 🏗️ Architecture

```
┌─────────────────┐    ┌─────────────────┐    ┌─────────────────┐
│   React Web     │    │  ASP.NET API    │    │  MongoDB Atlas  │
│   Frontend      │◄──►│   Backend       │◄──►│   Database      │
│                 │    │                 │    │                 │
│ • Real-time UI  │    │ • JWT Auth      │    │ • Global Data   │
│ • Mobile Ready  │    │ • REST API      │    │ • Auto Backup   │
│ • PWA Ready     │    │ • SignalR       │    │ • Scalable      │
└─────────────────┘    └─────────────────┘    └─────────────────┘
```

## 🚀 Quick Deploy

### Prerequisites
- MongoDB Atlas account (free)
- GitHub account
- Render/Vercel account (free)

### 1. MongoDB Atlas Setup
```bash
# Create free cluster at https://www.mongodb.com/atlas
# Create user: karatflow
# Add IP access: 0.0.0.0/0
# Get connection string
```

### 2. Deploy API (Render)
1. Go to [Render](https://render.com)
2. Connect this GitHub repo
3. Create Web Service with:
   - Runtime: .NET 9
   - Build: `dotnet publish -c Release -o ./publish`
   - Start: `dotnet ./publish/KaratFlowAPI.dll`

### 3. Deploy Web (Vercel)
1. Go to [Vercel](https://vercel.com)
2. Connect this GitHub repo
3. Settings:
   - Root: `KaratFlowWeb`
   - Build: `npm install && npm run build`
   - Output: `dist`

### 4. Configure Environment
Set these in your deployment platform:
```bash
MongoDB__ConnectionString=mongodb+srv://karatflow:PASSWORD@cluster.mongodb.net/?retryWrites=true&w=majority
Jwt__Key=YourSecretJWTKeyHere123456789
```

## 📱 Usage

### Demo Accounts
| Username | Password | Balance |
|----------|----------|---------|
| admin    | password123 | 10,000 Karats |
| alice    | password123 | 5,000 Karats |
| bob      | password123 | 2,500 Karats |
| charlie  | password123 | 7,500 Karats |

### Test Payments
1. Login as **admin**
2. Send **100 Karats** to **alice**
3. Login as **alice** in new tab
4. See the payment instantly! ✨

### NFC Cards
- Card: `KFC123456789` (Admin's card)
- Card: `KFC987654321` (Alice's card)
- Daily limit: 1,000 Karats

## 🔧 Development

### Local Setup
```bash
# Backend
cd KaratFlowAPI
dotnet run

# Frontend
cd KaratFlowWeb
npm install
npm run dev
```

### API Endpoints
```
Authentication:
POST /api/auth/register    - Register user
POST /api/auth/login       - User login
POST /api/auth/refresh     - Refresh token

Payments:
POST /api/payment/send      - Send payment
POST /api/payment/nfc       - NFC payment
GET  /api/payment/balance   - Get balance
GET  /api/payment/history   - Transaction history

System:
GET  /health                 - Health check
GET  /swagger                - API docs
```

## 📦 Projects

### KaratFlowAPI
- ASP.NET Core Web API
- MongoDB Atlas integration
- JWT authentication
- Real-time SignalR

### KaratFlowWeb
- React + Vite frontend
- Tailwind CSS styling
- Socket.IO client
- Mobile-responsive

### KaratFlowAvalonia
- Cross-platform desktop app
- SQLite embedded database
- NFC hardware support
- Modern MVVM architecture

### KaratFlowMultiUser
- Windows Forms multi-user app
- Shared SQLite database
- User switching
- LAN deployment

## 🌍 Deployment Options

### Cloud (Recommended)
- **Render** for API backend
- **Vercel** for web frontend
- **MongoDB Atlas** for database
- **Global access** from anywhere

### Self-Hosted
- **Docker** containers
- **Local MongoDB**
- **IIS/Nginx** hosting
- **On-premise** deployment

### Desktop Apps
- **Windows** (.exe)
- **macOS** (.app)
- **Linux** (AppImage)
- **Cross-platform** support

## 🔒 Security Features

- **JWT Authentication** with expiration
- **Password Hashing** with BCrypt
- **HTTPS Encryption** for all traffic
- **Input Validation** on all endpoints
- **Rate Limiting** protection
- **CORS Configuration** for web apps

## 📊 Real-Time Features

- **Live Balance Updates** - Instant balance changes
- **Payment Notifications** - Real-time payment alerts
- **Transaction History** - Live transaction feed
- **Multi-User Sync** - Cross-device synchronization

## 🎨 UI/UX Features

- **Modern Design** with Tailwind CSS
- **Mobile-First** responsive layout
- **Dark Mode Support** (planned)
- **Accessibility** features
- **Touch-Friendly** interactions
- **Loading States** and animations

## 📱 Mobile Optimization

- **Progressive Web App** capabilities
- **Touch Gestures** support
- **Responsive Typography**
- **Fast Loading** with code splitting
- **Offline Support** (planned)

## 🔮 Future Features

- **Mobile Apps** (React Native)
- **Advanced Analytics** dashboard
- **Multi-Currency** support
- **Recurring Payments**
- **Bank Integration**
- **International** transfers
- **Advanced Security** (2FA)

## 📞 Support

### Documentation
- **API Docs**: Available at `/swagger`
- **User Guide**: Built into web app
- **Deployment Guides**: See `deploy-*.md` files

### Quick Links
- **MongoDB Atlas**: https://cloud.mongodb.com
- **Render**: https://render.com
- **Vercel**: https://vercel.com
- **GitHub**: https://github.com/your-username/karatflow

## 🎉 What You Get

✅ **Complete cloud-based digital currency system**  
✅ **Real-time payments** between users globally  
✅ **Modern web interface** accessible from any device  
✅ **Secure authentication** with JWT tokens  
✅ **NFC payment processing** with hardware support  
✅ **Mobile-responsive** design for all screen sizes  
✅ **Production-ready** deployment configuration  
✅ **Comprehensive documentation** and guides  

---

**🚀 Ready to deploy your own digital currency system?**

Follow the deployment guides in the `deploy-*.md` files to get your Karat Flow system running in minutes!
