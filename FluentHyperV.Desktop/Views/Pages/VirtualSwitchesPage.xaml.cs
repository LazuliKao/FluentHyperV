using FluentHyperV.Desktop.ViewModels.Pages;
using Wpf.Ui.Abstractions.Controls;
using System.Windows.Input;

namespace FluentHyperV.Desktop.Views.Pages;

/// <summary>
/// Interaction logic for VirtualSwitchesPage.xaml
/// </summary>
public partial class VirtualSwitchesPage : INavigableView<VirtualSwitchesViewModel>
{
    public VirtualSwitchesPage(VirtualSwitchesViewModel viewModel)
    {
        ViewModel = viewModel;
        DataContext = this;
        InitializeComponent();
    }

    public VirtualSwitchesViewModel ViewModel { get; }

    /// <summary>
    /// 点击对话框遮罩层时关闭对话框
    /// </summary>
    private void DialogOverlay_MouseDown(object sender, MouseButtonEventArgs e)
    {
        // 只有在点击遮罩层本身时才关闭对话框，点击对话框内容时不关闭
        if (e.Source == sender)
        {
            ViewModel.CloseCreateDialogCommand.Execute(null);
        }
    }
}
