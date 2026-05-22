using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using KaratFlowAvalonia.Models;

namespace KaratFlowAvalonia.Services
{
    public class LocalAuthService
    {
        private readonly Dictionary<string, User> _users;
        private User? _currentUser;
        private readonly FirebaseService _firebaseService;

        public LocalAuthService()
        {
            // Initialize secure credential manager
            var credentialManager = new SecureCredentialManager(null);
            
            // Initialize Firebase service with credentials from secure credential manager
            var firebaseUrl = credentialManager.FirebaseUrl;
            var firebaseSecret = credentialManager.FirebaseSecret;
            _firebaseService = new FirebaseService(firebaseUrl, firebaseSecret);
            
            // Load users from Firebase (async, but we'll do it synchronously for now)
            _users = new Dictionary<string, User>();
            _ = LoadUsersFromFirebaseAsync();
            
            _currentUser = null;
            
            Console.WriteLine($"🔐 LocalAuthService initialized with Firebase");
            Console.WriteLine($"👤 Gravatar ready: {credentialManager.HasGravatarCredentials}");
        }

        private async Task LoadUsersFromFirebaseAsync()
        {
            try
            {
                var users = await _firebaseService.LoadUsersAsync();
                foreach (var userPair in users)
                {
                    _users[userPair.Key] = userPair.Value;
                }
                Console.WriteLine($"💾 Loaded {_users.Count} users from Firebase");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"⚠️  Error loading users from Firebase: {ex.Message}");
            }
        }

        public async Task<LoginResponse> LoginAsync(string username, string password)
        {
            try
            {
                var userKey = username.ToLower();
                if (_users.ContainsKey(userKey))
                {
                    var user = _users[userKey];
                    if (VerifyPassword(password, user.PasswordHash))
                    {
                        user.IsLoggedIn = true;
                        user.LastLogin = DateTime.Now;
                        _currentUser = user;
                        _ = _firebaseService.SaveUserAsync(userKey, user);

                        return new LoginResponse 
                        { 
                            Success = true, 
                            Message = "Login successful",
                            User = user,
                            Token = GenerateToken(user)
                        };
                    }
                }

                return new LoginResponse 
                { 
                    Success = false, 
                    Message = "Invalid username or password" 
                };
            }
            catch (Exception ex)
            {
                return new LoginResponse 
                { 
                    Success = false, 
                    Message = $"Login failed: {ex.Message}" 
                };
            }
        }

        public async Task<LoginResponse> CreateAccountAsync(string username, string email, string password, string confirmPassword)
        {
            try
            {
                // Validation
                if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
                {
                    return new LoginResponse 
                    { 
                        Success = false, 
                        Message = "All fields are required" 
                    };
                }

                if (password != confirmPassword)
                {
                    return new LoginResponse 
                    { 
                        Success = false, 
                        Message = "Passwords do not match" 
                    };
                }

                if (password.Length < 6)
                {
                    return new LoginResponse 
                    { 
                        Success = false, 
                        Message = "Password must be at least 6 characters" 
                    };
                }

                var userKey = username.ToLower();
                if (_users.ContainsKey(userKey))
                {
                    return new LoginResponse 
                    { 
                        Success = false, 
                        Message = "Username already exists" 
                    };
                }

                // Create new user
                var newUser = new User
                {
                    Id = Guid.NewGuid().ToString(),
                    Username = username,
                    Email = email,
                    PasswordHash = HashPassword(password),
                    IsLoggedIn = true,
                    LastLogin = DateTime.Now,
                    AvatarUrl = User.GenerateGravatarUrl(email, username)
                };

                _users[userKey] = newUser;
                _currentUser = newUser;
                _ = _firebaseService.SaveUserAsync(userKey, newUser);

                return new LoginResponse 
                { 
                    Success = true, 
                    Message = "Account created successfully",
                    User = newUser,
                    Token = GenerateToken(newUser)
                };
            }
            catch (Exception ex)
            {
                return new LoginResponse 
                { 
                    Success = false, 
                    Message = $"Account creation failed: {ex.Message}" 
                };
            }
        }

