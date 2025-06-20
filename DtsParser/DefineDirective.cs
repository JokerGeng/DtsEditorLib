using System;
using System.Collections.Generic;
using System.Text;

namespace DtsParser
{
    public class DefineDirective : AstNode
    {
        public string Name { get; set; }
        public string Value { get; set; }
    }
}
