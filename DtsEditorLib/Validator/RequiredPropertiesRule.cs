using System.Collections.Generic;
using DtsEditorLib.Models;

namespace DtsEditorLib.Validator
{
    // 必需属性规则
    public class RequiredPropertiesRule : IValidationRule
    {
        public string Name => "RequiredProperties";

        private readonly Dictionary<string, string[]> requiredProps = new Dictionary<string, string[]>
        {
            ["cpu"] = new[] { "device_type", "reg" },
            ["memory"] = new[] { "device_type", "reg" },
            ["interrupt-controller"] = new[] { "interrupt-controller" }
        };

        public void Validate(DeviceTreeNode node, DeviceTree deviceTree, List<ValidationResult> results)
        {
            // 检查是否有compatible属性来确定设备类型
            var find = node.Properties.Find(t => t.Name == "compatible");
            if (find != null)
            {
                var compatible = find.Value?.ToString();
                if (!string.IsNullOrEmpty(compatible))
                {
                    CheckRequiredProperties(node, compatible, results);
                }
            }

            // 检查特定节点名称的必需属性
            if (requiredProps.ContainsKey(node.Name))
            {
                CheckRequiredProperties(node, node.Name, results);
            }
        }

        private void CheckRequiredProperties(DeviceTreeNode node, string nodeType, List<ValidationResult> results)
        {
            if (!requiredProps.ContainsKey(nodeType))
                return;

            foreach (var requiredProp in requiredProps[nodeType])
            {
                var find = node.Properties.Find(t => t.Name == requiredProp);
                if (find == null)
                {
                    results.Add(new ValidationResult
                    {
                        Severity = ValidationSeverity.Error,
                        Message = $"Missing required property '{requiredProp}' for {nodeType} node",
                        NodePath = node.FullPath,
                        LineNumber = node.LineNumber,
                        RuleName = Name
                    });
                }
            }
        }
    }
}
