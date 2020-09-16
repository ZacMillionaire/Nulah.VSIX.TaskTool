using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nulah.VSIX.TaskTool.StandardLib.Markdown.Models
{
    interface INulahMarkdownBlock
    {
        bool IsOpen { get; set; }
        bool IsClosed { get; set; }
    }
}
