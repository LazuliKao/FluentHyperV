using FluentHyperV.Desktop.ViewModels.Pages;
using Wpf.Ui.Abstractions.Controls;

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
}
