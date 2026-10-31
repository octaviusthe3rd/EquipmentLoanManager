# Campus Equipment Simulator (CampusGear)

Requires the .NET 10 SDK.

    dotnet build
    dotnet run --project CampusGear
    dotnet test

Projects
- CampusGear        Avalonia UI (views + view-models, no business rules)
- CampusGear.Core   business rules (Entity, EquipmentItem, Borrower, Loan, Inventory, LoanManager)
- CampusGear.Tests  xUnit tests (49 cases)

See TDD-EVIDENCE.md for how to record two real TDD cycles.
