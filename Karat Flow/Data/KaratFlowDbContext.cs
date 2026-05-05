using System;
using Microsoft.EntityFrameworkCore;
using Karat_Flow.Models;

namespace Karat_Flow.Data
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

            // Configure User entity
            modelBuilder.Entity<User>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Username).IsRequired().HasMaxLength(50);
                entity.Property(e => e.Email).IsRequired().HasMaxLength(100);
                entity.Property(e => e.FirstName).HasMaxLength(50);
                entity.Property(e => e.LastName).HasMaxLength(50);
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
                
                // One-to-one relationship with Account
                entity.HasOne(u => u.Account)
                      .WithOne(a => a.User)
                      .HasForeignKey<Account>(a => a.UserId);
            });

            // Configure Account entity
            modelBuilder.Entity<Account>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.AccountNumber).IsRequired().HasMaxLength(20);
                entity.Property(e => e.Balance).HasDefaultValue(0);
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
                entity.Property(e => e.LastUpdated).HasDefaultValueSql("GETUTCDATE()");

                // Generate unique account numbers
                entity.Property(e => e.AccountNumber)
                      .HasDefaultValueSql("('KF' + RIGHT('000000000' + CAST(ABS(CHECKSUM(NEWID())) % 1000000000 AS VARCHAR(10)), 9))");
            });

            // Configure Transaction entity
            modelBuilder.Entity<Transaction>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Amount).HasPrecision(18, 2);
                entity.Property(e => e.Description).IsRequired().HasMaxLength(200);
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETUTCDATE()");

                // Foreign key relationships
                entity.HasOne(t => t.FromAccount)
                      .WithMany(a => a.SentTransactions)
                      .HasForeignKey(t => t.FromAccountId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(t => t.ToAccount)
                      .WithMany(a => a.ReceivedTransactions)
                      .HasForeignKey(t => t.ToAccountId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(t => t.NFCCard)
                      .WithMany(c => c.Transactions)
                      .HasForeignKey(t => t.NFCCardId)
                      .OnDelete(DeleteBehavior.SetNull);
            });

            // Configure NFCCard entity
            modelBuilder.Entity<NFCCard>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.CardNumber).IsRequired().HasMaxLength(50);
                entity.Property(e => e.UID).IsRequired().HasMaxLength(100);
                entity.Property(e => e.IssuedAt).HasDefaultValueSql("GETUTCDATE()");
                entity.Property(e => e.LastUsed).HasDefaultValueSql("GETUTCDATE()");
                entity.Property(e => e.DailyLimit).HasPrecision(18, 2).HasDefaultValue(1000);
                entity.Property(e => e.CurrentDailyAmount).HasPrecision(18, 2).HasDefaultValue(0);

                // Foreign key relationship
                entity.HasOne(c => c.User)
                      .WithMany(u => u.NFCCards)
                      .HasForeignKey(c => c.UserId)
                      .OnDelete(DeleteBehavior.Cascade);

                // Generate unique card numbers
                entity.Property(e => e.CardNumber)
                      .HasDefaultValueSql("('KFC' + RIGHT('000000000' + CAST(ABS(CHECKSUM(NEWID())) % 1000000000 AS VARCHAR(10)), 9))");
            });

            // Seed initial data
            SeedData(modelBuilder);
        }

        private void SeedData(ModelBuilder modelBuilder)
        {
            // Create a default admin user with initial karats
            var adminUser = new User
            {
                Id = 1,
                Username = "admin",
                Email = "admin@karatflow.com",
                FirstName = "Admin",
                LastName = "User",
                CreatedAt = DateTime.UtcNow
            };

            var adminAccount = new Account
            {
                Id = 1,
                UserId = 1,
                AccountNumber = "KF000000001",
                Balance = 10000, // Start with 10,000 karats
                CreatedAt = DateTime.UtcNow,
                LastUpdated = DateTime.UtcNow
            };

            modelBuilder.Entity<User>().HasData(adminUser);
            modelBuilder.Entity<Account>().HasData(adminAccount);
        }
    }
}
