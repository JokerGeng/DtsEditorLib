using System;
using System.Collections.Generic;
using System.Text;
using DtsEditorLib.Models;

namespace DtsEditorLib.Validator
{
    public class DeviceTreeValidator
    {
        private readonly List<ValidationResult> results = new List<ValidationResult>();
        private readonly Dictionary<string, IValidationRule> rules = new Dictionary<string, IValidationRule>();

        public DeviceTreeValidator()
        {
            InitializeStandardRules();
        }

        public List<ValidationResult> Validate(DeviceTree deviceTree)
        {
            results.Clear();
            ValidateNode(deviceTree.Root, deviceTree);
            return new List<ValidationResult>(results);
        }

        private void ValidateNode(DeviceTreeNode node, DeviceTree deviceTree)
        {
            // 应用所有规则
            foreach (var rule in rules.Values)
            {
                rule.Validate(node, deviceTree, results);
            }

            // 递归验证子节点
            foreach (var child in node.Children)
            {
                ValidateNode(child, deviceTree);
            }
        }

        private void InitializeStandardRules()
        {
            AddRule(new NodeNameRule());
            AddRule(new RequiredPropertiesRule());
            AddRule(new PropertyTypeRule());
            AddRule(new ReferenceValidationRule());
            AddRule(new AddressCellsRule());
            AddRule(new CompatibleStringRule());
        }

        public void AddRule(IValidationRule rule)
        {
            rules[rule.Name] = rule;
        }

        public void RemoveRule(string ruleName)
        {
            rules.Remove(ruleName);
        }
    }
}
