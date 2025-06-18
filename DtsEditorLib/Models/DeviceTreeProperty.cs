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
        public int LineNumber { get; set; }
        public string Label { get; set; }

        public DeviceTreeProperty(string name)
        {
            Name = name;
            ValueType = PropertyValueType.Empty;
        }

        public T GetValue<T>()
        {
            if (Value is T directValue)
                return directValue;

            return (T)Convert.ChangeType(Value, typeof(T));
        }

        public string[] GetStringArray() => Value as string[];
        public int[] GetIntegerArray() => Value as int[];
        public byte[] GetByteArray() => Value as byte[];

        public List<int[]> GetListArray() => Value as List<int[]>;

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
                case PropertyValueType.LabelReference:
                    return $"{Name} = &{Value};";
                case PropertyValueType.ValueReference:
                    return $"{Name} = <&{Value}>;";
                default:
                    return $"{Name} = {RawValue};";
            }
        }
    }
}
