using System;
using System.Collections.Generic;
using System.Text;

namespace DtsParser
{
    public class DtsBitsValue : DtsValue
    {
        public UInt16 Value { get; }

        public DtsBitsValue(UInt16 value)
        {
            Value = value;
        }

        public override string ToString()
        {
            return "/bits/ " + Value;
        }
    }
}
