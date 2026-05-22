using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using KaratFlowAvalonia.Services;

namespace KaratFlowAvalonia.ViewModels;

public partial class MainWindowViewModel : ViewModelBase
{
    // User data
    private decimal _currentBalance = 0;
    private string _currentUsername = string.Empty;
    private string _currentAccountNumber = "KF000000001";
    
    // UI state
    private bool _isPaymentSectionVisible = false;
    private bool _isNFCSectionVisible = false;
    private string _recipientUsername = string.Empty;
    private decimal _paymentAmount = 0;
    private string _paymentDescription = string.Empty;
    private string _paymentStatus = string.Empty;
    private string _nfcStatus = "Ready to accept payment";
    private bool _isNFCProcessing = false;
    private bool _useRealNFC = true; // Toggle for real vs simulated NFC
    
    // Services
    private INFCService _nfcService;
    private FirebaseService _firebaseService;
    private LocalAuthService _authService;
    
    // Transactions
    private List<Transaction> _transactions = new List<Transaction>
    {
        new Transaction { FromUser = "admin", ToUser = "alice", Amount = 500, Description = "Welcome bonus", CreatedAt = DateTime.UtcNow.AddDays(-5) },
        new Transaction { FromUser = "alice", ToUser = "bob", Amount = 150, Description = "Coffee payment", CreatedAt = DateTime.UtcNow.AddDays(-1) },
        new Transaction { FromUser = "bob", ToUser = "charlie", Amount = 75, Description = "Lunch split", CreatedAt = DateTime.UtcNow.AddDays(-2) }
    };

    public string Greeting => "💎 Karat Flow - Digital Currency";
    public string Subtitle => "Cross-Platform Digital Currency System";
    public string WelcomeMessage => string.IsNullOrEmpty(_currentUsername) ? "Welcome!" : $"Welcome back, {_currentUsername}!";
    public string BalanceDisplay => $"{CurrentBalance:N0} Karats";
    public string AccountDisplay => $"Account: {CurrentAccountNumber}";
    
    public decimal CurrentBalance
    {
        get => _currentBalance;
        set => SetProperty(ref _currentBalance, value);
    }
    
    public string CurrentUsername => _currentUsername;
    public string CurrentAccountNumber => _currentAccountNumber;
    
    public bool IsPaymentSectionVisible
    {
        get => _isPaymentSectionVisible;
        set => SetProperty(ref _isPaymentSectionVisible, value);
    }
    
    public bool IsNFCSectionVisible
    {
        get => _isNFCSectionVisible;
        set => SetProperty(ref _isNFCSectionVisible, value);
    }
    
    public string RecipientUsername
    {
        get => _recipientUsername;
        set => SetProperty(ref _recipientUsername, value);
    }
    
    public decimal PaymentAmount
    {
        get => _paymentAmount;
        set => SetProperty(ref _paymentAmount, value);
    }
    
    public string PaymentDescription
    {
        get => _paymentDescription;
        set => SetProperty(ref _paymentDescription, value);
    }
    
    public string PaymentStatus
    {
        get => _paymentStatus;
        set => SetProperty(ref _paymentStatus, value);
    }
    
    public string NFCStatus
    {
        get => _nfcStatus;
        set => SetProperty(ref _nfcStatus, value);
    }
    
    public bool IsNFCProcessing
    {
        get => _isNFCProcessing;
        set => SetProperty(ref _isNFCProcessing, value);
    }
    
    public List<Transaction> Transactions => _transactions.OrderByDescending(t => t.CreatedAt).ToList();
    
    public bool UseRealNFC
    {
        get => _useRealNFC;
        set => SetProperty(ref _useRealNFC, value);
    }
    
    public IRelayCommand SendPaymentCommand { get; }
    public IRelayCommand CancelPaymentCommand { get; }
    public IRelayCommand ShowSendPaymentCommand { get; }
    public IRelayCommand ShowReceivePaymentCommand { get; }
    public IRelayCommand ShowNFCPaymentCommand { get; }
    public IRelayCommand CancelNFCCommand { get; }
    
