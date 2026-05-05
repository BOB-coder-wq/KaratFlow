using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using KaratFlowAvalonia.Models;

namespace KaratFlowAvalonia.Services
{
    public class EmbeddedDatabaseService
    {
        private readonly string _databasePath;
        private readonly KaratFlowDbContext _context;
        private bool _isInitialized = false;

        public EmbeddedDatabaseService()
        {
            // Create database in user's local app data folder
            var appDataFolder = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "KaratFlow"
            );
            
            Directory.CreateDirectory(appDataFolder);
            _databasePath = Path.Combine(appDataFolder, "karatflow.db");
            
            // Configure SQLite context
            var optionsBuilder = new DbContextOptionsBuilder<KaratFlowDbContext>()
                .UseSqlite($"Data Source={_databasePath}");
                
            _context = new KaratFlowDbContext(optionsBuilder.Options);
        }

        public async Task<bool> InitializeAsync()
        {
            try
            {
                // Ensure database is created
                await _context.Database.EnsureCreatedAsync();
                
                // Seed initial data if database is empty
                if (!await _context.Users.AnyAsync())
                {
                    await SeedInitialDataAsync();
                }
                
                _isInitialized = true;
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Database initialization failed: {ex.Message}");
                return false;
            }
        }

        private async Task SeedInitialDataAsync()
        {
            // Create sample users
            var users = new List<User>
            {
                new User 
                { 
                    Username = "admin", 
                    Email = "admin@karatflow.com",
                    PasswordHash = "hashed_password",
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                },
                new User 
                { 
                    Username = "alice", 
                    Email = "alice@karatflow.com",
                    PasswordHash = "hashed_password",
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                },
                new User 
                { 
                    Username = "bob", 
                    Email = "bob@karatflow.com",
                    PasswordHash = "hashed_password",
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                }
            };

            await _context.Users.AddRangeAsync(users);
            await _context.SaveChangesAsync();

            // Create accounts for users
            var accounts = new List<Account>
            {
                new Account 
                { 
                    UserId = users[0].Id, 
                    AccountNumber = "KF000000001",
                    Balance = 10000m,
                    Currency = "KARAT",
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                },
                new Account 
                { 
                    UserId = users[1].Id, 
                    AccountNumber = "KF000000002",
                    Balance = 5000m,
                    Currency = "KARAT",
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                },
                new Account 
                { 
                    UserId = users[2].Id, 
                    AccountNumber = "KF000000003",
                    Balance = 2500m,
                    Currency = "KARAT",
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                }
            };

            await _context.Accounts.AddRangeAsync(accounts);
            await _context.SaveChangesAsync();

            // Create sample transactions
            var transactions = new List<Transaction>
            {
                new Transaction 
                { 
                    FromAccountId = accounts[0].Id,
                    ToAccountId = accounts[1].Id,
                    Amount = 500m,
                    Description = "Welcome bonus",
                    Status = TransactionStatus.Completed,
                    Type = TransactionType.Payment,
                    CreatedAt = DateTime.UtcNow.AddDays(-5)
                },
                new Transaction 
                { 
                    FromAccountId = accounts[1].Id,
                    ToAccountId = accounts[2].Id,
                    Amount = 150m,
                    Description = "Coffee payment",
                    Status = TransactionStatus.Completed,
                    Type = TransactionType.Payment,
                    CreatedAt = DateTime.UtcNow.AddDays(-1)
                },
                new Transaction 
                { 
                    FromAccountId = accounts[2].Id,
                    ToAccountId = accounts[1].Id,
                    Amount = 75m,
                    Description = "Lunch split",
                    Status = TransactionStatus.Completed,
                    Type = TransactionType.Payment,
                    CreatedAt = DateTime.UtcNow.AddDays(-2)
                }
            };

            await _context.Transactions.AddRangeAsync(transactions);
            await _context.SaveChangesAsync();

            // Create sample NFC cards
            var nfcCards = new List<NFCCard>
            {
                new NFCCard 
                { 
                    CardNumber = "KFC123456789",
                    CardUID = "A1B2C3D4E5F6",
                    AccountId = accounts[0].Id,
                    Status = NFCStatus.Active,
                    DailyLimit = 1000m,
                    DailyUsed = 0m,
                    LastResetDate = DateTime.UtcNow.Date,
                    ExpiresAt = DateTime.UtcNow.AddYears(2),
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                },
                new NFCCard 
                { 
                    CardNumber = "KFC987654321",
                    CardUID = "F6E5D4C3B2A1",
                    AccountId = accounts[1].Id,
                    Status = NFCStatus.Active,
                    DailyLimit = 1000m,
                    DailyUsed = 0m,
                    LastResetDate = DateTime.UtcNow.Date,
                    ExpiresAt = DateTime.UtcNow.AddYears(2),
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                }
            };

            await _context.NFCCards.AddRangeAsync(nfcCards);
            await _context.SaveChangesAsync();
        }

