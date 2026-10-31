using CampusGear.Core;
using Xunit;

namespace CampusGear.Tests;

public class BorrowerTests
{
    [Fact]
    public void Borrower_with_a_name_is_added()
    {
        var manager = Fixture.Configured();

        var result = manager.AddBorrower("Ana");

        Assert.True(result.Success);
        Assert.Single(manager.Borrowers);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Empty_name_is_rejected(string name)
    {
        var manager = Fixture.Configured();

        var result = manager.AddBorrower(name);

        Assert.False(result.Success);
        Assert.Empty(manager.Borrowers);
    }

    [Fact]
    public void Duplicate_borrower_id_is_rejected()
    {
        var manager = Fixture.Configured();
        manager.AddBorrower("Ana", "B-100");

        var result = manager.AddBorrower("Ben", "B-100");

        Assert.False(result.Success);
        Assert.Single(manager.Borrowers);
    }

    [Fact]
    public void Generated_ids_are_unique()
    {
        var manager = Fixture.Configured();
        manager.AddBorrower("Ana", "B-001");

        var generated = manager.AddBorrower("Ben");

        Assert.True(generated.Success);
        Assert.NotEqual("B-001", generated.Value!.Id);
    }

    [Fact]
    public void Borrower_without_loans_can_be_removed()
    {
        var manager = Fixture.Configured();
        var id = Fixture.Add(manager, "Ana");

        var result = manager.RemoveBorrower(id);

        Assert.True(result.Success);
        Assert.Empty(manager.Borrowers);
    }

    [Fact]
    public void Borrower_with_an_active_loan_cannot_be_removed()
    {
        var manager = Fixture.Configured();
        var id = Fixture.Add(manager, "Ana");
        manager.Borrow(id, "Laptops");

        var result = manager.RemoveBorrower(id);

        Assert.False(result.Success);
        Assert.Single(manager.Borrowers);
    }

    [Fact]
    public void Removing_a_borrower_also_removes_their_waiting_list_entries()
    {
        var manager = Fixture.Configured(cameras: 1);
        var ana = Fixture.Add(manager, "Ana");
        var ben = Fixture.Add(manager, "Ben");
        manager.Borrow(ana, "360 Cameras");
        manager.JoinWaitingList(ben, "360 Cameras");

        manager.RemoveBorrower(ben);

        Assert.Empty(manager.GetWaitingList("360 Cameras"));
    }

    [Fact]
    public void Removing_a_borrower_keeps_their_completed_loan_history()
    {
        var manager = Fixture.Configured();
        var id = Fixture.Add(manager, "Ana");
        var loan = manager.Borrow(id, "Laptops").Value!;
        manager.Return(loan.Id);

        manager.RemoveBorrower(id);

        var history = Assert.Single(manager.CompletedLoans);
        Assert.Equal("Ana", history.BorrowerName);
    }
}