    public MainWindowViewModel(KaratFlowAvalonia.Models.User? user = null)
    {
        // Initialize commands
        SendPaymentCommand = new RelayCommand(async () => await SendPayment());
        CancelPaymentCommand = new RelayCommand(CancelPayment);
        ShowSendPaymentCommand = new RelayCommand(ShowSendPayment);
        ShowReceivePaymentCommand = new RelayCommand(ShowReceivePayment);
        ShowNFCPaymentCommand = new RelayCommand(async () => await ShowNFCPayment());
        CancelNFCCommand = new RelayCommand(CancelNFC);
        
        // Set user data from logged-in user
        if (user != null)
        {
            _currentUsername = user.Username;
            _currentBalance = user.Balance;
            _currentAccountNumber = $"KF{user.Id.Substring(0, 8).ToUpper()}";
        }
        
        // Initialize secure credential manager
        var credentialManager = new SecureCredentialManager(null);
        
        // Initialize Firebase service with credentials from secure credential manager
        var firebaseUrl = credentialManager.FirebaseUrl;
        var firebaseSecret = credentialManager.FirebaseSecret;
        _firebaseService = new FirebaseService(firebaseUrl, firebaseSecret);
        
        // Initialize LocalAuthService with Firebase connection
        _authService = new LocalAuthService();
        Console.WriteLine($"🔥 LocalAuthService initialized for payments");
        
        // Load transactions from Firebase (async, but we'll do it synchronously for now)
        _transactions = new List<Transaction>();
        _ = LoadTransactionsFromFirebaseAsync();
        
        _nfcService = NFCServiceFactory.CreateNFCService();
        
        Console.WriteLine($"🔥 Firebase service initialized for transactions");
        
        // Subscribe to NFC service events
        _nfcService.CardDetected += OnCardDetected;
        _nfcService.StatusChanged += OnNFCStatusChanged;
        _nfcService.ErrorOccurred += OnNFCErrorOccurred;
        
        // Initialize NFC service
        _ = InitializeNFCServiceAsync();
    }
    
