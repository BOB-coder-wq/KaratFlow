using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using Karat_Flow.Data;
using Karat_Flow.Models;
using Microsoft.EntityFrameworkCore;

namespace Karat_Flow.ViewModels
{
    public class AccountViewModel : ViewModelBase
    {
        private readonly KaratFlowDbContext _context;
        private User? _currentUser;
        private Account? _currentAccount;
        private decimal _balance;
        private string _accountNumber = string.Empty;
        private ObservableCollection<Transaction> _transactions = new();
        private ObservableCollection<NFCCard> _nfcCards = new();
        private string _transactionDescription = string.Empty;
        private decimal _transactionAmount;
        private string _recipientUsername = string.Empty;
        private bool _isLoading;

        public AccountViewModel(KaratFlowDbContext context)
        {
            _context = context;
            LoadCurrentUserAsync();
        }

        public User? CurrentUser
        {
            get => _currentUser;
            set
            {
                _currentUser = value;
                OnPropertyChanged(nameof(CurrentUser));
                OnPropertyChanged(nameof(DisplayName));
            }
        }

        public Account? CurrentAccount
        {
            get => _currentAccount;
            set
            {
                _currentAccount = value;
                OnPropertyChanged(nameof(CurrentAccount));
            }
        }

        public decimal Balance
        {
            get => _balance;
            set
            {
                _balance = value;
                OnPropertyChanged(nameof(Balance));
                OnPropertyChanged(nameof(BalanceDisplay));
            }
        }

        public string BalanceDisplay => $"{Balance:N0} Karats";

        public string AccountNumber
        {
            get => _accountNumber;
            set
            {
                _accountNumber = value;
                OnPropertyChanged(nameof(AccountNumber));
            }
        }

        public string DisplayName => $"{CurrentUser?.FirstName} {CurrentUser?.LastName}".Trim();

        public ObservableCollection<Transaction> Transactions
        {
            get => _transactions;
            set
            {
                _transactions = value;
                OnPropertyChanged(nameof(Transactions));
            }
        }

        public ObservableCollection<NFCCard> NFCCards
        {
            get => _nfcCards;
            set
            {
                _nfcCards = value;
                OnPropertyChanged(nameof(NFCCards));
            }
        }

        public string TransactionDescription
        {
            get => _transactionDescription;
            set
            {
                _transactionDescription = value;
                OnPropertyChanged(nameof(TransactionDescription));
            }
        }

        public decimal TransactionAmount
        {
            get => _transactionAmount;
            set
            {
                _transactionAmount = value;
                OnPropertyChanged(nameof(TransactionAmount));
            }
        }

        public string RecipientUsername
        {
            get => _recipientUsername;
            set
            {
                _recipientUsername = value;
                OnPropertyChanged(nameof(RecipientUsername));
            }
        }

        public bool IsLoading
        {
            get => _isLoading;
            set
            {
                _isLoading = value;
                OnPropertyChanged(nameof(IsLoading));
            }
        }

        private async Task LoadCurrentUserAsync()
        {
            try
            {
                IsLoading = true;

                // For now, load the admin user. In a real app, this would be authentication-based
                var user = await _context.Users
                    .Include(u => u.Account)
                    .Include(u => u.NFCCards)
                    .FirstOrDefaultAsync(u => u.Username == "admin");

                if (user != null)
                {
                    CurrentUser = user;
                    CurrentAccount = user.Account;
                    Balance = user.Account?.Balance ?? 0;
                    AccountNumber = user.Account?.AccountNumber ?? string.Empty;

                    // Load NFC cards
                    NFCCards.Clear();
                    foreach (var card in user.NFCCards)
                    {
                        NFCCards.Add(card);
                    }

                    await LoadTransactionsAsync();
                }
            }
            catch (Exception ex)
            {
                // Handle error
                Console.WriteLine($"Error loading user: {ex.Message}");
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task LoadTransactionsAsync()
        {
            if (CurrentAccount == null) return;

            var transactions = await _context.Transactions
                .Include(t => t.FromAccount)
                .Include(t => t.ToAccount)
                .Where(t => t.FromAccountId == CurrentAccount.Id || t.ToAccountId == CurrentAccount.Id)
                .OrderByDescending(t => t.CreatedAt)
                .Take(50)
                .ToListAsync();

            Transactions.Clear();
            foreach (var transaction in transactions)
            {
                Transactions.Add(transaction);
            }
        }

        public async Task<bool> SendPaymentAsync()
        {
            if (CurrentAccount == null || string.IsNullOrWhiteSpace(RecipientUsername) || TransactionAmount <= 0)
                return false;

            try
            {
                IsLoading = true;

                // Find recipient
                var recipient = await _context.Users
                    .Include(u => u.Account)
                    .FirstOrDefaultAsync(u => u.Username == RecipientUsername);

                if (recipient?.Account == null)
                    return false;

                // Check balance
                if (Balance < TransactionAmount)
                    return false;

                // Create transaction
                var transaction = new Transaction
                {
                    FromAccountId = CurrentAccount.Id,
                    ToAccountId = recipient.Account.Id,
                    Amount = TransactionAmount,
                    Description = string.IsNullOrWhiteSpace(TransactionDescription) 
                        ? $"Payment to {RecipientUsername}" 
                        : TransactionDescription,
                    Type = TransactionType.Payment,
                    Status = TransactionStatus.Completed,
                    CompletedAt = DateTime.UtcNow
                };

                // Update balances
                CurrentAccount.Balance -= TransactionAmount;
                recipient.Account.Balance += TransactionAmount;

                _context.Transactions.Add(transaction);
                await _context.SaveChangesAsync();

                // Refresh data
                Balance = CurrentAccount.Balance;
                await LoadTransactionsAsync();

                // Clear form
                TransactionAmount = 0;
                TransactionDescription = string.Empty;
                RecipientUsername = string.Empty;

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error sending payment: {ex.Message}");
                return false;
            }
            finally
            {
                IsLoading = false;
            }
        }

        public async Task<bool> RequestNFCPaymentAsync(decimal amount, string description)
        {
            // This would be implemented when NFC integration is added
            // For now, just return true to simulate NFC payment acceptance
            await Task.Delay(1000); // Simulate processing time
            
            if (CurrentAccount != null)
            {
                CurrentAccount.Balance += amount;
                await _context.SaveChangesAsync();
                Balance = CurrentAccount.Balance;
                await LoadTransactionsAsync();
                
                return true;
            }
            
            return false;
        }
    }
}
