using System.Collections.Generic;
using System.Text;

namespace DtsParser
{
    public class DtsDocument
    {
        public List<string> Comments { get; }
        public List<DtsIncludeDirective> Includes { get; }
        public DtsNode RootNode { get; set; }
        public string Version { get; set; }

        public DtsDocument()
        {
            Includes = new List<DtsIncludeDirective>();
            Comments = new List<string>();
        }

        public void AddInclude(DtsIncludeDirective include)
        {
            Includes.Add(include);
        }
    }
}
