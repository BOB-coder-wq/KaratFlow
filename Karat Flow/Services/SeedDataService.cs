using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Karat_Flow.Data;
using Karat_Flow.Models;
using Microsoft.EntityFrameworkCore;

namespace Karat_Flow.Services
{
    public class SeedDataService
    {
        private readonly KaratFlowDbContext _context;

        public SeedDataService(KaratFlowDbContext context)
        {
            _context = context;
        }

        public async Task SeedSampleDataAsync()
        {
            // Check if data already exists
            var existingUsers = await _context.Users.ToListAsync();
            if (existingUsers.Count > 1) // We already have admin user
            {
                return; // Data already seeded
            }

            // Create sample users
            var users = new[]
            {
                new User
                {
                    Username = "alice",
                    Email = "alice@karatflow.com",
                    FirstName = "Alice",
                    LastName = "Smith",
                    CreatedAt = DateTime.UtcNow
                },
                new User
                {
                    Username = "bob",
                    Email = "bob@karatflow.com",
                    FirstName = "Bob",
                    LastName = "Johnson",
                    CreatedAt = DateTime.UtcNow
                },
                new User
                {
                    Username = "charlie",
                    Email = "charlie@karatflow.com",
                    FirstName = "Charlie",
                    LastName = "Brown",
                    CreatedAt = DateTime.UtcNow
                }
            };

            foreach (var user in users)
            {
                _context.Users.Add(user);
            }

            await _context.SaveChangesAsync();

            // Create accounts for users
            var accounts = new List<Account>();
            foreach (var user in users)
            {
                accounts.Add(new Account
                {
                    UserId = user.Id,
                    AccountNumber = $"KF{user.Id:D9}",
                    Balance = new Random().Next(1000, 5000), // Random balance between 1000-5000
                    CreatedAt = DateTime.UtcNow,
                    LastUpdated = DateTime.UtcNow
                });
            }

            _context.Accounts.AddRange(accounts);
            await _context.SaveChangesAsync();

            // Create some sample transactions
            var sampleTransactions = new[]
            {
                new Transaction
                {
                    FromAccountId = 1, // Admin account
                    ToAccountId = accounts[0].Id, // Alice
                    Amount = 500,
                    Description = "Welcome bonus",
                    Type = TransactionType.Payment,
                    Status = TransactionStatus.Completed,
                    CompletedAt = DateTime.UtcNow
                },
                new Transaction
                {
                    FromAccountId = accounts[0].Id, // Alice
                    ToAccountId = accounts[1].Id, // Bob
                    Amount = 150,
                    Description = "Coffee payment",
                    Type = TransactionType.Payment,
                    Status = TransactionStatus.Completed,
                    CompletedAt = DateTime.UtcNow.AddDays(-1)
                },
                new Transaction
                {
                    FromAccountId = accounts[1].Id, // Bob
                    ToAccountId = accounts[2].Id, // Charlie
                    Amount = 75,
                    Description = "Lunch split",
                    Type = TransactionType.Transfer,
                    Status = TransactionStatus.Completed,
                    CompletedAt = DateTime.UtcNow.AddDays(-2)
                }
            };

            _context.Transactions.AddRange(sampleTransactions);
            await _context.SaveChangesAsync();

            // Create NFC cards for users
            var nfcCards = new List<NFCCard>();
            foreach (var user in users)
            {
                nfcCards.Add(new NFCCard
                {
                    UserId = user.Id,
                    CardNumber = $"KFC{new Random().Next(100000000, 999999999)}",
                    UID = GenerateRandomUID(),
                    Status = NFCStatus.Active,
                    IssuedAt = DateTime.UtcNow,
                    LastUsed = DateTime.UtcNow,
                    DailyLimit = 1000,
                    CurrentDailyAmount = 0,
                    ExpiresAt = DateTime.UtcNow.AddYears(3)
                });
            }

            _context.NFCCards.AddRange(nfcCards);
            await _context.SaveChangesAsync();
        }

        private string GenerateRandomUID()
        {
            var bytes = new byte[8];
            new Random().NextBytes(bytes);
            return Convert.ToHexString(bytes).ToLower();
        }
    }
}
