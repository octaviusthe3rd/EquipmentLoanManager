using System.Globalization;

namespace CampusGear.Core;

/// <summary>Central place for each category's name and maximum quantity.</summary>
public static class CapacityLimits
{
    public static IReadOnlyList<string> Categories { get; } = new[]
    {
        "360 Cameras", "Laptops", "VR Headsets", "Microphones", "Tripods",
    };

    public static IReadOnlyDictionary<string, int> Max { get; } = new Dictionary<string, int>
    {
        ["360 Cameras"] = 5,
        ["Laptops"] = 8,
        ["VR Headsets"] = 10,
        ["Microphones"] = 6,
        ["Tripods"] = 12,
    };

    public static bool IsValidQuantity(string category, int quantity) =>
        Max.TryGetValue(category, out var max) && quantity >= 0 && quantity <= max;

    /// <summary>Parses typed text into a whole number from 0 to the category maximum.</summary>
    public static bool TryParseQuantity(string category, string? text, out int quantity, out string error)
    {
        quantity = 0;
        error = "";

        if (!Max.TryGetValue(category, out var max))
        {
            error = "Unknown category.";
            return false;
        }

        var ok = int.TryParse(text?.Trim(), NumberStyles.None, CultureInfo.InvariantCulture, out quantity)
                 && quantity <= max;
        if (!ok)
        {
            quantity = 0;
            error = $"Enter a whole number from 0 to {max}.";
        }

        return ok;
    }
}
