using System;
using System.Collections.Generic;
using System.Linq;
using CampusGear.Core;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace CampusGear.ViewModels;

public partial class QuantityInput : ObservableObject
{
    public QuantityInput(string category)
    {
        Category = category;
        Max = CapacityLimits.Max[category];
    }

    public string Category { get; }
    public int Max { get; }

    [ObservableProperty]
    private string _text = "2";

    [ObservableProperty]
    private string _error = "";
}

public partial class SetupViewModel : PageViewModel
{
    private readonly LoanManager _manager;
    private readonly Action _done;

    public SetupViewModel(LoanManager manager, Action done) : base(_ => { })
    {
        _manager = manager;
        _done = done;
        Inputs = CapacityLimits.Categories.Select(c => new QuantityInput(c)).ToList();
    }

    public List<QuantityInput> Inputs { get; }

    [RelayCommand]
    private void Confirm()
    {
        var quantities = new Dictionary<string, int>();
        var allValid = true;

        foreach (var input in Inputs)
        {
            if (CapacityLimits.TryParseQuantity(input.Category, input.Text, out var quantity, out var error))
            {
                input.Error = "";
                quantities[input.Category] = quantity;
            }
            else
            {
                input.Error = error;
                allValid = false;
            }
        }

        if (!allValid)
        {
            Report(OperationResult.Fail("Fix the highlighted entries and try again."));
            return;
        }

        var result = _manager.ConfigureInventory(quantities);
        if (!result.Success)
        {
            Report(result);
            return;
        }

        _done();
    }
}
