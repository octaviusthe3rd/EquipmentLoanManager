namespace CampusGear.Core;

/// <summary>Owns every equipment item. Items only exist because Configure created them.</summary>
public class Inventory
{
    private static readonly Dictionary<string, string> IdPrefix = new()
    {
        ["360 Cameras"] = "CAM",
        ["Laptops"] = "LAP",
        ["VR Headsets"] = "VR",
        ["Microphones"] = "MIC",
        ["Tripods"] = "TRI",
    };

    private readonly List<EquipmentItem> _items = new();

    public IReadOnlyList<EquipmentItem> Items => _items;
    public bool IsConfigured { get; private set; }

    public OperationResult Configure(IReadOnlyDictionary<string, int> quantities)
    {
        if (IsConfigured)
        {
            return OperationResult.Fail("Equipment is already configured for this session.");
        }

        foreach (var (category, quantity) in quantities)
        {
            if (!CapacityLimits.Max.ContainsKey(category))
            {
                return OperationResult.Fail($"Unknown category '{category}'.");
            }

            if (!CapacityLimits.IsValidQuantity(category, quantity))
            {
                return OperationResult.Fail(
                    $"{category}: enter a whole number from 0 to {CapacityLimits.Max[category]}.");
            }
        }

        foreach (var category in CapacityLimits.Categories)
        {
            quantities.TryGetValue(category, out var count);
            for (var i = 1; i <= count; i++)
            {
                _items.Add(new EquipmentItem($"{IdPrefix[category]}-{i:000}", category));
            }
        }

        IsConfigured = true;
        return OperationResult.Ok($"Configured {_items.Count} items.");
    }

    public int Configured(string category) => _items.Count(i => i.Category == category);

    public int Available(string category) => _items.Count(i => i.Category == category && i.IsAvailable);

    public int Borrowed(string category) => Configured(category) - Available(category);

    public EquipmentItem? FindAvailable(string category) =>
        _items.FirstOrDefault(i => i.Category == category && i.IsAvailable);

    public EquipmentItem? Find(string id) => _items.FirstOrDefault(i => i.Id == id);

    public IReadOnlyList<CategoryStock> Stock() =>
        CapacityLimits.Categories
            .Select(c => new CategoryStock(c, Configured(c), Available(c), Borrowed(c)))
            .ToList();
}
