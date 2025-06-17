using System;
using System.Collections.Generic;
using System.Text;

namespace DtsEditorLib.Template
{
    public class TemplateParameter
    {
        public string Name { get; set; }
        public string Type { get; set; } // string, int, bool, array
        public string Description { get; set; }
        public object DefaultValue { get; set; }
        public bool Required { get; set; }
        public string[] ValidValues { get; set; }
    }
}
