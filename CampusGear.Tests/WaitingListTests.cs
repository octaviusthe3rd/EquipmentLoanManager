using CampusGear.Core;
using Xunit;

namespace CampusGear.Tests;

public class WaitingListTests
{
    private static (LoanManager Manager, string Ana, string Ben, string Cara) CameraScenario()
    {
        var manager = Fixture.Configured(cameras: 1);
        var ana = Fixture.Add(manager, "Ana");
        var ben = Fixture.Add(manager, "Ben");
        var cara = Fixture.Add(manager, "Cara");
        manager.Borrow(ana, "360 Cameras");
        return (manager, ana, ben, cara);
    }

    [Fact]
    public void Borrower_can_join_when_every_item_is_borrowed()
    {
        var (manager, _, ben, _) = CameraScenario();

        var result = manager.JoinWaitingList(ben, "360 Cameras");

        Assert.True(result.Success);
        Assert.Single(manager.GetWaitingList("360 Cameras"));
    }

    [Fact]
    public void Joining_is_rejected_while_an_item_is_available()
    {
        var manager = Fixture.Configured(cameras: 1);
        var id = Fixture.Add(manager, "Ana");

        var result = manager.JoinWaitingList(id, "360 Cameras");

        Assert.False(result.Success);
    }

    [Fact]
    public void Duplicate_entry_in_the_same_queue_is_rejected()
    {
        var (manager, _, ben, _) = CameraScenario();
        manager.JoinWaitingList(ben, "360 Cameras");

        var result = manager.JoinWaitingList(ben, "360 Cameras");

        Assert.False(result.Success);
        Assert.Single(manager.GetWaitingList("360 Cameras"));
    }

    [Fact]
    public void Joining_a_category_configured_with_zero_stock_is_rejected()
    {
        var manager = Fixture.Configured(tripods: 0);
        var id = Fixture.Add(manager, "Ana");

        var result = manager.JoinWaitingList(id, "Tripods");

        Assert.False(result.Success);
    }

    [Fact]
    public void Queue_keeps_first_come_first_served_order()
    {
        var (manager, _, ben, cara) = CameraScenario();
        manager.JoinWaitingList(ben, "360 Cameras");
        manager.JoinWaitingList(cara, "360 Cameras");

        var names = manager.GetWaitingList("360 Cameras").Select(b => b.Name).ToList();

        Assert.Equal(new List<string> { "Ben", "Cara" }, names);
    }

    [Fact]
    public void Cancelling_a_request_removes_only_that_borrower()
    {
        var (manager, _, ben, cara) = CameraScenario();
        manager.JoinWaitingList(ben, "360 Cameras");
        manager.JoinWaitingList(cara, "360 Cameras");

        var result = manager.CancelWaiting(ben, "360 Cameras");

        Assert.True(result.Success);
        var remaining = Assert.Single(manager.GetWaitingList("360 Cameras"));
        Assert.Equal("Cara", remaining.Name);
    }

    [Fact]
    public void Cancelling_when_not_on_the_list_is_rejected()
    {
        var (manager, _, ben, _) = CameraScenario();

        var result = manager.CancelWaiting(ben, "360 Cameras");

        Assert.False(result.Success);
    }

    [Fact]
    public void New_requests_cannot_bypass_an_existing_queue()
    {
        var (manager, ana, ben, cara) = CameraScenario();
        manager.JoinWaitingList(ben, "360 Cameras");
        var loan = manager.ActiveLoans.Single();
        manager.Return(loan.Id);

        var result = manager.Borrow(cara, "360 Cameras");

        Assert.False(result.Success);
    }

    [Fact]
    public void Returning_an_item_reports_the_next_waiting_borrower()
    {
        var (manager, _, ben, _) = CameraScenario();
        manager.JoinWaitingList(ben, "360 Cameras");
        var loan = manager.ActiveLoans.Single();

        var result = manager.Return(loan.Id);

        Assert.Contains("Ben", result.Message);
        Assert.Equal("Ben", manager.NextWaiting("360 Cameras")!.Name);
    }

    [Fact]
    public void Issuing_to_the_next_borrower_creates_a_loan_and_removes_the_request()
    {
        var (manager, _, ben, _) = CameraScenario();
        manager.JoinWaitingList(ben, "360 Cameras");
        manager.Return(manager.ActiveLoans.Single().Id);

        var result = manager.IssueToNextWaiting("360 Cameras");

        Assert.True(result.Success);
        Assert.Equal(ben, result.Value!.BorrowerId);
        Assert.Empty(manager.GetWaitingList("360 Cameras"));
    }

    [Fact]
    public void Issuing_is_rejected_until_an_item_is_available()
    {
        var (manager, _, ben, _) = CameraScenario();
        manager.JoinWaitingList(ben, "360 Cameras");

        var result = manager.IssueToNextWaiting("360 Cameras");

        Assert.False(result.Success);
        Assert.Single(manager.GetWaitingList("360 Cameras"));
    }

    [Fact]
    public void Eligibility_is_checked_again_before_issuing()
    {
        var manager = Fixture.Configured(cameras: 1, laptops: 1, mics: 1);
        var ana = Fixture.Add(manager, "Ana");
        var ben = Fixture.Add(manager, "Ben");
        var cameraLoan = manager.Borrow(ana, "360 Cameras").Value!;
        manager.JoinWaitingList(ben, "360 Cameras");
        manager.Borrow(ben, "Laptops");
        manager.Borrow(ben, "Microphones");
        manager.Return(cameraLoan.Id);

        var result = manager.IssueToNextWaiting("360 Cameras");

        Assert.False(result.Success);
        Assert.Contains("not eligible", result.Message);
    }
}
