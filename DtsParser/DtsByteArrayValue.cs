using System.Collections.Generic;
using System.Linq;

namespace DtsParser
{
    //[val1...]
    public class DtsByteArrayValue : DtsValue
    {
        public List<string> Values { get; }
        public DtsByteArrayValue()
        {
            Values = new List<string>();
        }

        public override string ToString()
        {
            return $"[{string.Join(" ", Values.Select(t => t))}]";
        }
    }
}
