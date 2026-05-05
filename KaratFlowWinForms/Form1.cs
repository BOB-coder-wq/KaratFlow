using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace KaratFlowWinForms;

public partial class Form1 : Form
{
    // Simulated user data
    private decimal currentBalance = 10000;
    private string currentUsername = "admin";
    private string currentAccountNumber = "KF000000001";
    
    private List<Transaction> transactions = new List<Transaction>
    {
        new Transaction { FromUser = "admin", ToUser = "alice", Amount = 500, Description = "Welcome bonus", CreatedAt = DateTime.UtcNow.AddDays(-5) },
        new Transaction { FromUser = "alice", ToUser = "bob", Amount = 150, Description = "Coffee payment", CreatedAt = DateTime.UtcNow.AddDays(-1) },
        new Transaction { FromUser = "bob", ToUser = "charlie", Amount = 75, Description = "Lunch split", CreatedAt = DateTime.UtcNow.AddDays(-2) }
    };

    private Label balanceLabel;
    private Label accountLabel;
    private Label welcomeLabel;
    private Button sendPaymentBtn;
    private Button receivePaymentBtn;
    private Button nfcPaymentBtn;
    private ListBox transactionsList;
    private GroupBox paymentGroup;
    private TextBox recipientTextBox;
    private TextBox amountTextBox;
    private TextBox descriptionTextBox;
    private Button confirmSendBtn;
    private Button cancelSendBtn;
    private GroupBox nfcGroup;
    private Label nfcStatusLabel;
    private ProgressBar nfcProgress;

