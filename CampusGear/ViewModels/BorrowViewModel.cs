using System;
using System.Collections.Generic;
using System.Linq;
using CampusGear.Core;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace CampusGear.ViewModels;

public partial class BorrowViewModel : PageViewModel
{
    private readonly LoanManager _manager;

    public BorrowViewModel(LoanManager manager, Action<string> status) : base(status)
    {
        _manager = manager;
        Borrowers = manager.Borrowers.Select(b => new Option(b.Id, b.ToString())).ToList();
    }

    public List<Option> Borrowers { get; }

    public List<string> Categories { get; } = CapacityLimits.Categories.ToList();

    [ObservableProperty]
    private Option? _selectedBorrower;

    [ObservableProperty]
    private string? _selectedCategory;

    [ObservableProperty]
    private string _availabilityText = "Choose a borrower and a category.";

    [ObservableProperty]
    private string _eligibilityText = "";

    [ObservableProperty]
    private bool _canBorrow;

    [ObservableProperty]
    private bool _canJoin;

    partial void OnSelectedBorrowerChanged(Option? value) => Refresh();

    partial void OnSelectedCategoryChanged(string? value) => Refresh();

    private void Refresh()
    {
        CanBorrow = false;
        CanJoin = false;

        if (SelectedBorrower is null || SelectedCategory is null)
        {
            AvailabilityText = "Choose a borrower and a category.";
            EligibilityText = "";
            return;
        }

        var category = SelectedCategory;
        var configured = _manager.Inventory.Configured(category);
        var available = _manager.Inventory.Available(category);
        var waiting = _manager.GetWaitingList(category).Count;

        AvailabilityText = $"{available} of {configured} available"
                           + (waiting > 0 ? $", {waiting} waiting" : "");

        var eligibility = _manager.CheckEligibility(SelectedBorrower.Id, category);
        EligibilityText = eligibility.Success ? "Eligible to borrow." : eligibility.Message;
        if (!eligibility.Success)
        {
            return;
        }

        var alreadyWaiting = _manager.IsWaiting(SelectedBorrower.Id, category);
        if (alreadyWaiting)
        {
            EligibilityText += " Already on the waiting list.";
        }

        CanBorrow = available > 0 && waiting == 0;
        CanJoin = (available == 0 || waiting > 0) && !alreadyWaiting;
    }

    [RelayCommand]
    private void Borrow()
    {
        if (SelectedBorrower is null || SelectedCategory is null)
        {
            return;
        }

        Report(_manager.Borrow(SelectedBorrower.Id, SelectedCategory));
        Refresh();
    }

    [RelayCommand]
    private void Join()
    {
        if (SelectedBorrower is null || SelectedCategory is null)
        {
            return;
        }

        Report(_manager.JoinWaitingList(SelectedBorrower.Id, SelectedCategory));
        Refresh();
    }
}
