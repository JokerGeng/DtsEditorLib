using System;
using System.Collections.Generic;
using System.Text;

namespace DtsParser
{
    // AST节点基类
    public abstract class AstNode
    {
        public int Line { get; set; }
        public int Column { get; set; }
    }

}
