using System.Windows;
using System.Windows.Controls;
using FluentHyperV.Desktop.ViewModels.Pages;
using Wpf.Ui.Abstractions.Controls;

namespace FluentHyperV.Desktop.Views.Pages;

public partial class VirtualMachineSettingsPage : INavigableView<VirtualMachineSettingsViewModel>
{
    public VirtualMachineSettingsViewModel ViewModel { get; }

    public VirtualMachineSettingsPage(VirtualMachineSettingsViewModel viewModel)
    {
        ViewModel = viewModel;
        DataContext = this;

        InitializeComponent();
    }

    private void SettingsTreeView_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
    {
        if (e.NewValue is TreeViewItem selectedItem && selectedItem.Tag is string tag)
        {
            // 隐藏所有设置面板
            HideAllSettingsPanels();

            // 根据选择的项目显示对应的设置面板
            switch (tag)
            {
                case "0": // 内存
                    MemorySettings.Visibility = Visibility.Visible;
                    break;
                case "1": // 处理器
                    ProcessorSettings.Visibility = Visibility.Visible;
                    break;
                case "2": // SCSI 控制器
                    // 显示控制器信息
                    break;
                case "3": // 硬盘驱动器
                    HardDriveSettings.Visibility = Visibility.Visible;
                    break;
                case "4": // 网络适配器
                    NetworkAdapterSettings.Visibility = Visibility.Visible;
                    break;
                case "5": // 安全性
                    SecuritySettings.Visibility = Visibility.Visible;
                    break;
                case "10": // 管理/名称
                case "11": // 集成服务
                case "12": // 检查点
                case "13": // 智能分页文件位置
                case "14": // 自动启动操作
                case "15": // 自动停止操作
                    ManagementSettings.Visibility = Visibility.Visible;
                    break;
            }
        }
        else if (e.NewValue is TreeViewItem item && item.Tag is int intTag)
        {
            // 处理整数标签
            HideAllSettingsPanels();

            switch (intTag)
            {
                case 0: // 内存
                    MemorySettings.Visibility = Visibility.Visible;
                    break;
                case 1: // 处理器
                    ProcessorSettings.Visibility = Visibility.Visible;
                    break;
                case 2: // SCSI 控制器
                    break;
                case 3: // 硬盘驱动器
                    HardDriveSettings.Visibility = Visibility.Visible;
                    break;
                case 4: // 网络适配器
                    NetworkAdapterSettings.Visibility = Visibility.Visible;
                    break;
                case 5: // 安全性
                    SecuritySettings.Visibility = Visibility.Visible;
                    break;
                case 10: // 管理
                case 11: // 集成服务
                case 12: // 检查点
                case 13: // 智能分页文件位置
                case 14: // 自动启动操作
                case 15: // 自动停止操作
                    ManagementSettings.Visibility = Visibility.Visible;
                    break;
            }
        }
    }

    private void HideAllSettingsPanels()
    {
        MemorySettings.Visibility = Visibility.Collapsed;
        ProcessorSettings.Visibility = Visibility.Collapsed;
        HardDriveSettings.Visibility = Visibility.Collapsed;
        NetworkAdapterSettings.Visibility = Visibility.Collapsed;
        SecuritySettings.Visibility = Visibility.Collapsed;
        ManagementSettings.Visibility = Visibility.Collapsed;
    }
}
