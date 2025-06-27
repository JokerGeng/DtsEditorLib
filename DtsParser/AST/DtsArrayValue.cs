using System.Collections.Generic;
using System.Linq;

namespace DtsParser.AST
{
    //<val...>
    public class DtsArrayValue : DtsValue
    {
        public List<DtsValue> Values { get; }

        public DtsArrayValue()
        {
            Values = new List<DtsValue>();
        }

        public override string ToString()
        {
            return $"<{string.Join(" ", Values.Select(t => t.ToString()))}>";
        }
    }

}
