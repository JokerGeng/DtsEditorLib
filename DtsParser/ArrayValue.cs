using System;
using System.Collections.Generic;
using System.Text;

namespace DtsParser
{
    public class ArrayValue : PropertyValue
    {
        public List<PropertyValue> Values { get; set; } = new List<PropertyValue>();
    }
}
