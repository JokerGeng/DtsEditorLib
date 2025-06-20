using System;
using System.Collections.Generic;
using System.Text;

namespace DtsParser
{
    /// <summary>
    /// 数字值
    /// </summary>
    public class DtsNumberValue : DtsValue
    {
        public long Value { get; set; }
        public bool IsHex { get; set; }

        public DtsNumberValue(long value, bool isHex = false)
        {
            Value = value;
            IsHex = isHex;
        }

        public override string ToString() => IsHex ? $"0x{Value:X}" : Value.ToString();
    }
}
