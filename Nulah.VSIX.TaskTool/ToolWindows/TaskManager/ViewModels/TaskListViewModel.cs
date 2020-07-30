using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

using Nulah.VSIX.TaskTool.StandardLib.Models;

namespace Nulah.VSIX.TaskTool.ToolWindows.TaskManager.ViewModels
{
    public class TaskListViewModel : ViewModelBase
    {
        public ICommand NewTaskCommand { get; private set; }


        public TaskListViewModel()
        {
        }
    }
}
