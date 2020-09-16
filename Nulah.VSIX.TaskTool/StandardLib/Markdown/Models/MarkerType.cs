using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nulah.VSIX.TaskTool.StandardLib.Markdown.Models
{
    enum MarkerType
    {
        UNKNOWN,
        TEXTCONTENT,
        WHITESPACE,
        HEADER,
        PARAGRAPH_OPEN,
        PARAGRAPH_CLOSE,
        STRONG_OPEN,
        STRONG_CLOSED,
        EMPHASIS_OPEN,
        EMPHASIS_CLOSED
    }
}
