using CampusGear.Core;
using Xunit;

namespace CampusGear.Tests;

public class BorrowingTests
{
    [Fact]
    public void Borrowing_assigns_an_item_and_creates_an_active_loan()
    {
        var manager = Fixture.Configured();
        var id = Fixture.Add(manager, "Ana");

        var result = manager.Borrow(id, "Laptops");

        Assert.True(result.Success);
        var loan = Assert.Single(manager.ActiveLoans);
        Assert.Equal(id, loan.BorrowerId);
        Assert.False(manager.Inventory.Find(loan.ItemId)!.IsAvailable);
    }

    [Fact]
    public void Loans_get_unique_ids()
    {
        var manager = Fixture.Configured();
        var ana = Fixture.Add(manager, "Ana");
        var ben = Fixture.Add(manager, "Ben");
        manager.Borrow(ana, "Laptops");
        manager.Borrow(ben, "Laptops");

        var ids = manager.ActiveLoans.Select(l => l.Id).ToList();

        Assert.Equal(2, ids.Distinct().Count());
    }

    [Fact]
    public void An_item_is_never_assigned_to_two_borrowers()
    {
        var manager = Fixture.Configured(laptops: 2);
        var ana = Fixture.Add(manager, "Ana");
        var ben = Fixture.Add(manager, "Ben");
        manager.Borrow(ana, "Laptops");
        manager.Borrow(ben, "Laptops");

        var itemIds = manager.ActiveLoans.Select(l => l.ItemId).ToList();

        Assert.Equal(2, itemIds.Distinct().Count());
    }

    [Fact]
    public void Borrower_cannot_hold_two_items_of_the_same_category()
    {
        var manager = Fixture.Configured(laptops: 2);
        var id = Fixture.Add(manager, "Ana");
        manager.Borrow(id, "Laptops");

        var result = manager.Borrow(id, "Laptops");

        Assert.False(result.Success);
    }

    [Fact]
    public void Borrower_cannot_have_more_than_two_active_loans()
    {
        var manager = Fixture.Configured(cameras: 1, laptops: 1, mics: 1);
        var id = Fixture.Add(manager, "Ana");
        manager.Borrow(id, "Laptops");
        manager.Borrow(id, "360 Cameras");

        var result = manager.Borrow(id, "Microphones");

        Assert.False(result.Success);
        Assert.Equal(2, manager.ActiveLoans.Count);
    }

    [Fact]
    public void Unregistered_borrower_cannot_borrow()
    {
        var manager = Fixture.Configured();

        var result = manager.Borrow("B-999", "Laptops");

        Assert.False(result.Success);
    }

    [Fact]
    public void Borrowing_from_a_category_with_zero_stock_is_rejected()
    {
        var manager = Fixture.Configured(tripods: 0);
        var id = Fixture.Add(manager, "Ana");

        var result = manager.Borrow(id, "Tripods");

        Assert.False(result.Success);
    }
}
