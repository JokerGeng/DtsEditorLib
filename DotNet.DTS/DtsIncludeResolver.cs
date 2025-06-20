using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

namespace DotNet.DTS
{
    public class DtsIncludeResolver
    {
        public static string ResolveIncludes(string filePath, HashSet<string> visited = null)
        {
            visited ??= new HashSet<string>();
            if (visited.Contains(filePath)) return "";
            visited.Add(filePath);

            var lines = File.ReadAllLines(filePath);
            var resolved = new StringBuilder();

            foreach (var line in lines)
            {
                if (line.Trim().StartsWith("#include"))
                {
                    var match = Regex.Match(line, "#include\\s+\"([^\"]+)\"");
                    if (match.Success)
                    {
                        var includePath = Path.Combine(Path.GetDirectoryName(filePath)!, match.Groups[1].Value);
                        resolved.AppendLine(ResolveIncludes(includePath, visited));
                    }
                }
                else
                {
                    resolved.AppendLine(line);
                }
            }

            return resolved.ToString();
        }
    }

}
