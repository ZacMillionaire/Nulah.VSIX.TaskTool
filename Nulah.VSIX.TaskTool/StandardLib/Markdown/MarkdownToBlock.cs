using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Nulah.VSIX.TaskTool.StandardLib.Markdown;
using Nulah.VSIX.TaskTool.StandardLib.Markdown.Models;
using Nulah.VSIX.TaskTool.StandardLib.Markdown.Models.Blocks;

namespace Nulah.VSIX.TaskTool.StandardLib
{
    // Skeleton to a specific set of markdown I'll be wanting to parse
    // for now I just return the content as a list of content blocks split on new lines
    public class MarkdownToBlock
    {
        public MarkdownToBlock()
        {

        }

        public List<ContentBlock> Parse(string markdownContent)
        {
            var content = new List<ContentBlock>();
            foreach (var contentLine in markdownContent.Split(new[] { Environment.NewLine }, StringSplitOptions.None))
            {
                var lineParser = new LineParser(contentLine);
                content.Add(lineParser.Parse());
            }

            return content;
        }
    }
}
