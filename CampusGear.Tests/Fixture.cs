using CampusGear.Core;

namespace CampusGear.Tests;

internal static class Fixture
{
    /// <summary>A manager with equipment already configured.</summary>
    public static LoanManager Configured(int cameras = 1, int laptops = 2, int vr = 0, int mics = 0, int tripods = 0)
    {
        var manager = new LoanManager();
        var result = manager.ConfigureInventory(new Dictionary<string, int>
        {
            ["360 Cameras"] = cameras,
            ["Laptops"] = laptops,
            ["VR Headsets"] = vr,
            ["Microphones"] = mics,
            ["Tripods"] = tripods,
        });
        if (!result.Success)
        {
            throw new InvalidOperationException(result.Message);
        }

        return manager;
    }

    public static string Add(LoanManager manager, string name) => manager.AddBorrower(name).Value!.Id;
}
