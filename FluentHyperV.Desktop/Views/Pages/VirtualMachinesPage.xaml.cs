using FluentHyperV.Desktop.ViewModels.Pages;
using Wpf.Ui.Abstractions.Controls;
using System.Windows.Input;
using System.Windows.Controls;

namespace FluentHyperV.Desktop.Views.Pages;

/// <summary>
/// Interaction logic for VirtualMachinesPage.xaml
/// </summary>
public partial class VirtualMachinesPage : INavigableView<VirtualMachinesViewModel>
{
    public VirtualMachinesPage(VirtualMachinesViewModel viewModel)
    {
        ViewModel = viewModel;
        DataContext = this;
        InitializeComponent();
    }

    public VirtualMachinesViewModel ViewModel { get; }

    private async void VirtualMachinesDataGrid_MouseDoubleClick(object sender, MouseButtonEventArgs e)
    {
        // 检查是否点击在数据行上（而不是空白区域）
        if (ViewModel.SelectedVirtualMachine != null)
        {
            // 双击时连接到虚拟机
            await ViewModel.ConnectToVirtualMachineCommand.ExecuteAsync(null);
        }
    }

    private void ContextMenu_Opened(object sender, System.Windows.RoutedEventArgs e)
    {
        if (sender is ContextMenu contextMenu &&
            contextMenu.PlacementTarget is System.Windows.FrameworkElement border)
        {
            // 在DataTemplate中，Border的DataContext就是VirtualMachineViewModel
            if (border.DataContext is VirtualMachineViewModel vmViewModel)
            {
                // 当右键菜单打开时，设置当前虚拟机为选中状态
                ViewModel.SelectedVirtualMachine = vmViewModel;
            }
        }
    }

    private async void StartButton_Click(object sender, System.Windows.RoutedEventArgs e)
    {
        if (sender is System.Windows.FrameworkElement button &&
            button.DataContext is VirtualMachineViewModel vmViewModel)
        {
            // 临时设置选中的虚拟机
            var previousSelection = ViewModel.SelectedVirtualMachine;
            ViewModel.SelectedVirtualMachine = vmViewModel;

            // 设置选中状态以便命令能正确工作
            vmViewModel.IsSelected = true;
            await ViewModel.StartVirtualMachineCommand.ExecuteAsync(null);

            // 恢复之前的选择
            ViewModel.SelectedVirtualMachine = previousSelection;
        }
    }

    private async void StopButton_Click(object sender, System.Windows.RoutedEventArgs e)
    {
        if (sender is System.Windows.FrameworkElement button &&
            button.DataContext is VirtualMachineViewModel vmViewModel)
        {
            // 临时设置选中的虚拟机
            var previousSelection = ViewModel.SelectedVirtualMachine;
            ViewModel.SelectedVirtualMachine = vmViewModel;

            // 设置选中状态以便命令能正确工作
            vmViewModel.IsSelected = true;
            await ViewModel.StopVirtualMachineCommand.ExecuteAsync(null);

            // 恢复之前的选择
            ViewModel.SelectedVirtualMachine = previousSelection;
        }
    }

    private async void ConnectButton_Click(object sender, System.Windows.RoutedEventArgs e)
    {
        if (sender is System.Windows.FrameworkElement button &&
            button.DataContext is VirtualMachineViewModel vmViewModel)
        {
            // 临时设置选中的虚拟机
            var previousSelection = ViewModel.SelectedVirtualMachine;
            ViewModel.SelectedVirtualMachine = vmViewModel;

            await ViewModel.ConnectToVirtualMachineCommand.ExecuteAsync(null);

            // 恢复之前的选择
            ViewModel.SelectedVirtualMachine = previousSelection;
        }
    }
}
