using System.Collections.Generic;
using System.Linq;

namespace DtsParser
{
    /// <summary>
    /// 序列值<>,<>
    /// </summary>
    public class DtsArrayStringValue : DtsValue
    {
        public List<DtsValue> Values { get; }

        public DtsArrayStringValue()
        {
            this.Values = new List<DtsValue>();
        }
        public override string ToString()
        {
            return string.Join(", ", Values.Select(t => t.ToString()));
        }
    }
}
