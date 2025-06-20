using System;
using System.Collections.Generic;
using System.Text;

namespace DtsParser
{
    // 设备树根节点
    public class DeviceTreeNode : AstNode
    {
        public List<IncludeDirective> Includes { get; set; } = new List<IncludeDirective>();
        public List<DefineDirective> Defines { get; set; } = new List<DefineDirective>();
        public DtsNode RootNode { get; set; }
    }
}
