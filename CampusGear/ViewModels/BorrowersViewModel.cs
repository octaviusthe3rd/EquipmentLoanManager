using System;
using System.Collections.ObjectModel;
using CampusGear.Core;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace CampusGear.ViewModels;

public partial class BorrowersViewModel : PageViewModel
{
    private readonly LoanManager _manager;

    public BorrowersViewModel(LoanManager manager, Action<string> status) : base(status)
    {
        _manager = manager;
        Reload();
    }

    public ObservableCollection<BorrowerRow> Rows { get; } = new();

    [ObservableProperty]
    private string _name = "";

    [ObservableProperty]
    private string _borrowerId = "";

    [ObservableProperty]
    private BorrowerRow? _selected;

    [RelayCommand]
    private void Add()
    {
        var result = _manager.AddBorrower(Name, BorrowerId);
        Report(result);
        if (result.Success)
        {
            Name = "";
            BorrowerId = "";
            Reload();
        }
    }

    [RelayCommand]
    private void Remove()
    {
        if (Selected is null)
        {
            Report(OperationResult.Fail("Select a borrower to remove."));
            return;
        }

        Report(_manager.RemoveBorrower(Selected.Id));
        Reload();
    }

    private void Reload()
    {
        Rows.Clear();
        foreach (var b in _manager.Borrowers)
        {
            Rows.Add(new BorrowerRow(b.Id, b.Name, _manager.ActiveLoanCount(b.Id), _manager.WaitingCount(b.Id)));
        }
    }
}
