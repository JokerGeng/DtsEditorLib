using System;
using System.Collections.Generic;
using System.Text;
using DtsEditorLib.Models;
using System.Text.RegularExpressions;

namespace DtsEditorLib.Validator
{
    // Compatible字符串规则
    public class CompatibleStringRule : IValidationRule
    {
        public string Name => "CompatibleString";
        private static readonly Regex CompatibleRegex = new Regex(@"^[a-zA-Z0-9][a-zA-Z0-9_.-]*,[a-zA-Z0-9][a-zA-Z0-9_.-]*$");

        public void Validate(DeviceTreeNode node, DeviceTree deviceTree, List<ValidationResult> results)
        {
            if (node.Properties.ContainsKey("compatible"))
            {
                var compatible = node.Properties["compatible"];
                var compatibleValue = compatible.Value?.ToString();

                if (!string.IsNullOrEmpty(compatibleValue) && !CompatibleRegex.IsMatch(compatibleValue))
                {
                    results.Add(new ValidationResult
                    {
                        Severity = ValidationSeverity.Warning,
                        Message = "Compatible string should follow 'manufacturer,model' format",
                        NodePath = node.FullPath,
                        PropertyName = "compatible",
                        LineNumber = compatible.LineNumber,
                        RuleName = Name
                    });
                }
            }
        }
    }
}
