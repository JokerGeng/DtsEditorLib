using System;
using System.Collections.Generic;
using System.Text;

namespace DtsEditorLib.Models
{
    internal class ParseContext
    {
        public List<ProcessedLine> Lines { get; set; }
        public DeviceTree DeviceTree { get; set; }
        public int CurrentIndex { get; set; }
    }

    // 处理后的行结构
    internal class ProcessedLine
    {
        public string Content { get; set; }
        public int LineNumber { get; set; }
    }
}
