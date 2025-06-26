using System.Collections.Generic;
using System.Linq;

namespace DtsParser
{
    /// <summary>
    /// DTS属性
    /// </summary>
    public class DtsProperty
    {
        public string Name { get; set; }
        ///<> list
        public List<DtsPropertyValue> Values { get; set; }
        public int Line { get; set; }

        public DtsProperty(string name, List<DtsPropertyValue> values, int line = 0)
        {
            Name = name;
            Values = values;
            Line = line;
        }

        public string ToString(string indent)
        {
            return Values?.Count > 0! ? $"{Name} = {string.Join($",\r{indent}", Values.Select(t => t.ToString()))};" : $"{Name};";
        }
        public override string ToString()
        {
            return Values?.Count > 0! ? $"{Name} = {string.Join(",\r", Values.Select(t => t.ToString()))};" : $"{Name};";
        }
    }

}
