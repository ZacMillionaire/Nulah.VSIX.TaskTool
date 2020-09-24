using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

using Nulah.VSIX.TaskTool.StandardLib;
using Nulah.VSIX.TaskTool.StandardLib.Markdown.Models.Blocks;

namespace Nulah.VSIX.TaskTool.ToolWindows.TaskManager.Controls
{
    /// <summary>
    /// Interaction logic for TaskContentView.xaml
    /// </summary>
    public partial class TaskContentView : UserControl
    {
        public string TaskContent
        {
            get
            {
                return (string)GetValue(TaskContentProperty);
            }
            set
            {
                SetValue(TaskContentProperty, value);
            }
        }

        public static readonly DependencyProperty TaskContentProperty =
            DependencyProperty.Register("TaskContent", typeof(string), typeof(TaskContentView), new PropertyMetadata(string.Empty, TaskContentSet));

        private static void TaskContentSet(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var taskContentView = d as TaskContentView;
            if (string.IsNullOrWhiteSpace(taskContentView.TaskContent) == false)
            {
                var parsedContent = new MarkdownToBlock().Parse(taskContentView.TaskContent);
                taskContentView.PopulateContent(parsedContent);
            }
        }

        public Thickness ContentBlockPadding
        {
            get { return (Thickness)GetValue(ContentBlockPaddingProperty); }
            set { SetValue(ContentBlockPaddingProperty, value); }
        }

        public static readonly DependencyProperty ContentBlockPaddingProperty =
            DependencyProperty.Register("TextBlockPadding", typeof(Thickness), typeof(TaskContentView), new FrameworkPropertyMetadata(new Thickness(0, 0, 0, 5), ContentBlockPaddingChanged));

        private static void ContentBlockPaddingChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var taskContentView = d as TaskContentView;
            if (e.NewValue is Thickness newPadding)
            {
                taskContentView.ContentBlockPadding = newPadding;
                taskContentView.ReflowContent();
            }
        }

        public double ContentFontSize
        {
            get { return (double)GetValue(ContentFontSizeProperty); }
            set { SetValue(ContentFontSizeProperty, value); }
        }

        public static readonly DependencyProperty ContentFontSizeProperty =
            DependencyProperty.Register("FontSize", typeof(double), typeof(TaskContentView), new FrameworkPropertyMetadata(12d, ContentFontSizeChanged));

