using System;
using System.Collections.Generic;
using System.Text;

namespace DtsParser.AST
{
    public class DtsCellStringValue : DtsValue
    {
        public string Value { get; }

        public DtsCellStringValue(string value)
        {
            Value = value;
        }

        public override string ToString()
        {
            return $"{Value}";
        }
    }
}
