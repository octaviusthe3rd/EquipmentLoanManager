namespace CampusGear.Core;

/// <summary>
/// All borrowing rules in one place. Holds the inventory, the registered
/// borrowers, every loan (active and completed) and one FIFO queue per category.
/// </summary>
public class LoanManager
{
    public const int MaxActiveLoans = 2;

    private readonly List<Borrower> _borrowers = new();
    private readonly List<Loan> _loans = new();
    private readonly Dictionary<string, Queue<string>> _queues = new();
    private int _loanCounter;
    private int _borrowerCounter;

    public Inventory Inventory { get; } = new();

    public LoanManager()
    {
        foreach (var category in CapacityLimits.Categories)
        {
            _queues[category] = new Queue<string>();
        }
    }

    public IReadOnlyList<Borrower> Borrowers => _borrowers;
    public IReadOnlyList<Loan> ActiveLoans => _loans.Where(l => l.IsActive).ToList();
    public IReadOnlyList<Loan> CompletedLoans => _loans.Where(l => !l.IsActive).ToList();

    // ---------- setup ----------

    public OperationResult ConfigureInventory(IReadOnlyDictionary<string, int> quantities) =>
        Inventory.Configure(quantities);

    // ---------- borrowers ----------

    public Borrower? FindBorrower(string id) =>
        _borrowers.FirstOrDefault(b => string.Equals(b.Id, id, StringComparison.OrdinalIgnoreCase));

