using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Nulah.VSIX.TaskTool.StandardLib.Models;

namespace Nulah.VSIX.TaskTool.ToolWindows.TaskManager.ViewModels.Pages
{
    public class NewTaskPageViewModel : ViewModelBase
    {
        private Guid _VMGuid;

        public Guid VMGuid
        {
            get { return _VMGuid; }
            set { _VMGuid = value; base.OnPropertyChanged(nameof(VMGuid)); }
        }

        public NewTaskPageViewModel()
        {
            VMGuid = Guid.NewGuid();
        }
    }
}
