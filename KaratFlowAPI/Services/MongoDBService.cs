using Microsoft.Extensions.Options;
using MongoDB.Driver;
using KaratFlowAPI.Models;
using System.Security.Cryptography;
using System.Text;

namespace KaratFlowAPI.Services
{
    public interface IKaratFlowService
    {
        Task<User?> GetUserByUsernameAsync(string username);
        Task<User?> CreateUserAsync(User user);
        Task<Account?> GetAccountByNumberAsync(string accountNumber);
        Task<List<Account>> GetUserAccountsAsync(string userId);
        Task<List<Transaction>> GetTransactionsAsync(string accountId, int limit = 50);
        Task<bool> ProcessPaymentAsync(string fromAccountId, string toAccountId, decimal amount, string description);
        Task<bool> ProcessNFCPaymentAsync(string cardNumber, decimal amount, string receivingAccountId);
        Task<NFCCard?> GetNFCCardByNumberAsync(string cardNumber);
        Task<List<NFCCard>> GetNFCCardsAsync(string accountId);
        Task<bool> UpdateAccountBalanceAsync(string accountId, decimal newBalance);
        Task<bool> CreateTransactionAsync(Transaction transaction);
    }

    public class MongoDBService : IKaratFlowService
    {
        private readonly IMongoDatabase _database;
        private readonly IMongoCollection<User> _users;
        private readonly IMongoCollection<Account> _accounts;
        private readonly IMongoCollection<Transaction> _transactions;
        private readonly IMongoCollection<NFCCard> _nfcCards;

        public MongoDBService(IOptions<MongoDBSettings> settings)
        {
            var client = new MongoClient(settings.Value.ConnectionString);
            _database = client.GetDatabase(settings.Value.DatabaseName);
            _users = _database.GetCollection<User>("users");
            _accounts = _database.GetCollection<Account>("accounts");
            _transactions = _database.GetCollection<Transaction>("transactions");
            _nfcCards = _database.GetCollection<NFCCard>("nfccards");
        }

        public async Task<User?> GetUserByUsernameAsync(string username)
        {
            return await _users.Find(u => u.Username == username.ToLower()).FirstOrDefaultAsync();
        }

        public async Task<User?> CreateUserAsync(User user)
        {
            user.Username = user.Username.ToLower();
            user.CreatedAt = DateTime.UtcNow;
            user.UpdatedAt = DateTime.UtcNow;
            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(user.PasswordHash);

            await _users.InsertOneAsync(user);
            return user;
        }

        public async Task<Account?> GetAccountByNumberAsync(string accountNumber)
        {
            var account = await _accounts.Find(a => a.AccountNumber == accountNumber).FirstOrDefaultAsync();
            if (account != null)
            {
                var user = await GetUserByUsernameAsync(account.UserId);
                account.User = user;
            }
            return account;
        }

        public async Task<List<Account>> GetUserAccountsAsync(string userId)
        {
            var accounts = await _accounts.Find(a => a.UserId == userId).ToListAsync();
            return accounts;
        }

        public async Task<List<Transaction>> GetTransactionsAsync(string accountId, int limit = 50)
        {
            var transactions = await _transactions
                .Find(t => t.FromAccountId == accountId || t.ToAccountId == accountId)
                .SortByDescending(t => t.CreatedAt)
                .Limit(limit)
                .ToListAsync();

            // Populate navigation properties
            foreach (var transaction in transactions)
            {
                transaction.FromAccount = await GetAccountByNumberAsync(transaction.FromAccountId);
                transaction.ToAccount = await GetAccountByNumberAsync(transaction.ToAccountId);
            }

            return transactions;
        }

