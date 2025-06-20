using System;
using System.Collections.Generic;
using System.Text;

namespace DtsParser
{
    public class NumberValue : PropertyValue
    {
        public long Value { get; set; }
        public bool IsHex { get; set; }
    }
}
