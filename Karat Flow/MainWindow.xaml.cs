using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Microsoft.EntityFrameworkCore;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Karat_Flow.Data;
using Karat_Flow.ViewModels;
using Karat_Flow.Models;
using Karat_Flow.Services;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Karat_Flow
{
    /// <summary>
    /// Main window for Karat Flow digital currency application
    /// </summary>
    public sealed partial class MainWindow : Window
    {
        private KaratFlowDbContext _context;
        private AccountViewModel _accountViewModel;
        private NFCService _nfcService;

        public MainWindow()
        {
            this.InitializeComponent();
            InitializeDatabase();
            InitializeViewModels();
            SetupUI();
        }

        private void InitializeDatabase()
        {
            var optionsBuilder = new DbContextOptionsBuilder<KaratFlowDbContext>();
            optionsBuilder.UseSqlServer("Server=(localdb)\\mssqllocaldb;Database=KaratFlowDB;Trusted_Connection=True;");
            _context = new KaratFlowDbContext(optionsBuilder.Options);
        }

        private async void InitializeViewModels()
        {
            _accountViewModel = new AccountViewModel(_context);
            _nfcService = new NFCService(_context);
            
            // Subscribe to NFC service events
            _nfcService.CardDetected += OnCardDetected;
            _nfcService.StatusChanged += OnNFCStatusChanged;
            
            // Wait for data to load
            await System.Threading.Tasks.Task.Delay(1000);
            
            // Update UI with loaded data
            UpdateUI();
        }

        private void UpdateUI()
        {
            if (_accountViewModel.CurrentUser != null)
            {
                WelcomeText.Text = $"Welcome back, {_accountViewModel.DisplayName}!";
                BalanceText.Text = _accountViewModel.BalanceDisplay;
                AccountNumberText.Text = $"Account: {_accountViewModel.AccountNumber}";
                
                // Bind transactions list
                TransactionsList.ItemsSource = _accountViewModel.Transactions.Take(10);
            }
        }

        private void SetupUI()
        {
            // Initially hide payment sections
            SendPaymentSection.Visibility = Visibility.Collapsed;
            NFCPaymentSection.Visibility = Visibility.Collapsed;
        }

        private void SendPaymentBtn_Click(object sender, RoutedEventArgs e)
        {
            SendPaymentSection.Visibility = Visibility.Visible;
            NFCPaymentSection.Visibility = Visibility.Collapsed;
        }

        private void ReceivePaymentBtn_Click(object sender, RoutedEventArgs e)
        {
            // For now, just show a message
            var dialog = new ContentDialog()
            {
                Title = "Receive Payment",
                Content = "Your account details:\n" +
                         $"Username: {_accountViewModel.CurrentUser?.Username}\n" +
                         $"Account Number: {_accountViewModel.AccountNumber}\n\n" +
                         "Share this information with the sender.",
                CloseButtonText = "OK",
                XamlRoot = this.Content.XamlRoot
            };
            _ = dialog.ShowAsync();
        }

        private void NFCPaymentBtn_Click(object sender, RoutedEventArgs e)
        {
            NFCPaymentSection.Visibility = Visibility.Visible;
            SendPaymentSection.Visibility = Visibility.Collapsed;
            StartNFCListening();
        }

        private async void StartNFCListening()
        {
            NFCStatusText.Text = "Waiting for NFC card...";
            NFCProgressRing.IsActive = true;
            
            try
            {
                // Use NFC service to simulate card detection
                var detectedCard = await _nfcService.SimulateCardDetectionAsync();
                
                if (detectedCard != null)
                {
                    // Simulate payment amount (in real app, this would come from the NFC card)
                    var randomAmount = new Random().Next(10, 500);
                    
                    // Process the NFC payment
                    var success = await _nfcService.ProcessNFCPaymentAsync(
                        detectedCard, 
                        randomAmount, 
                        _accountViewModel.CurrentAccount?.Id ?? 0
                    );
                    
                    if (success)
                    {
                        NFCStatusText.Text = $"Payment received: {randomAmount} Karats from {detectedCard.CardNumber}";
                        NFCProgressRing.IsActive = false;
                        
                        await System.Threading.Tasks.Task.Delay(2000);
                        CancelNFCBtn_Click(null, null);
                        
                        // Update UI
                        UpdateUI();
                    }
                    else
                    {
                        NFCStatusText.Text = "Payment failed";
                        NFCProgressRing.IsActive = false;
                    }
                }
            }
            catch (Exception ex)
            {
                NFCStatusText.Text = $"Error: {ex.Message}";
                NFCProgressRing.IsActive = false;
            }
        }

        private void OnCardDetected(NFCCard card)
        {
            // This event is fired when a card is detected
            NFCStatusText.Text = $"Card detected: {card.CardNumber}";
        }

        private void OnNFCStatusChanged(string status)
        {
            // This event is fired when NFC status changes
            NFCStatusText.Text = status;
        }

        private async void ConfirmSendBtn_Click(object sender, RoutedEventArgs e)
        {
            if (decimal.TryParse(AmountBox.Text, out var amount) && amount > 0)
            {
                _accountViewModel.TransactionAmount = amount;
                _accountViewModel.RecipientUsername = RecipientUsernameBox.Text;
                _accountViewModel.TransactionDescription = DescriptionBox.Text;

                var success = await _accountViewModel.SendPaymentAsync();
                
                if (success)
                {
                    var dialog = new ContentDialog()
                    {
                        Title = "Payment Sent",
                        Content = $"Successfully sent {amount} Karats to {RecipientUsernameBox.Text}!\n\nRecipient balance information is private for security.",
                        CloseButtonText = "OK",
                        XamlRoot = this.Content.XamlRoot
                    };
                    _ = dialog.ShowAsync();
                    
                    // Clear form and hide section
                    CancelSendBtn_Click(null, null);
                    
                    // Update UI
                    UpdateUI();
                }
                else
                {
                    var dialog = new ContentDialog()
                    {
                        Title = "Payment Failed",
                        Content = "Unable to send payment. Please check the recipient username and your balance.",
                        CloseButtonText = "OK",
                        XamlRoot = this.Content.XamlRoot
                    };
                    _ = dialog.ShowAsync();
                }
            }
            else
            {
                var dialog = new ContentDialog()
                {
                    Title = "Invalid Amount",
                    Content = "Please enter a valid amount greater than 0.",
                    CloseButtonText = "OK",
                    XamlRoot = this.Content.XamlRoot
                };
                _ = dialog.ShowAsync();
            }
        }

        private void CancelSendBtn_Click(object sender, RoutedEventArgs e)
        {
            SendPaymentSection.Visibility = Visibility.Collapsed;
            RecipientUsernameBox.Text = string.Empty;
            AmountBox.Text = string.Empty;
            DescriptionBox.Text = string.Empty;
        }

        private void CancelNFCBtn_Click(object sender, RoutedEventArgs e)
        {
            NFCPaymentSection.Visibility = Visibility.Collapsed;
            NFCProgressRing.IsActive = false;
            NFCStatusText.Text = "Ready to accept payment";
        }

        private void ViewAllBtn_Click(object sender, RoutedEventArgs e)
        {
            // For now, just show all transactions in the current list
            TransactionsList.ItemsSource = _accountViewModel.Transactions;
        }
    }
}
