using System;
using System.Collections.Generic;
using System.Text;

namespace DtsParser
{
    /// <summary>
    /// 数组值
    /// </summary>
    public class DtsArrayValue : DtsValue
    {
        public List<DtsValue> Values { get; set; }

        public DtsArrayValue()
        {
            Values = new List<DtsValue>();
        }

        public override string ToString()
        {
            return $"<{string.Join(", ", Values)}>";
        }
    }

}
