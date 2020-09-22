using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nulah.VSIX.TaskTool.ToolWindows.TaskManager.Models
{
    public class NulahDBMeta
    {
        public int SchemaVersion { get; private set; }
        public string TaskListName { get; set; }
        public bool IsProjectDatabase { get; set; }
        public string ProjectName { get; set; }
        public string ProjectOriginalLocation { get; set; }
        public DateTime CreatedUTC { get; private set; }

        public NulahDBMeta()
        {
            SchemaVersion = 1;
            IsProjectDatabase = false;
            CreatedUTC = DateTime.UtcNow;
        }
    }
}
