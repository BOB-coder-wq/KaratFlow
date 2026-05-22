using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Firebase.Database;
using Firebase.Database.Query;
using KaratFlowAvalonia.Models;
using KaratFlowAvalonia.ViewModels;

namespace KaratFlowAvalonia.Services
{
    public class FirebaseService
    {
        private readonly FirebaseClient _firebaseClient;
        private readonly string _usersPath = "users";
        private readonly string _transactionsPath = "transactions";

        public FirebaseService(string firebaseUrl, string firebaseSecret = "", string serviceAccountPath = "")
        {
            try
            {
                FirebaseOptions clientOptions;

                // Use service account if provided, otherwise use secret
                if (!string.IsNullOrEmpty(serviceAccountPath) && File.Exists(serviceAccountPath))
                {
                    // Read service account JSON and extract the private key
                    var serviceAccountJson = File.ReadAllText(serviceAccountPath);
                    var serviceAccount = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, string>>(serviceAccountJson);
                    
                    // For FirebaseDatabase.net, we need to use the database secret or Firebase Auth
                    // The service account is for Firebase Admin SDK, not FirebaseDatabase.net
                    // We'll use the secret as fallback
                    Console.WriteLine($"⚠️  Service account provided but FirebaseDatabase.net requires database secret");
                    Console.WriteLine($"   Using secret authentication instead");
                    
                    clientOptions = new FirebaseOptions
                    {
                        AuthTokenAsyncFactory = () => Task.FromResult(firebaseSecret)
                    };
                }
                else
                {
                    clientOptions = new FirebaseOptions
                    {
                        AuthTokenAsyncFactory = () => Task.FromResult(firebaseSecret)
                    };
                }

                _firebaseClient = new FirebaseClient(firebaseUrl, clientOptions);
                Console.WriteLine($"🔥 Firebase service initialized");
                Console.WriteLine($"   Firebase URL: {firebaseUrl}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"⚠️  Firebase initialization error: {ex.Message}");
                throw;
            }
        }

        public async Task SaveUsersAsync(Dictionary<string, User> users)
        {
            try
            {
                foreach (var userPair in users)
                {
                    await _firebaseClient
                        .Child(_usersPath)
                        .Child(userPair.Key)
                        .PutAsync(userPair.Value);
                }
                Console.WriteLine($"💾 Saved {users.Count} users to Firebase");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"⚠️  Error saving users to Firebase: {ex.Message}");
            }
        }

        public async Task<Dictionary<string, User>> LoadUsersAsync()
        {
            try
            {
                var users = await _firebaseClient
                    .Child(_usersPath)
                    .OnceSingleAsync<Dictionary<string, User>>();
                
                Console.WriteLine($"📂 Loaded {users?.Count ?? 0} users from Firebase");
                return users ?? new Dictionary<string, User>();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"⚠️  Error loading users from Firebase: {ex.Message}");
                return new Dictionary<string, User>();
            }
        }

        public async Task SaveTransactionsAsync(List<Transaction> transactions)
        {
            try
            {
                await _firebaseClient
                    .Child(_transactionsPath)
                    .PutAsync(transactions);
                
                Console.WriteLine($"💾 Saved {transactions.Count} transactions to Firebase");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"⚠️  Error saving transactions to Firebase: {ex.Message}");
            }
        }

        public async Task<List<Transaction>> LoadTransactionsAsync()
        {
            try
            {
                var transactions = await _firebaseClient
                    .Child(_transactionsPath)
                    .OnceSingleAsync<List<Transaction>>();
                
                Console.WriteLine($"📂 Loaded {transactions?.Count ?? 0} transactions from Firebase");
                return transactions ?? new List<Transaction>();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"⚠️  Error loading transactions from Firebase: {ex.Message}");
                return new List<Transaction>();
            }
        }

        public async Task SaveUserAsync(string username, User user)
        {
            try
            {
                await _firebaseClient
                    .Child(_usersPath)
                    .Child(username.ToLower())
                    .PutAsync(user);
                
                Console.WriteLine($"💾 Saved user {username} to Firebase");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"⚠️  Error saving user to Firebase: {ex.Message}");
            }
        }

        public async Task<User?> GetUserAsync(string username)
        {
            try
            {
                // Try to get all users and find the matching one
                var allUsers = await _firebaseClient
                    .Child(_usersPath)
                    .OnceSingleAsync<Dictionary<string, User>>();
                
                Console.WriteLine($"🔍 Loaded {allUsers?.Count ?? 0} users from Firebase");
                if (allUsers != null)
                {
                    foreach (var userKey in allUsers.Keys)
                    {
                        Console.WriteLine($"  - User key: {userKey}");
                    }
                }
                
                var searchKey = username.ToLower();
                Console.WriteLine($"🔍 Searching for user: {searchKey}");
                
                if (allUsers != null && allUsers.ContainsKey(searchKey))
                {
                    Console.WriteLine($"🔍 Found user: {username}");
                    return allUsers[searchKey];
                }
                
                Console.WriteLine($"🔍 User not found: {username}");
                return null;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"⚠️  Error loading user from Firebase: {ex.Message}");
                return null;
            }
        }

        public void ClearAllData()
        {
            try
            {
                _firebaseClient.Child(_usersPath).DeleteAsync();
                _firebaseClient.Child(_transactionsPath).DeleteAsync();
                Console.WriteLine("🗑️  Cleared all Firebase data");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"⚠️  Error clearing Firebase data: {ex.Message}");
            }
        }
    }
}
