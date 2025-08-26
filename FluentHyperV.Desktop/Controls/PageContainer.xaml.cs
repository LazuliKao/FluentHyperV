using System.Windows;
using System.Windows.Controls;

namespace FluentHyperV.Desktop.Controls
{
    /// <summary>
    /// PageContainer.xaml 的交互逻辑
    /// </summary>
    public partial class PageContainer : UserControl
    {
        public static readonly DependencyProperty HeaderProperty =
            DependencyProperty.Register("Header", typeof(string), typeof(PageContainer), new PropertyMetadata(string.Empty));

        public static readonly DependencyProperty ContainerContentProperty =
            DependencyProperty.Register("ContainerContent", typeof(object), typeof(PageContainer), new PropertyMetadata(null));

        public static readonly DependencyProperty HasPaddingProperty =
            DependencyProperty.Register("HasPadding", typeof(bool), typeof(PageContainer), new PropertyMetadata(true));

        public static readonly DependencyProperty IsDynamicScrollViewerEnabledProperty =
            DependencyProperty.Register("IsDynamicScrollViewerEnabled", typeof(bool), typeof(PageContainer), new PropertyMetadata(false));

        public static readonly DependencyProperty IsTextFloatingProperty =
            DependencyProperty.Register("IsTextFloating", typeof(bool), typeof(PageContainer), new PropertyMetadata(false));

        public PageContainer()
        {
            InitializeComponent();
        }

        public string Header
        {
            get { return (string)GetValue(HeaderProperty); }
            set { SetValue(HeaderProperty, value); }
        }

        public object ContainerContent
        {
            get { return GetValue(ContainerContentProperty); }
            set { SetValue(ContainerContentProperty, value); }
        }

        public bool HasPadding
        {
            get { return (bool)GetValue(HasPaddingProperty); }
            set { SetValue(HasPaddingProperty, value); }
        }

        public bool IsDynamicScrollViewerEnabled
        {
            get { return (bool)GetValue(IsDynamicScrollViewerEnabledProperty); }
            set { SetValue(IsDynamicScrollViewerEnabledProperty, value); }
        }

        public bool IsTextFloating
        {
            get { return (bool)GetValue(IsTextFloatingProperty); }
            set { SetValue(IsTextFloatingProperty, value); }
        }
    }
}
