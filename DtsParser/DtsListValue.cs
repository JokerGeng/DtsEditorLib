using System;
using System.Collections.Generic;
using System.Text;

namespace DtsParser
{
    /// <summary>
    /// 序列值<>,<>
    /// </summary>
    public class DtsListValue : DtsValue
    {
        public List<DtsValue> Value { get; }

        public DtsListValue(List<DtsValue> values)
        {
            this.Value = values;
        }
        public override string ToString()
        {
            return "Todo DtsListValue";
        }
    }
}
