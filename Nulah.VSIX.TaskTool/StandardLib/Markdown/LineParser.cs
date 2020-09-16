using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Nulah.VSIX.TaskTool.StandardLib.Markdown.Models;
using Nulah.VSIX.TaskTool.StandardLib.Markdown.Models.Blocks;

namespace Nulah.VSIX.TaskTool.StandardLib.Markdown
{
    // Skeleton to a specific set of markdown I'll be wanting to parse
    // for now I just return the content in a paragraph block
    class LineParser
    {
        private readonly string _currentLine;
        private int _parseOffset = 0;
        private int _tempOffset = 0;

        /// <summary>
        /// If the line starts with a valid header opener, all other markdown will be treated as text
        /// </summary>
        private bool _lineIsHeader;
        private int _headerDepth;

        public LineParser(string line)
        {
            _currentLine = line;
        }

        public ParagraphBlock Parse()
        {
            /*
            foreach (var c in _currentLine)
            {
                ParseCharacter(c);
            }
            */
            return new ParagraphBlock
            {
                Content = _currentLine
            };
        }

        private void ParseCharacter(char unparsedChar)
        {
            /*
            if (HeaderCheck(unparsedChar) == true)
            {

            }*/
            _parseOffset++;
        }
        /*
        private bool HeaderCheck(char unparsedChar)
        {
            var headerDepth = 0;
            var isHeader = false;
            if (unparsedChar == '#')
            {
                headerDepth = 1;
                _tempOffset = _parseOffset;

                while (headerDepth < 6)
                {
                    var nextChar = ReadNext();
                    if (nextChar == MarkerType.WHITESPACE)
                    {
                        isHeader = true;
                        _headerDepth = headerDepth;
                        return isHeader;
                    }
                    else if (nextChar == MarkerType.HEADER)
                    {
                        headerDepth++;
                    }
                }
            }
            return isHeader;
        }
        */
        /// <summary>
        /// Reads the next character to parse without increasing the offset
        /// </summary>
        /// <returns></returns>
        private MarkerType ReadNext()
        {
            if (char.IsWhiteSpace(_currentLine[_tempOffset]) == true)
            {
                return MarkerType.WHITESPACE;
            }

            return MarkerType.UNKNOWN;
        }
    }
}
