using System;
using System.Collections.Generic;
using System.Text;

namespace DtsParser
{
    public class DtsByteArrayValue : DtsValue
    {
        public List<string> Values { get; }
        public DtsByteArrayValue()
        {
            Values = new List<string>();
        }

        public override string ToString()
        {
            return $"[{string.Join(", ", Values)}]";
        }
    }
}
