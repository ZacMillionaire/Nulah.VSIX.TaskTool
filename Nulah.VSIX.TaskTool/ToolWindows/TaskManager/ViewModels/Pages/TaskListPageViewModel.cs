using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

using Nulah.VSIX.TaskTool.StandardLib.Models;

namespace Nulah.VSIX.TaskTool.ToolWindows.TaskManager.ViewModels.Pages
{
    public class TaskListPageViewModel : ViewModelBase
    {

        private List<TaskListDisplayItem> _taskListDisplayItems;

        public List<TaskListDisplayItem> Tasks
        {
            get { return _taskListDisplayItems; }
            set { _taskListDisplayItems = value; OnPropertyChanged(nameof(Tasks)); }
        }

        public ICommand InProgressChangeCommand { get; private set; }
        public ICommand IsCompleteChangeCommand { get; private set; }

        public TaskListPageViewModel()
        {
            InProgressChangeCommand = new RelayCommand(taskListDisplayItem => InProgressCheckedChanged(taskListDisplayItem as TaskListDisplayItem), x => true);
            IsCompleteChangeCommand = new RelayCommand(taskListDisplayItem => IsCompletedCheckedChanged(taskListDisplayItem as TaskListDisplayItem), x => true);
        }

        /// <summary>
        /// Called whenever the page this viewmodel is attached to is loaded, eg: frame navigation events or being set as content
        /// </summary>
        public void OnPageLoaded()
        {
            GetTaskList();
        }

        private void GetTaskList()
        {
            Tasks = GetDependency<TaskListManager>().GetTasks()
                .Select(x => new TaskListDisplayItem
                {
                    Id = x.Id,
                    Content = x.Content,
                    Title = x.Title,
                    InProgress = x.InProgress,
                    IsComplete = x.IsComplete
                })
                .ToList();
        }

        private void InProgressCheckedChanged(TaskListDisplayItem taskListDisplayItem)
        {
            bool a = GetDependency<TaskListManager>().UpdateTaskProgressState(taskListDisplayItem.Id, taskListDisplayItem.InProgress);
        }

        private void IsCompletedCheckedChanged(TaskListDisplayItem taskListDisplayItem)
        {
            bool a = GetDependency<TaskListManager>().UpdateTaskCompletedState(taskListDisplayItem.Id, taskListDisplayItem.IsComplete);
        }
    }

    public class TaskListDisplayItem
    {
        public Guid Id { get; set; }
        public string Title { get; set; }
        public string Content { get; set; }
        public bool IsComplete { get; set; }
        public bool InProgress { get; set; }
    }
}
