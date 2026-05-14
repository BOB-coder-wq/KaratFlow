using System;
using System.Security.Cryptography;
using System.Text;
using KaratFlowAvalonia.Services;

namespace KaratFlowAvalonia.Models
{
    public class User
    {
        public string Id { get; set; } = string.Empty;
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string AvatarUrl { get; set; } = string.Empty;
        public string GoogleId { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty;
        public bool IsLoggedIn { get; set; }
        public DateTime LastLogin { get; set; }

        public static User CreateGuest()
        {
            return new User
            {
                Id = "guest",
                Username = "Guest",
                Email = "guest@karatflow.local",
                AvatarUrl = GenerateGravatarUrl("guest@karatflow.local", "Guest"),
                IsLoggedIn = false,
                LastLogin = DateTime.UtcNow
            };
        }

        public static User CreateGoogleUser(string googleId, string email, string name, string picture)
        {
            return new User
            {
                Id = Guid.NewGuid().ToString(),
                Username = name.Replace(" ", "").ToLower(),
                Email = email,
                GoogleId = googleId,
                AvatarUrl = picture ?? GenerateGravatarUrl(email, name),
                IsLoggedIn = true,
                LastLogin = DateTime.UtcNow
            };
        }

        public static string GenerateGravatarUrl(string email, string name)
        {
            if (!string.IsNullOrEmpty(email))
            {
                // Create MD5 hash of email (lowercase, trimmed)
                var emailToHash = email.ToLower().Trim();
                var md5 = System.Security.Cryptography.MD5.Create();
                var hashBytes = md5.ComputeHash(System.Text.Encoding.UTF8.GetBytes(emailToHash));
                var sb = new System.Text.StringBuilder();
                for (int i = 0; i < hashBytes.Length; i++)
                {
                    sb.Append(hashBytes[i].ToString("x2"));
                }
                var emailHash = sb.ToString();
                
                // Get Gravatar API key from secure credential manager
                var credentialManager = new SecureCredentialManager(null);
                var gravatarApiKey = credentialManager.GravatarApiKey;
                
                if (!string.IsNullOrEmpty(gravatarApiKey))
                {
                    // Gravatar URL with real API key
                    return $"https://www.gravatar.com/avatar/{emailHash}?s=128&d=identicon&r=pg&apikey={gravatarApiKey}";
                }
                else
                {
                    // Gravatar URL with default avatar
                    return $"https://www.gravatar.com/avatar/{emailHash}?s=128&d=identicon&r=pg";
                }
            }
            else
            {
                // Fallback to UI Avatars if no email
                var encodedName = Uri.EscapeDataString(name);
                return $"https://ui-avatars.com/api/?name={encodedName}&size=128&background=random&color=fff";
            }
        }
    }
}
