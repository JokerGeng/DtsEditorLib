using System.Collections.Generic;
using System.Linq;

namespace DtsParser.AST
{
    public class DtsListValue : DtsValue
    {
        /// <summary>
        /// main bearing <see cref="DtsStringValue"/>
        /// </summary>
        public List<DtsValue> Values { get; }

        public DtsListValue()
        {
            Values = new List<DtsValue>();
        }
        public override string ToString()
        {
            return string.Join(", ", Values.Select(t => t.ToString()));
        }
    }
}
