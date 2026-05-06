import React, { useState, useEffect } from 'react';
import axios from 'axios';
import { MagnifyingGlassIcon, CreditCardIcon, UserIcon, CurrencyDollarIcon, ArrowPathIcon, BellIcon } from '@heroicons/react/24/outline';
import io from 'socket.io-client';

function App() {
  const [user, setUser] = useState(null);
  const [accounts, setAccounts] = useState([]);
  const [transactions, setTransactions] = useState([]);
  const [balance, setBalance] = useState(0);
  const [showSendPayment, setShowSendPayment] = useState(false);
  const [showNFCPayment, setShowNFCPayment] = useState(false);
  const [notification, setNotification] = useState(null);
  const [socket, setSocket] = useState(null);

  // API base URL
  const API_BASE = 'https://karatflow-api.vercel.app/api';

  useEffect(() => {
    // Initialize socket connection
    const newSocket = io('https://karatflow-api.vercel.app');
    setSocket(newSocket);

    newSocket.on('payment-processed', (data) => {
      // Update UI when payment is processed
      if (data.toAccountId === accounts[0]?.id) {
        setNotification({
          type: 'success',
          message: `Payment received: ${data.amount} Karats`
        });
        loadUserData();
      }
    });

    // Check for existing token
    const token = localStorage.getItem('karatflow_token');
    if (token) {
      loadUserData(token);
    }

    return () => {
      newSocket.disconnect();
    };
  }, []);

  const loadUserData = async (token) => {
    try {
      const response = await axios.get(`${API_BASE}/payment/balance`, {
        headers: { Authorization: `Bearer ${token}` }
      });
      
      setUser(response.data.user);
      setBalance(response.data.balance);
      loadTransactions(token);
      loadAccounts(token);
    } catch (error) {
      console.error('Error loading user data:', error);
      if (error.response?.status === 401) {
        localStorage.removeItem('karatflow_token');
        setUser(null);
      }
    }
  };

  const loadTransactions = async (token) => {
    try {
      const response = await axios.get(`${API_BASE}/payment/history`, {
        headers: { Authorization: `Bearer ${token}` }
      });
      setTransactions(response.data.transactions);
    } catch (error) {
      console.error('Error loading transactions:', error);
    }
  };

  const loadAccounts = async (token) => {
    try {
      const response = await axios.get(`${API_BASE}/payment/accounts`, {
        headers: { Authorization: `Bearer ${token}` }
      });
      setAccounts(response.data.accounts);
    } catch (error) {
      console.error('Error loading accounts:', error);
    }
  };

  const handleLogin = async (username, password) => {
    try {
      const response = await axios.post(`${API_BASE}/auth/login`, {
        username,
        password
      });

      const { token, user } = response.data;
      localStorage.setItem('karatflow_token', token);
      setUser(user);
      loadUserData(token);
      
      setNotification({
        type: 'success',
        message: `Welcome back, ${user.username}!`
      });
    } catch (error) {
      setNotification({
        type: 'error',
        message: 'Login failed. Please check your credentials.'
      });
    }
  };

  const handleSendPayment = async (recipientUsername, amount, description) => {
    try {
      const token = localStorage.getItem('karatflow_token');
      const response = await axios.post(`${API_BASE}/payment/send`, {
        recipientUsername,
        amount: parseFloat(amount),
        description
      }, {
        headers: { Authorization: `Bearer ${token}` }
      });

      if (response.data.success) {
        setNotification({
          type: 'success',
          message: `Payment of ${amount} Karats sent to ${recipientUsername}!`
        });
        setShowSendPayment(false);
        loadUserData(token);
      } else {
        setNotification({
          type: 'error',
          message: response.data.message || 'Payment failed'
        });
      }
    } catch (error) {
      setNotification({
        type: 'error',
        message: error.response?.data?.message || 'Payment failed'
      });
    }
  };

  const handleNFCPayment = async (cardNumber, amount) => {
    try {
      const token = localStorage.getItem('karatflow_token');
      const response = await axios.post(`${API_BASE}/payment/nfc`, {
        cardNumber,
        amount: parseFloat(amount)
      }, {
        headers: { Authorization: `Bearer ${token}` }
      });

      if (response.data.success) {
        setNotification({
          type: 'success',
          message: `NFC payment of ${amount} Karats received!`
        });
        setShowNFCPayment(false);
        loadUserData(token);
      } else {
        setNotification({
          type: 'error',
          message: response.data.message || 'NFC payment failed'
        });
      }
    } catch (error) {
      setNotification({
        type: 'error',
        message: error.response?.data?.message || 'NFC payment failed'
      });
    }
  };

  const handleLogout = () => {
    localStorage.removeItem('karatflow_token');
    setUser(null);
    setAccounts([]);
    setTransactions([]);
    setBalance(0);
  };

  if (!user) {
    return <Login onLogin={handleLogin} notification={notification} />;
  }

  return (
    <div className="min-h-screen bg-gradient-to-br from-orange-50 to-white">
      <Header user={user} onLogout={handleLogout} />
      
      {notification && (
        <Notification
          notification={notification}
          onClose={() => setNotification(null)}
        />
      )}

      <div className="container mx-auto px-4 py-8">
        <div className="grid grid-cols-1 lg:grid-cols-3 gap-6">
          {/* Balance Card */}
          <BalanceCard balance={balance} account={accounts[0]} />

          {/* Quick Actions */}
          <QuickActions
            onSendPayment={() => setShowSendPayment(true)}
            onReceivePayment={() => setShowSendPayment(true)}
            onNFCPayment={() => setShowNFCPayment(true)}
          />

          {/* Transaction History */}
          <TransactionHistory transactions={transactions} />
        </div>
      </div>

      {/* Send Payment Modal */}
      {showSendPayment && (
        <SendPaymentModal
          onClose={() => setShowSendPayment(false)}
          onSendPayment={handleSendPayment}
        />
      )}

      {/* NFC Payment Modal */}
      {showNFCPayment && (
        <NFCPaymentModal
          onClose={() => setShowNFCPayment(false)}
          onNFCPayment={handleNFCPayment}
        />
      )}
    </div>
  );
}

