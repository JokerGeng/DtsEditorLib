using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DtsParser
{
    public enum DtsPropertyValueType
    {
        String,
        Number,
        Reference,
        Array,
        Bracket,
        Expression  // 新增：复杂表达式
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
                case DtsPropertyValueType.Expression:
                    return ((DtsExpression)Value).ToString();
                case DtsPropertyValueType.String:
                case DtsPropertyValueType.Number:
                case DtsPropertyValueType.Reference:
                    return Value?.ToString() ?? "";
                case DtsPropertyValueType.Array:
                    var array = (List<DtsPropertyValue>)Value;
                    return $"<{string.Join(" ", array.Select(v => v.ToString()))}>";
                default:
                    return Value?.ToString() ?? "";
            }
        }
    }
}
