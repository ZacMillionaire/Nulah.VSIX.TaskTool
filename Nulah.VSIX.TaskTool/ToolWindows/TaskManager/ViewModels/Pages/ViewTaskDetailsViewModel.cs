using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

using Microsoft.VisualStudio.Threading;

using Nulah.VSIX.TaskTool.StandardLib.Models;

namespace Nulah.VSIX.TaskTool.ToolWindows.TaskManager.ViewModels.Pages
{
    public class ViewTaskDetailsViewModel : ViewModelBase
    {
        private Guid _taskGuid;

        public Guid TaskGuid
        {
            get { return _taskGuid; }
            set { _taskGuid = value; OnPropertyChanged(nameof(TaskGuid)); }
        }

        private string _title;

        public string Title
        {
            get { return _title; }
            set { _title = value; OnPropertyChanged(nameof(Title)); }
        }

        private bool _inProgress;

        public bool InProgress
        {
            get { return _inProgress; }
            set { _inProgress = value; OnPropertyChanged(nameof(InProgress)); }
        }

        private bool _isComplete;

        public bool IsComplete
        {
            get { return _isComplete; }
            set { _isComplete = value; OnPropertyChanged(nameof(IsComplete)); }
        }

        private string _content;

        public string Content
        {
            get { return _content; }
            set { _content = value; OnPropertyChanged(nameof(Content)); }
        }

        private bool _taskLoaded;

        public bool TaskViewReady
        {
            get { return _taskLoaded; }
            set { _taskLoaded = value; OnPropertyChanged(nameof(TaskViewReady)); }
        }


        public ICommand InProgressChangeCommand { get; private set; }
        public ICommand IsCompleteChangeCommand { get; private set; }


        public ViewTaskDetailsViewModel()
        {
            InProgressChangeCommand = new RelayCommand(taskListDisplayItem => InProgressCheckedChanged(taskListDisplayItem as TaskListDisplayItem), x => true);
            IsCompleteChangeCommand = new RelayCommand(taskListDisplayItem => IsCompletedCheckedChanged(taskListDisplayItem as TaskListDisplayItem), x => true);
        }

        internal async Task LoadTaskAsync(Guid taskGuid)
        {
            TaskViewReady = false;
            Title = "Loading task";
            await Task.Run(() =>
             {
                 var selectedTask = GetDependency<TaskListManager>().GetTask(taskGuid);
                 if (selectedTask != null)
                 {
                     TaskGuid = taskGuid;
                     Title = selectedTask.Title;
                     InProgress = selectedTask.InProgress;
                     IsComplete = selectedTask.IsComplete;
                     Content = selectedTask.Content;
                 }
                 else
                 {
                     Title = "No task found";
                 }
                 TaskViewReady = true;
             });
        }

        private void InProgressCheckedChanged(TaskListDisplayItem taskListDisplayItem)
        {
            bool progressUpdate = GetDependency<TaskListManager>().UpdateTaskProgressState(TaskGuid, InProgress);
            if (progressUpdate == true)
            {
                // Updating task progress sets IsComplete to false, so reflect this on the model
                IsComplete = false;
            }
            else
            {
                // TODO: test to make sure this correctly works and returns InProgress back to its previous state by inverting the new state
                // (just bodge the method to return false 50% of the time or something
                InProgress = !InProgress;
            }
        }

        private void IsCompletedCheckedChanged(TaskListDisplayItem taskListDisplayItem)
        {
            bool completionUpdate = GetDependency<TaskListManager>().UpdateTaskCompletedState(TaskGuid, IsComplete);
            if (completionUpdate == true)
            {
                // Updating task completion sets InProgress to false, so reflect this on the model
                InProgress = false;
            }
            else
            {
                // TODO: test to make sure this correctly works and returns InProgress back to its previous state by inverting the new state
                // (just bodge the method to return false 50% of the time or something
                IsComplete = !IsComplete;
            }
        }
    }
}
