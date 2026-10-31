using System.Collections.ObjectModel;
using CampusGear.Core;

namespace CampusGear.ViewModels;

public class RecordsViewModel : ViewModelBase
{
    public RecordsViewModel(LoanManager manager)
    {
        foreach (var loan in manager.ActiveLoans)
        {
            Active.Add(Describe(manager, loan, "Active"));
        }

        foreach (var loan in manager.CompletedLoans)
        {
            Completed.Add(Describe(manager, loan, "Completed"));
        }
    }

    public ObservableCollection<LoanRow> Active { get; } = new();

    public ObservableCollection<LoanRow> Completed { get; } = new();

    private static LoanRow Describe(LoanManager manager, Loan loan, string status)
    {
        var removed = manager.FindBorrower(loan.BorrowerId) is null ? " - removed" : "";
        return new LoanRow(loan.Id, $"{loan.BorrowerName} ({loan.BorrowerId}){removed}", loan.ItemId, loan.Category, status);
    }
}
