using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

using Nulah.VSIX.TaskTool.StandardLib.Models;

namespace Nulah.VSIX.TaskTool.ToolWindows.TaskManager.ViewModels.Pages
{
    public class TaskListPageViewModel : ViewModelBase
    {

        private ObservableCollection<TaskListDisplayItem> _taskListDisplayItems;

        public ObservableCollection<TaskListDisplayItem> Tasks
        {
            get { return _taskListDisplayItems; }
            set { _taskListDisplayItems = value; OnPropertyChanged(nameof(Tasks)); }
        }

        public ICommand InProgressChangeCommand { get; private set; }
        public ICommand IsCompleteChangeCommand { get; private set; }

        private TaskListManager _taskListManager;

        public TaskListPageViewModel()
        {
            _taskListManager = GetDependency<TaskListManager>();

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

        private bool _taskListLoading;

        public bool TaskListLoading
        {
            get { return _taskListLoading; }
            set { _taskListLoading = value; OnPropertyChanged(nameof(TaskListLoading)); }
        }

        private bool _taskListReady;

        public bool TaskListReady
        {
            get { return _taskListReady; }
            set { _taskListReady = value; OnPropertyChanged(nameof(TaskListReady)); }
        }


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
        public async Task OnPageLoadedAsync()
        {
            // Only refresh the list if the task database has been updated in some way
            if (_taskListManager.ListChanged == true)
            {
                TaskListLoading = true;
                TaskListReady = false;
                // Run GetTaskList as a task to avoid delaying the extension from first rendering
                await Task.Run(() =>
                {
                    GetTaskList();
                    TaskListLoading = false;
                    TaskListReady = true;
                });
            }
        }

        private void GetTaskList()
        {
            var taskList = _taskListManager.GetTasks()
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
            Tasks = new ObservableCollection<TaskListDisplayItem>(SortTasks(taskList, _currentSortOrder));
        }

        private void InProgressCheckedChanged(TaskListDisplayItem taskListDisplayItem)
        {
            // TODO: sync these methods to be the same functionality as ViewTaskDetailsPage
            bool a = _taskListManager.UpdateTaskProgressState(taskListDisplayItem.Id, taskListDisplayItem.InProgress);
            // Updating task progress sets IsComplete to false, so reflect this on the model
            taskListDisplayItem.IsComplete = false;
        }

        private void IsCompletedCheckedChanged(TaskListDisplayItem taskListDisplayItem)
        {
            // TODO: sync these methods to be the same functionality as ViewTaskDetailsPage
            bool a = _taskListManager.UpdateTaskCompletedState(taskListDisplayItem.Id, taskListDisplayItem.IsComplete);
            // Updating task completion sets InProgress to false, so reflect this on the model
            taskListDisplayItem.InProgress = false;
        }

        public void SortTaskList(TaskListSort newSortOrder)
        {
            // Update the current sort order as the user may change sort mode as its being loaded
            _currentSortOrder = newSortOrder;
            if (Tasks == null || Tasks.Count == 0)
            {
                return;
            }

            Tasks = new ObservableCollection<TaskListDisplayItem>(SortTasks(_taskListDisplayItems, newSortOrder));
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

    public class TaskListDisplayItem : INotifyPropertyChanged
    {
        private bool _inProgress;
        private bool _isComplete;

        public Guid Id { get; set; }
        public string Title { get; set; }
        public string Content { get; set; }
        public bool InProgress
        {
            get => _inProgress; set
            {
                if (_inProgress != value)
                {
                    _inProgress = value;
                    OnPropertyChanged(nameof(InProgress));
                }
            }
        }
        public bool IsComplete
        {
            get => _isComplete;
            set
            {
                if (_isComplete != value)
                {
                    _isComplete = value;
                    OnPropertyChanged(nameof(IsComplete));
                }
            }
        }
        public DateTime CreatedUTC { get; set; }
        public DateTime UpdatedUTC { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
