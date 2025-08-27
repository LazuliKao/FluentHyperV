using System.Collections.ObjectModel;
using Wpf.Ui.Controls;

namespace FluentHyperV.Desktop.ViewModels.Windows;

public partial class MainWindowViewModel : ObservableObject
{
    [ObservableProperty]
    private string _applicationTitle = "Fluent HyperV";

    [ObservableProperty]
    private ObservableCollection<object> _menuItems = new()
    {
        new NavigationViewItem
        {
            Content = "仪表板",
            Icon = new SymbolIcon { Symbol = SymbolRegular.Home24 },
            TargetPageType = typeof(Views.Pages.DashboardPage),
        },
        new NavigationViewItem
        {
            Content = "管理",
            Icon = new SymbolIcon { Symbol = SymbolRegular.Server24 },
            TargetPageType = typeof(Views.Pages.VirtualMachinesPage),
        },
        new NavigationViewItem
        {
            Content = "创建",
            Icon = new SymbolIcon { Symbol = SymbolRegular.FormNew24 },
            TargetPageType = typeof(Views.Pages.CreateVirtualMachinePage),
        },
        new NavigationViewItem
        {
            Content = "网络",
            Icon = new SymbolIcon { Symbol = SymbolRegular.Router24 },
            TargetPageType = typeof(Views.Pages.VirtualSwitchesPage),
        },
    };

    [ObservableProperty]
    private ObservableCollection<object> _footerMenuItems = new()
    {
        new NavigationViewItem
        {
            Content = "设置",
            Icon = new SymbolIcon { Symbol = SymbolRegular.Settings24 },
            TargetPageType = typeof(Views.Pages.SettingsPage),
        },
    };

    [ObservableProperty]
    private ObservableCollection<MenuItem> _trayMenuItems = new()
    {
        new MenuItem { Header = "Home", Tag = "tray_home" },
    };

    [ObservableProperty]
    private bool _isFlyoutOpen;

    [RelayCommand]
    private void ToggleFlyout()
    {
        IsFlyoutOpen = !IsFlyoutOpen;
    }
}
