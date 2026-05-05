using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace KaratFlowAvalonia.ViewModels;

public partial class MainWindowViewModel : ViewModelBase
{
    // User data
    private decimal _currentBalance = 10000;
    private string _currentUsername = "admin";
    private string _currentAccountNumber = "KF000000001";
    
    // UI state
    private bool _isPaymentSectionVisible = false;
    private bool _isNFCSectionVisible = false;
    private string _recipientUsername = string.Empty;
    private decimal _paymentAmount = 0;
    private string _paymentDescription = string.Empty;
    private string _nfcStatus = "Ready to accept payment";
    private bool _isNFCProcessing = false;
    
    // Transactions
    private List<Transaction> _transactions = new List<Transaction>
    {
        new Transaction { FromUser = "admin", ToUser = "alice", Amount = 500, Description = "Welcome bonus", CreatedAt = DateTime.UtcNow.AddDays(-5) },
        new Transaction { FromUser = "alice", ToUser = "bob", Amount = 150, Description = "Coffee payment", CreatedAt = DateTime.UtcNow.AddDays(-1) },
        new Transaction { FromUser = "bob", ToUser = "charlie", Amount = 75, Description = "Lunch split", CreatedAt = DateTime.UtcNow.AddDays(-2) }
    };

    public string Greeting => "💎 Karat Flow - Digital Currency";
    public string Subtitle => "Cross-Platform Digital Currency System";
    public string WelcomeMessage => $"Welcome back, Admin User!";
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
        if (PaymentAmount > 0 && !string.IsNullOrWhiteSpace(RecipientUsername))
        {
            if (PaymentAmount > CurrentBalance)
            {
                // Show error message
                return;
            }

            // Update balance
            CurrentBalance -= PaymentAmount;
            
            // Add transaction
            _transactions.Add(new Transaction 
            { 
                FromUser = CurrentUsername, 
                ToUser = RecipientUsername, 
                Amount = PaymentAmount, 
                Description = string.IsNullOrWhiteSpace(PaymentDescription) 
                    ? $"Payment to {RecipientUsername}" 
                    : PaymentDescription, 
                CreatedAt = DateTime.UtcNow 
            });
            
            OnPropertyChanged(nameof(Transactions));
            
            // Clear form
            RecipientUsername = string.Empty;
            PaymentAmount = 0;
            PaymentDescription = string.Empty;
            IsPaymentSectionVisible = false;
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
}

public class Transaction
{
    public string FromUser { get; set; } = string.Empty;
    public string ToUser { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string Description { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}