    private async Task LoadTransactionsFromFirebaseAsync()
    {
        try
        {
            var transactions = await _firebaseService.LoadTransactionsAsync();
            _transactions = transactions;
            Console.WriteLine($"💾 Loaded {_transactions.Count} transactions from Firebase");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"⚠️  Error loading transactions from Firebase: {ex.Message}");
        }
    }

    private async Task InitializeNFCServiceAsync()
    {
        try
        {
            await _nfcService.InitializeAsync();
        }
        catch (Exception ex)
        {
            // Fall back to simulated NFC if real NFC fails
            UseRealNFC = false;
            NFCStatus = "Real NFC unavailable, using simulation";
        }
    }
    
    private void OnCardDetected(NFCCard card)
    {
        NFCStatus = $"Card detected: {card.CardNumber}";
    }
    
    private void OnNFCStatusChanged(string status)
    {
        NFCStatus = status;
    }
    
    private void OnNFCErrorOccurred(NFCError error)
    {
        NFCStatus = $"Error: {error.Message}";
        IsNFCProcessing = false;
    }
    
    private void ShowSendPayment()
    {
        IsPaymentSectionVisible = true;
        IsNFCSectionVisible = false;
    }
    
    private void ShowReceivePayment()
    {
        // Show account details dialog
    }
    
    private async Task ShowNFCPayment()
    {
        IsNFCSectionVisible = true;
        IsPaymentSectionVisible = false;
        IsNFCProcessing = true;
        
        if (UseRealNFC)
        {
            await ProcessRealNFCPaymentAsync();
        }
        else
        {
            await ProcessSimulatedNFCPaymentAsync();
        }
    }
    
    private async Task ProcessRealNFCPaymentAsync()
    {
        try
        {
            // Detect real NFC card
            var card = await _nfcService.DetectCardAsync();
            
            if (card != null)
            {
                NFCStatus = $"Processing payment from {card.CardNumber}...";
                
                // Process payment with real NFC service
                var result = await _nfcService.ProcessPaymentAsync(card, 0, 0);
                
                if (result.Success)
                {
                    // Update balance
                    CurrentBalance += result.Amount;
                    
                    // Add transaction
                    _transactions.Add(new Transaction 
                    { 
                        FromUser = "NFC Card", 
                        ToUser = CurrentUsername, 
                        Amount = result.Amount, 
                        Description = "NFC Card Payment", 
                        CreatedAt = DateTime.UtcNow 
                    });
                    
                    OnPropertyChanged(nameof(Transactions));
                    
                    NFCStatus = $"✅ Payment received: {result.Amount:N0} Karats";
                }
                else
                {
                    NFCStatus = $"❌ Payment failed: {result.Message}";
                }
            }
        }
        catch (Exception ex)
        {
            NFCStatus = $"❌ NFC Error: {ex.Message}";
        }
        finally
        {
            IsNFCProcessing = false;
            await Task.Delay(2000);
            IsNFCSectionVisible = false;
        }
    }
    
    private async Task ProcessSimulatedNFCPaymentAsync()
    {
        NFCStatus = "Scanning for NFC cards...";
        
        // Simulate NFC card detection
        await Task.Delay(2000);
        
        NFCStatus = "Card detected: KFC123456789";
        await Task.Delay(1000);
        
        // Simulate payment processing
        var randomAmount = new Random().Next(10, 500);
        NFCStatus = $"Processing payment of {randomAmount} Karats...";
        await Task.Delay(1500);
        
        // Update balance
        CurrentBalance += randomAmount;
        
        // Add transaction
        _transactions.Add(new Transaction 
        { 
            FromUser = "NFC Card", 
            ToUser = CurrentUsername, 
            Amount = randomAmount, 
            Description = "NFC Card Payment", 
            CreatedAt = DateTime.UtcNow 
        });
        
        OnPropertyChanged(nameof(Transactions));
        
        NFCStatus = $"✅ Payment received: {randomAmount} Karats";
        IsNFCProcessing = false;
        
        await Task.Delay(2000);
        IsNFCSectionVisible = false;
    }
    
    private async Task SendPayment()
    {
        if (PaymentAmount > 0 && !string.IsNullOrWhiteSpace(RecipientUsername))
        {
            PaymentStatus = "⏳ Checking recipient...";
            
            // Check if recipient exists in Firebase using the initialized auth service
            var recipient = _authService.GetUserByUsername(RecipientUsername);
            
            if (recipient == null)
            {
                PaymentStatus = $"❌ User '{RecipientUsername}' does not exist";
                return;
            }

            // Check sender's balance
            if (PaymentAmount > CurrentBalance)
            {
                PaymentStatus = "❌ Insufficient balance";
                return;
            }

            PaymentStatus = "⏳ Processing payment...";
            
            // Update sender's balance in Firebase
            var newSenderBalance = CurrentBalance - PaymentAmount;
            _authService.UpdateUserBalance(CurrentUsername, newSenderBalance);
            
            // Update local balance
            CurrentBalance = newSenderBalance;
            
            // Update recipient's balance in Firebase
            var newRecipientBalance = recipient.Balance + PaymentAmount;
            _authService.UpdateUserBalance(RecipientUsername, newRecipientBalance);
            
            // Add transaction
            var newTransaction = new Transaction 
            { 
                FromUser = CurrentUsername, 
                ToUser = RecipientUsername, 
                Amount = PaymentAmount, 
                Description = string.IsNullOrWhiteSpace(PaymentDescription) 
                    ? $"Payment to {RecipientUsername}" 
                    : PaymentDescription, 
                CreatedAt = DateTime.UtcNow 
            };
            
            _transactions.Add(newTransaction);
            
            // Save transactions to Firebase
            _ = _firebaseService.SaveTransactionsAsync(_transactions);
            
            PaymentStatus = $"✅ Successfully sent {PaymentAmount} Karats to {RecipientUsername}";
            
            OnPropertyChanged(nameof(Transactions));
            OnPropertyChanged(nameof(BalanceDisplay));
            
            // Clear form
            RecipientUsername = string.Empty;
            PaymentAmount = 0;
            PaymentDescription = string.Empty;
            IsPaymentSectionVisible = false;
        }
    }
    
    private void CancelPayment()
    {
        IsPaymentSectionVisible = false;
    }
    
    private void CancelNFC()
    {
        IsNFCSectionVisible = false;
        IsNFCProcessing = false;
    }
}

public class Transaction
{
    public string FromUser { get; set; } = string.Empty;
    public string ToUser { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string Description { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}
