using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using KaratFlowMultiUser.Models;
using KaratFlowMultiUser.Services;

namespace KaratFlowMultiUser
{
    public partial class Form1 : Form
    {
        // Database service
        private readonly DatabaseService _databaseService;
        
        // Current user data
        private User? _currentUser;
        private Account? _currentAccount;
        
        // UI controls
        private Label balanceLabel = null!;
        private Label accountLabel = null!;
        private Label welcomeLabel = null!;
        private Label databasePathLabel = null!;
        private Button sendPaymentBtn = null!;
        private Button receivePaymentBtn = null!;
        private Button nfcPaymentBtn = null!;
        private Button refreshBtn = null!;
        private Button switchUserBtn = null!;
        private ListBox transactionsList = null!;
        private GroupBox paymentGroup = null!;
        private TextBox recipientTextBox = null!;
        private TextBox amountTextBox = null!;
        private TextBox descriptionTextBox = null!;
        private Button confirmSendBtn = null!;
        private Button cancelSendBtn = null!;
        private GroupBox nfcGroup = null!;
        private Label nfcStatusLabel = null!;
        private ProgressBar nfcProgress = null!;
        private ComboBox userComboBox = null!;

        public Form1()
        {
            _databaseService = new DatabaseService();
            InitializeComponent();
            SetupUI();
            _ = InitializeAsync();
        }

        private void InitializeComponent()
        {
            this.Text = "💎 Karat Flow - Multi-User Digital Currency";
            this.Size = new Size(850, 650);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = Color.FromArgb(255, 245, 245);
        }

        private void SetupUI()
        {
            // Title
            var titleLabel = new Label
            {
                Text = "💎 Karat Flow",
                Font = new Font("Segoe UI", 28, FontStyle.Bold),
                ForeColor = Color.FromArgb(255, 107, 53),
                Location = new Point(20, 20),
                Size = new Size(300, 50),
                AutoSize = true
            };
            this.Controls.Add(titleLabel);

            // Subtitle
            var subtitleLabel = new Label
            {
                Text = "Multi-User Digital Currency System",
                Font = new Font("Segoe UI", 10, FontStyle.Italic),
                ForeColor = Color.FromArgb(255, 107, 53),
                Location = new Point(25, 55),
                Size = new Size(300, 20),
                AutoSize = true
            };
            this.Controls.Add(subtitleLabel);

            // User selection
            var userPanel = new Panel
            {
                Location = new Point(20, 85),
                Size = new Size(350, 60),
                BackColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle
            };
            this.Controls.Add(userPanel);

            var userLabel = new Label
            {
                Text = "Current User:",
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                Location = new Point(15, 10),
                Size = new Size(100, 20),
                AutoSize = true
            };
            userPanel.Controls.Add(userLabel);

            userComboBox = new ComboBox
            {
                Location = new Point(15, 30),
                Size = new Size(150, 25),
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            userComboBox.SelectedIndexChanged += UserComboBox_SelectedIndexChanged;
            userPanel.Controls.Add(userComboBox);

            switchUserBtn = new Button
            {
                Text = "Switch User",
                BackColor = Color.FromArgb(76, 175, 80),
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 9),
                Location = new Point(175, 30),
                Size = new Size(80, 25),
                FlatStyle = FlatStyle.Flat
            };
            switchUserBtn.Click += SwitchUserBtn_Click;
            userPanel.Controls.Add(switchUserBtn);

            refreshBtn = new Button
            {
                Text = "🔄 Refresh",
                BackColor = Color.FromArgb(33, 150, 243),
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 9),
                Location = new Point(265, 30),
                Size = new Size(70, 25),
                FlatStyle = FlatStyle.Flat
            };
            refreshBtn.Click += RefreshBtn_Click;
            userPanel.Controls.Add(refreshBtn);

            // Database path
            databasePathLabel = new Label
            {
                Text = $"Database: {_databaseService.GetDatabasePath()}",
                Font = new Font("Segoe UI", 8),
                ForeColor = Color.Gray,
                Location = new Point(20, 155),
                Size = new Size(400, 15),
                AutoSize = true
            };
            this.Controls.Add(databasePathLabel);

            // Balance card
            var balancePanel = new Panel
            {
                Location = new Point(20, 180),
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
                Text = "0 Karats",
                Font = new Font("Segoe UI", 28, FontStyle.Bold),
                ForeColor = Color.FromArgb(255, 107, 53),
                Location = new Point(15, 35),
                Size = new Size(300, 40),
                AutoSize = true
            };
            balancePanel.Controls.Add(balanceLabel);

            accountLabel = new Label
            {
                Text = "Account: Unknown",
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
                Location = new Point(390, 180),
                Size = new Size(420, 140),
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
                Size = new Size(190, 40)
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
                Location = new Point(215, 50),
                Size = new Size(190, 40)
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
                Size = new Size(390, 40)
            };
            nfcPaymentBtn.Click += NFCPaymentBtn_Click;
            actionsPanel.Controls.Add(nfcPaymentBtn);

            // Send Payment Section (initially hidden)
            paymentGroup = new GroupBox
            {
                Text = "Send Karats",
                Location = new Point(20, 330),
                Size = new Size(790, 150),
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
                Size = new Size(380, 60),
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
                Location = new Point(20, 490),
                Size = new Size(790, 120),
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
                Location = new Point(20, 620),
                Size = new Size(790, 250),
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
                Size = new Size(760, 190),
                Font = new Font("Consolas", 9),
                ScrollAlwaysVisible = true
            };
            historyPanel.Controls.Add(transactionsList);
        }

