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
            // 获取TabControl
            var tabControl = FindParentTabControl(tabItem);
            if (tabControl != null)
            {
                // 获取被点击的Tab的索引
                int clickedIndex = tabControl.Items.IndexOf(tabItem);
                // 获取当前选中的Tab索引
                int currentIndex = tabControl.SelectedIndex;

                // 拦截逻辑：只允许点击当前步骤及之前的步骤（基于MaxCompletedStep）
                if (clickedIndex > ViewModel.MaxCompletedStep)
                {
                    e.Handled = true;
                    System.Diagnostics.Debug.WriteLine(
                        $"拦截了Tab点击: 点击索引 {clickedIndex}, 当前索引 {currentIndex}, MaxCompletedStep: {ViewModel.MaxCompletedStep}"
                    );
                    return;
                }

                // 如果允许访问，更新ViewModel的CurrentStep并重新计算导航状态
                // 使用Dispatcher确保在UI线程中执行
                Dispatcher.BeginInvoke(
                    () =>
                    {
                        ViewModel.CurrentStep = clickedIndex;
                        System.Diagnostics.Debug.WriteLine(
                            $"允许Tab点击，更新CurrentStep到: {clickedIndex}"
                        );
                    },
                    DispatcherPriority.Input
                );
            }
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

    private TabControl? FindParentTabControl(DependencyObject? element)
    {
        while (element != null)
        {
            if (element is TabControl tabControl)
            {
                return tabControl;
            }
            element = VisualTreeHelper.GetParent(element);
        }
        return null;
    }
}
