using Microsoft.Extensions.Configuration;
using System;
using System.IO;
using System.Collections.Generic;

namespace KaratFlowAvalonia.Services
{
    public class SecureCredentialManager
    {
        private readonly IConfiguration _configuration;

        public SecureCredentialManager(IConfiguration configuration)
        {
            _configuration = configuration;
            LoadSecureCredentials();
            CreateSecureConfigTemplate();
            LoadEnvironmentCredentials();
            SaveSecureCredentials();
            Console.WriteLine($"🔐 Secure Credential Manager initialized");
            Console.WriteLine($"📁 Environment fallback enabled");
            Console.WriteLine($"🔒 Template-based config available");
            Console.WriteLine($"🌐 Real credentials loaded: {HasRealCredentials}");
            Console.WriteLine($"🔑 Google OAuth ready: {HasGoogleCredentials}");
            Console.WriteLine($"👤 Gravatar ready: {HasGravatarCredentials}");
        }

        public string GoogleClientId => GetSecureCredential("GoogleOAuth:ClientId", "YOUR_GOOGLE_CLIENT_ID");
        public string GoogleClientSecret => GetSecureCredential("GoogleOAuth:ClientSecret", "YOUR_GOOGLE_CLIENT_SECRET");
        public string GoogleRedirectUri => GetSecureCredential("GoogleOAuth:RedirectUri", "http://localhost:5000/callback");
        public string GravatarApiKey => GetSecureCredential("Gravatar:ApiKey", "YOUR_GRAVATAR_API_KEY");

        public bool HasRealCredentials => HasGoogleCredentials && HasGravatarCredentials;
        public bool HasGoogleCredentials => !string.IsNullOrEmpty(GoogleClientId) && GoogleClientId != "YOUR_GOOGLE_CLIENT_ID";
        public bool HasGravatarCredentials => !string.IsNullOrEmpty(GravatarApiKey) && GravatarApiKey != "YOUR_GRAVATAR_API_KEY";

        private string GetSecureCredential(string key, string fallback)
        {
            // Priority order: Environment > Secure File > appsettings.json > fallback
            return Environment.GetEnvironmentVariable(key) 
                   ?? GetSecureFileCredential(key, fallback) 
                   ?? GetAppsettingsCredential(key, fallback);
        }

        private string GetSecureFileCredential(string key, string fallback)
        {
            try
            {
                var secureFile = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".karatflow", "credentials.json");
                if (File.Exists(secureFile))
                {
                    var secureContent = File.ReadAllText(secureFile);
                    var credentials = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, string>>(secureContent);
                    return credentials?.GetValueOrDefault(key, fallback);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"⚠️  Error reading secure file: {ex.Message}");
            }
            return fallback;
        }

        private string GetAppsettingsCredential(string key, string fallback)
        {
            try
            {
                return _configuration[key] ?? fallback;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"⚠️  Error reading appsettings: {ex.Message}");
                return fallback;
            }
        }

        private void LoadSecureCredentials()
        {
            Console.WriteLine($"🔐 Loading secure credentials from multiple sources...");
        }

        private void LoadEnvironmentCredentials()
        {
            Console.WriteLine($"🌐 Checking environment variables...");
            Console.WriteLine($"   GOOGLE_CLIENT_ID: {(!string.IsNullOrEmpty(Environment.GetEnvironmentVariable("GOOGLE_CLIENT_ID")) ? "✅ Found" : "❌ Not found")}");
            Console.WriteLine($"   GOOGLE_CLIENT_SECRET: {(!string.IsNullOrEmpty(Environment.GetEnvironmentVariable("GOOGLE_CLIENT_SECRET")) ? "✅ Found" : "❌ Not found")}");
            Console.WriteLine($"   GRAVATAR_API_KEY: {(!string.IsNullOrEmpty(Environment.GetEnvironmentVariable("GRAVATAR_API_KEY")) ? "✅ Found" : "❌ Not found")}");
        }

        private void CreateSecureConfigTemplate()
        {
            try
            {
                var secureDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".karatflow");
                Directory.CreateDirectory(secureDir);

                var templateFile = Path.Combine(secureDir, "credentials.template.json");
                if (!File.Exists(templateFile))
                {
                    var template = new Dictionary<string, string>
                    {
                        ["GoogleOAuth:ClientId"] = "YOUR_GOOGLE_CLIENT_ID",
                        ["GoogleOAuth:ClientSecret"] = "YOUR_GOOGLE_CLIENT_SECRET",
                        ["GoogleOAuth:RedirectUri"] = "http://localhost:5000/callback",
                        ["Gravatar:ApiKey"] = "YOUR_GRAVATAR_API_KEY",
                        ["_comment"] = "Copy this file to credentials.json and fill in your real credentials"
                    };

                    File.WriteAllText(templateFile, System.Text.Json.JsonSerializer.Serialize(template, new System.Text.Json.JsonSerializerOptions { WriteIndented = true }));
                    Console.WriteLine($"📄 Created template: {templateFile}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"⚠️  Error creating template: {ex.Message}");
            }
        }

        private void SaveSecureCredentials()
        {
            if (HasRealCredentials)
            {
                try
                {
                    var secureFile = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".karatflow", "credentials.json");
                    var credentials = new Dictionary<string, string>
                    {
                        ["GoogleOAuth:ClientId"] = GoogleClientId,
                        ["GoogleOAuth:ClientSecret"] = GoogleClientSecret,
                        ["GoogleOAuth:RedirectUri"] = GoogleRedirectUri,
                        ["Gravatar:ApiKey"] = GravatarApiKey,
                        ["_lastUpdated"] = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssZ")
                    };

                    File.WriteAllText(secureFile, System.Text.Json.JsonSerializer.Serialize(credentials, new System.Text.Json.JsonSerializerOptions { WriteIndented = true }));
                    Console.WriteLine($"💾 Secure credentials saved to: {secureFile}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"⚠️  Error saving secure credentials: {ex.Message}");
                }
            }
        }
    }
}
