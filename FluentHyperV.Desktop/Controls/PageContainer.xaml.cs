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
    public static readonly DependencyProperty HeaderProperty = DependencyProperty.Register(
        "Header",
        typeof(string),
        typeof(PageContainer),
        new(string.Empty)
    );

    public static readonly DependencyProperty ContainerContentProperty =
        DependencyProperty.Register(
            "ContainerContent",
            typeof(object),
            typeof(PageContainer),
            new(null)
        );

    public static readonly DependencyProperty HasPaddingProperty = DependencyProperty.Register(
        "HasPadding",
        typeof(bool),
        typeof(PageContainer),
        new(true)
    );

    public static readonly DependencyProperty IsDynamicScrollViewerEnabledProperty =
        DependencyProperty.Register(
            "IsDynamicScrollViewerEnabled",
            typeof(bool),
            typeof(PageContainer),
            new(false)
        );

    public static readonly DependencyProperty IsTextFloatingProperty = DependencyProperty.Register(
        "IsTextFloating",
        typeof(bool),
        typeof(PageContainer),
        new(false)
    );

    public PageContainer()
    {
        InitializeComponent();
    }

    //public partial string Header1 { get; }

    public string Header
    {
        get => (string)GetValue(HeaderProperty);
        set => SetValue(HeaderProperty, value);
    }

    public object ContainerContent
    {
        get => GetValue(ContainerContentProperty);
        set => SetValue(ContainerContentProperty, value);
    }

    public bool HasPadding
    {
        get => (bool)GetValue(HasPaddingProperty);
        set => SetValue(HasPaddingProperty, value);
    }

    public bool IsDynamicScrollViewerEnabled
    {
        get => (bool)GetValue(IsDynamicScrollViewerEnabledProperty);
        set => SetValue(IsDynamicScrollViewerEnabledProperty, value);
    }

    public bool IsTextFloating
    {
        get => (bool)GetValue(IsTextFloatingProperty);
        set => SetValue(IsTextFloatingProperty, value);
    }
}
