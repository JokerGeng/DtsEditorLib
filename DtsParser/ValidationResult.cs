using System;
using System.Collections.Generic;
using System.Text;

namespace DtsParser
{
    public class ValidationResult
    {
        public ValidationSeverity Severity { get; set; }
        public string Message { get; set; }
        public string NodePath { get; set; }
        public string PropertyName { get; set; }
        public int LineNumber { get; set; }
        public string RuleName { get; set; }

        public override string ToString()
        {
            var location = !string.IsNullOrEmpty(NodePath) ? $" at {NodePath}" : "";
            var property = !string.IsNullOrEmpty(PropertyName) ? $".{PropertyName}" : "";
            var line = LineNumber > 0 ? $" (line {LineNumber})" : "";

            return $"{Severity}: {Message}{location}{property}{line}";
        }
    }
}
