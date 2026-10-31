using CampusGear.Core;
using Xunit;

namespace CampusGear.Tests;

public class CapacityLimitsTests
{
    [Theory]
    [InlineData("Laptops", 0)]
    [InlineData("Laptops", 8)]
    [InlineData("Tripods", 12)]
    public void Quantity_within_range_is_accepted(string category, int quantity)
    {
        Assert.True(CapacityLimits.IsValidQuantity(category, quantity));
    }

    [Theory]
    [InlineData("Laptops", -1)]
    [InlineData("Laptops", 9)]
    [InlineData("Unknown", 1)]
    public void Quantity_out_of_range_or_unknown_category_is_rejected(string category, int quantity)
    {
        Assert.False(CapacityLimits.IsValidQuantity(category, quantity));
    }

    [Theory]
    [InlineData("abc")]
    [InlineData("-1")]
    [InlineData("6")]
    [InlineData("2.5")]
    [InlineData("")]
    public void Typed_text_that_is_not_a_valid_quantity_is_rejected(string text)
    {
        var ok = CapacityLimits.TryParseQuantity("360 Cameras", text, out _, out var error);

        Assert.False(ok);
        Assert.Contains("0 to 5", error);
    }

    [Fact]
    public void Typed_text_with_a_valid_quantity_is_parsed()
    {
        var ok = CapacityLimits.TryParseQuantity("360 Cameras", " 5 ", out var quantity, out _);

        Assert.True(ok);
        Assert.Equal(5, quantity);
    }
}
