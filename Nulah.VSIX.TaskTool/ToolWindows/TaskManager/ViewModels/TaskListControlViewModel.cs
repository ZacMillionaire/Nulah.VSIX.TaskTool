using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

using VSShell = Microsoft.VisualStudio.Shell;

using Nulah.VSIX.TaskTool.StandardLib;

using Nulah.VSIX.TaskTool.StandardLib.Models;
using Nulah.VSIX.TaskTool.ToolWindows.TaskManager.Controls;
using Nulah.VSIX.TaskTool.ToolWindows.TaskManager.Controls.Pages;
using Nulah.VSIX.TaskTool.ToolWindows.TaskManager.Models;
using Nulah.VSIX.TaskTool.ToolWindows.TaskManager.ViewModels.Pages;
using EnvDTE;

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

        private DatabaseSelectViewModel _taskSourceViewModel;

        public DatabaseSelectViewModel TaskSourceViewModel
        {
            get { return _taskSourceViewModel; }
            set { _taskSourceViewModel = value; }
        }

        private readonly TaskListManager _taskListManager;


        public TaskListPage TaskListPageContent { get; private set; }

        private bool _viewModelReady = false;

        public TaskListControlViewModel()
        {
            _taskListManager = GetDependency<TaskListManager>();

            TaskListPageContent = new TaskListPage();
            TaskListPageContent.TaskSelected += TaskSelectedInList;
            SortOptions = _vmDataContext.SortOptions;

            // Look at localising these default values and maybe swap the dictionary around to be <TaskListSort, string>?
            SelectedSortOption = "Created Descending"; // TODO: should come from a configuration/last used maybe? Should persist either way
            _vmDataContext.SetInitialSortOrder(SortOptions["Created Descending"]); // TODO: should come from configuration/last used mode maybe? Should persist either way


            TaskSourceViewModel = new DatabaseSelectViewModel(_taskListManager.AvailableTaskLists, _taskListManager.AvailableTaskLists.First());
            TaskSourceViewModel.SelectedTaskListChange += TaskSourceViewModel_SelectedTaskListChange;
            TaskSourceViewModel.TaskListModified += TaskSourceViewModel_TaskListModified;

            BindSolutionChangeEvent();

            // Mark that the viewmodel has been properly intialised
            _viewModelReady = true;
        }

        private SolutionEvents _solutionEvents;

        private void BindSolutionChangeEvent()
        {
            // Any access to the VS Shell or internals should only be done on the main UI thread
            VSShell.ThreadHelper.ThrowIfNotOnUIThread();
            var VSI = new VisualStudioInternals();
            var dte = VSI.GetActiveIDE();
            _solutionEvents = dte.Events.SolutionEvents;

            _solutionEvents.Opened += SolutionEvents_Opened;
            _solutionEvents.AfterClosing += _solutionEvents_AfterClosing;

            /*
            _solutionEvents.ProjectAdded += _solutionEvents_ProjectAdded;
            _solutionEvents.ProjectRemoved += _solutionEvents_ProjectRemoved;
            // rename should open a dialog prompt to rename a task file if it exists
            _solutionEvents.ProjectRenamed += _solutionEvents_ProjectRenamed;
            */
        }

        private void SolutionEvents_Opened()
        {
            TaskSourceViewModel.UpdateTaskList(_taskListManager.GetDatabaseList());
        }

        private void _solutionEvents_AfterClosing()
        {
            TaskSourceViewModel.UpdateTaskList(_taskListManager.GetDatabaseList());
        }

        private void _solutionEvents_ProjectAdded(Project Project)
        {
            throw new NotImplementedException();
        }

        private void _solutionEvents_ProjectRemoved(Project Project)
        {
            throw new NotImplementedException();
        }
        private void _solutionEvents_ProjectRenamed(Project Project, string OldName)
        {
            throw new NotImplementedException();
        }

        private void TaskSourceViewModel_TaskListModified(object sender, EventArgs e)
        {
            // Update task list on the database control
            // This is a bit dumb but a future refactor will pull that control into this VM
            TaskSourceViewModel.UpdateTaskList(_taskListManager.GetDatabaseList());
        }

        private void TaskSourceViewModel_SelectedTaskListChange(object sender, DatabaseSource targetTaskSource)
        {
            // Don't act on task list changes until the viewmodel is ready
            if (_viewModelReady == false)
            {
                return;
            }
            _taskListManager.SwitchDatabase(targetTaskSource);
            _vmDataContext.UpdateTaskList();
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
