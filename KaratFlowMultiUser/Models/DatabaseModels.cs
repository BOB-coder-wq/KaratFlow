using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace KaratFlowMultiUser.Models
{
    public class KaratFlowDbContext : DbContext
    {
        public KaratFlowDbContext(DbContextOptions<KaratFlowDbContext> options) : base(options)
        {
        }

        public DbSet<User> Users { get; set; }
        public DbSet<Account> Accounts { get; set; }
        public DbSet<Transaction> Transactions { get; set; }
        public DbSet<NFCCard> NFCCards { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // User configuration
            modelBuilder.Entity<User>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => e.Username).IsUnique();
                entity.HasIndex(e => e.Email).IsUnique();
            });

            // Account configuration
            modelBuilder.Entity<Account>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => e.AccountNumber).IsUnique();
                entity.Property(e => e.Balance).HasPrecision(18, 2);
                entity.HasOne(e => e.User)
                    .WithMany(u => u.Accounts)
                    .HasForeignKey(e => e.UserId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // Transaction configuration
            modelBuilder.Entity<Transaction>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Amount).HasPrecision(18, 2);
                entity.HasOne(e => e.FromAccount)
                    .WithMany()
                    .HasForeignKey(e => e.FromAccountId)
                    .OnDelete(DeleteBehavior.Restrict);
                entity.HasOne(e => e.ToAccount)
                    .WithMany()
                    .HasForeignKey(e => e.ToAccountId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // NFC Card configuration
            modelBuilder.Entity<NFCCard>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => e.CardNumber).IsUnique();
                entity.Property(e => e.DailyLimit).HasPrecision(18, 2);
                entity.Property(e => e.DailyUsed).HasPrecision(18, 2);
                entity.HasOne(e => e.Account)
                    .WithMany()
                    .HasForeignKey(e => e.AccountId)
                    .OnDelete(DeleteBehavior.Cascade);
            });
        }
    }

    public class User
    {
        public int Id { get; set; }
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty;
        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        
        // Navigation properties
        public virtual ICollection<Account> Accounts { get; set; } = new List<Account>();
    }

    public class Account
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string AccountNumber { get; set; } = string.Empty;
        public decimal Balance { get; set; }
        public string Currency { get; set; } = "KARAT";
        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        
        // Navigation properties
        public virtual User User { get; set; } = null!;
        public virtual ICollection<Transaction> FromTransactions { get; set; } = new List<Transaction>();
        public virtual ICollection<Transaction> ToTransactions { get; set; } = new List<Transaction>();
        public virtual ICollection<NFCCard> NFCCards { get; set; } = new List<NFCCard>();
    }

    public class Transaction
    {
        public int Id { get; set; }
        public int FromAccountId { get; set; }
        public int ToAccountId { get; set; }
        public decimal Amount { get; set; }
        public string Description { get; set; } = string.Empty;
        public TransactionType Type { get; set; }
        public TransactionStatus Status { get; set; }
        public DateTime CreatedAt { get; set; }
        
        // Navigation properties
        public virtual Account FromAccount { get; set; } = null!;
        public virtual Account ToAccount { get; set; } = null!;
    }

    public class NFCCard
    {
        public int Id { get; set; }
        public string CardNumber { get; set; } = string.Empty;
        public string CardUID { get; set; } = string.Empty;
        public int AccountId { get; set; }
        public NFCStatus Status { get; set; }
        public decimal DailyLimit { get; set; }
        public decimal DailyUsed { get; set; }
        public DateTime LastResetDate { get; set; }
        public DateTime ExpiresAt { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        
        // Navigation properties
        public virtual Account Account { get; set; } = null!;
    }

    public enum TransactionType
    {
        Payment,
        Deposit,
        Withdrawal,
        Transfer
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
