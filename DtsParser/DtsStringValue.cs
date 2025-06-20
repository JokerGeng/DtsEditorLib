﻿using System;
using System.Collections.Generic;
using System.Text;

namespace DtsParser
{
    /// <summary>
    /// 字符串值
    /// </summary>
    public class DtsStringValue : DtsValue
    {
        public string Value { get; set; }

        public DtsStringValue(string value)
        {
            Value = value;
        }

        public override string ToString() => $"\"{Value}\"";
    }
}
