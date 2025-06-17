using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using DtsEditorLib.Models;

namespace DtsEditorLib.Validator
{
    // 节点名称规则
    public class NodeNameRule : IValidationRule
    {
        public string Name => "NodeName";
        private static readonly Regex ValidNameRegex = new Regex(@"^[a-zA-Z0-9][a-zA-Z0-9_.-]*$");

        public void Validate(DeviceTreeNode node, DeviceTree deviceTree, List<ValidationResult> results)
        {
            if (node.Name == "/" || string.IsNullOrEmpty(node.Name))
                return;

            if (!ValidNameRegex.IsMatch(node.Name))
            {
                results.Add(new ValidationResult
                {
                    Severity = ValidationSeverity.Error,
                    Message = $"Invalid node name '{node.Name}'. Node names must start with alphanumeric and contain only alphanumeric, underscore, dot, or hyphen.",
                    NodePath = node.FullPath,
                    LineNumber = node.LineNumber,
                    RuleName = Name
                });
            }
        }
    }
}
