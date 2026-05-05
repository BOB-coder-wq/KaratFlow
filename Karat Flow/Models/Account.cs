using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Karat_Flow.Models
{
    public class Account
    {
        public int Id { get; set; }
        
        [Required]
        public int UserId { get; set; }
        
        // Navigation property back to user
        public User User { get; set; } = null!;
        
        [Required]
        [StringLength(20)]
        public string AccountNumber { get; set; } = string.Empty;
        
        public decimal Balance { get; set; } = 0;
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        public DateTime LastUpdated { get; set; } = DateTime.UtcNow;
        
        // Navigation properties for transactions
        public ICollection<Transaction> SentTransactions { get; set; } = new List<Transaction>();
        public ICollection<Transaction> ReceivedTransactions { get; set; } = new List<Transaction>();
    }
}
