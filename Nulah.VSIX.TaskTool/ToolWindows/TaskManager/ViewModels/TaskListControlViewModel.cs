using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

using Nulah.VSIX.TaskTool.StandardLib.Models;
using Nulah.VSIX.TaskTool.ToolWindows.TaskManager.Controls;
using Nulah.VSIX.TaskTool.ToolWindows.TaskManager.Controls.Pages;
using Nulah.VSIX.TaskTool.ToolWindows.TaskManager.ViewModels.Pages;

namespace Nulah.VSIX.TaskTool.ToolWindows.TaskManager.ViewModels
{
    public class TaskListControlViewModel : ViewModelBase
    {
        public ICommand NewTaskCommand { get; private set; }

        private Dictionary<string, TaskListSort> _sortOptions;

        public Dictionary<string, TaskListSort> SortOptions
        {
            get { return _sortOptions; }
            set { _sortOptions = value; OnPropertyChanged(nameof(SortOptions)); }
        }

        private string _selectedSortOption;

        public string SelectedSortOption
        {
            get { return _selectedSortOption; }
            set
            {
                _selectedSortOption = value;
                OnPropertyChanged(nameof(SelectedSortOption));
                SelectedSortChanged(_sortOptions[value]);
            }
        }

        private TaskListControl _taskListUserControl;

        internal void RegisterUserControl(TaskListControl taskListUserControl)
        {
            _taskListUserControl = taskListUserControl;
        }

        private TaskListPageViewModel _vmDataContext => ((TaskListPageViewModel)TaskListPageContent.DataContext);

        public TaskListPage TaskListPageContent { get; private set; }

        private bool _viewModelReady = false;

        public TaskListControlViewModel()
        {
            TaskListPageContent = new TaskListPage();
            TaskListPageContent.TaskSelected += TaskSelectedInList;
            SortOptions = _vmDataContext.SortOptions;
            // Look at localising these default values and maybe swap the dictionary around to be <TaskListSort, string>?
            SelectedSortOption = "Created Descending"; // TODO: should come from a configuration/last used maybe? Should persist either way
            _vmDataContext.SetInitialSortOrder(SortOptions["Created Descending"]); // TODO: should come from configuration/last used mode maybe? Should persist either way
            // Mark that the viewmodel has been properly intialised
            _viewModelReady = true;
        }

        private void SelectedSortChanged(TaskListSort newSortOrder)
        {
            // Don't sort anything until the view model is loaded.
            // Yes, this gets called before the constructor because of the magic of WPF
            if (_viewModelReady == false)
            {
                return;
            }
            _vmDataContext.SortTaskList(newSortOrder);
        }

        private void TaskSelectedInList(object sender, Guid taskGuid)
        {
            _taskListUserControl.LoadSelectedTask(taskGuid);
        }
    }
}
