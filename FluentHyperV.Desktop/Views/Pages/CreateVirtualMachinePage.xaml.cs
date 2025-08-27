using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using FluentHyperV.Desktop.ViewModels.Pages;
using Wpf.Ui.Abstractions.Controls;

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

        // 添加Tab选择变化事件处理
        Loaded += CreateVirtualMachinePage_Loaded;
    }

    public CreateVirtualMachineViewModel ViewModel { get; }

    private void CreateVirtualMachinePage_Loaded(object sender, RoutedEventArgs e)
    {
        // 为整个页面添加鼠标点击事件处理器，拦截所有Tab点击
        this.AddHandler(
            UIElement.PreviewMouseDownEvent,
            new MouseButtonEventHandler(Page_PreviewMouseDown),
            true
        );
        System.Diagnostics.Debug.WriteLine("页面Tab点击限制已启用");
    }

    private void Page_PreviewMouseDown(object sender, MouseButtonEventArgs e)
    {
        // 检查点击的元素是否是TabItem或其子元素
        var clickedElement = e.OriginalSource as DependencyObject;
        var tabItem = FindParentTabItem(clickedElement);

        if (tabItem != null)
        {
            e.Handled = true;
            return;
        }
    }

    private TabItem? FindParentTabItem(DependencyObject? element)
    {
        while (element != null)
        {
            if (element is TabItem tabItem)
            {
                return tabItem;
            }
            element = VisualTreeHelper.GetParent(element);
        }
        return null;
    }
}
