using System.Collections.Generic;
using System.Linq;
using DtsEditorLib.Models;

namespace DtsEditorLib.Validator
{
    // 地址单元规则
    public class AddressCellsRule : IValidationRule
    {
        public string Name => "AddressCells";

        public void Validate(DeviceTreeNode node, DeviceTree deviceTree, List<ValidationResult> results)
        {
            // 如果节点有子节点，应该有 #address-cells 和 #size-cells
            if (node.Children.Any())
            {
                var hasAddressCells = node.Properties.Find(p => p.Name.Contains("#address-cells"));
                var hasSizeCells = node.Properties.Find(p => p.Name.Contains("#size-cells"));

                if (hasAddressCells == null)
                {
                    results.Add(new ValidationResult
                    {
                        Severity = ValidationSeverity.Warning,
                        Message = "Node with children should have #address-cells property",
                        NodePath = node.FullPath,
                        LineNumber = node.LineNumber,
                        RuleName = Name
                    });
                }

                if (hasSizeCells == null)
                {
                    results.Add(new ValidationResult
                    {
                        Severity = ValidationSeverity.Warning,
                        Message = "Node with children should have #size-cells property",
                        NodePath = node.FullPath,
                        LineNumber = node.LineNumber,
                        RuleName = Name
                    });
                }
            }
        }
    }
}
