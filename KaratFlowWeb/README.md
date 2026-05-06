# Karat Flow Web Application

A modern, responsive web application for the Karat Flow digital currency system built with React and Vite.

## 🌟 Features

- **Real-time Payments**: Send and receive Karats instantly
- **NFC Integration**: Accept NFC card payments
- **Live Updates**: Real-time balance and transaction updates via WebSockets
- **Cloud Backend**: Powered by MongoDB Atlas for global accessibility
- **JWT Authentication**: Secure user authentication with token-based auth
- **Responsive Design**: Works on desktop, tablet, and mobile
- **Modern UI**: Built with Tailwind CSS and Heroicons

## 🚀 Quick Start

### Prerequisites
- Node.js 18+ 
- npm or yarn
- MongoDB Atlas connection string

### Installation

```bash
# Clone the repository
git clone <repository-url>
cd karatflow-web

# Install dependencies
npm install

# Start development server
npm run dev

# Build for production
npm run build

# Deploy to Vercel
npm run deploy
```

### Environment Variables

Create a `.env.local` file:

```env
VITE_API_BASE_URL=https://your-api-url.com/api
VITE_SOCKET_URL=https://your-api-url.com
```

## 📱 Usage

### Login
Use any of the demo accounts:
- **Username**: `admin` | **Password**: `password123`
- **Username**: `alice` | **Password**: `password123`  
- **Username**: `bob` | **Password**: `password123`
- **Username**: `charlie` | **Password**: `password123`

### Send Payment
1. Click "Send Payment" in Quick Actions
2. Enter recipient username
3. Enter amount and optional description
4. Confirm payment
5. Real-time balance update

### Accept NFC Payment
1. Click "Accept NFC Payment"
2. Click "Scan NFC" or enter card manually
3. Enter payment amount
4. Process payment
5. Automatic balance update

### Transaction History
- View all transactions (sent and received)
- Real-time updates when new transactions occur
- Color-coded: Green (received), Red (sent)

## 🔧 Technology Stack

### Frontend
- **React 18** - Modern UI framework
- **Vite** - Fast build tool and dev server
- **Tailwind CSS** - Utility-first CSS framework
- **Heroicons** - Beautiful icon library
- **Axios** - HTTP client for API calls
- **Socket.IO Client** - Real-time communication

### Backend
- **ASP.NET Core 9** - Web API framework
- **MongoDB Atlas** - Cloud database
- **JWT Authentication** - Token-based security
- **SignalR** - Real-time communication
- **Swagger/OpenAPI** - API documentation

## 🌐 Deployment

### Vercel (Recommended)
```bash
# Install Vercel CLI
npm i -g vercel

# Deploy
vercel --prod
```

### Environment Configuration
- **API URL**: `https://karatflow-api.vercel.app/api`
- **WebSocket URL**: `https://karatflow-api.vercel.app`
- **MongoDB Atlas**: Global cloud database

## 🔒 Security Features

- **JWT Tokens**: Secure authentication with expiration
- **Password Hashing**: BCrypt for secure password storage
- **HTTPS Only**: All communications encrypted
- **Input Validation**: Server-side validation for all inputs
- **Rate Limiting**: Protection against abuse
- **Privacy Protection**: Recipient balances hidden

## 📊 API Endpoints

### Authentication
- `POST /api/auth/register` - Register new user
- `POST /api/auth/login` - User login
- `POST /api/auth/refresh` - Refresh JWT token

### Payments
- `POST /api/payment/send` - Send payment
- `POST /api/payment/nfc` - Process NFC payment
- `GET /api/payment/balance` - Get account balance
- `GET /api/payment/history` - Transaction history
- `GET /api/payment/accounts` - User accounts

### System
- `GET /health` - Health check
- `GET /` - API information
- `GET /swagger` - API documentation

## 🔄 Real-time Features

### WebSocket Events
- `payment-processed` - Triggered when payment completes
- `balance-updated` - Triggered when balance changes
- `transaction-created` - Triggered when new transaction created

### Live Updates
- Instant balance updates after payments
- Real-time transaction history
- Live notifications for incoming payments
- Multi-user synchronization

## 🎨 UI Components

### Responsive Design
- **Mobile-first approach** with breakpoints
- **Touch-friendly** buttons and forms
- **Dark mode support** (planned)
- **Accessibility** features (ARIA labels, keyboard navigation)

### Interactive Elements
- **Animated modals** for payments
- **Loading states** for async operations
- **Success/error notifications**
- **Hover effects** and transitions
- **Progress indicators** for operations

## 📱 Mobile Optimization

- **Touch gestures** for mobile interactions
- **Responsive typography** scaling
- **Optimized images** and assets
- **Fast loading** with code splitting
- **Offline support** (planned)

## 🔧 Development

### Local Development
```bash
# Start frontend
npm run dev

# Start backend (separate terminal)
cd ../KaratFlowAPI
dotnet run
```

### Proxy Configuration
Vite proxy redirects `/api` requests to backend during development.

### Code Structure
```
src/
├── components/          # Reusable UI components
├── hooks/              # Custom React hooks
├── services/           # API service functions
├── utils/              # Utility functions
├── styles/             # CSS and styling
└── App.jsx             # Main application component
```

## 🚀 Performance

### Optimization
- **Code splitting** for faster initial load
- **Lazy loading** of components
- **Image optimization** and compression
- **Caching strategies** for API responses
- **Bundle analysis** and optimization

### Metrics
- **First Contentful Paint** < 1.5s
- **Largest Contentful Paint** optimized
- **Cumulative Layout Shift** minimized
- **Bundle size** < 500KB (gzipped)

## 🔮 Future Features

### Planned Enhancements
- **Mobile App** (React Native)
- **Dark Mode** theme
- **Advanced Analytics** dashboard
- **Multi-currency** support
- **Recurring payments** setup
- **Payment notifications** via email/SMS
- **Advanced security** (2FA, biometric)
- **International** payment support

### Integrations
- **Bank account** linking
- **Credit/debit card** payments
- **Third-party wallet** support
- **Exchange rate** APIs
- **Accounting software** integration

## 📞 Support

### Documentation
- **API Docs**: Available at `/swagger`
- **User Guide**: Interactive tutorials
- **Developer Docs**: Integration guides

### Contact
- **Issues**: GitHub repository
- **Support**: support@karatflow.com
- **Community**: Discord server

---

**Built with ❤️ using modern web technologies for the best user experience.**