// Components
const Header = ({ user, onLogout }) => (
  <header className="bg-white shadow-sm border-b border-gray-200">
    <div className="container mx-auto px-4 py-4">
      <div className="flex justify-between items-center">
        <div className="flex items-center space-x-2">
          <div className="text-2xl font-bold text-orange-600">💎</div>
          <h1 className="text-2xl font-bold text-gray-900">Karat Flow</h1>
        </div>
        
        {user && (
          <div className="flex items-center space-x-4">
            <span className="text-gray-600">Welcome, {user.username}</span>
            <button
              onClick={onLogout}
              className="bg-red-500 text-white px-4 py-2 rounded-md hover:bg-red-600 transition-colors"
            >
              Logout
            </button>
          </div>
        )}
      </div>
    </div>
  </header>
);

const Login = ({ onLogin, notification }) => {
  const [username, setUsername] = useState('');
  const [password, setPassword] = useState('');

  const handleSubmit = (e) => {
    e.preventDefault();
    onLogin(username, password);
  };

  return (
    <div className="min-h-screen flex items-center justify-center bg-gradient-to-br from-orange-100 to-white">
      <div className="max-w-md w-full space-y-8 p-8">
        <div className="text-center">
          <div className="text-4xl font-bold text-orange-600 mb-2">💎</div>
          <h2 className="text-3xl font-bold text-gray-900">Karat Flow</h2>
          <p className="text-gray-600">Digital Currency System</p>
        </div>

        <form onSubmit={handleSubmit} className="space-y-6">
          <div>
            <label className="block text-sm font-medium text-gray-700">Username</label>
            <input
              type="text"
              value={username}
              onChange={(e) => setUsername(e.target.value)}
              className="mt-1 block w-full px-3 py-2 border border-gray-300 rounded-md shadow-sm focus:ring-orange-500 focus:border-orange-500"
              placeholder="Enter your username"
              required
            />
          </div>

          <div>
            <label className="block text-sm font-medium text-gray-700">Password</label>
            <input
              type="password"
              value={password}
              onChange={(e) => setPassword(e.target.value)}
              className="mt-1 block w-full px-3 py-2 border border-gray-300 rounded-md shadow-sm focus:ring-orange-500 focus:border-orange-500"
              placeholder="Enter your password"
              required
            />
          </div>

          <button
            type="submit"
            className="w-full flex justify-center py-3 px-4 border border-transparent rounded-md shadow-sm text-sm font-medium text-white bg-orange-600 hover:bg-orange-700 focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-orange-500"
          >
            Sign In
          </button>
        </form>

        <div className="text-center text-sm text-gray-600">
          Demo accounts: admin/alice/bob/charlie (password: password123)
        </div>
      </div>

      {notification && (
        <Notification
          notification={notification}
          onClose={() => setNotification(null)}
        />
      )}
    </div>
  );
};

