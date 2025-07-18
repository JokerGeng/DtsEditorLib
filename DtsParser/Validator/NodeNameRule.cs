﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using DtsParser.AST;
using DtsParser.Models;

namespace DtsParser.Validator
{
    // 节点名称规则
    public class NodeNameRule : IValidationRule
    {
        public string Name => "NodeName";
        private static readonly Regex ValidNameRegex = new Regex(@"^[a-zA-Z0-9][a-zA-Z0-9_.-]*$");

        public void Validate(DtsNode node, DtsDocument deviceTree, List<ValidationResult> results)
        {
            if (node.Name == "/" || string.IsNullOrEmpty(node.Name))
                return;

            if (!ValidNameRegex.IsMatch(node.Name))
            {
                results.Add(new ValidationResult
                {
                    Severity = ValidationSeverity.Error,
                    Message = $"Invalid node name '{node.Name}'. Node names must start with alphanumeric and contain only alphanumeric, underscore, dot, or hyphen.",
                    NodePath = node.Path,
                    LineNumber = node.Line,
                    RuleName = Name
                });
            }
        }
    }
}
