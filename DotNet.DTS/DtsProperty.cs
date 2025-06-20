using System;
using System.Collections.Generic;
using System.Text;

namespace DotNet.DTS
{
    public class DtsProperty
    {
        public string Name { get; set; }
        public object Value { get; set; } // string | List<string> | List<uint> 等
    }
}
