using System;
using System.Collections.Generic;
using System.Text;

namespace DtsParser
{
    public interface IValidationRule
    {
        string Name { get; }
        void Validate(DtsNode node, DtsDocument deviceTree, List<ValidationResult> results);
    }
}
