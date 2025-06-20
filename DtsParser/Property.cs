using System;
using System.Collections.Generic;
using System.Text;

namespace DtsParser
{
    // 属性
    public class Property : AstNode
    {
        public string Name { get; set; }
        public List<PropertyValue> Values { get; set; } = new List<PropertyValue>();
    }
}