    public Form1()
    {
        this.Text = "💎 Karat Flow - Digital Currency";
        this.Size = new Size(800, 600);
        this.StartPosition = FormStartPosition.CenterScreen;
        this.BackColor = Color.FromArgb(255, 245, 245);
        SetupUI();
    }

    
    private void SetupUI()
    {
        // Enable double buffering for smooth rendering
        this.DoubleBuffered = true;
        
        // Title with gradient effect
        var titleLabel = new Label
        {
            Text = "💎 Karat Flow",
            Font = new Font("Segoe UI", 28, FontStyle.Bold),
            ForeColor = Color.FromArgb(255, 107, 53),
            Location = new Point(20, 20),
            Size = new Size(350, 50),
            AutoSize = true
        };
        this.Controls.Add(titleLabel);

        // Subtitle
        var subtitleLabel = new Label
        {
            Text = "Digital Currency System",
            Font = new Font("Segoe UI", 10, FontStyle.Italic),
            ForeColor = Color.FromArgb(255, 107, 53),
            Location = new Point(25, 55),
            Size = new Size(200, 20),
            AutoSize = true
        };
        this.Controls.Add(subtitleLabel);

        // Welcome label with modern styling
        welcomeLabel = new Label
        {
            Text = $"Welcome back, Admin User!",
            Font = new Font("Segoe UI", 12, FontStyle.Bold),
            ForeColor = Color.FromArgb(51, 51, 51),
            Location = new Point(20, 85),
            Size = new Size(300, 25),
            AutoSize = true
        };
        this.Controls.Add(welcomeLabel);

        // Balance card with modern styling
        var balancePanel = new Panel
        {
            Location = new Point(20, 120),
            Size = new Size(350, 140),
            BackColor = Color.White,
            BorderStyle = BorderStyle.FixedSingle
        };
        this.Controls.Add(balancePanel);

        var balanceTitleLabel = new Label
        {
            Text = "Account Balance",
            Font = new Font("Segoe UI", 10),
            ForeColor = Color.Gray,
            Location = new Point(15, 15),
            Size = new Size(150, 20),
            AutoSize = true
        };
        balancePanel.Controls.Add(balanceTitleLabel);

        balanceLabel = new Label
        {
            Text = $"{currentBalance:N0} Karats",
            Font = new Font("Segoe UI", 28, FontStyle.Bold),
            ForeColor = Color.FromArgb(255, 107, 53),
            Location = new Point(15, 35),
            Size = new Size(300, 40),
            AutoSize = true
        };
        balancePanel.Controls.Add(balanceLabel);

        accountLabel = new Label
        {
            Text = $"Account: {currentAccountNumber}",
            Font = new Font("Segoe UI", 9),
            ForeColor = Color.Gray,
            Location = new Point(15, 80),
            Size = new Size(200, 20),
            AutoSize = true
        };
        balancePanel.Controls.Add(accountLabel);

        // Quick actions
        var actionsPanel = new Panel
        {
            Location = new Point(390, 100),
            Size = new Size(370, 120),
            BackColor = Color.White,
            BorderStyle = BorderStyle.FixedSingle
        };
        this.Controls.Add(actionsPanel);

        var actionsTitleLabel = new Label
        {
            Text = "Quick Actions",
            Font = new Font("Segoe UI", 14, FontStyle.Bold),
            Location = new Point(15, 15),
            Size = new Size(150, 25),
            AutoSize = true
        };
        actionsPanel.Controls.Add(actionsTitleLabel);

        sendPaymentBtn = new Button
        {
            Text = "💸 Send Payment",
            BackColor = Color.FromArgb(255, 107, 53),
            ForeColor = Color.White,
            Font = new Font("Segoe UI", 10, FontStyle.Bold),
            FlatStyle = FlatStyle.Flat,
            Location = new Point(15, 50),
            Size = new Size(160, 40)
        };
        sendPaymentBtn.Click += SendPaymentBtn_Click;
        actionsPanel.Controls.Add(sendPaymentBtn);

        receivePaymentBtn = new Button
        {
            Text = "💰 Receive Payment",
            BackColor = Color.FromArgb(76, 175, 80),
            ForeColor = Color.White,
            Font = new Font("Segoe UI", 10, FontStyle.Bold),
            FlatStyle = FlatStyle.Flat,
            Location = new Point(185, 50),
            Size = new Size(160, 40)
        };
        receivePaymentBtn.Click += ReceivePaymentBtn_Click;
        actionsPanel.Controls.Add(receivePaymentBtn);

        nfcPaymentBtn = new Button
        {
            Text = "📱 Accept NFC Payment",
            BackColor = Color.FromArgb(33, 150, 243),
            ForeColor = Color.White,
            Font = new Font("Segoe UI", 10, FontStyle.Bold),
            FlatStyle = FlatStyle.Flat,
            Location = new Point(15, 100),
            Size = new Size(330, 40)
        };
        nfcPaymentBtn.Click += NFCPaymentBtn_Click;
        actionsPanel.Controls.Add(nfcPaymentBtn);

        // Send Payment Section (initially hidden)
        paymentGroup = new GroupBox
        {
            Text = "Send Karats",
            Location = new Point(20, 230),
            Size = new Size(740, 150),
            BackColor = Color.White,
            Visible = false
        };
        this.Controls.Add(paymentGroup);

        var recipientLabel = new Label
        {
            Text = "Recipient Username:",
            Location = new Point(15, 30),
            Size = new Size(120, 25),
            AutoSize = true
        };
        paymentGroup.Controls.Add(recipientLabel);

        recipientTextBox = new TextBox
        {
            Location = new Point(140, 30),
            Size = new Size(200, 25)
        };
        paymentGroup.Controls.Add(recipientTextBox);

        var amountLabel = new Label
        {
            Text = "Amount (Karats):",
            Location = new Point(15, 65),
            Size = new Size(120, 25),
            AutoSize = true
        };
        paymentGroup.Controls.Add(amountLabel);

        amountTextBox = new TextBox
        {
            Location = new Point(140, 65),
            Size = new Size(200, 25)
        };
        paymentGroup.Controls.Add(amountTextBox);

        var descLabel = new Label
        {
            Text = "Description (Optional):",
            Location = new Point(380, 30),
            Size = new Size(150, 25),
            AutoSize = true
        };
        paymentGroup.Controls.Add(descLabel);

        descriptionTextBox = new TextBox
        {
            Location = new Point(380, 55),
            Size = new Size(330, 60),
            Multiline = true
        };
        paymentGroup.Controls.Add(descriptionTextBox);

        confirmSendBtn = new Button
        {
            Text = "Send Payment",
            BackColor = Color.FromArgb(255, 107, 53),
            ForeColor = Color.White,
            Location = new Point(15, 100),
            Size = new Size(120, 35),
            FlatStyle = FlatStyle.Flat
        };
        confirmSendBtn.Click += ConfirmSendBtn_Click;
        paymentGroup.Controls.Add(confirmSendBtn);

        cancelSendBtn = new Button
        {
            Text = "Cancel",
            BackColor = Color.Gray,
            ForeColor = Color.White,
            Location = new Point(145, 100),
            Size = new Size(80, 35),
            FlatStyle = FlatStyle.Flat
        };
        cancelSendBtn.Click += (s, e) => paymentGroup.Visible = false;
        paymentGroup.Controls.Add(cancelSendBtn);

        // NFC Payment Section (initially hidden)
        nfcGroup = new GroupBox
        {
            Text = "NFC Payment",
            Location = new Point(20, 230),
            Size = new Size(740, 120),
            BackColor = Color.White,
            Visible = false
        };
        this.Controls.Add(nfcGroup);

        var nfcLabel = new Label
        {
            Text = "Waiting for NFC card...",
            Location = new Point(15, 30),
            Size = new Size(200, 25),
            AutoSize = true
        };
        nfcGroup.Controls.Add(nfcLabel);

        nfcProgress = new ProgressBar
        {
            Location = new Point(15, 60),
            Size = new Size(300, 20),
            Style = ProgressBarStyle.Marquee
        };
        nfcGroup.Controls.Add(nfcProgress);

        nfcStatusLabel = new Label
        {
            Text = "Ready to accept payment",
            Location = new Point(15, 90),
            Size = new Size(300, 25),
            ForeColor = Color.Blue,
            AutoSize = true
        };
        nfcGroup.Controls.Add(nfcStatusLabel);

        var cancelNFCBtn = new Button
        {
            Text = "Cancel",
            BackColor = Color.Gray,
            ForeColor = Color.White,
            Location = new Point(15, 115),
            Size = new Size(80, 35),
            FlatStyle = FlatStyle.Flat
        };
        cancelNFCBtn.Click += (s, e) => { nfcGroup.Visible = false; nfcProgress.Style = ProgressBarStyle.Blocks; };
        nfcGroup.Controls.Add(cancelNFCBtn);

        // Transaction History
        var historyPanel = new Panel
        {
            Location = new Point(20, 390),
            Size = new Size(740, 200),
            BackColor = Color.White,
            BorderStyle = BorderStyle.FixedSingle
        };
        this.Controls.Add(historyPanel);

        var historyTitleLabel = new Label
        {
            Text = "Recent Transactions",
            Font = new Font("Segoe UI", 14, FontStyle.Bold),
            Location = new Point(15, 15),
            Size = new Size(200, 25),
            AutoSize = true
        };
        historyPanel.Controls.Add(historyTitleLabel);

        transactionsList = new ListBox
        {
            Location = new Point(15, 45),
            Size = new Size(710, 140),
            Font = new Font("Consolas", 9),
            ScrollAlwaysVisible = true
        };
        historyPanel.Controls.Add(transactionsList);

        // Load transactions
        RefreshTransactions();
    }

