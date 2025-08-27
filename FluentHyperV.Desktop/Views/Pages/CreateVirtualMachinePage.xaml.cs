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
        // 找到TabControl并添加事件处理器
        var tabControl = FindTabControl(this);
        if (tabControl != null)
        {
            tabControl.PreviewMouseDown += TabControl_PreviewMouseDown;
            System.Diagnostics.Debug.WriteLine("TabControl事件处理器已绑定");
        }
    }

    private TabControl? FindTabControl(DependencyObject parent)
    {
        var childCount = System.Windows.Media.VisualTreeHelper.GetChildrenCount(parent);
        for (int i = 0; i < childCount; i++)
        {
            var child = System.Windows.Media.VisualTreeHelper.GetChild(parent, i);
            if (child is TabControl tabControl)
            {
                return tabControl;
            }
            var result = FindTabControl(child);
            if (result != null)
                return result;
        }
        return null;
    }

    private void TabControl_PreviewMouseDown(object sender, MouseButtonEventArgs e)
    {
        if (sender is TabControl tabControl)
        {
            // 找到被点击的TabItem
            var clickedTabItem = FindTabItemFromPoint(tabControl, e.GetPosition(tabControl));
            if (clickedTabItem != null)
            {
                // 获取TabItem的索引
                var tabItemIndex = tabControl.Items.IndexOf(clickedTabItem);

                System.Diagnostics.Debug.WriteLine(
                    $"点击Tab索引: {tabItemIndex}, 当前步骤={ViewModel.CurrentStep}, 最大完成步骤={ViewModel.MaxCompletedStep}"
                );

                // 检查是否允许访问这个步骤
                if (!CanAccessStep(tabItemIndex))
                {
                    System.Diagnostics.Debug.WriteLine($"阻止点击Tab {tabItemIndex}");
                    e.Handled = true; // 阻止事件继续传播
                    return;
                }

                System.Diagnostics.Debug.WriteLine($"允许点击Tab {tabItemIndex}");
                // 更新ViewModel的当前步骤
                ViewModel.CurrentStep = tabItemIndex;
            }
        }
    }

    private TabItem? FindTabItemFromPoint(TabControl tabControl, Point point)
    {
        var hitTest = VisualTreeHelper.HitTest(tabControl, point);
        if (hitTest?.VisualHit != null)
        {
            var element = hitTest.VisualHit;

            // 向上查找TabItem
            while (element != null && element != tabControl)
            {
                if (element is TabItem tabItem)
                {
                    return tabItem;
                }
                element = VisualTreeHelper.GetParent(element);
            }
        }
        return null;
    }

    private bool CanAccessStep(int targetStep)
    {
        // 只允许访问已完成的步骤或下一步
        return targetStep <= ViewModel.MaxCompletedStep;
    }
}
