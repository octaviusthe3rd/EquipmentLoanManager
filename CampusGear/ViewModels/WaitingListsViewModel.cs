using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using CampusGear.Core;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace CampusGear.ViewModels;

public partial class WaitingListsViewModel : PageViewModel
{
    private readonly LoanManager _manager;

    public WaitingListsViewModel(LoanManager manager, Action<string> status) : base(status)
    {
        _manager = manager;
        _selectedCategory = Categories[0];
        Reload();
    }

    public List<string> Categories { get; } = CapacityLimits.Categories.ToList();

    public ObservableCollection<WaitingRow> Entries { get; } = new();

    [ObservableProperty]
    private string _selectedCategory;

    [ObservableProperty]
    private WaitingRow? _selectedEntry;

    [ObservableProperty]
    private bool _canIssue;

    [ObservableProperty]
    private string _hint = "";

    partial void OnSelectedCategoryChanged(string value) => Reload();

    [RelayCommand]
    private void Cancel()
    {
        if (SelectedEntry is null)
        {
            Report(OperationResult.Fail("Select a request to cancel."));
            return;
        }

        Report(_manager.CancelWaiting(SelectedEntry.BorrowerId, SelectedCategory));
        Reload();
    }

    [RelayCommand]
    private void IssueFirst()
    {
        Report(_manager.IssueToNextWaiting(SelectedCategory));
        Reload();
    }

    private void Reload()
    {
        Entries.Clear();
        var position = 1;
        foreach (var borrower in _manager.GetWaitingList(SelectedCategory))
        {
            Entries.Add(new WaitingRow(position++, borrower.Id, borrower.ToString()));
        }

        var available = _manager.Inventory.Available(SelectedCategory);
        CanIssue = Entries.Count > 0 && available > 0;
        Hint = Entries.Count == 0
            ? $"Nobody is waiting for {SelectedCategory}."
            : available == 0
                ? "No item is available yet. Issuing becomes possible after a return."
                : "An item is available. Issue it to the first borrower in line.";
    }
}
