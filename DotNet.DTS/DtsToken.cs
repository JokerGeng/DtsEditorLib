using System;
using System.Collections.Generic;
using System.Text;

namespace DotNet.DTS
{
    public struct DtsToken
    {
        public DtsTokenType Type { get;}
        public string Lexeme { get; }

        public DtsToken(DtsTokenType type, string lexeme)
        {
            this.Type = type;
            this.Lexeme = lexeme;
        }
    }
}
