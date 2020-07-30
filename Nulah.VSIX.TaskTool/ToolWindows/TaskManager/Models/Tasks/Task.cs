using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nulah.VSIX.TaskTool.ToolWindows.TaskManager.Models.Tasks
{
    public class Task
    {
        public Guid Id { get; set; }
        public Guid? ParentId { get; set; }
        public DateTime CreatedUTC { get; set; }
        public DateTime UpdatedUTC { get; set; }
        public string Title { get; set; }
        public string Content { get; set; }
        public bool IsComplete { get; set; }
        public bool InProgress { get; set; }
        public bool IsSubTask { get; set; }
    }
}
