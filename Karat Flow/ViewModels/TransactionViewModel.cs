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
    public class TransactionViewModel : ViewModelBase
    {
        private readonly KaratFlowDbContext _context;
        private ObservableCollection<Transaction> _allTransactions = new();
        private ObservableCollection<Transaction> _filteredTransactions = new();
        private TransactionType? _selectedType;
        private TransactionStatus? _selectedStatus;
        private DateTime? _startDate;
        private DateTime? _endDate;
        private string _searchTerm = string.Empty;
        private bool _isLoading;

        public TransactionViewModel(KaratFlowDbContext context)
        {
            _context = context;
            LoadTransactionsAsync();
        }

        public ObservableCollection<Transaction> AllTransactions
        {
            get => _allTransactions;
            set
            {
                _allTransactions = value;
                OnPropertyChanged(nameof(AllTransactions));
            }
        }

        public ObservableCollection<Transaction> FilteredTransactions
        {
            get => _filteredTransactions;
            set
            {
                _filteredTransactions = value;
                OnPropertyChanged(nameof(FilteredTransactions));
            }
        }

        public TransactionType? SelectedType
        {
            get => _selectedType;
            set
            {
                _selectedType = value;
                OnPropertyChanged(nameof(SelectedType));
                FilterTransactions();
            }
        }

        public TransactionStatus? SelectedStatus
        {
            get => _selectedStatus;
            set
            {
                _selectedStatus = value;
                OnPropertyChanged(nameof(SelectedStatus));
                FilterTransactions();
            }
        }

        public DateTime? StartDate
        {
            get => _startDate;
            set
            {
                _startDate = value;
                OnPropertyChanged(nameof(StartDate));
                FilterTransactions();
            }
        }

        public DateTime? EndDate
        {
            get => _endDate;
            set
            {
                _endDate = value;
                OnPropertyChanged(nameof(EndDate));
                FilterTransactions();
            }
        }

        public string SearchTerm
        {
            get => _searchTerm;
            set
            {
                _searchTerm = value;
                OnPropertyChanged(nameof(SearchTerm));
                FilterTransactions();
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

        public Array TransactionTypes => Enum.GetValues(typeof(TransactionType));
        public Array TransactionStatuses => Enum.GetValues(typeof(TransactionStatus));

        private async Task LoadTransactionsAsync()
        {
            try
            {
                IsLoading = true;

                var transactions = await _context.Transactions
                    .Include(t => t.FromAccount)
                    .ThenInclude(a => a.User)
                    .Include(t => t.ToAccount)
                    .ThenInclude(a => a.User)
                    .Include(t => t.NFCCard)
                    .OrderByDescending(t => t.CreatedAt)
                    .Take(1000)
                    .ToListAsync();

                AllTransactions.Clear();
                foreach (var transaction in transactions)
                {
                    AllTransactions.Add(transaction);
                }

                FilterTransactions();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading transactions: {ex.Message}");
            }
            finally
            {
                IsLoading = false;
            }
        }

        private void FilterTransactions()
        {
            var filtered = AllTransactions.AsEnumerable();

            if (SelectedType.HasValue)
            {
                filtered = filtered.Where(t => t.Type == SelectedType.Value);
            }

            if (SelectedStatus.HasValue)
            {
                filtered = filtered.Where(t => t.Status == SelectedStatus.Value);
            }

            if (StartDate.HasValue)
            {
                filtered = filtered.Where(t => t.CreatedAt.Date >= StartDate.Value.Date);
            }

            if (EndDate.HasValue)
            {
                filtered = filtered.Where(t => t.CreatedAt.Date <= EndDate.Value.Date);
            }

            if (!string.IsNullOrWhiteSpace(SearchTerm))
            {
                var searchLower = SearchTerm.ToLower();
                filtered = filtered.Where(t => 
                    t.Description.ToLower().Contains(searchLower) ||
                    t.FromAccount.User.Username.ToLower().Contains(searchLower) ||
                    t.ToAccount.User.Username.ToLower().Contains(searchLower) ||
                    t.Amount.ToString().Contains(searchLower));
            }

            FilteredTransactions.Clear();
            foreach (var transaction in filtered)
            {
                FilteredTransactions.Add(transaction);
            }
        }

        public async Task RefreshTransactionsAsync()
        {
            await LoadTransactionsAsync();
        }

        public string GetTransactionDisplay(Transaction transaction)
        {
            var amount = $"{transaction.Amount:N0} Karats";
            var status = transaction.Status.ToString();
            var type = transaction.Type.ToString();
            
            var fromUser = transaction.FromAccount?.User?.Username ?? "Unknown";
            var toUser = transaction.ToAccount?.User?.Username ?? "Unknown";
            
            return $"{type}: {fromUser} → {toUser} | {amount} | {status}";
        }

        public string GetTransactionDescription(Transaction transaction)
        {
            var fromUser = transaction.FromAccount?.User?.Username ?? "Unknown";
            var toUser = transaction.ToAccount?.User?.Username ?? "Unknown";
            
            if (transaction.Type == TransactionType.Payment || transaction.Type == TransactionType.Transfer)
            {
                return $"{transaction.Description} (From {fromUser} to {toUser})";
            }
            else if (transaction.Type == TransactionType.NFC_Payment)
            {
                return $"NFC Payment: {transaction.Description}";
            }
            else
            {
                return transaction.Description;
            }
        }
    }
}
