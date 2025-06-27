using System;
using System.Collections.Generic;
using System.Text;
using DtsParser.AST;
using DtsParser.Models;

namespace DtsParser.Validator
{
    public interface IValidationRule
    {
        string Name { get; }
        void Validate(DtsNode node, DtsDocument deviceTree, List<ValidationResult> results);
    }
}
