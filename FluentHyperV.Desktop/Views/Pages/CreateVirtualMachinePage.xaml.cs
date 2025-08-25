using FluentHyperV.Desktop.ViewModels.Pages;
using Wpf.Ui.Abstractions.Controls;
using System.Windows.Controls;

namespace FluentHyperV.Desktop.Views.Pages;

/// <summary>
/// Interaction logic for CreateVirtualMachinePage.xaml
/// </summary>
public partial class CreateVirtualMachinePage : Page, INavigableView<CreateVirtualMachineViewModel>
{
    public CreateVirtualMachinePage(CreateVirtualMachineViewModel viewModel)
    {
        ViewModel = viewModel;
        DataContext = this;
        InitializeComponent();
    }

    public CreateVirtualMachineViewModel ViewModel { get; }
}
