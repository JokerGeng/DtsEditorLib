using System.Collections.Generic;
using System.Linq;

namespace DtsParser
{
    /// <summary>
    /// 数组值<val...>,[val1...]
    /// </summary>
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
