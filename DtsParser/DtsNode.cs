using System;
using System.Collections.Generic;
using System.Text;

namespace DtsParser
{
    // DTS节点
    public class DtsNode : AstNode
    {
        public string Name { get; set; }
        public string Label { get; set; }
        public string UnitAddress { get; set; } // @后面的地址部分
        public List<Property> Properties { get; set; } = new List<Property>();
        public List<DtsNode> Children { get; set; } = new List<DtsNode>();
        public List<string> DeletedNodes { get; set; } = new List<string>();
        public List<string> DeletedProperties { get; set; } = new List<string>();

        public string FullName => UnitAddress != null ? $"{Name}@{UnitAddress}" : Name;
    }
}
