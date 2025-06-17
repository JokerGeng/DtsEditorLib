using System;
using System.Collections.Generic;
using System.Text;

namespace DtsEditorLib.Template
{
    public class DeviceTreeTemplate
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string Content { get; set; }
        public List<TemplateParameter> Parameters { get; set; } = new List<TemplateParameter>();
        public Dictionary<string, object> DefaultValues { get; set; } = new Dictionary<string, object>();
    }
}
