using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Karat_Flow.Models
{
    public enum NFCStatus
    {
        Active,
        Inactive,
        Lost,
        Stolen,
        Expired
    }

    public class NFCCard
    {
        public int Id { get; set; }
        
        [Required]
        public int UserId { get; set; }
        public User User { get; set; } = null!;
        
        [Required]
        [StringLength(50)]
        public string CardNumber { get; set; } = string.Empty;
        
        [Required]
        [StringLength(100)]
        public string UID { get; set; } = string.Empty; // Unique NFC identifier
        
        [Required]
        public NFCStatus Status { get; set; } = NFCStatus.Active;
        
        public DateTime IssuedAt { get; set; } = DateTime.UtcNow;
        
        public DateTime? ExpiresAt { get; set; }
        
        public DateTime LastUsed { get; set; } = DateTime.UtcNow;
        
        // Daily transaction limit for security
        public decimal DailyLimit { get; set; } = 1000;
        
        // Current daily transaction amount
        public decimal CurrentDailyAmount { get; set; } = 0;
        
        // Navigation properties for transactions
        public ICollection<Transaction> Transactions { get; set; } = new List<Transaction>();
    }
}
