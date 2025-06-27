using System;

namespace DtsParser
{
    /// <summary>
    /// 数字值
    /// </summary>
    public class DtsNumberValue : DtsValue
    {
        public UInt64 Value { get; }
        public bool IsHex { get; }

        public int Length { get; }

        public DtsNumberValue(UInt64 value, bool isHex = false, int length = -1)
        {
            Value = value;
            IsHex = isHex;
            Length = length;
        }

        public override string ToString()
        {
            if (Length != -1 && IsHex)
            {
                return "0x" + Value.ToString($"X{Length}");
            }
            return Value.ToString();
        }
    }
}
