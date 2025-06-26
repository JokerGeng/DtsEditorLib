using System.Linq;

namespace DtsParser
{
    public enum DtsPropertyValueType
    {
        String,
        Number,
        Reference,
        Array,
        List,
        Bits,
        Bracket
    }

    public class DtsPropertyValue
    {
        public DtsPropertyValueType Type { get; }
        public object Value { get; }

        public DtsPropertyValue(DtsPropertyValueType type, object value)
        {
            Type = type;
            Value = value;
        }

        public override string ToString()
        {
            switch (Type)
            {
                case DtsPropertyValueType.String:
                case DtsPropertyValueType.Number:
                case DtsPropertyValueType.Reference:
                case DtsPropertyValueType.Bits:
                    return ((DtsValue)Value).ToString() ?? "";
                case DtsPropertyValueType.Array:
                    var array = (DtsArrayValue)Value;
                    return $"<{string.Join(" ", array.Values.Select(v => v.ToString()))}>";
                case DtsPropertyValueType.List:
                    var list = (DtsArrayStringValue)Value;
                    return list.ToString();
                default:
                    return Value?.ToString() ?? "";
            }
        }
    }
}
