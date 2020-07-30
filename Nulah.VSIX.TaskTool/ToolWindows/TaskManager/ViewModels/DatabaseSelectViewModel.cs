using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Navigation;

using Nulah.VSIX.TaskTool.StandardLib.Models;

namespace Nulah.VSIX.TaskTool.ToolWindows.TaskManager.ViewModels
{
    internal class DatabaseSelectViewModel : ViewModelBase
    {
        public ICommand UseGlobalTaskStoreCommand { get; private set; }
        public ICommand CreateSolutionTaskStoreCommand { get; private set; }

        private bool _globalStoreInUse = false;

        public DatabaseSelectViewModel()
        {
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
