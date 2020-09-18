using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Navigation;

using Nulah.VSIX.TaskTool.StandardLib.Models;
using Nulah.VSIX.TaskTool.ToolWindows.TaskManager.Models;

namespace Nulah.VSIX.TaskTool.ToolWindows.TaskManager.ViewModels
{
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
                _selectedTaskItem = value;
                OnPropertyChanged(nameof(SelectedTaskItem));
                SelectedTaskListChange?.Invoke(this, _selectedTaskItem);
            }
        }

        private bool _globalStoreInUse = false;

        public event EventHandler<DatabaseSource> SelectedTaskListChange;

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

        public DatabaseSelectViewModel(List<DatabaseSource> availableDatabases, DatabaseSource activeDatabase)
        {
            AvailableTaskList = availableDatabases;
            SelectedTaskItem = activeDatabase;

            UseGlobalTaskStoreCommand = new RelayCommand(_ => SwitchToGlobalTaskList(), (x) => CanUseGlobalStore());
            CreateSolutionTaskStoreCommand = new RelayCommand(_ => CreateSolutionTaskStore(), (x) => false);
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

        }
    }
}
