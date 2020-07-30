using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nulah.VSIX.TaskTool.ToolWindows.TaskManager.Models
{
    public class Database
    {
        public Guid Id { get; set; }
        public string LastKnownLocation { get; set; }
        public DateTime CreatedUTC { get; set; }
    }
}
