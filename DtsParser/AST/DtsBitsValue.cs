using System;
using System.Collections.Generic;
using System.Linq;

namespace DtsParser.AST
{
    public class DtsBitsValue : DtsValue
    {
        public ushort Value { get; }

        public List<DtsValue> Values { get; }

        public DtsBitsValue(ushort value)
        {
            Value = value;
            Values = new List<DtsValue>();
        }

        public override string ToString()
        {
            return $"/bits/ {Value} {string.Join(", ", Values.Select(t => t.ToString()))}";
        }
    }
}
