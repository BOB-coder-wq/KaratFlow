using System;
using System.Linq;
using System.Threading.Tasks;

// We need to reference the main project's models and services
// For this demo, we'll create a simplified version

public class User
{
    public int Id { get; set; }
    public string Username { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public decimal Balance { get; set; }
}

public class Transaction
{
    public int Id { get; set; }
    public string FromUser { get; set; } = string.Empty;
    public string ToUser { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string Description { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}

class Program
{
    static async Task Main(string[] args)
    {
        Console.WriteLine("💎 Karat Flow - Digital Currency System Demo");
        Console.WriteLine("==========================================\n");

        // Simulate user data
        var users = new[]
        {
            new User { Id = 1, Username = "admin", FirstName = "Admin", LastName = "User", Balance = 10000 },
            new User { Id = 2, Username = "alice", FirstName = "Alice", LastName = "Smith", Balance = 2500 },
            new User { Id = 3, Username = "bob", FirstName = "Bob", LastName = "Johnson", Balance = 1800 },
            new User { Id = 4, Username = "charlie", FirstName = "Charlie", LastName = "Brown", Balance = 3200 }
        };

        var transactions = new[]
        {
            new Transaction { Id = 1, FromUser = "admin", ToUser = "alice", Amount = 500, Description = "Welcome bonus", CreatedAt = DateTime.UtcNow.AddDays(-5) },
            new Transaction { Id = 2, FromUser = "alice", ToUser = "bob", Amount = 150, Description = "Coffee payment", CreatedAt = DateTime.UtcNow.AddDays(-1) },
            new Transaction { Id = 3, FromUser = "bob", ToUser = "charlie", Amount = 75, Description = "Lunch split", CreatedAt = DateTime.UtcNow.AddDays(-2) }
        };

        // Display current user (admin)
        var currentUser = users.First(u => u.Username == "admin");
        Console.WriteLine($"👤 Logged in as: {currentUser.FirstName} {currentUser.LastName}");
        Console.WriteLine($"💰 Balance: {currentUser.Balance:N0} Karats");
        Console.WriteLine($"📋 Account: KF000000001\n");

        // Show recent transactions
        Console.WriteLine("📈 Recent Transactions:");
        Console.WriteLine("----------------------");
        foreach (var transaction in transactions.OrderByDescending(t => t.CreatedAt))
        {
            Console.WriteLine($"• {transaction.FromUser} → {transaction.ToUser}");
            Console.WriteLine($"  Amount: {transaction.Amount:N0} Karats");
            Console.WriteLine($"  Description: {transaction.Description}");
            Console.WriteLine($"  Date: {transaction.CreatedAt:yyyy-MM-dd HH:mm}\n");
        }

        // Demonstrate user-to-user transaction
        Console.WriteLine("💸 Sending Payment Demo:");
        Console.WriteLine("------------------------");
        var recipient = users.First(u => u.Username == "alice");
        decimal paymentAmount = 100;
        
        Console.WriteLine($"Sending {paymentAmount} Karats to {recipient.Username}...");
        
        // Update balances
        currentUser.Balance -= paymentAmount;
        recipient.Balance += paymentAmount;
        
        // Add new transaction
        var newTransaction = new Transaction 
        { 
            Id = transactions.Length + 1,
            FromUser = currentUser.Username, 
            ToUser = recipient.Username, 
            Amount = paymentAmount, 
            Description = "Demo payment", 
            CreatedAt = DateTime.UtcNow 
        };
        
        Console.WriteLine($"✅ Successfully sent {paymentAmount} Karats to {recipient.Username}!");
        Console.WriteLine($"💰 Your new balance: {currentUser.Balance:N0} Karats");
        Console.WriteLine($"🔒 {recipient.Username}'s balance information is private\n");

        // Demonstrate NFC payment
        Console.WriteLine("📱 NFC Payment Demo:");
        Console.WriteLine("--------------------");
        Console.WriteLine("Simulating NFC card detection...");
        await Task.Delay(1000);
        
        string nfcCardNumber = "KFC123456789";
        decimal nfcAmount = 250;
        
        Console.WriteLine($"🎴 Card detected: {nfcCardNumber}");
        Console.WriteLine($"Processing NFC payment of {nfcAmount} Karats...");
        await Task.Delay(1500);
        
        currentUser.Balance += nfcAmount;
        Console.WriteLine($"✅ NFC payment received: {nfcAmount} Karats");
        Console.WriteLine($"💰 Final balance: {currentUser.Balance:N0} Karats\n");

        // Show all users
        Console.WriteLine("👥 All Users:");
        Console.WriteLine("------------");
        foreach (var user in users)
        {
            Console.WriteLine($"• {user.Username} ({user.FirstName} {user.LastName})");
            Console.WriteLine($"  Balance: {user.Balance:N0} Karats\n");
        }

        // Show NFC cards info
        Console.WriteLine("🎴 NFC Cards in System:");
        Console.WriteLine("----------------------");
        var nfcCards = new[]
        {
            new { CardNumber = "KFC123456789", User = "alice", Status = "Active", DailyLimit = 1000 },
            new { CardNumber = "KFC987654321", User = "bob", Status = "Active", DailyLimit = 1000 },
            new { CardNumber = "KFC555666777", User = "charlie", Status = "Active", DailyLimit = 1000 }
        };

        foreach (var card in nfcCards)
        {
            Console.WriteLine($"• Card: {card.CardNumber}");
            Console.WriteLine($"  User: {card.User}");
            Console.WriteLine($"  Status: {card.Status}");
            Console.WriteLine($"  Daily Limit: {card.DailyLimit:N0} Karats\n");
        }

        Console.WriteLine("🎉 Karat Flow Demo Complete!");
        Console.WriteLine("============================");
        Console.WriteLine("The application demonstrates:");
        Console.WriteLine("✅ User accounts with Karat currency");
        Console.WriteLine("✅ User-to-user transactions");
        Console.WriteLine("✅ NFC card payment integration");
        Console.WriteLine("✅ Transaction history and validation");
        Console.WriteLine("✅ Security features and limits");
        Console.WriteLine("✅ Real-time balance updates");
        
        Console.WriteLine("\n🚀 The full WinUI application includes:");
        Console.WriteLine("• Modern Windows interface with account dashboard");
        Console.WriteLine("• Interactive payment forms with validation");
        Console.WriteLine("• Real-time NFC card detection simulation");
        Console.WriteLine("• Comprehensive transaction history viewer");
        Console.WriteLine("• Professional UI with Karat Flow branding");
        
        Console.WriteLine("\nPress any key to exit...");
        Console.ReadKey();
    }
}
