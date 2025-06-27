using System;
using System.Collections.Generic;
using System.Text;

namespace DtsParser.AST
{
    /// <summary>
    /// 字符串值
    /// </summary>
    public class DtsStringValue : DtsValue
    {
        public string Value { get;}

        public DtsStringValue(string value)
        {
            Value = value;
        }

        public override string ToString()
        {
            return $"\"{Value}\"";
        }
    }
}
