using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Nulah.VSIX.TaskTool.StandardLib.Models;

namespace Nulah.VSIX.TaskTool.ToolWindows.TaskManager.ViewModels.Pages
{
    public class TaskListPageViewModel : ViewModelBase
    {
        private Guid _VMGuid;

        public Guid VMGuid
        {
            get { return _VMGuid; }
            set { _VMGuid = value; base.OnPropertyChanged(nameof(VMGuid)); }
        }

        public TaskListPageViewModel()
        {
            VMGuid = Guid.NewGuid();
        }
    }
    public class TaskListDisplayItem
    {
        public Guid Id { get; set; }
        public string Title { get; set; }
        public string Content { get; set; }
        public bool IsComplete { get; set; }
        public bool InProgress { get; set; }
    }
}
