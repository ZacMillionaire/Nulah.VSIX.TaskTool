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

        public readonly Dictionary<string, TaskListSort> SortOptions = new Dictionary<string, TaskListSort>
        {
            { "Created Ascending",TaskListSort.CreatedAsc },
            { "Created Descending",TaskListSort.CreatedDesc },
            { "Updated Ascending",TaskListSort.UpdatedAsc },
            { "Updated Descending",TaskListSort.UpdatedDesc }
        };

        private TaskListSort _currentSortOrder;

        /// <summary>
        /// Called from a view model to set the default sort order on first load
        /// </summary>
        /// <param name="selectedSortOption"></param>
        internal void SetInitialSortOrder(TaskListSort selectedSortOption)
        {
            _currentSortOrder = selectedSortOption;
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
            var taskList = GetDependency<TaskListManager>().GetTasks()
                .Select(x => new TaskListDisplayItem
                {
                    Id = x.Id,
                    Content = x.Content,
                    Title = x.Title,
                    InProgress = x.InProgress,
                    IsComplete = x.IsComplete,
                    CreatedUTC = x.CreatedUTC,
                    UpdatedUTC = x.UpdatedUTC
                });
            Tasks = SortTasks(taskList, _currentSortOrder).ToList();
        }

        private void InProgressCheckedChanged(TaskListDisplayItem taskListDisplayItem)
        {
            bool a = GetDependency<TaskListManager>().UpdateTaskProgressState(taskListDisplayItem.Id, taskListDisplayItem.InProgress);
        }

        private void IsCompletedCheckedChanged(TaskListDisplayItem taskListDisplayItem)
        {
            bool a = GetDependency<TaskListManager>().UpdateTaskCompletedState(taskListDisplayItem.Id, taskListDisplayItem.IsComplete);
        }

        public void SortTaskList(TaskListSort newSortOrder)
        {
            if (Tasks == null || Tasks.Count == 0)
            {
                return;
            }

            Tasks = SortTasks(_taskListDisplayItems, newSortOrder).ToList();
            _currentSortOrder = newSortOrder;
        }

        private IEnumerable<TaskListDisplayItem> SortTasks(IEnumerable<TaskListDisplayItem> taskList, TaskListSort sortMode)
        {
            switch (sortMode)
            {
                default:
                case TaskListSort.CreatedAsc:
                    return taskList.OrderBy(x => x.CreatedUTC);
                case TaskListSort.CreatedDesc:
                    return taskList.OrderByDescending(x => x.CreatedUTC);
                case TaskListSort.UpdatedAsc:
                    return taskList.OrderBy(x => x.UpdatedUTC);
                case TaskListSort.UpdatedDesc:
                    return taskList.OrderByDescending(x => x.UpdatedUTC);
            }
        }
    }

    public enum TaskListSort
    {
        CreatedAsc,
        CreatedDesc,
        UpdatedAsc,
        UpdatedDesc
    }

    public class TaskListDisplayItem
    {
        public Guid Id { get; set; }
        public string Title { get; set; }
        public string Content { get; set; }
        public bool IsComplete { get; set; }
        public bool InProgress { get; set; }
        public DateTime CreatedUTC { get; set; }
        public DateTime UpdatedUTC { get; set; }
    }
}
