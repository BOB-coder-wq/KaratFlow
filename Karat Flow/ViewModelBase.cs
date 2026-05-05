using System.ComponentModel;
using System.Runtime.CompilerServices;

public class ViewModelBase : INotifyPropertyChanged
{
    // This is the special event that 'yells' when data changes.
    public event PropertyChangedEventHandler? PropertyChanged;

    // This method is used to notify the View that a property has changed.
    protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}