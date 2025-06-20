using System;
using System.Collections.Generic;
using System.Text;

namespace DtsParser
{
    public class DtsDocument
    {
        public List<DtsIncludeDirective> Includes { get; }
        public DtsNode RootNode { get; set; }
        public string Version { get; set; }

        public DtsDocument()
        {
            Includes = new List<DtsIncludeDirective>();
        }

        public void AddInclude(DtsIncludeDirective include)
        {
            Includes.Add(include);
        }

        public override string ToString()
        {
            var sb = new StringBuilder();

            if (!string.IsNullOrEmpty(Version))
            {
                sb.AppendLine(Version);
                sb.AppendLine();
            }

            foreach (var include in Includes)
            {
                sb.AppendLine(include.ToString());
            }

            if (Includes.Count > 0)
            {
                sb.AppendLine();
            }

            if (RootNode != null)
            {
                sb.Append(RootNode.ToString());
            }

            return sb.ToString();
        }
    }
}
