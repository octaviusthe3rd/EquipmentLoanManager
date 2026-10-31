using CampusGear.Core;
using Xunit;

namespace CampusGear.Tests;

public class ReturnTests
{
    [Fact]
    public void Returning_a_loan_completes_it_and_frees_the_item()
    {
        var manager = Fixture.Configured();
        var id = Fixture.Add(manager, "Ana");
        var loan = manager.Borrow(id, "Laptops").Value!;

        var result = manager.Return(loan.Id);

        Assert.True(result.Success);
        Assert.Empty(manager.ActiveLoans);
        Assert.Single(manager.CompletedLoans);
        Assert.True(manager.Inventory.Find(loan.ItemId)!.IsAvailable);
    }

    [Fact]
    public void Returning_the_same_loan_twice_is_rejected()
    {
        var manager = Fixture.Configured();
        var id = Fixture.Add(manager, "Ana");
        var loan = manager.Borrow(id, "Laptops").Value!;
        manager.Return(loan.Id);

        var second = manager.Return(loan.Id);

        Assert.False(second.Success);
    }

    [Fact]
    public void Returning_an_unknown_loan_is_rejected()
    {
        var manager = Fixture.Configured();

        var result = manager.Return("L-999");

        Assert.False(result.Success);
    }

    [Fact]
    public void Borrower_can_borrow_again_after_returning()
    {
        var manager = Fixture.Configured(laptops: 1);
        var id = Fixture.Add(manager, "Ana");
        var loan = manager.Borrow(id, "Laptops").Value!;
        manager.Return(loan.Id);

        var again = manager.Borrow(id, "Laptops");

        Assert.True(again.Success);
    }
}
