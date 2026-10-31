using CampusGear.Core;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace CampusGear.ViewModels;

public partial class MainWindowViewModel : ViewModelBase
{
    private readonly LoanManager _manager = new();

    [ObservableProperty]
    private ViewModelBase _currentPage;

    [ObservableProperty]
    private string _statusMessage = "Configure the equipment to begin.";

    [ObservableProperty]
    private bool _isConfigured;

    public MainWindowViewModel()
    {
        _currentPage = new SetupViewModel(_manager, OnSetupDone);
    }

    private void OnSetupDone()
    {
        IsConfigured = true;
        ShowInventory();
    }

    private void Say(string message) => StatusMessage = message;

    private void Go(ViewModelBase page, string status)
    {
        CurrentPage = page;
        StatusMessage = status;
    }

    // Each screen is rebuilt when opened, so it always shows current data.
    [RelayCommand]
    private void ShowInventory() => Go(new InventoryViewModel(_manager), "Showing inventory.");

    [RelayCommand]
    private void ShowBorrowers() => Go(new BorrowersViewModel(_manager, Say), "Showing borrowers.");

    [RelayCommand]
    private void ShowBorrow() => Go(new BorrowViewModel(_manager, Say), "Borrow equipment.");

    [RelayCommand]
    private void ShowReturns() => Go(new ReturnsViewModel(_manager, Say), "Return equipment.");

    [RelayCommand]
    private void ShowWaiting() => Go(new WaitingListsViewModel(_manager, Say), "Showing waiting lists.");

    [RelayCommand]
    private void ShowRecords() => Go(new RecordsViewModel(_manager), "Showing loan records.");
}