    public OperationResult<Borrower> AddBorrower(string? name, string? id = null)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return OperationResult<Borrower>.Fail("Borrower name cannot be empty.");
        }

        id = id?.Trim();
        if (string.IsNullOrEmpty(id))
        {
            do
            {
                id = $"B-{++_borrowerCounter:000}";
            }
            while (FindBorrower(id) is not null);
        }
        else if (FindBorrower(id) is not null)
        {
            return OperationResult<Borrower>.Fail($"Borrower ID '{id}' already exists.");
        }

        var borrower = new Borrower(id, name.Trim());
        _borrowers.Add(borrower);
        return OperationResult<Borrower>.Ok($"Borrower {borrower} added.", borrower);
    }

    public OperationResult RemoveBorrower(string id)
    {
        var borrower = FindBorrower(id);
        if (borrower is null)
        {
            return OperationResult.Fail("Borrower not found.");
        }

        if (ActiveLoanCount(borrower.Id) > 0)
        {
            return OperationResult.Fail($"{borrower.Name} still has active loans and cannot be removed.");
        }

        foreach (var category in CapacityLimits.Categories)
        {
            RemoveFromQueue(category, borrower.Id);
        }

        _borrowers.Remove(borrower);
        return OperationResult.Ok($"Borrower {borrower} removed. Their waiting-list requests were removed too.");
    }

    public int ActiveLoanCount(string borrowerId) =>
        _loans.Count(l => l.IsActive && l.BorrowerId == borrowerId);

    public int WaitingCount(string borrowerId) =>
        _queues.Values.Count(q => q.Contains(borrowerId));

    public Loan? GetHolder(string itemId) =>
        _loans.FirstOrDefault(l => l.IsActive && l.ItemId == itemId);

    // ---------- eligibility ----------

    public OperationResult CheckEligibility(string borrowerId, string category)
    {
        var borrower = FindBorrower(borrowerId);
        if (borrower is null)
        {
            return OperationResult.Fail("Only registered borrowers can borrow equipment.");
        }

        if (!CapacityLimits.Max.ContainsKey(category))
        {
            return OperationResult.Fail("Unknown equipment category.");
        }

        if (Inventory.Configured(category) == 0)
        {
            return OperationResult.Fail($"No {category} are configured, so none can be borrowed.");
        }

        if (ActiveLoanCount(borrower.Id) >= MaxActiveLoans)
        {
            return OperationResult.Fail($"{borrower.Name} already has {MaxActiveLoans} active loans.");
        }

        if (_loans.Any(l => l.IsActive && l.BorrowerId == borrower.Id && l.Category == category))
        {
            return OperationResult.Fail($"{borrower.Name} already holds a {category} item.");
        }

        return OperationResult.Ok("Eligible.");
    }

    // ---------- borrowing ----------

    public OperationResult<Loan> Borrow(string borrowerId, string category)
    {
        var eligibility = CheckEligibility(borrowerId, category);
        if (!eligibility.Success)
        {
            return OperationResult<Loan>.Fail(eligibility.Message);
        }

        if (_queues[category].Count > 0)
        {
            return OperationResult<Loan>.Fail(
                $"Other borrowers are already waiting for {category}. Join the waiting list.");
        }

        var item = Inventory.FindAvailable(category);
        if (item is null)
        {
            return OperationResult<Loan>.Fail($"All {category} are borrowed. Join the waiting list.");
        }

        return CreateLoan(FindBorrower(borrowerId)!, item);
    }

    private OperationResult<Loan> CreateLoan(Borrower borrower, EquipmentItem item)
    {
        item.IsAvailable = false;
        var loan = new Loan($"L-{++_loanCounter:000}", borrower, item);
        _loans.Add(loan);
        return OperationResult<Loan>.Ok($"Loan {loan.Id}: {item.Id} ({item.Category}) lent to {borrower}.", loan);
    }

    // ---------- returns ----------

    public OperationResult<Loan> Return(string loanId)
    {
        var loan = _loans.FirstOrDefault(l => l.Id == loanId);
        if (loan is null)
        {
            return OperationResult<Loan>.Fail("Loan not found.");
        }

        if (!loan.IsActive)
        {
            return OperationResult<Loan>.Fail($"Loan {loan.Id} was already returned.");
        }

        loan.Status = LoanStatus.Completed;
        var item = Inventory.Find(loan.ItemId);
        if (item is not null)
        {
            item.IsAvailable = true;
        }

        var message = $"Loan {loan.Id} completed. {loan.ItemId} is available again.";
        var next = NextWaiting(loan.Category);
        if (next is not null)
        {
            message += $" Next in line for {loan.Category}: {next}.";
        }

        return OperationResult<Loan>.Ok(message, loan);
    }

    // ---------- waiting lists ----------

    public IReadOnlyList<Borrower> GetWaitingList(string category) =>
        _queues.TryGetValue(category, out var queue)
            ? queue.Select(id => FindBorrower(id)!).ToList()
            : new List<Borrower>();

    public Borrower? NextWaiting(string category) =>
        _queues.TryGetValue(category, out var queue) && queue.Count > 0 ? FindBorrower(queue.Peek()) : null;

    public bool IsWaiting(string borrowerId, string category) =>
        _queues.TryGetValue(category, out var queue) && queue.Contains(borrowerId);

    public OperationResult JoinWaitingList(string borrowerId, string category)
    {
        var eligibility = CheckEligibility(borrowerId, category);
        if (!eligibility.Success)
        {
            return eligibility;
        }

        var borrower = FindBorrower(borrowerId)!;
        var queue = _queues[category];
        if (queue.Contains(borrower.Id))
        {
            return OperationResult.Fail($"{borrower.Name} is already waiting for {category}.");
        }

        if (queue.Count == 0 && Inventory.Available(category) > 0)
        {
            return OperationResult.Fail($"{category} are available. Borrow one instead of waiting.");
        }

        queue.Enqueue(borrower.Id);
        return OperationResult.Ok($"{borrower} joined the {category} waiting list at position {queue.Count}.");
    }

    public OperationResult CancelWaiting(string borrowerId, string category)
    {
        if (!IsWaiting(borrowerId, category))
        {
            return OperationResult.Fail("That borrower is not on this waiting list.");
        }

        RemoveFromQueue(category, borrowerId);
        return OperationResult.Ok($"Waiting-list request for {category} cancelled.");
    }

    public OperationResult<Loan> IssueToNextWaiting(string category)
    {
        var next = NextWaiting(category);
        if (next is null)
        {
            return OperationResult<Loan>.Fail($"Nobody is waiting for {category}.");
        }

        var item = Inventory.FindAvailable(category);
        if (item is null)
        {
            return OperationResult<Loan>.Fail($"No {category} item is available yet.");
        }

        var eligibility = CheckEligibility(next.Id, category);
        if (!eligibility.Success)
        {
            return OperationResult<Loan>.Fail($"{next.Name} is not eligible right now: {eligibility.Message}");
        }

        _queues[category].Dequeue();
        return CreateLoan(next, item);
    }

    private void RemoveFromQueue(string category, string borrowerId)
    {
        _queues[category] = new Queue<string>(_queues[category].Where(id => id != borrowerId));
    }

    // ---------- views ----------

    public IReadOnlyList<CategoryStock> Stock() => Inventory.Stock();
}
