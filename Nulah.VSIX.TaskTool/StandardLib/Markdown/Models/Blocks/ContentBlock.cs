using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nulah.VSIX.TaskTool.StandardLib.Markdown.Models.Blocks
{
    public class ContentBlock : INulahMarkdownBlock
    {
        public bool IsOpen { get; set; }
        public bool IsClosed { get; set; }
        public string Content { get; set; }
    }
}
