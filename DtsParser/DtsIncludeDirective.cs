using System;

namespace DtsParser
{
    public class DtsIncludeDirective
    {
        public string Path { get; }
        public bool IsSystemInclude { get; }
        public int Line { get; }
        public string ResolvedPath { get; set; }
        public string Content { get; set; }

        public DtsIncludeDirective(string path, bool isSystemInclude, int line)
        {
            Path = path ?? throw new ArgumentNullException(nameof(path));
            IsSystemInclude = isSystemInclude;
            Line = line;
        }

        public override string ToString()
        {
            return IsSystemInclude ? $"#include <{Path}>" : $"#include \"{Path}\"";
        }
    }
}
