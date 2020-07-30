using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Nulah.VSIX.TaskTool.Data;
using Nulah.VSIX.TaskTool.StandardLib.Models;

namespace Nulah.VSIX.TaskTool.ToolWindows.TaskManager.ViewModels
{
    internal class TaskManagerMainViewModel : ViewModelBase
    {
        public TaskManagerMainViewModel()
        {
            base.RegisterDependency<TaskListManager>();
        }
    }
}
