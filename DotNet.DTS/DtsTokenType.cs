using System;
using System.Collections.Generic;
using System.Text;

namespace DotNet.DTS
{
    public enum DtsTokenType
    {
        Identifier, StringLiteral, NumberLiteral,
        LeftBrace, RightBrace, Semicolon, Equals, LessThan, GreaterThan, At,
        Slash, EndOfFile
    }
}