    private void RefreshTransactions()
    {
        transactionsList.Items.Clear();
        foreach (var transaction in transactions.OrderByDescending(t => t.CreatedAt))
        {
            var item = $"{transaction.FromUser} → {transaction.ToUser} | {transaction.Amount:N0} K | {transaction.Description} | {transaction.CreatedAt:yyyy-MM-dd HH:mm}";
            transactionsList.Items.Add(item);
        }
    }

    private void SendPaymentBtn_Click(object? sender, EventArgs e)
    {
        paymentGroup.Visible = true;
        nfcGroup.Visible = false;
    }

    private void ReceivePaymentBtn_Click(object? sender, EventArgs e)
    {
        MessageBox.Show($"Your account details:\n\nUsername: {currentUsername}\nAccount Number: {currentAccountNumber}\n\nShare this information with the sender.", 
            "Receive Payment", MessageBoxButtons.OK, MessageBoxIcon.Information);
    }

    private async void NFCPaymentBtn_Click(object? sender, EventArgs e)
    {
        nfcGroup.Visible = true;
        paymentGroup.Visible = false;
        
        nfcProgress.Style = ProgressBarStyle.Marquee;
        nfcStatusLabel.Text = "Scanning for NFC cards...";
        
        // Simulate NFC card detection
        await Task.Delay(2000);
        
        nfcStatusLabel.Text = "Card detected: KFC123456789";
        await Task.Delay(1000);
        
        // Simulate payment processing
        var randomAmount = new Random().Next(10, 500);
        nfcStatusLabel.Text = $"Processing payment of {randomAmount} Karats...";
        await Task.Delay(1500);
        
        // Update balance
        currentBalance += randomAmount;
        balanceLabel.Text = $"{currentBalance:N0} Karats";
        
        // Add transaction
        transactions.Add(new Transaction 
        { 
            FromUser = "NFC Card", 
            ToUser = currentUsername, 
            Amount = randomAmount, 
            Description = "NFC Card Payment", 
            CreatedAt = DateTime.UtcNow 
        });
        
        RefreshTransactions();
        
        nfcStatusLabel.Text = $"✅ Payment received: {randomAmount} Karats";
        nfcProgress.Style = ProgressBarStyle.Blocks;
        
        await Task.Delay(2000);
        nfcGroup.Visible = false;
    }

    private void ConfirmSendBtn_Click(object? sender, EventArgs e)
    {
        if (decimal.TryParse(amountTextBox.Text, out var amount) && amount > 0 && !string.IsNullOrWhiteSpace(recipientTextBox.Text))
        {
            if (amount > currentBalance)
            {
                MessageBox.Show("Insufficient balance!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            // Update balance
            currentBalance -= amount;
            balanceLabel.Text = $"{currentBalance:N0} Karats";
            
            // Add transaction
            transactions.Add(new Transaction 
            { 
                FromUser = currentUsername, 
                ToUser = recipientTextBox.Text, 
                Amount = amount, 
                Description = string.IsNullOrWhiteSpace(descriptionTextBox.Text) 
                    ? $"Payment to {recipientTextBox.Text}" 
                    : descriptionTextBox.Text, 
                CreatedAt = DateTime.UtcNow 
            });
            
            RefreshTransactions();
            
            MessageBox.Show($"Successfully sent {amount:N0} Karats to {recipientTextBox.Text}!\n\nRecipient balance information is private for security.", 
                "Payment Sent", MessageBoxButtons.OK, MessageBoxIcon.Information);
            
            // Clear form
            recipientTextBox.Text = "";
            amountTextBox.Text = "";
            descriptionTextBox.Text = "";
            paymentGroup.Visible = false;
        }
        else
        {
            MessageBox.Show("Please enter valid recipient and amount!", "Invalid Input", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }
    }
}


public class Transaction
{
    public string FromUser { get; set; } = string.Empty;
    public string ToUser { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string Description { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}