const BalanceCard = ({ balance, account }) => (
  <div className="bg-white rounded-lg shadow-lg p-6 border border-gray-200">
    <h3 className="text-lg font-semibold text-gray-900 mb-4">Account Balance</h3>
    <div className="text-3xl font-bold text-orange-600 mb-2">
      {balance.toLocaleString()} Karats
    </div>
    {account && (
      <div className="text-sm text-gray-600">
        Account: {account.accountNumber}
      </div>
    )}
  </div>
);

const QuickActions = ({ onSendPayment, onReceivePayment, onNFCPayment }) => (
  <div className="bg-white rounded-lg shadow-lg p-6 border border-gray-200">
    <h3 className="text-lg font-semibold text-gray-900 mb-4">Quick Actions</h3>
    <div className="space-y-3">
      <button
        onClick={onSendPayment}
        className="w-full flex items-center justify-center px-4 py-3 border border-transparent rounded-md shadow-sm text-sm font-medium text-white bg-orange-600 hover:bg-orange-700 focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-orange-500"
      >
        <CurrencyDollarIcon className="h-5 w-5 mr-2" />
        Send Payment
      </button>
      
      <button
        onClick={onReceivePayment}
        className="w-full flex items-center justify-center px-4 py-3 border border-transparent rounded-md shadow-sm text-sm font-medium text-white bg-green-600 hover:bg-green-700 focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-green-500"
      >
        <UserIcon className="h-5 w-5 mr-2" />
        Receive Payment
      </button>
      
      <button
        onClick={onNFCPayment}
        className="w-full flex items-center justify-center px-4 py-3 border border-transparent rounded-md shadow-sm text-sm font-medium text-white bg-blue-600 hover:bg-blue-700 focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-blue-500"
      >
        <CreditCardIcon className="h-5 w-5 mr-2" />
        Accept NFC Payment
      </button>
    </div>
  </div>
);

const TransactionHistory = ({ transactions }) => (
  <div className="bg-white rounded-lg shadow-lg p-6 border border-gray-200 lg:col-span-3">
    <h3 className="text-lg font-semibold text-gray-900 mb-4">Recent Transactions</h3>
    <div className="space-y-3 max-h-96 overflow-y-auto">
      {transactions.length === 0 ? (
        <p className="text-gray-500 text-center py-8">No transactions yet</p>
      ) : (
        transactions.map((transaction) => (
          <div key={transaction.id} className="border-b border-gray-200 pb-3">
            <div className="flex justify-between items-start">
              <div className="flex-1">
                <div className="font-medium text-gray-900">
                  {transaction.fromAccount?.user?.username} → {transaction.toAccount?.user?.username}
                </div>
                <div className="text-sm text-gray-600">{transaction.description}</div>
                <div className="text-xs text-gray-500">
                  {new Date(transaction.createdAt).toLocaleString()}
                </div>
              </div>
              <div className={`text-lg font-bold ${
                transaction.fromAccount?.user?.username === 'NFC_CARD' ? 'text-green-600' : 'text-red-600'
              }`}>
                {transaction.fromAccount?.user?.username === 'NFC_CARD' ? '+' : '-'}
                {transaction.amount.toLocaleString()} K
              </div>
            </div>
          </div>
        ))
      )}
    </div>
  </div>
);

const SendPaymentModal = ({ onClose, onSendPayment }) => {
  const [recipient, setRecipient] = useState('');
  const [amount, setAmount] = useState('');
  const [description, setDescription] = useState('');

  const handleSubmit = (e) => {
    e.preventDefault();
    onSendPayment(recipient, amount, description);
  };

  return (
    <div className="fixed inset-0 bg-black bg-opacity-50 flex items-center justify-center z-50">
      <div className="bg-white rounded-lg p-8 max-w-md w-full mx-4">
        <h3 className="text-xl font-bold text-gray-900 mb-6">Send Karats</h3>
        
        <form onSubmit={handleSubmit} className="space-y-4">
          <div>
            <label className="block text-sm font-medium text-gray-700">Recipient Username</label>
            <input
              type="text"
              value={recipient}
              onChange={(e) => setRecipient(e.target.value)}
              className="mt-1 block w-full px-3 py-2 border border-gray-300 rounded-md"
              placeholder="Enter username"
              required
            />
          </div>

          <div>
            <label className="block text-sm font-medium text-gray-700">Amount (Karats)</label>
            <input
              type="number"
              value={amount}
              onChange={(e) => setAmount(e.target.value)}
              className="mt-1 block w-full px-3 py-2 border border-gray-300 rounded-md"
              placeholder="0.00"
              step="0.01"
              min="0.01"
              required
            />
          </div>

          <div>
            <label className="block text-sm font-medium text-gray-700">Description (Optional)</label>
            <textarea
              value={description}
              onChange={(e) => setDescription(e.target.value)}
              className="mt-1 block w-full px-3 py-2 border border-gray-300 rounded-md"
              rows={3}
              placeholder="Payment description"
            />
          </div>

          <div className="flex space-x-3">
            <button
              type="submit"
              className="flex-1 bg-orange-600 text-white px-4 py-2 rounded-md hover:bg-orange-700"
            >
              Send Payment
            </button>
            <button
              type="button"
              onClick={onClose}
              className="flex-1 bg-gray-300 text-gray-700 px-4 py-2 rounded-md hover:bg-gray-400"
            >
              Cancel
            </button>
          </div>
        </form>
      </div>
    </div>
  );
};