        private static void ContentFontSizeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var taskContentView = d as TaskContentView;
            if (e.NewValue is double newFontSize)
            {
                taskContentView.ContentFontSize = newFontSize;
                taskContentView.ReflowContent();
            }
        }

        private void PopulateContent(List<ContentBlock> content)
        {
            if (content.Count == 0)
            {
                return;
            }

            foreach (var contentBlock in content)
            {
                var textbox = new TextBlock()
                {
                    TextWrapping = TextWrapping.Wrap,
                    Padding = ContentBlockPadding,
                    FontSize = ContentFontSize
                };
                textbox.Text = contentBlock.Content;
                ContentPanel.Children.Add(textbox);
            }
        }

        /// <summary>
        /// Updates the content font sizes and paddings on dependency property changes
        /// </summary>
        private void ReflowContent()
        {
            foreach (var uiElementChild in ContentPanel.Children)
            {
                if (uiElementChild is TextBlock textblockContent)
                {
                    textblockContent.FontSize = ContentFontSize;
                    textblockContent.Padding = ContentBlockPadding;
                }
            }
        }

        public TaskContentView()
        {
            InitializeComponent();

            if (WPFHelpers.IsDesignTime)
            {
                var loremList = new List<string>
                {
                    "Lorem ipsum dolor sit amet, consectetur adipiscing elit. Duis hendrerit dapibus fringilla. Donec non finibus velit. Nullam id sagittis augue. Sed suscipit risus est, ac volutpat dui porta vitae. Donec malesuada nunc a lacinia semper. Pellentesque fermentum, purus eget molestie aliquam, urna ligula molestie sapien, imperdiet molestie neque eros sagittis arcu. Nulla consectetur ex eget justo finibus, ut gravida nisl aliquet. In bibendum, felis sed posuere ullamcorper, dolor nunc faucibus erat, at sollicitudin quam arcu nec nisi. Sed fringilla malesuada nulla ut ultrices. Morbi ac tempor nunc, sed ullamcorper eros. Aliquam ut viverra erat. Vestibulum elementum iaculis lacinia. Curabitur vulputate ex ac bibendum dapibus. Donec laoreet neque quis arcu luctus tempor. Duis imperdiet, ligula viverra fermentum sollicitudin, diam metus blandit lorem, mattis consequat velit neque a tortor.",
                    "",
                    "Duis ut lobortis justo. Nulla volutpat ligula lacus, rhoncus consectetur sapien consectetur nec. Proin euismod augue eu arcu tincidunt rhoncus. Ut in libero sed erat blandit tincidunt sed fringilla erat. Ut rhoncus mattis orci, quis luctus nunc lacinia molestie. Donec lacinia pellentesque blandit. Nulla egestas arcu sed justo aliquam vulputate. Sed venenatis ligula magna, ac aliquam lectus volutpat in. Cras varius tincidunt imperdiet. Donec lacus dui, tristique eu pellentesque non, viverra eget diam. Curabitur erat tortor, vehicula non libero ac, finibus ullamcorper risus. Nam ullamcorper mauris sed porta convallis. Mauris luctus, nulla vitae egestas dapibus, eros turpis tincidunt ex, id pretium est turpis nec nulla.",
                    "",
                    "Integer feugiat augue ex, a tincidunt lacus sodales eu. Phasellus eu aliquam augue. In nibh lacus, tempor non tempor vestibulum, sagittis eu ipsum. Cras lacinia sed nisl cursus porttitor. Fusce non urna eu quam commodo venenatis vitae quis lectus. Nam massa nisl, tincidunt ac gravida vel, aliquam in velit. Vivamus tristique felis interdum tellus ullamcorper luctus. Proin id ligula efficitur, tempor ante quis, consequat quam. Sed consectetur tempus erat. Vestibulum ante ipsum primis in faucibus orci luctus et ultrices posuere cubilia curae; Proin iaculis sagittis dictum. Fusce dictum, lacus non tempus semper, magna elit commodo erat, eleifend laoreet ligula est eget massa.Duis vel fringilla nulla, quis efficitur nisi. Phasellus rutrum justo non sem auctor tincidunt.Ut dapibus posuere facilisis. Praesent ac tempor tortor.",
                    "",
                    "Suspendisse urna tellus, efficitur vestibulum vulputate in, ornare sed arcu.Etiam eget varius enim, eu consequat nisi. Aliquam molestie vel magna a tincidunt. Integer ante quam, aliquam at viverra eu, accumsan at dui. Cras dictum tortor nec tortor cursus semper.Integer lobortis rutrum lacinia. Vestibulum iaculis pharetra nunc, ut consequat ante. Mauris et lorem semper augue aliquet convallis.Aliquam blandit libero et mauris accumsan, quis faucibus elit pharetra.Sed lacinia sem sed lectus varius pellentesque.Nulla pharetra pellentesque diam, et lobortis mauris pharetra a. Duis sem ligula, facilisis nec dapibus in, ullamcorper posuere lorem.Praesent nec luctus neque, vel bibendum nunc. Nunc vestibulum rutrum mauris sit amet venenatis.Morbi non porttitor erat, in pellentesque arcu.",
                    "",
                    "Praesent quis lectus ac neque malesuada facilisis vel ut sem. Aenean nec tortor ut metus sodales varius at et turpis. Vivamus ac vestibulum justo, pulvinar ultrices metus. Aliquam fermentum augue quis lorem scelerisque lobortis.In feugiat augue lectus, ac lobortis libero tincidunt vel. Donec vulputate, urna ac tempus aliquam, lorem arcu porta arcu, vitae eleifend quam ex a massa.Pellentesque id varius justo. Duis sagittis dictum sagittis.",
                    ""
                };
                foreach (var loremText in loremList)
                {
                    var textbox = new TextBlock()
                    {
                        TextWrapping = TextWrapping.Wrap,
                        Padding = ContentBlockPadding,
                        FontSize = ContentFontSize
                    };
                    textbox.Text = loremText;
                    ContentPanel.Children.Add(textbox);
                }
            }
        }
    }
}
