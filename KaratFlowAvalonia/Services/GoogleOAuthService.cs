using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;

namespace KaratFlowAvalonia.Services
{
    public class GoogleOAuthService
    {
        private readonly HttpClient _httpClient;
        private readonly string _clientId;
        private readonly string _clientSecret;
        private readonly string _redirectUri;

        public GoogleOAuthService(IConfiguration configuration)
        {
            _httpClient = new HttpClient();
            _clientId = configuration["GoogleOAuth:ClientId"] ?? "";
            _clientSecret = configuration["GoogleOAuth:ClientSecret"] ?? "";
            _redirectUri = configuration["GoogleOAuth:RedirectUri"] ?? "http://localhost:5000/auth/google/callback";
        }

        public string GetAuthorizationUrl()
        {
            var scopes = new[] { "openid", "email", "profile" };
            var scopeParam = string.Join(" ", scopes);
            
            return $"https://accounts.google.com/o/oauth2/v2/auth?" +
                   $"client_id={_clientId}&" +
                   $"redirect_uri={Uri.EscapeDataString(_redirectUri)}&" +
                   $"response_type=code&" +
                   $"scope={Uri.EscapeDataString(scopeParam)}&" +
                   $"access_type=offline";
        }

        public async Task<GoogleUser?> ExchangeCodeForUserAsync(string authorizationCode)
        {
            try
            {
                // Exchange authorization code for access token
                var tokenResponse = await ExchangeCodeForTokenAsync(authorizationCode);
                if (tokenResponse == null)
                    return null;

                // Extract user info from ID token
                var userFromToken = ExtractUserFromIdToken(tokenResponse.IdToken);
                if (userFromToken != null)
                    return userFromToken;

                // If JWT parsing fails, try the userinfo API as fallback
                Console.WriteLine("JWT parsing failed, trying userinfo API as fallback");
                return await GetUserInfoAsync(tokenResponse.AccessToken);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Google OAuth error: {ex.Message}");
                return null;
            }
        }

        private async Task<GoogleTokenResponse?> ExchangeCodeForTokenAsync(string authorizationCode)
        {
            try
            {
                var tokenRequest = new Dictionary<string, string>
                {
                    ["client_id"] = _clientId,
                    ["client_secret"] = _clientSecret,
                    ["code"] = authorizationCode,
                    ["grant_type"] = "authorization_code",
                    ["redirect_uri"] = _redirectUri
                };

                var content = new FormUrlEncodedContent(tokenRequest);
                var response = await _httpClient.PostAsync("https://oauth2.googleapis.com/token", content);
                
                var responseContent = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"Token response status: {response.StatusCode}");
                Console.WriteLine($"Token response content: {responseContent}");
                
                if (!response.IsSuccessStatusCode)
                {
                    Console.WriteLine($"Token exchange failed: {response.StatusCode}");
                    return null;
                }

                return JsonSerializer.Deserialize<GoogleTokenResponse>(responseContent);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Token exchange exception: {ex.Message}");
                return null;
            }
        }

        private async Task<GoogleUser?> GetUserInfoAsync(string accessToken)
        {
            try
            {
                var request = new HttpRequestMessage(HttpMethod.Get, "https://www.googleapis.com/oauth2/v2/userinfo");
                request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);
                
                var response = await _httpClient.SendAsync(request);
                var responseContent = await response.Content.ReadAsStringAsync();
                
                Console.WriteLine($"User info response status: {response.StatusCode}");
                Console.WriteLine($"User info response content: {responseContent}");
                
                if (!response.IsSuccessStatusCode)
                {
                    Console.WriteLine($"User info request failed: {response.StatusCode}");
                    return null;
                }

                var googleUser = JsonSerializer.Deserialize<GoogleUserInfo>(responseContent);
                
                if (googleUser == null)
                {
                    Console.WriteLine("Failed to deserialize user info");
                    return null;
                }

                Console.WriteLine($"Got user: {googleUser.Email}");
                return new GoogleUser
                {
                    Id = googleUser.Id,
                    Email = googleUser.Email,
                    Name = googleUser.Name,
                    Picture = googleUser.Picture,
                    VerifiedEmail = googleUser.VerifiedEmail
                };
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Get user info exception: {ex.Message}");
                return null;
            }
        }

        private GoogleUser? ExtractUserFromIdToken(string idToken)
        {
            try
            {
                Console.WriteLine($"ID Token length: {idToken.Length}");
                Console.WriteLine($"ID Token first 100 chars: {idToken.Substring(0, Math.Min(100, idToken.Length))}");
                
                // JWT tokens have 3 parts: header.payload.signature
                var parts = idToken.Split('.');
                Console.WriteLine($"Token parts count: {parts.Length}");
                
                if (parts.Length != 3)
                {
                    Console.WriteLine($"Invalid ID token format: expected 3 parts, got {parts.Length}");
                    return null;
                }

                // Decode the payload (middle part)
                var payload = parts[1];
                Console.WriteLine($"Payload part length: {payload.Length}");
                Console.WriteLine($"Payload part first 100 chars: {payload.Substring(0, Math.Min(100, payload.Length))}");
                
                // Convert base64url to base64
                payload = payload.Replace('-', '+').Replace('_', '/');
                
                // Add padding if needed
                while (payload.Length % 4 != 0)
                {
                    payload += "=";
                }

                Console.WriteLine($"Final payload length: {payload.Length}");

                var payloadBytes = Convert.FromBase64String(payload);
                var payloadJson = Encoding.UTF8.GetString(payloadBytes);
                
                Console.WriteLine($"ID Token payload JSON: {payloadJson}");

                var tokenData = JsonSerializer.Deserialize<Dictionary<string, object>>(payloadJson);
                if (tokenData == null)
                {
                    Console.WriteLine("Failed to parse ID token payload");
                    return null;
                }

                // Extract user information
                var email = tokenData.GetValueOrDefault("email")?.ToString() ?? "";
                var name = tokenData.GetValueOrDefault("name")?.ToString() ?? "";
                var picture = tokenData.GetValueOrDefault("picture")?.ToString() ?? "";
                var sub = tokenData.GetValueOrDefault("sub")?.ToString() ?? "";
                var emailVerified = bool.Parse(tokenData.GetValueOrDefault("email_verified")?.ToString() ?? "false");

                Console.WriteLine($"Extracted user from ID token: {email} ({name})");

                return new GoogleUser
                {
                    Id = sub,
                    Email = email,
                    Name = name,
                    Picture = picture,
                    VerifiedEmail = emailVerified
                };
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error extracting user from ID token: {ex.Message}");
                return null;
            }
        }
    }

    public class GoogleTokenResponse
    {
        public string AccessToken { get; set; } = string.Empty;
        public string TokenType { get; set; } = string.Empty;
        public int ExpiresIn { get; set; }
        public string RefreshToken { get; set; } = string.Empty;
        public string Scope { get; set; } = string.Empty;
        public string IdToken { get; set; } = string.Empty;
    }

    public class GoogleUserInfo
    {
        public string Id { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Picture { get; set; } = string.Empty;
        public bool VerifiedEmail { get; set; }
    }

    public class GoogleUser
    {
        public string Id { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Picture { get; set; } = string.Empty;
        public bool VerifiedEmail { get; set; }
    }
}
