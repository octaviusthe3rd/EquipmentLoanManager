using CampusGear.Core;
using Xunit;

namespace CampusGear.Tests;

public class InventoryTests
{
    [Fact]
    public void Configuring_two_laptops_and_three_vr_headsets_creates_five_items()
    {
        var manager = Fixture.Configured(cameras: 0, laptops: 2, vr: 3);

        Assert.Equal(5, manager.Inventory.Items.Count);
    }

    [Fact]
    public void Every_item_gets_a_unique_id()
    {
        var manager = Fixture.Configured(cameras: 2, laptops: 3, vr: 2, mics: 2, tripods: 2);

        var ids = manager.Inventory.Items.Select(i => i.Id).ToList();

        Assert.Equal(ids.Count, ids.Distinct().Count());
    }

    [Fact]
    public void Quantity_above_the_category_maximum_is_rejected()
    {
        var manager = new LoanManager();

        var result = manager.ConfigureInventory(new Dictionary<string, int> { ["360 Cameras"] = 6 });

        Assert.False(result.Success);
        Assert.Empty(manager.Inventory.Items);
    }

    [Fact]
    public void Negative_quantity_is_rejected()
    {
        var manager = new LoanManager();

        var result = manager.ConfigureInventory(new Dictionary<string, int> { ["Laptops"] = -1 });

        Assert.False(result.Success);
    }

    [Fact]
    public void Configured_available_and_borrowed_totals_are_calculated()
    {
        var manager = Fixture.Configured(laptops: 3);
        var id = Fixture.Add(manager, "Ana");
        manager.Borrow(id, "Laptops");

        var stock = manager.Stock().First(s => s.Category == "Laptops");

        Assert.Equal(3, stock.Configured);
        Assert.Equal(2, stock.Available);
        Assert.Equal(1, stock.Borrowed);
    }
}
