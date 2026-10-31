namespace CampusGear.ViewModels;

public record CategorySummary(string Category, int Configured, int Available, int Borrowed);

public record EquipmentRow(string ItemId, string Category, string Status, string HeldBy)
{
    public bool IsBorrowed => Status == "Borrowed";
}

public record BorrowerRow(string Id, string Name, int ActiveLoans, int Waiting);

public record LoanRow(string LoanId, string Borrower, string ItemId, string Category, string Status);

public record WaitingRow(int Position, string BorrowerId, string Borrower);

/// <summary>An item for a ComboBox. ToString is what the ComboBox displays.</summary>
public class Option
{
    public Option(string id, string display)
    {
        Id = id;
        Display = display;
    }

    public string Id { get; }
    public string Display { get; }

    public override string ToString() => Display;
}
