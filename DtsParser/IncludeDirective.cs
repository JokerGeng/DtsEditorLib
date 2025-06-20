using System;
using System.Collections.Generic;
using System.Text;

namespace DtsParser
{
    // 预处理指令
    public class IncludeDirective : AstNode
    {
        public string FilePath { get; set; }
    }
}
