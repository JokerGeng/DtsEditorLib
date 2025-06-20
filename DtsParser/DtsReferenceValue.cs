using System;
using System.Collections.Generic;
using System.Text;

namespace DtsParser
{
    /// <summary>
    /// 引用值
    /// </summary>
    public class DtsReferenceValue : DtsValue
    {
        public string Reference { get; set; }

        public DtsReferenceValue(string reference)
        {
            Reference = reference;
        }

        public override string ToString() => $"&{Reference}";
    }
}
