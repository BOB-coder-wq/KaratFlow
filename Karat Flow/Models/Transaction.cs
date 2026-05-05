using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Karat_Flow.Models
{
    public enum TransactionType
    {
        Payment,
        Deposit,
        Withdrawal,
        NFC_Payment,
        Transfer
    }

    public enum TransactionStatus
    {
        Pending,
        Completed,
        Failed,
        Cancelled
    }

    public class Transaction
    {
        public int Id { get; set; }
        
        [Required]
        public int FromAccountId { get; set; }
        public Account FromAccount { get; set; } = null!;
        
        [Required]
        public int ToAccountId { get; set; }
        public Account ToAccount { get; set; } = null!;
        
        [Required]
        public decimal Amount { get; set; }
        
        [Required]
        [StringLength(200)]
        public string Description { get; set; } = string.Empty;
        
        [Required]
        public TransactionType Type { get; set; }
        
        [Required]
        public TransactionStatus Status { get; set; } = TransactionStatus.Pending;
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        public DateTime? CompletedAt { get; set; }
        
        // Optional NFC card reference for NFC payments
        public int? NFCCardId { get; set; }
        public NFCCard? NFCCard { get; set; }
    }
}
