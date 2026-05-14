using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using KaratFlowAvalonia.Models;
using KaratFlowAvalonia.ViewModels;

namespace KaratFlowAvalonia.Services
{
    public class PersistenceService
    {
        private readonly string _dataDirectory;
        private readonly string _usersFile;
        private readonly string _transactionsFile;

        public PersistenceService()
        {
            _dataDirectory = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "KaratFlow");
            
            Directory.CreateDirectory(_dataDirectory);
            
            _usersFile = Path.Combine(_dataDirectory, "users.json");
            _transactionsFile = Path.Combine(_dataDirectory, "transactions.json");
            
            Console.WriteLine($"📁 Persistence service initialized");
            Console.WriteLine($"   Data directory: {_dataDirectory}");
        }

        public void SaveUsers(Dictionary<string, User> users)
        {
            try
            {
                var json = JsonSerializer.Serialize(users, new JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText(_usersFile, json);
                Console.WriteLine($"💾 Saved {users.Count} users to storage");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"⚠️  Error saving users: {ex.Message}");
            }
        }

        public Dictionary<string, User> LoadUsers()
        {
            try
            {
                if (File.Exists(_usersFile))
                {
                    var json = File.ReadAllText(_usersFile);
                    var users = JsonSerializer.Deserialize<Dictionary<string, User>>(json);
                    Console.WriteLine($"📂 Loaded {users?.Count ?? 0} users from storage");
                    return users ?? new Dictionary<string, User>();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"⚠️  Error loading users: {ex.Message}");
            }
            return new Dictionary<string, User>();
        }

        public void SaveTransactions(List<Transaction> transactions)
        {
            try
            {
                var json = JsonSerializer.Serialize(transactions, new JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText(_transactionsFile, json);
                Console.WriteLine($"💾 Saved {transactions.Count} transactions to storage");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"⚠️  Error saving transactions: {ex.Message}");
            }
        }

        public List<Transaction> LoadTransactions()
        {
            try
            {
                if (File.Exists(_transactionsFile))
                {
                    var json = File.ReadAllText(_transactionsFile);
                    var transactions = JsonSerializer.Deserialize<List<Transaction>>(json);
                    Console.WriteLine($"📂 Loaded {transactions?.Count ?? 0} transactions from storage");
                    return transactions ?? new List<Transaction>();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"⚠️  Error loading transactions: {ex.Message}");
            }
            return new List<Transaction>();
        }

        public void ClearAllData()
        {
            try
            {
                if (File.Exists(_usersFile))
                    File.Delete(_usersFile);
                
                if (File.Exists(_transactionsFile))
                    File.Delete(_transactionsFile);
                
                Console.WriteLine("🗑️  Cleared all persistent data");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"⚠️  Error clearing data: {ex.Message}");
            }
        }
    }
}
