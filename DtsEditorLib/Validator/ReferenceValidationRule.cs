using System;
using System.Collections.Generic;
using System.Text;
using DtsEditorLib.Models;

namespace DtsEditorLib.Validator
{
    // 引用验证规则
    public class ReferenceValidationRule : IValidationRule
    {
        public string Name => "ReferenceValidation";

        public void Validate(DeviceTreeNode node, DeviceTree deviceTree, List<ValidationResult> results)
        {
            foreach (var property in node.Properties.Values)
            {
                if (property.ValueType == PropertyValueType.LabelReference)
                {
                    var referencedLabel = property.Value?.ToString();
                    if (!string.IsNullOrEmpty(referencedLabel) && !deviceTree.Labels.ContainsKey(referencedLabel))
                    {
                        results.Add(new ValidationResult
                        {
                            Severity = ValidationSeverity.Error,
                            Message = $"Reference to undefined label '{referencedLabel}'",
                            NodePath = node.FullPath,
                            PropertyName = property.Name,
                            LineNumber = property.LineNumber,
                            RuleName = Name
                        });
                    }
                }
            }
        }
    }
}
