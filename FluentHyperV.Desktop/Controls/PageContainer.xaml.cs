using System.Windows.Controls;
using System.Windows.Markup;
using WpfExtensions.Xaml.Converters;

namespace FluentHyperV.Desktop.Controls;

/// <summary>
/// PageContainer.xaml 的交互逻辑
/// </summary>
[ContentProperty("ContainerContent")]
public partial class PageContainer : UserControl
{
    public class BoolToPaddingConverter : ValueConverterBase<bool, Thickness>
    {
        protected override Thickness ConvertNonNullValue(bool value) =>
            value ? new(42, 0, 42, 42) : new Thickness(0);
    }

    public class BoolToPaddingConverterForText : ValueConverterBase<bool, Thickness>
    {
        protected override Thickness ConvertNonNullValue(bool value) =>
            value ? new(0, 36, 0, 0) : new Thickness(42, 36, 0, 0);
    }

    public class BoolToGridRowConverter : ValueConverterBase<bool, int>
    {
        protected override int ConvertNonNullValue(bool value) => value ? 1 : 0;
    }

    public static BoolToPaddingConverter BoolToPaddingConverterInstance => new();
    public static BoolToPaddingConverterForText BoolToPaddingConverterForTextInstance => new();
    public static BoolToGridRowConverter BoolToGridRowConverterInstance => new();

    public PageContainer()
    {
        InitializeComponent();
    }

    [DependencyProperty("")]
    public partial string Header { get; set; }

    [DependencyProperty]
    public partial object ContainerContent { get; set; }

    [DependencyProperty(true)]
    public partial bool HasPadding { get; set; }

    [DependencyProperty(false)]
    public partial bool IsDynamicScrollViewerEnabled { get; set; }

    [DependencyProperty(false)]
    public partial bool IsTextFloating { get; set; }
}
