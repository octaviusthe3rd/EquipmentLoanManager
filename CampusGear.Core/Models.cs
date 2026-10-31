namespace CampusGear.Core;

/// <summary>Base class for anything with a unique ID.</summary>
public abstract class Entity
{
    public string Id { get; }

    protected Entity(string id) => Id = id;
}

public class EquipmentItem : Entity
{
    public string Category { get; }
    public bool IsAvailable { get; internal set; } = true;

    public EquipmentItem(string id, string category) : base(id) => Category = category;
}

public class Borrower : Entity
{
    public string Name { get; }

    public Borrower(string id, string name) : base(id) => Name = name;

    public override string ToString() => $"{Name} ({Id})";
}

public enum LoanStatus
{
    Active,
    Completed,
}

/// <summary>
/// Links a borrower to an item. The borrower's name is copied onto the loan
/// so the history stays readable after the borrower is removed.
/// </summary>
public class Loan : Entity
{
    public string BorrowerId { get; }
    public string BorrowerName { get; }
    public string ItemId { get; }
    public string Category { get; }
    public LoanStatus Status { get; internal set; } = LoanStatus.Active;
    public bool IsActive => Status == LoanStatus.Active;

    public Loan(string id, Borrower borrower, EquipmentItem item) : base(id)
    {
        BorrowerId = borrower.Id;
        BorrowerName = borrower.Name;
        ItemId = item.Id;
        Category = item.Category;
    }
}

public record CategoryStock(string Category, int Configured, int Available, int Borrowed);