        public async Task<bool> ProcessPaymentAsync(string fromAccountId, string toAccountId, decimal amount, string description)
        {
            using var session = await _database.Client.StartSessionAsync();
            session.StartTransaction();

            try
            {
                var fromAccount = await _accounts.Find(a => a.Id == fromAccountId).FirstOrDefaultAsync();
                var toAccount = await _accounts.Find(a => a.Id == toAccountId).FirstOrDefaultAsync();

                if (fromAccount == null || toAccount == null || fromAccount.Balance < amount)
                {
                    await session.AbortTransactionAsync();
                    return false;
                }

                // Update balances
                var updateFrom = Builders<Account>.Update
                    .Set(a => a.Balance, fromAccount.Balance - amount)
                    .Set(a => a.UpdatedAt, DateTime.UtcNow);

                var updateTo = Builders<Account>.Update
                    .Set(a => a.Balance, toAccount.Balance + amount)
                    .Set(a => a.UpdatedAt, DateTime.UtcNow);

                await _accounts.UpdateOneAsync(session, a => a.Id == fromAccountId, updateFrom);
                await _accounts.UpdateOneAsync(session, a => a.Id == toAccountId, updateTo);

                // Create transaction record
                var transaction = new Transaction
                {
                    FromAccountId = fromAccountId,
                    ToAccountId = toAccountId,
                    Amount = amount,
                    Description = description,
                    Type = TransactionType.Payment,
                    Status = TransactionStatus.Completed,
                    CreatedAt = DateTime.UtcNow,
                    ProcessedAt = DateTime.UtcNow
                };

                await _transactions.InsertOneAsync(session, transaction);

                await session.CommitTransactionAsync();
                return true;
            }
            catch
            {
                await session.AbortTransactionAsync();
                return false;
            }
        }

        public async Task<bool> ProcessNFCPaymentAsync(string cardNumber, decimal amount, string receivingAccountId)
        {
            var card = await GetNFCCardByNumberAsync(cardNumber);
            if (card == null || card.Status != NFCStatus.Active)
            {
                return false;
            }

            // Check daily limit
            if (card.LastResetDate.Date < DateTime.UtcNow.Date)
            {
                // Reset daily usage
                var updateCard = Builders<NFCCard>.Update
                    .Set(c => c.DailyUsed, 0)
                    .Set(c => c.LastResetDate, DateTime.UtcNow.Date);

                await _nfcCards.UpdateOneAsync(c => c.Id == card.Id, updateCard);
                card.DailyUsed = 0;
            }

            if (card.DailyUsed + amount > card.DailyLimit)
            {
                return false;
            }

            // Process payment
            var success = await ProcessPaymentAsync(card.AccountId, receivingAccountId, amount, "NFC Card Payment");
            
            if (success)
            {
                // Update card daily usage
                var updateUsage = Builders<NFCCard>.Update
                    .Set(c => c.DailyUsed, card.DailyUsed + amount)
                    .Set(c => c.UpdatedAt, DateTime.UtcNow);

                await _nfcCards.UpdateOneAsync(c => c.Id == card.Id, updateUsage);
            }

            return success;
        }

        public async Task<NFCCard?> GetNFCCardByNumberAsync(string cardNumber)
        {
            var card = await _nfcCards.Find(c => c.CardNumber == cardNumber).FirstOrDefaultAsync();
            if (card != null)
            {
                card.Account = await GetAccountByNumberAsync(card.AccountId);
            }
            return card;
        }

        public async Task<List<NFCCard>> GetNFCCardsAsync(string accountId)
        {
            var cards = await _nfcCards.Find(c => c.AccountId == accountId).ToListAsync();
            return cards;
        }

        public async Task<bool> UpdateAccountBalanceAsync(string accountId, decimal newBalance)
        {
            var update = Builders<Account>.Update
                .Set(a => a.Balance, newBalance)
                .Set(a => a.UpdatedAt, DateTime.UtcNow);

            var result = await _accounts.UpdateOneAsync(a => a.Id == accountId, update);
            return result.IsAcknowledged;
        }

        public async Task<bool> CreateTransactionAsync(Transaction transaction)
        {
            transaction.CreatedAt = DateTime.UtcNow;
            transaction.ProcessedAt = DateTime.UtcNow;
            
            await _transactions.InsertOneAsync(transaction);
            return true;
        }

