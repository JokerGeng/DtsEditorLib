using System;
using System.Collections.Generic;
using System.Text;

namespace DtsParser
{
    /// <summary>
    /// DTS属性
    /// </summary>
    public class DtsProperty
    {
        public string Name { get; set; }
        public DtsValue Value { get; set; }
        public int Line { get; set; }

        public DtsProperty(string name, DtsValue value = null, int line = 0)
        {
            Name = name;
            Value = value;
            Line = line;
        }

        public override string ToString()
        {
            return Value != null ? $"{Name} = {Value};" : $"{Name};";
        }
    }

}
