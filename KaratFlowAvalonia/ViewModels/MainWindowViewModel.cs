using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using KaratFlowAvalonia.Services;
using KaratFlowAvalonia.Models;

namespace KaratFlowAvalonia.ViewModels;

public partial class MainWindowViewModel : ViewModelBase
{
    // Database service
    private readonly EmbeddedDatabaseService _databaseService;
    
    // NFC Service
    private readonly INFCService _nfcService;
    
    // User data
    private User? _currentUser;
    private Account? _currentAccount;
    
    // UI state
    private bool _isPaymentSectionVisible = false;
    private bool _isNFCSectionVisible = false;
    private string _recipientUsername = string.Empty;
    private decimal _paymentAmount = 0;
    private string _paymentDescription = string.Empty;
    private string _nfcStatus = "Ready to accept payment";
    private bool _isNFCProcessing = false;
    private bool _useRealNFC = true; // Toggle for real vs simulated NFC
    
    // Transactions
    private List<Transaction> _transactions = new List<Transaction>();

    public string Greeting => "💎 Karat Flow - Digital Currency";
    public string Subtitle => "Self-Contained Digital Currency System";
    public string WelcomeMessage => $"Welcome back, {_currentUser?.Username ?? "User"}!";
    public string BalanceDisplay => $"{CurrentBalance:N0} Karats";
    public string AccountDisplay => $"Account: {CurrentAccountNumber}";
    public string DatabasePath => _databaseService.GetDatabasePath();
    
    public decimal CurrentBalance
    {
        get => _currentAccount?.Balance ?? 0;
        set 
        { 
            if (_currentAccount != null)
            {
                _currentAccount.Balance = value;
                SetProperty(ref _currentAccount.Balance, value);
            }
        }
    }
    
    public string CurrentUsername => _currentUser?.Username ?? "Unknown";
    public string CurrentAccountNumber => _currentAccount?.AccountNumber ?? "Unknown";
    
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
    
    public MainWindowViewModel()
    {
        _databaseService = new EmbeddedDatabaseService();
        _nfcService = NFCServiceFactory.CreateNFCService();
        
        // Subscribe to NFC service events
        _nfcService.CardDetected += OnCardDetected;
        _nfcService.StatusChanged += OnNFCStatusChanged;
        _nfcService.ErrorOccurred += OnNFCErrorOccurred;
        
        // Initialize services
        _ = InitializeServicesAsync();
    }
    
    private async Task InitializeServicesAsync()
    {
        try
        {
            // Initialize database
            await _databaseService.InitializeAsync();
            
            // Load current user (admin for demo)
            _currentUser = await _databaseService.GetUserByUsernameAsync("admin");
            if (_currentUser != null)
            {
                _currentAccount = _currentUser.Accounts.FirstOrDefault();
            }
            
            // Load transactions
            if (_currentAccount != null)
            {
                _transactions = await _databaseService.GetTransactionsAsync(_currentAccount.Id);
            }
            
            // Initialize NFC service
            await _nfcService.InitializeAsync();
            
            // Notify UI of changes
            OnPropertyChanged(nameof(CurrentBalance));
            OnPropertyChanged(nameof(CurrentUsername));
            OnPropertyChanged(nameof(CurrentAccountNumber));
            OnPropertyChanged(nameof(Transactions));
        }
        catch (Exception ex)
        {
            // Fall back to simulated NFC if real NFC fails
            UseRealNFC = false;
            NFCStatus = "Real NFC unavailable, using simulation";
        }
    }
    
    private void OnCardDetected(ServiceNFCCard card)
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
    
    [RelayCommand]
    private void ShowSendPayment()
    {
        IsPaymentSectionVisible = true;
        IsNFCSectionVisible = false;
    }
    
    [RelayCommand]
    private void ShowReceivePayment()
    {
        // Show account details dialog
    }
    
    [RelayCommand]
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
            
            if (card != null && _currentAccount != null)
            {
                NFCStatus = $"Processing payment from {card.CardNumber}...";
                
                // Process payment with database
                var success = await _databaseService.ProcessNFCPaymentAsync(
                    card.CardNumber, 
                    card.Balance, 
                    _currentAccount.Id
                );
                
                if (success)
                {
                    // Reload data
                    await ReloadUserDataAsync();
                    
                    NFCStatus = $"✅ Payment received: {card.Balance:N0} Karats";
                }
                else
                {
                    NFCStatus = $"❌ Payment failed";
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
    
    [RelayCommand]
    private async Task SendPayment()
    {
        if (PaymentAmount > 0 && !string.IsNullOrWhiteSpace(RecipientUsername) && _currentAccount != null)
        {
            if (PaymentAmount > CurrentBalance)
            {
                NFCStatus = "Insufficient balance!";
                return;
            }

            try
            {
                // Get recipient account
                var recipientUser = await _databaseService.GetUserByUsernameAsync(RecipientUsername);
                if (recipientUser == null)
                {
                    NFCStatus = "Recipient not found!";
                    return;
                }

                var recipientAccount = recipientUser.Accounts.FirstOrDefault();
                if (recipientAccount == null)
                {
                    NFCStatus = "Recipient has no account!";
                    return;
                }

                // Process payment through database
                var success = await _databaseService.ProcessPaymentAsync(
                    _currentAccount.Id, 
                    recipientAccount.Id, 
                    PaymentAmount, 
                    string.IsNullOrWhiteSpace(PaymentDescription) 
                        ? $"Payment to {RecipientUsername}" 
                        : PaymentDescription
                );

                if (success)
                {
                    // Reload data
                    await ReloadUserDataAsync();
                    
                    NFCStatus = $"✅ Successfully sent {PaymentAmount:N0} Karats to {RecipientUsername}!";
                    
                    // Clear form
                    RecipientUsername = string.Empty;
                    PaymentAmount = 0;
                    PaymentDescription = string.Empty;
                    IsPaymentSectionVisible = false;
                }
                else
                {
                    NFCStatus = "❌ Payment failed!";
                }
            }
            catch (Exception ex)
            {
                NFCStatus = $"❌ Error: {ex.Message}";
            }
        }
    }
    
    [RelayCommand]
    private void CancelPayment()
    {
        IsPaymentSectionVisible = false;
    }
    
    [RelayCommand]
    private void CancelNFC()
    {
        IsNFCSectionVisible = false;
        IsNFCProcessing = false;
    }
    
    private async Task ReloadUserDataAsync()
    {
        if (_currentAccount != null)
        {
            // Reload account data
            _currentAccount = await _databaseService.GetAccountByNumberAsync(_currentAccount.AccountNumber);
            
            // Reload transactions
            _transactions = await _databaseService.GetTransactionsAsync(_currentAccount.Id);
            
            // Notify UI of changes
            OnPropertyChanged(nameof(CurrentBalance));
            OnPropertyChanged(nameof(Transactions));
        }
    }
}
