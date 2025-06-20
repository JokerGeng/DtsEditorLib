using System;
using System.Collections.Generic;
using System.Text;

namespace DotNet.DTS
{
    public class DtsNode
    {
        public string Name { get; set; }
        public Dictionary<string, DtsProperty> Properties { get; set; } = new Dictionary<string, DtsProperty>();
        public List<DtsNode> Children { get; set; } = new List<DtsNode>();
    }
}
