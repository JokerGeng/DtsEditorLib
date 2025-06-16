using System;
using System.Collections.Generic;
using System.Text;

namespace DtsEditorLib.Models
{
    public class ValidationResult
    {
        public ValidationLevel Level { get; set; }
        public string Message { get; set; }
        public string NodePath { get; set; }

        public ValidationResult(ValidationLevel level, string message, string nodePath = null)
        {
            Level = level;
            Message = message;
            NodePath = nodePath;
        }
    }
}
