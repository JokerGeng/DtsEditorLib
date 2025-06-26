using System.Collections.Generic;
using System.Linq;

namespace DtsParser
{
    public class DtsArrayStringValue : DtsValue
    {
        /// <summary>
        /// main bearing <see cref="DtsStringValue"/>
        /// </summary>
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