const NFCPaymentModal = ({ onClose, onNFCPayment }) => {
  const [cardNumber, setCardNumber] = useState('');
  const [amount, setAmount] = useState('');
  const [isScanning, setIsScanning] = useState(false);

  const handleScan = () => {
    setIsScanning(true);
    setTimeout(() => {
      setCardNumber('KFC123456789');
      setAmount('250');
      setIsScanning(false);
    }, 2000);
  };

  const handleSubmit = (e) => {
    e.preventDefault();
    onNFCPayment(cardNumber, amount);
  };

  return (
    <div className="fixed inset-0 bg-black bg-opacity-50 flex items-center justify-center z-50">
      <div className="bg-white rounded-lg p-8 max-w-md w-full mx-4">
        <h3 className="text-xl font-bold text-gray-900 mb-6">Accept NFC Payment</h3>
        
        <form onSubmit={handleSubmit} className="space-y-4">
          <div>
            <label className="block text-sm font-medium text-gray-700">Card Number</label>
            <div className="flex space-x-2">
              <input
                type="text"
                value={cardNumber}
                onChange={(e) => setCardNumber(e.target.value)}
                className="flex-1 mt-1 block px-3 py-2 border border-gray-300 rounded-md"
                placeholder="Scan NFC card or enter manually"
                required
              />
              <button
                type="button"
                onClick={handleScan}
                disabled={isScanning}
                className="px-4 py-2 bg-blue-600 text-white rounded-md hover:bg-blue-700 disabled:bg-gray-400"
              >
                {isScanning ? 'Scanning...' : '📱 Scan'}
              </button>
            </div>
          </div>

          <div>
            <label className="block text-sm font-medium text-gray-700">Amount (Karats)</label>
            <input
              type="number"
              value={amount}
              onChange={(e) => setAmount(e.target.value)}
              className="mt-1 block w-full px-3 py-2 border border-gray-300 rounded-md"
              placeholder="0.00"
              step="0.01"
              min="0.01"
              required
            />
          </div>

          <div className="flex space-x-3">
            <button
              type="submit"
              className="flex-1 bg-blue-600 text-white px-4 py-2 rounded-md hover:bg-blue-700"
            >
              Process Payment
            </button>
            <button
              type="button"
              onClick={onClose}
              className="flex-1 bg-gray-300 text-gray-700 px-4 py-2 rounded-md hover:bg-gray-400"
            >
              Cancel
            </button>
          </div>
        </form>
      </div>
    </div>
  );
};

const Notification = ({ notification, onClose }) => (
  <div className={`fixed top-4 right-4 p-4 rounded-lg shadow-lg z-50 ${
    notification.type === 'success' ? 'bg-green-100 border-green-400' : 'bg-red-100 border-red-400'
  }`}>
    <div className="flex">
      <div className="flex-shrink-0">
        {notification.type === 'success' ? (
          <div className="h-6 w-6 text-green-600">✅</div>
        ) : (
          <div className="h-6 w-6 text-red-600">❌</div>
        )}
      </div>
      <div className="ml-3">
        <p className={`text-sm font-medium ${
          notification.type === 'success' ? 'text-green-800' : 'text-red-800'
        }`}>
          {notification.message}
        </p>
      </div>
      <div className="ml-auto pl-3">
        <button
          onClick={onClose}
          className={`inline-flex rounded-md p-1.5 focus:outline-none focus:ring-2 focus:ring-offset-2 ${
            notification.type === 'success' 
              ? 'bg-green-100 text-green-600 hover:bg-green-200 focus:ring-green-500' 
              : 'bg-red-100 text-red-600 hover:bg-red-200 focus:ring-red-500'
          }`}
        >
          <span className="sr-only">Dismiss</span>
          <BellIcon className="h-5 w-5" />
        </button>
      </div>
    </div>
  </div>
);

export default App;
