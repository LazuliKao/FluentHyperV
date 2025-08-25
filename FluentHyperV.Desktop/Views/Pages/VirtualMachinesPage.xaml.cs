using FluentHyperV.Desktop.ViewModels.Pages;
using Wpf.Ui.Abstractions.Controls;
using System.Windows.Input;

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
}
