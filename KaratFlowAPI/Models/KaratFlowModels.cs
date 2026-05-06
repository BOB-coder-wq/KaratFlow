using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System.ComponentModel.DataAnnotations;

namespace KaratFlowAPI.Models
{
    public class User
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }

        [Required]
        [StringLength(50)]
        public string Username { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        [StringLength(100)]
        public string Email { get; set; } = string.Empty;

        [Required]
        [StringLength(255)]
        public string PasswordHash { get; set; } = string.Empty;

        public bool IsActive { get; set; } = true;

        [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties (not stored in MongoDB, populated via queries)
        [BsonIgnore]
        public List<Account>? Accounts { get; set; }
    }

    public class Account
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }

        [Required]
        public string UserId { get; set; } = string.Empty;

        [Required]
        [StringLength(20)]
        public string AccountNumber { get; set; } = string.Empty;

        [Required]
        [Range(0, double.MaxValue)]
        public decimal Balance { get; set; } = 0;

        [Required]
        [StringLength(10)]
        public string Currency { get; set; } = "KARAT";

        public bool IsActive { get; set; } = true;

        [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        [BsonIgnore]
        public User? User { get; set; }
    }

    public class Transaction
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }

        [Required]
        public string FromAccountId { get; set; } = string.Empty;

        [Required]
        public string ToAccountId { get; set; } = string.Empty;

        [Required]
        [Range(0.01, double.MaxValue)]
        public decimal Amount { get; set; }

        [StringLength(500)]
        public string Description { get; set; } = string.Empty;

        [Required]
        public TransactionType Type { get; set; } = TransactionType.Payment;

        [Required]
        public TransactionStatus Status { get; set; } = TransactionStatus.Pending;

        [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
        public DateTime ProcessedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        [BsonIgnore]
        public Account? FromAccount { get; set; }

        [BsonIgnore]
        public Account? ToAccount { get; set; }
    }

    public class NFCCard
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }

        [Required]
        [StringLength(20)]
        public string CardNumber { get; set; } = string.Empty;

        [Required]
        [StringLength(32)]
        public string CardUID { get; set; } = string.Empty;

        [Required]
        public string AccountId { get; set; } = string.Empty;

        [Required]
        public NFCStatus Status { get; set; } = NFCStatus.Active;

        [Required]
        [Range(0, double.MaxValue)]
        public decimal DailyLimit { get; set; } = 1000;

        [Range(0, double.MaxValue)]
        public decimal DailyUsed { get; set; } = 0;

        [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
        public DateTime LastResetDate { get; set; } = DateTime.UtcNow.Date;

        [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
        public DateTime ExpiresAt { get; set; } = DateTime.UtcNow.AddYears(2);

        [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        [BsonIgnore]
        public Account? Account { get; set; }
    }

    public class LoginRequest
    {
        [Required]
        [StringLength(50)]
        public string Username { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string Password { get; set; } = string.Empty;
    }

    public class LoginResponse
    {
        public string Token { get; set; } = string.Empty;
        public User User { get; set; } = null!;
        public DateTime ExpiresAt { get; set; }
    }

    public class PaymentRequest
    {
        [Required]
        [StringLength(50)]
        public string RecipientUsername { get; set; } = string.Empty;

        [Required]
        [Range(0.01, double.MaxValue)]
        public decimal Amount { get; set; }

        [StringLength(500)]
        public string? Description { get; set; }
    }

    public class PaymentResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public Transaction? Transaction { get; set; }
    }

    public class NFCPaymentRequest
    {
        [Required]
        [StringLength(20)]
        public string CardNumber { get; set; } = string.Empty;

        [Required]
        [Range(0.01, double.MaxValue)]
        public decimal Amount { get; set; }
    }

    public enum TransactionType
    {
        Payment,
        Deposit,
        Withdrawal,
        Transfer,
        NFC
    }

    public enum TransactionStatus
    {
        Pending,
        Completed,
        Failed,
        Cancelled
    }

    public enum NFCStatus
    {
        Active,
        Inactive,
        Lost,
        Stolen,
        Expired
    }
}
