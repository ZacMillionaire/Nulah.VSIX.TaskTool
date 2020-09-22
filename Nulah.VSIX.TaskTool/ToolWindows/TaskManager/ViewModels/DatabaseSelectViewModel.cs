using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Navigation;

using Microsoft.VisualStudio.Shell;

using Nulah.VSIX.TaskTool.StandardLib;
using Nulah.VSIX.TaskTool.StandardLib.Models;
using Nulah.VSIX.TaskTool.ToolWindows.TaskManager.Models;
using Nulah.VSIX.TaskTool.ToolWindows.TaskManager.Windows;

namespace Nulah.VSIX.TaskTool.ToolWindows.TaskManager.ViewModels
{
    /// <summary>
    /// TODO: move this control into <see cref="TaskListControlViewModel"/> to avoid the weird stuff I'm doing with relay commands
    /// </summary>
    public class DatabaseSelectViewModel : ViewModelBase
    {
        public ICommand UseGlobalTaskStoreCommand { get; private set; }
        public ICommand CreateSolutionTaskStoreCommand { get; private set; }

        private List<DatabaseSource> _availableTaskList;

        public List<DatabaseSource> AvailableTaskList
        {
            get { return _availableTaskList; }
            set { _availableTaskList = value; OnPropertyChanged(nameof(AvailableTaskList)); }
        }

        private DatabaseSource _selectedTaskItem;

        public DatabaseSource SelectedTaskItem
        {
            get { return _selectedTaskItem; }
            set
            {
                if (value != null)
                {
                    _selectedTaskItem = value;
                }
                OnPropertyChanged(nameof(SelectedTaskItem));
                SelectedTaskListChange?.Invoke(this, _selectedTaskItem);
            }
        }

        private bool _globalStoreInUse = false;

        public event EventHandler<DatabaseSource> SelectedTaskListChange;
        public event EventHandler TaskListModified;

        /// <summary>
        /// Design time constructor
        /// </summary>
        public DatabaseSelectViewModel()
        {
            AvailableTaskList = new List<DatabaseSource>
            {
                new DatabaseSource{
                    DisplayName = "<all lists>",
                    DatabaseName = "ALL"
                },
                new DatabaseSource{
                    DisplayName = "Global",
                    DatabaseName = "Global"
                }
            };
            SelectedTaskItem = AvailableTaskList.First();
        }

        /// <summary>
        /// Runtime constructor
        /// </summary>
        /// <param name="availableDatabases"></param>
        /// <param name="activeDatabase"></param>
        public DatabaseSelectViewModel(List<DatabaseSource> availableDatabases, DatabaseSource activeDatabase)
        {
            AvailableTaskList = availableDatabases;
            SelectedTaskItem = activeDatabase;

            UseGlobalTaskStoreCommand = new RelayCommand(_ => SwitchToGlobalTaskList(), (x) => CanUseGlobalStore());
            CreateSolutionTaskStoreCommand = new RelayCommand(_ => CreateSolutionTaskStore(), (x) => CanCreateTask());
        }

        private bool CanCreateTask()
        {
            // TODO: disable while create task window is open maybe
            return true;
        }

        private bool CanUseGlobalStore()
        {
            return !_globalStoreInUse;
        }

        private void SwitchToGlobalTaskList()
        {
            _globalStoreInUse = true;
        }

        private void CreateSolutionTaskStore()
        {
            var tasklistSourceManager = new TaskListSourceManager();
            tasklistSourceManager.ShowDialog();
            TaskListModified?.Invoke(this, null);
        }

        internal void UpdateTaskList(List<DatabaseSource> updatedTaskList)
        {
            // Used to preserve the last selected item across AvailableTaskList updates
            var selectedTaskStoreDbName = _selectedTaskItem.DatabaseName;
            AvailableTaskList = updatedTaskList;

            if (updatedTaskList.Any(x => x.DatabaseName == selectedTaskStoreDbName) == false)
            {
                // Default to the first task in the list if our previous selected task list stopped existing
                SelectedTaskItem = AvailableTaskList.First();
            }
            else
            {
                // Select the previously selected item
                SelectedTaskItem = AvailableTaskList.First(x => x.DatabaseName == selectedTaskStoreDbName);
            }
        }
    }
}
