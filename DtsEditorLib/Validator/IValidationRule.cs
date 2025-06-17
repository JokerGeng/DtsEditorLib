using System;
using System.Collections.Generic;
using System.Text;
using DtsEditorLib.Models;

namespace DtsEditorLib.Validator
{
    // 验证规则接口
    public interface IValidationRule
    {
        string Name { get; }
        void Validate(DeviceTreeNode node, DeviceTree deviceTree, List<ValidationResult> results);
    }
}
