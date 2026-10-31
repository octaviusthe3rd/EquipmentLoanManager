using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using CampusGear.ViewModels;

namespace CampusGear.Views;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        AvaloniaXamlLoader.Load(this);
        DataContext = new MainWindowViewModel();
    }
}
