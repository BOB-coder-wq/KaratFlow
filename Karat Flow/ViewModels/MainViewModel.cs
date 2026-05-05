using System.Collections.ObjectModel;
using Karat_Flow.Models; // Ensure this is the namespace for your Transaction class

public class MainViewModel : ViewModelBase
{
    // 1. The Smart Clipboard (Observable Collection)
    // It holds the list of transactions and automatically notifies the screen when items are added or removed.
    public ObservableCollection<Transaction> Transactions { get; set; }

    private decimal _runningBalance;

    // 2. The Running Balance (A simple property that must 'yell' when it changes)
    public decimal RunningBalance
    {
        get { return _runningBalance; }
        set
        {
            if (_runningBalance != value)
            {
                _runningBalance = value;
                OnPropertyChanged(); // <-- This line tells the screen (View) to update the running balance text box!
            }
        }
    }

    public MainViewModel()
    {
        // Initialize the Smart Clipboard when the helper is created
        Transactions = new ObservableCollection<Transaction>();

        // Start the balance at zero
        RunningBalance = 0.00M;

        // TODO: In the next step, you will add the SignalR connection logic here!
        // LoadInitialData(); // You might load initial data here
    }

    // You will add a method here to calculate the balance whenever a new transaction is added.
    public void UpdateBalance(Transaction newTransaction)
    {
        // Simple balance update logic
        RunningBalance += newTransaction.Amount;
    }
}