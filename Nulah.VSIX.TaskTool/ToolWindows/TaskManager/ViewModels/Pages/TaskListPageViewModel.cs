using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

        public TaskListPageViewModel()
        {
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