        public async Task<User?> GetUserByUsernameAsync(string username)
        {
            if (!_isInitialized) return null;
            return await _context.Users
                .Include(u => u.Accounts)
                .FirstOrDefaultAsync(u => u.Username == username);
        }

        public async Task<Account?> GetAccountByNumberAsync(string accountNumber)
        {
            if (!_isInitialized) return null;
            return await _context.Accounts
                .Include(a => a.User)
                .FirstOrDefaultAsync(a => a.AccountNumber == accountNumber);
        }

        public async Task<List<Transaction>> GetTransactionsAsync(int accountId, int limit = 50)
        {
            if (!_isInitialized) return new List<Transaction>();
            return await _context.Transactions
                .Where(t => t.FromAccountId == accountId || t.ToAccountId == accountId)
                .OrderByDescending(t => t.CreatedAt)
                .Take(limit)
                .Include(t => t.FromAccount)
                .Include(t => t.ToAccount)
                .ToListAsync();
        }

        public async Task<bool> ProcessPaymentAsync(int fromAccountId, int toAccountId, decimal amount, string description)
        {
            if (!_isInitialized) return false;

            using var transaction = await _context.Database.BeginTransactionAsync();
            
            try
            {
                var fromAccount = await _context.Accounts.FindAsync(fromAccountId);
                var toAccount = await _context.Accounts.FindAsync(toAccountId);

                if (fromAccount == null || toAccount == null || fromAccount.Balance < amount)
                {
                    await transaction.RollbackAsync();
                    return false;
                }

                // Update balances
                fromAccount.Balance -= amount;
                toAccount.Balance += amount;
                fromAccount.UpdatedAt = DateTime.UtcNow;
                toAccount.UpdatedAt = DateTime.UtcNow;

                // Create transaction record
                var transactionRecord = new Transaction
                {
                    FromAccountId = fromAccountId,
                    ToAccountId = toAccountId,
                    Amount = amount,
                    Description = description,
                    Status = TransactionStatus.Completed,
                    Type = TransactionType.Payment,
                    CreatedAt = DateTime.UtcNow
                };

                await _context.Transactions.AddAsync(transactionRecord);
                await _context.SaveChangesAsync();

                await transaction.CommitAsync();
                return true;
            }
            catch
            {
                await transaction.RollbackAsync();
                return false;
            }
        }

        public async Task<NFCCard?> GetNFCCardByNumberAsync(string cardNumber)
        {
            if (!_isInitialized) return null;
            return await _context.NFCCards
                .Include(c => c.Account)
                .ThenInclude(a => a.User)
                .FirstOrDefaultAsync(c => c.CardNumber == cardNumber);
        }

        public async Task<bool> ProcessNFCPaymentAsync(string cardNumber, decimal amount, int receivingAccountId)
        {
            if (!_isInitialized) return false;

            var card = await GetNFCCardByNumberAsync(cardNumber);
            if (card == null || card.Status != NFCStatus.Active)
            {
                return false;
            }

            // Check daily limit
            if (card.DailyUsed + amount > card.DailyLimit)
            {
                return false;
            }

            // Reset daily usage if it's a new day
            if (card.LastResetDate < DateTime.UtcNow.Date)
            {
                card.DailyUsed = 0;
                card.LastResetDate = DateTime.UtcNow.Date;
            }

            // Process payment
            var success = await ProcessPaymentAsync(card.AccountId, receivingAccountId, amount, "NFC Card Payment");
            
            if (success)
            {
                card.DailyUsed += amount;
                card.UpdatedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();
            }

            return success;
        }

        public async Task<List<NFCCard>> GetNFCCardsAsync(int accountId)
        {
            if (!_isInitialized) return new List<NFCCard>();
            return await _context.NFCCards
                .Where(c => c.AccountId == accountId)
                .OrderByDescending(c => c.CreatedAt)
                .ToListAsync();
        }

        public string GetDatabasePath() => _databasePath;

        public async Task<bool> BackupDatabaseAsync(string backupPath)
        {
            try
            {
                if (File.Exists(_databasePath))
                {
                    File.Copy(_databasePath, backupPath, true);
                    return true;
                }
                return false;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> RestoreDatabaseAsync(string backupPath)
        {
            try
            {
                if (File.Exists(backupPath))
                {
                    File.Copy(backupPath, _databasePath, true);
                    await _context.Database.EnsureCreatedAsync();
                    return true;
                }
                return false;
            }
            catch
            {
                return false;
            }
        }

        public void Dispose()
        {
            _context?.Dispose();
        }
    }
}
