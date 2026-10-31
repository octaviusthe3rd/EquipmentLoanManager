using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using CampusGear.Core;
using CommunityToolkit.Mvvm.ComponentModel;

namespace CampusGear.ViewModels;

public partial class InventoryViewModel : ViewModelBase
{
    private const string All = "All categories";

    private readonly List<EquipmentRow> _allItems;

    public InventoryViewModel(LoanManager manager)
    {
        Summaries = new ObservableCollection<CategorySummary>(
            manager.Stock().Select(s => new CategorySummary(s.Category, s.Configured, s.Available, s.Borrowed)));

        _allItems = manager.Inventory.Items.Select(item =>
        {
            var holder = manager.GetHolder(item.Id);
            return new EquipmentRow(
                item.Id,
                item.Category,
                item.IsAvailable ? "Available" : "Borrowed",
                holder is null ? "-" : $"{holder.BorrowerName} ({holder.BorrowerId})");
        }).ToList();

        Categories = new List<string> { All };
        Categories.AddRange(CapacityLimits.Categories);
        ApplyFilter();
    }

    public ObservableCollection<CategorySummary> Summaries { get; }

    public ObservableCollection<EquipmentRow> VisibleItems { get; } = new();

    public List<string> Categories { get; }

    [ObservableProperty]
    private string _selectedCategory = All;

    public int TotalItems => Summaries.Sum(s => s.Configured);
    public int AvailableItems => Summaries.Sum(s => s.Available);
    public int BorrowedItems => Summaries.Sum(s => s.Borrowed);
    public int CategoryCount => Summaries.Count(s => s.Configured > 0);

    partial void OnSelectedCategoryChanged(string value) => ApplyFilter();

    private void ApplyFilter()
    {
        VisibleItems.Clear();
        foreach (var item in _allItems.Where(i => SelectedCategory == All || i.Category == SelectedCategory))
        {
            VisibleItems.Add(item);
        }
    }
}
