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

using Nulah.VSIX.TaskTool.ToolWindows.TaskManager.Controls.Pages;

namespace Nulah.VSIX.TaskTool.ToolWindows.TaskManager.Controls
{
    /// <summary>
    /// Interaction logic for TaskListControl.xaml
    /// </summary>
    public partial class TaskListControl : UserControl
    {
        public TaskListControl()
        {
            InitializeComponent();
            Init();
        }

        public void Init()
        {
            BackToTaskListButton.Visibility = Visibility.Collapsed;
            NewTaskTitle.Visibility = Visibility.Collapsed;

            TaskPageFrame.Content = new TaskListPage();
            TaskPageFrame.ContentRendered += TaskPageFrame_ContentRendered;
        }

        private void TaskPageFrame_ContentRendered(object sender, EventArgs e)
        {
            if (TaskPageFrame.CanGoBack == true)
            {
                NewTaskButton.Visibility = Visibility.Collapsed;

                NewTaskTitle.Visibility = Visibility.Visible;
                BackToTaskListButton.Visibility = Visibility.Visible;
            }
            else
            {
                NewTaskButton.Visibility = Visibility.Visible;

                NewTaskTitle.Visibility = Visibility.Collapsed;
                BackToTaskListButton.Visibility = Visibility.Collapsed;
            }
        }

        private void NewTaskButton_Click(object sender, RoutedEventArgs e)
        {
            TaskPageFrame.Content = new NewTaskPage();
        }

        private void BackToTaskListButton_Click(object sender, RoutedEventArgs e)
        {
            TaskPageFrame.GoBack();
        }
    }
}
