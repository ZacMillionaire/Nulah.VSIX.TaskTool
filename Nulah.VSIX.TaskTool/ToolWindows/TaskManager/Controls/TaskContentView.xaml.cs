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
            var t = d as TaskContentView;
            var a = t.TaskContent;
            if (string.IsNullOrWhiteSpace(a) == false)
            {
                var parsedContent = new MarkdownToBlock().Parse(a);
                t.PopulateContent(parsedContent);
            }
        }

        private void PopulateContent(List<ContentBlock> content)
        {
            if (content.Count == 0)
            {
                return;
            }
            var grid = new Grid();
            var rowCount = 0;
            foreach (var contentBlock in content)
            {
                var textbox = new TextBlock();
                textbox.Text = contentBlock.Content;
                grid.RowDefinitions.Add(new RowDefinition
                {
                    Height = GridLength.Auto,
                });

                Grid.SetRow(textbox, rowCount);
                grid.Children.Add(textbox);
                rowCount++;
            }
            this.Content = grid;
        }

        public TaskContentView()
        {
            InitializeComponent();
        }
    }
}
