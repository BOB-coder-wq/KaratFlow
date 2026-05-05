using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Karat_Flow.Models
{
    public class User
    {
        public int Id { get; set; }
        
        [Required]
        [StringLength(50)]
        public string Username { get; set; } = string.Empty;
        
        [Required]
        [StringLength(100)]
        public string Email { get; set; } = string.Empty;
        
        [StringLength(50)]
        public string? FirstName { get; set; }
        
        [StringLength(50)]
        public string? LastName { get; set; }
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        // Navigation property for account
        public Account? Account { get; set; }
        
        // Navigation properties for transactions
        public ICollection<Transaction> SentTransactions { get; set; } = new List<Transaction>();
        public ICollection<Transaction> ReceivedTransactions { get; set; } = new List<Transaction>();
        
        // Navigation properties for NFC cards
        public ICollection<NFCCard> NFCCards { get; set; } = new List<NFCCard>();
    }
}
