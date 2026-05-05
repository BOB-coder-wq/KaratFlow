using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Karat_Flow.Data;
using Karat_Flow.Models;
using Microsoft.EntityFrameworkCore;

namespace Karat_Flow.Services
{
    public class NFCService
    {
        private readonly KaratFlowDbContext _context;
        private Random _random = new Random();

        public NFCService(KaratFlowDbContext context)
        {
            _context = context;
        }

        public event Action<NFCCard>? CardDetected;
        public event Action<string>? StatusChanged;

        public async Task<NFCCard?> SimulateCardDetectionAsync()
        {
            StatusChanged?.Invoke("Scanning for NFC cards...");

            // Simulate scanning delay
            await Task.Delay(2000);

            // Get a random existing NFC card or create a new one for simulation
            var existingCards = await _context.NFCCards.ToListAsync();
            
            NFCCard detectedCard;
            if (existingCards.Count > 0)
            {
                detectedCard = existingCards[_random.Next(existingCards.Count)];
            }
            else
            {
                // Create a simulated card for demonstration
                detectedCard = new NFCCard
                {
                    CardNumber = $"KFC{_random.Next(100000000, 999999999)}",
                    UID = GenerateRandomUID(),
                    Status = NFCStatus.Active,
                    IssuedAt = DateTime.UtcNow,
                    LastUsed = DateTime.UtcNow,
                    DailyLimit = 1000,
                    CurrentDailyAmount = 0
                };
            }

            StatusChanged?.Invoke($"Card detected: {detectedCard.CardNumber}");
            CardDetected?.Invoke(detectedCard);

            return detectedCard;
        }

        public async Task<bool> ProcessNFCPaymentAsync(NFCCard card, decimal amount, int receivingAccountId)
        {
            try
            {
                StatusChanged?.Invoke("Processing payment...");

                // Validate card status
                if (card.Status != NFCStatus.Active)
                {
                    StatusChanged?.Invoke("Card is not active");
                    return false;
                }

                // Check daily limit
                if (card.CurrentDailyAmount + amount > card.DailyLimit)
                {
                    StatusChanged?.Invoke("Daily limit exceeded");
                    return false;
                }

                // Get receiving account
                var receivingAccount = await _context.Accounts.FindAsync(receivingAccountId);
                if (receivingAccount == null)
                {
                    StatusChanged?.Invoke("Invalid receiving account");
                    return false;
                }

                // Create system account for NFC payments (simplified)
                var systemAccount = await GetOrCreateSystemAccountAsync();

                // Create NFC transaction
                var transaction = new Transaction
                {
                    FromAccountId = systemAccount.Id,
                    ToAccountId = receivingAccountId,
                    Amount = amount,
                    Description = $"NFC Payment from card {card.CardNumber}",
                    Type = TransactionType.NFC_Payment,
                    Status = TransactionStatus.Completed,
                    CompletedAt = DateTime.UtcNow,
                    NFCCardId = card.Id
                };

                // Update card daily usage
                card.CurrentDailyAmount += amount;
                card.LastUsed = DateTime.UtcNow;

                // Update receiving account balance
                receivingAccount.Balance += amount;

                // Save changes
                _context.Transactions.Add(transaction);
                await _context.SaveChangesAsync();

                StatusChanged?.Invoke($"Payment successful: {amount} Karats received");
                return true;
            }
            catch (Exception ex)
            {
                StatusChanged?.Invoke($"Payment failed: {ex.Message}");
                return false;
            }
        }

        private async Task<Account> GetOrCreateSystemAccountAsync()
        {
            var systemUser = await _context.Users
                .Include(u => u.Account)
                .FirstOrDefaultAsync(u => u.Username == "system");

            if (systemUser?.Account == null)
            {
                // Create system user and account
                systemUser = new User
                {
                    Username = "system",
                    Email = "system@karatflow.com",
                    FirstName = "System",
                    LastName = "Account",
                    CreatedAt = DateTime.UtcNow
                };

                var systemAccount = new Account
                {
                    AccountNumber = "KF000000000",
                    Balance = 999999999, // Large balance for system
                    CreatedAt = DateTime.UtcNow,
                    LastUpdated = DateTime.UtcNow
                };

                systemUser.Account = systemAccount;
                _context.Users.Add(systemUser);
                await _context.SaveChangesAsync();

                return systemAccount;
            }

            return systemUser.Account;
        }

        public async Task<List<NFCCard>> GetUserCardsAsync(int userId)
        {
            return await _context.NFCCards
                .Where(c => c.UserId == userId)
                .ToListAsync();
        }

        public async Task<NFCCard> CreateNFCCardAsync(int userId)
        {
            var card = new NFCCard
            {
                UserId = userId,
                CardNumber = $"KFC{_random.Next(100000000, 999999999)}",
                UID = GenerateRandomUID(),
                Status = NFCStatus.Active,
                IssuedAt = DateTime.UtcNow,
                LastUsed = DateTime.UtcNow,
                DailyLimit = 1000,
                CurrentDailyAmount = 0,
                ExpiresAt = DateTime.UtcNow.AddYears(3)
            };

            _context.NFCCards.Add(card);
            await _context.SaveChangesAsync();

            return card;
        }

        private string GenerateRandomUID()
        {
            var bytes = new byte[8];
            _random.NextBytes(bytes);
            return Convert.ToHexString(bytes).ToLower();
        }

        public void ResetDailyLimits()
        {
            // This would typically be called daily (e.g., via a scheduled service)
            var cards = _context.NFCCards.ToList();
            foreach (var card in cards)
            {
                card.CurrentDailyAmount = 0;
            }
            _context.SaveChanges();
        }
    }
}