        // Seed initial data
        public async Task SeedInitialDataAsync()
        {
            // Check if data exists
            var existingUsers = await _users.CountDocumentsAsync(FilterDefinition<User>.Empty);
            if (existingUsers > 0) return;

            // Create sample users
            var users = new List<User>
            {
                new User 
                { 
                    Username = "admin", 
                    Email = "admin@karatflow.com",
                    PasswordHash = "password123",
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                },
                new User 
                { 
                    Username = "alice", 
                    Email = "alice@karatflow.com",
                    PasswordHash = "password123",
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                },
                new User 
                { 
                    Username = "bob", 
                    Email = "bob@karatflow.com",
                    PasswordHash = "password123",
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                },
                new User 
                { 
                    Username = "charlie", 
                    Email = "charlie@karatflow.com",
                    PasswordHash = "password123",
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                }
            };

            await _users.InsertManyAsync(users);

            // Create accounts
            var accounts = new List<Account>
            {
                new Account 
                { 
                    UserId = users[0].Id!, 
                    AccountNumber = "KF000000001",
                    Balance = 10000m,
                    Currency = "KARAT",
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                },
                new Account 
                { 
                    UserId = users[1].Id!, 
                    AccountNumber = "KF000000002",
                    Balance = 5000m,
                    Currency = "KARAT",
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                },
                new Account 
                { 
                    UserId = users[2].Id!, 
                    AccountNumber = "KF000000003",
                    Balance = 2500m,
                    Currency = "KARAT",
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                },
                new Account 
                { 
                    UserId = users[3].Id!, 
                    AccountNumber = "KF000000004",
                    Balance = 7500m,
                    Currency = "KARAT",
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                }
            };

            await _accounts.InsertManyAsync(accounts);

            // Create sample transactions
            var transactions = new List<Transaction>
            {
                new Transaction 
                { 
                    FromAccountId = accounts[0].Id!,
                    ToAccountId = accounts[1].Id!,
                    Amount = 500m,
                    Description = "Welcome bonus",
                    Status = TransactionStatus.Completed,
                    Type = TransactionType.Payment,
                    CreatedAt = DateTime.UtcNow.AddDays(-5),
                    ProcessedAt = DateTime.UtcNow.AddDays(-5)
                },
                new Transaction 
                { 
                    FromAccountId = accounts[1].Id!,
                    ToAccountId = accounts[2].Id!,
                    Amount = 150m,
                    Description = "Coffee payment",
                    Status = TransactionStatus.Completed,
                    Type = TransactionType.Payment,
                    CreatedAt = DateTime.UtcNow.AddDays(-1),
                    ProcessedAt = DateTime.UtcNow.AddDays(-1)
                },
                new Transaction 
                { 
                    FromAccountId = accounts[2].Id!,
                    ToAccountId = accounts[3].Id!,
                    Amount = 75m,
                    Description = "Lunch split",
                    Status = TransactionStatus.Completed,
                    Type = TransactionType.Payment,
                    CreatedAt = DateTime.UtcNow.AddDays(-2),
                    ProcessedAt = DateTime.UtcNow.AddDays(-2)
                }
            };

            await _transactions.InsertManyAsync(transactions);

            // Create sample NFC cards
            var nfcCards = new List<NFCCard>
            {
                new NFCCard 
                { 
                    CardNumber = "KFC123456789",
                    CardUID = "A1B2C3D4E5F6",
                    AccountId = accounts[0].Id!,
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
                    AccountId = accounts[1].Id!,
                    Status = NFCStatus.Active,
                    DailyLimit = 1000m,
                    DailyUsed = 0m,
                    LastResetDate = DateTime.UtcNow.Date,
                    ExpiresAt = DateTime.UtcNow.AddYears(2),
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                }
            };

            await _nfcCards.InsertManyAsync(nfcCards);
        }
    }

    public class MongoDBSettings
    {
        public string ConnectionString { get; set; } = string.Empty;
        public string DatabaseName { get; set; } = "KaratFlowDB";
    }
}
