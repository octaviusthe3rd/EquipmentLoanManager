using System;
using System.Collections.ObjectModel;
using CampusGear.Core;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace CampusGear.ViewModels;

public partial class ReturnsViewModel : PageViewModel
{
    private readonly LoanManager _manager;
    private string? _pendingCategory;

    public ReturnsViewModel(LoanManager manager, Action<string> status) : base(status)
    {
        _manager = manager;
        Reload();
    }

    public ObservableCollection<LoanRow> ActiveLoans { get; } = new();

    [ObservableProperty]
    private LoanRow? _selected;

    [ObservableProperty]
    private bool _hasNext;

    [ObservableProperty]
    private string _nextText = "";

    [RelayCommand]
    private void ReturnSelected()
    {
        if (Selected is null)
        {
            Report(OperationResult.Fail("Select a loan to return."));
            return;
        }

        var category = Selected.Category;
        var result = _manager.Return(Selected.LoanId);
        Report(result);
        Reload();
        UpdateNext(result.Success ? category : _pendingCategory);
    }

    [RelayCommand]
    private void IssueNext()
    {
        if (_pendingCategory is null)
        {
            return;
        }

        Report(_manager.IssueToNextWaiting(_pendingCategory));
        Reload();
        UpdateNext(_pendingCategory);
    }

    private void UpdateNext(string? category)
    {
        _pendingCategory = category;
        var next = category is null ? null : _manager.NextWaiting(category);
        HasNext = category is not null && next is not null && _manager.Inventory.Available(category) > 0;
        NextText = HasNext ? $"Next in queue for {category}: {next}" : "";
    }

    private void Reload()
    {
        ActiveLoans.Clear();
        foreach (var loan in _manager.ActiveLoans)
        {
            ActiveLoans.Add(new LoanRow(loan.Id, $"{loan.BorrowerName} ({loan.BorrowerId})", loan.ItemId, loan.Category, "Active"));
        }
    }
}
