using System;
using System.Collections.Generic;
using System.Text;

namespace DtsParser
{
    internal class DtsHexValue : DtsValue
    {
        public string Value { get;}
        public DtsHexValue(string vale)
        {
            this.Value = vale;
        }
        public override string ToString()
        {
            return this.Value;
        }
    }
}