        private async Task InitializeAsync()
        {
            try
            {
                await _databaseService.InitializeAsync();
                await LoadUsersAsync();
                await LoadUserDataAsync();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Database initialization failed: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async Task LoadUsersAsync()
        {
            var users = await _databaseService.GetAllUsersAsync();
            
            userComboBox.Items.Clear();
            foreach (var user in users)
            {
                userComboBox.Items.Add($"{user.Username} ({user.Accounts.FirstOrDefault()?.AccountNumber ?? "No Account"})");
            }
            
            if (userComboBox.Items.Count > 0)
            {
                userComboBox.SelectedIndex = 0;
            }
        }

        private async Task LoadUserDataAsync()
        {
            if (userComboBox.SelectedIndex >= 0)
            {
                var selectedText = userComboBox.SelectedItem.ToString();
                var username = selectedText.Split(' ')[0];
                
                _currentUser = await _databaseService.GetUserByUsernameAsync(username);
                if (_currentUser != null)
                {
                    _currentAccount = _currentUser.Accounts.FirstOrDefault();
                    
                    // Update UI
                    balanceLabel.Text = $"{_currentAccount?.Balance:N0} Karats";
                    accountLabel.Text = $"Account: {_currentAccount?.AccountNumber ?? "Unknown"}";
                    
                    await LoadTransactionsAsync();
                }
            }
        }

        private async Task LoadTransactionsAsync()
        {
            if (_currentAccount != null)
            {
                var transactions = await _databaseService.GetTransactionsAsync(_currentAccount.Id);
                
                transactionsList.Items.Clear();
                foreach (var transaction in transactions)
                {
                    var fromUser = transaction.FromAccount?.User?.Username ?? "Unknown";
                    var toUser = transaction.ToAccount?.User?.Username ?? "Unknown";
                    var item = $"{fromUser} → {toUser} | {transaction.Amount:N0} K | {transaction.Description} | {transaction.CreatedAt:yyyy-MM-dd HH:mm}";
                    transactionsList.Items.Add(item);
                }
            }
        }

        private void UserComboBox_SelectedIndexChanged(object? sender, EventArgs e)
        {
            _ = LoadUserDataAsync();
        }

        private async void SwitchUserBtn_Click(object? sender, EventArgs e)
        {
            await LoadUsersAsync();
            await LoadUserDataAsync();
        }

        private async void RefreshBtn_Click(object? sender, EventArgs e)
        {
            try
            {
                await _databaseService.RefreshDataAsync();
                await LoadUserDataAsync();
                
                // Show success message
                var originalText = refreshBtn.Text;
                refreshBtn.Text = "✅ Refreshed";
                refreshBtn.BackColor = Color.Green;
                
                await Task.Delay(1000);
                
                refreshBtn.Text = originalText;
                refreshBtn.BackColor = Color.FromArgb(33, 150, 243);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Refresh failed: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void SendPaymentBtn_Click(object? sender, EventArgs e)
        {
            paymentGroup.Visible = true;
            nfcGroup.Visible = false;
        }

        private void ReceivePaymentBtn_Click(object? sender, EventArgs e)
        {
            if (_currentAccount != null)
            {
                MessageBox.Show($"Your account details:\n\nUsername: {_currentUser?.Username}\nAccount Number: {_currentAccount.AccountNumber}\n\nShare this information with the sender.", 
                    "Receive Payment", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
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
            
            if (_currentAccount != null)
            {
                // Process NFC payment
                var success = await _databaseService.ProcessNFCPaymentAsync("KFC123456789", randomAmount, _currentAccount.Id);
                
                if (success)
                {
                    // Reload data
                    await LoadUserDataAsync();
                    
                    nfcStatusLabel.Text = $"✅ Payment received: {randomAmount} Karats";
                }
                else
                {
                    nfcStatusLabel.Text = "❌ Payment failed";
                }
            }
            
            nfcProgress.Style = ProgressBarStyle.Blocks;
            
            await Task.Delay(2000);
            nfcGroup.Visible = false;
        }

        private async void ConfirmSendBtn_Click(object? sender, EventArgs e)
        {
            if (decimal.TryParse(amountTextBox.Text, out var amount) && amount > 0 && !string.IsNullOrWhiteSpace(recipientTextBox.Text) && _currentAccount != null)
            {
                if (amount > _currentAccount.Balance)
                {
                    MessageBox.Show("Insufficient balance!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                try
                {
                    // Get recipient account
                    var recipientUser = await _databaseService.GetUserByUsernameAsync(recipientTextBox.Text);
                    if (recipientUser == null)
                    {
                        MessageBox.Show("Recipient not found!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }

                    var recipientAccount = recipientUser.Accounts.FirstOrDefault();
                    if (recipientAccount == null)
                    {
                        MessageBox.Show("Recipient has no account!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }

                    // Process payment
                    var success = await _databaseService.ProcessPaymentAsync(
                        _currentAccount.Id, 
                        recipientAccount.Id, 
                        amount, 
                        string.IsNullOrWhiteSpace(descriptionTextBox.Text) 
                            ? $"Payment to {recipientTextBox.Text}" 
                            : descriptionTextBox.Text
                    );

                    if (success)
                    {
                        // Reload data
                        await LoadUserDataAsync();
                        
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
                        MessageBox.Show("Payment failed!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Payment error: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            else
            {
                MessageBox.Show("Please enter valid recipient and amount!", "Invalid Input", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }
    }
}
