using System;
using System.Collections.Generic;
using System.Text;

namespace DtsParser
{
    public class DtsValidator
    {
        private readonly List<ValidationResult> results = new List<ValidationResult>();
        private readonly Dictionary<string, IValidationRule> rules = new Dictionary<string, IValidationRule>();

        public DtsValidator()
        {
            InitializeStandardRules();
        }

        public List<ValidationResult> Validate(DtsDocument deviceTree)
        {
            results.Clear();
            ValidateNode(deviceTree.RootNode, deviceTree);
            return new List<ValidationResult>(results);
        }

        private void ValidateNode(DtsNode node, DtsDocument deviceTree)
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
