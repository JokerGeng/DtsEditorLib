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
        public List<DtsValue> Values { get; }
        public int Line { get; set; }

        public DtsProperty(string name)
        {
            Name = name;
            this.Values = new List<DtsValue>();
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
