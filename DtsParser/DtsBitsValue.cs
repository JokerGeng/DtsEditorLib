using System;
using System.Collections.Generic;
using System.Linq;

namespace DtsParser
{
    public class DtsBitsValue : DtsValue
    {
        public UInt16 Value { get; }

        public List<DtsValue> Values { get; }

        public DtsBitsValue(UInt16 value)
        {
            Value = value;
            Values = new List<DtsValue>();
        }

        public override string ToString()
        {
            return $"/bits/ {Value} {string.Join(",", Values.Select(t => $"<{t.ToString()}>"))}";
        }
    }
}
