using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DtsEditorLib.Models
{
    //设备树属性
    public class DeviceTreeProperty
    {
        public string Name { get; set; }
        public PropertyValueType ValueType { get; set; }
        public object Value { get; set; }
        public string RawValue { get; set; }
        public List<string> Comments { get; set; } = new List<string>();

        public T GetValue<T>()
        {
            if (Value is T directValue)
                return directValue;

            return (T)Convert.ChangeType(Value, typeof(T));
        }

        public override string ToString()
        {
            switch (ValueType)
            {
                case PropertyValueType.Empty:
                    return $"{Name};";
                case PropertyValueType.String:
                    return $"{Name} = \"{Value}\";";
                case PropertyValueType.Integer:
                    return $"{Name} = <{Value}>;";
                case PropertyValueType.IntegerArray:
                    var intArray = (int[])Value;
                    return $"{Name} = <{string.Join(" ", intArray)}>;";
                case PropertyValueType.ByteArray:
                    var byteArray = (byte[])Value;
                    return $"{Name} = [{string.Join(" ", byteArray.Select(b => $"{b:X2}"))}];";
                case PropertyValueType.Reference:
                    return $"{Name} = <&{Value}>;";
                default:
                    return $"{Name} = {RawValue};";
            }
        }
    }
}