        public async Task<LoginResponse> LoginWithGoogleAsync(string googleId, string email, string name, string picture)
        {
            try
            {
                // Check if user exists
                var existingUser = _users.Values.FirstOrDefault(u => u.GoogleId == googleId || u.Email == email);
                
                if (existingUser != null)
                {
                    // Update existing user
                    existingUser.IsLoggedIn = true;
                    existingUser.LastLogin = DateTime.Now;
                    if (!string.IsNullOrEmpty(picture))
                    {
                        existingUser.AvatarUrl = picture;
                    }
                    _currentUser = existingUser;

                    return new LoginResponse 
                    { 
                        Success = true, 
                        Message = "Google login successful",
                        User = existingUser,
                        Token = GenerateToken(existingUser)
                    };
                }
                else
                {
                    // Create new user from Google
                    var newUser = new User
                    {
                        Id = Guid.NewGuid().ToString(),
                        Username = name.Replace(" ", "").ToLower(),
                        Email = email,
                        GoogleId = googleId,
                        IsLoggedIn = true,
                        LastLogin = DateTime.Now,
                        AvatarUrl = picture ?? User.GenerateGravatarUrl(email, name)
                    };

                    _users[newUser.Username.ToLower()] = newUser;
                    _currentUser = newUser;
                    _ = _firebaseService.SaveUserAsync(newUser.Username.ToLower(), newUser);

                    return new LoginResponse 
                    { 
                        Success = true, 
                        Message = "Google account created successfully",
                        User = newUser,
                        Token = GenerateToken(newUser)
                    };
                }
            }
            catch (Exception ex)
            {
                return new LoginResponse 
                { 
                    Success = false, 
                    Message = $"Google login failed: {ex.Message}" 
                };
            }
        }

        public User? GetCurrentUser()
        {
            return _currentUser;
        }

        public void Logout()
        {
            if (_currentUser != null)
            {
                _currentUser.IsLoggedIn = false;
                var userKey = _currentUser.Username.ToLower();
                _ = _firebaseService.SaveUserAsync(userKey, _currentUser);
                _currentUser = null;
            }
        }

        public void UpdateUserBalance(string username, decimal newBalance)
        {
            var userKey = username.ToLower();
            if (_users.ContainsKey(userKey))
            {
                _users[userKey].Balance = newBalance;
                _ = _firebaseService.SaveUserAsync(userKey, _users[userKey]);
                Console.WriteLine($"💰 Updated balance for {username}: {newBalance} karats");
            }
        }

        public User? GetUserByUsername(string username)
        {
            var userKey = username.ToLower();
            
            // Always query Firebase to get the most up-to-date data
            try
            {
                var task = _firebaseService.GetUserAsync(userKey);
                if (task.Wait(TimeSpan.FromSeconds(5)) && task.Result != null)
                {
                    // Update cache
                    _users[userKey] = task.Result;
                    return task.Result;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading user from Firebase: {ex.Message}");
            }
            
            // Fallback to cache if Firebase query fails
            if (_users.ContainsKey(userKey))
            {
                return _users[userKey];
            }
            
            return null;
        }

        private string HashPassword(string password)
        {
            using var sha256 = SHA256.Create();
            var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
            return Convert.ToBase64String(hashedBytes);
        }

        private bool VerifyPassword(string password, string hashedPassword)
        {
            var hash = HashPassword(password);
            return hash == hashedPassword;
        }

        private string GenerateToken(User user)
        {
            // Simple token generation - in production, use JWT
            return $"token_{user.Id}_{DateTime.UtcNow.Ticks}";
        }
    }

    public class LoginResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public User? User { get; set; }
        public string Token { get; set; } = string.Empty;
    }
}
