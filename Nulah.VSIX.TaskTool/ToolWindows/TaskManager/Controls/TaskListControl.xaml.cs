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
using Nulah.VSIX.TaskTool.ToolWindows.TaskManager.ViewModels;

namespace Nulah.VSIX.TaskTool.ToolWindows.TaskManager.Controls
{
    /// <summary>
    /// Interaction logic for TaskListControl.xaml
    /// </summary>
    public partial class TaskListControl : UserControl
    {
        // Called on first load of the task list control, either from visual studio starting up (if the tool was previously pinned open)
        // or when first opened/expanded from a dock
        public TaskListControl()
        {
            InitializeComponent();
            Init();
        }

        public void Init()
        {
            BackToTaskListButton.IsEnabled = false;
            ((TaskListControlViewModel)this.DataContext).RegisterUserControl(this);
            // Ensure that the sort order options combobox is correctly updated to match its contents
            SortOrder.UpdateLayout();

            TaskPageFrame.ContentRendered += TaskPageFrame_ContentRendered;
        }

        private void TaskPageFrame_ContentRendered(object sender, EventArgs e)
        {
            // If we're viewing a task/creating a task, being able to change the task source, source ordering
            // and the ability to create a new task should be disabled
            if (TaskPageFrame.CanGoBack == true)
            {
                NewTaskButton.IsEnabled = false;
                TaskListSection.IsEnabled = false;
                TaskSortSection.IsEnabled = false;

                BackToTaskListButton.IsEnabled = true;
            }
            else
            {
                NewTaskButton.IsEnabled = true;
                TaskListSection.IsEnabled = true;
                TaskSortSection.IsEnabled = true;

                BackToTaskListButton.IsEnabled = false;
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

        internal void LoadSelectedTask(Guid taskGuid)
        {
            TaskPageFrame.Content = new ViewTaskDetailsPage(taskGuid);
        }
    }
}
