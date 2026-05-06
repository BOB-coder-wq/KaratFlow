using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using KaratFlowAPI.Models;
using KaratFlowAPI.Services;

namespace KaratFlowAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IKaratFlowService _karatFlowService;
        private readonly IConfiguration _configuration;

        public AuthController(IKaratFlowService karatFlowService, IConfiguration configuration)
        {
            _karatFlowService = karatFlowService;
            _configuration = configuration;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] User user)
        {
            try
            {
                // Check if user already exists
                var existingUser = await _karatFlowService.GetUserByUsernameAsync(user.Username);
                if (existingUser != null)
                {
                    return BadRequest(new { message = "Username already exists" });
                }

                // Create new user
                var createdUser = await _karatFlowService.CreateUserAsync(user);
                
                // Create default account for new user
                var account = new Account
                {
                    UserId = createdUser.Id!,
                    AccountNumber = GenerateAccountNumber(),
                    Balance = 1000m, // Welcome bonus
                    Currency = "KARAT",
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                return Ok(new { 
                    message = "User registered successfully",
                    userId = createdUser.Id,
                    username = createdUser.Username,
                    accountNumber = account.AccountNumber,
                    welcomeBonus = 1000
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"Registration failed: {ex.Message}" });
            }
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest loginRequest)
        {
            try
            {
                var user = await _karatFlowService.GetUserByUsernameAsync(loginRequest.Username);
                if (user == null)
                {
                    return Unauthorized(new { message = "Invalid username or password" });
                }

                // Verify password
                if (!BCrypt.Net.BCrypt.Verify(loginRequest.Password, user.PasswordHash))
                {
                    return Unauthorized(new { message = "Invalid username or password" });
                }

                // Generate JWT token
                var token = GenerateJwtToken(user);

                // Get user accounts
                var accounts = await _karatFlowService.GetUserAccountsAsync(user.Id!);

                return Ok(new LoginResponse
                {
                    Token = token,
                    User = user,
                    ExpiresAt = DateTime.UtcNow.AddHours(24)
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"Login failed: {ex.Message}" });
            }
        }

        [HttpPost("refresh")]
        [Authorize]
        public async Task<IActionResult> RefreshToken()
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (userId == null)
                {
                    return Unauthorized(new { message = "Invalid token" });
                }

                // Get user from database
                var user = await _karatFlowService.GetUserByUsernameAsync(User.Identity?.Name!);
                if (user == null)
                {
                    return Unauthorized(new { message = "User not found" });
                }

                // Generate new token
                var token = GenerateJwtToken(user);

                return Ok(new { token = token, expiresAt = DateTime.UtcNow.AddHours(24) });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"Token refresh failed: {ex.Message}" });
            }
        }

        private string GenerateJwtToken(User user)
        {
            var key = _configuration["Jwt:Key"] ?? throw new InvalidOperationException("JWT key not configured");
            var issuer = _configuration["Jwt:Issuer"] ?? "KaratFlow";
            var audience = _configuration["Jwt:Audience"] ?? "KaratFlowUsers";

            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id!),
                new Claim(ClaimTypes.Name, user.Username),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Role, "User")
            };

            var token = new JwtSecurityToken(
                issuer: issuer,
                audience: audience,
                claims: claims,
                expires: DateTime.UtcNow.AddHours(24),
                signingCredentials: credentials);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        private string GenerateAccountNumber()
        {
            var random = new Random();
            return $"KF{random.Next(100000000, 999999999):D9}";
        }
    }
}
