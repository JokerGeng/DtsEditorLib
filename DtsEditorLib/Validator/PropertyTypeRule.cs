using System;
using System.Collections.Generic;
using System.Text;
using DtsEditorLib.Models;

namespace DtsEditorLib.Validator
{
    // 属性类型规则
    public class PropertyTypeRule : IValidationRule
    {
        public string Name => "PropertyType";

        public void Validate(DeviceTreeNode node, DeviceTree deviceTree, List<ValidationResult> results)
        {
            foreach (var property in node.Properties.Values)
            {
                ValidatePropertyType(property, node, results);
            }
        }

        private void ValidatePropertyType(DeviceTreeProperty property, DeviceTreeNode node, List<ValidationResult> results)
        {
            switch (property.Name)
            {
                case "reg":
                case "ranges":
                    if (property.ValueType != PropertyValueType.IntegerArray)
                    {
                        results.Add(new ValidationResult
                        {
                            Severity = ValidationSeverity.Error,
                            Message = $"Property '{property.Name}' should be an integer array",
                            NodePath = node.FullPath,
                            PropertyName = property.Name,
                            LineNumber = property.LineNumber,
                            RuleName = Name
                        });
                    }
                    break;

                case "compatible":
                case "model":
                case "status":
                    if (property.ValueType != PropertyValueType.String)
                    {
                        results.Add(new ValidationResult
                        {
                            Severity = ValidationSeverity.Warning,
                            Message = $"Property '{property.Name}' should be a string",
                            NodePath = node.FullPath,
                            PropertyName = property.Name,
                            LineNumber = property.LineNumber,
                            RuleName = Name
                        });
                    }
                    break;
            }
        }
    }
}
