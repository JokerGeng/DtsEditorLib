using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DotNet.DTS
{
    public class DtsParser
    {
        private readonly List<DtsToken> _tokens;
        private int _position;

        public DtsParser(IEnumerable<DtsToken> tokens) => _tokens = tokens.ToList();

        public DtsNode Parse()
        {
            return ParseNode("/");
        }

        private DtsNode ParseNode(string name)
        {
            var node = new DtsNode { Name = name };

            Expect(DtsTokenType.LeftBrace);
            while (!Match(DtsTokenType.RightBrace))
            {
                if (Peek().Type == DtsTokenType.Identifier)
                {
                    string id = Peek().Lexeme;
                    var lookahead = Peek(1);

                    if (lookahead.Type == DtsTokenType.Equals)
                    {
                        var prop = ParseProperty();
                        node.Properties[prop.Name] = prop;
                    }
                    else
                    {
                        node.Children.Add(ParseChildNode());
                    }
                }
            }

            return node;
        }

        private DtsNode ParseChildNode()
        {
            string name = Expect(DtsTokenType.Identifier).Lexeme;
            if (Match(DtsTokenType.At)) name += "@" + Expect(DtsTokenType.Identifier).Lexeme;
            return ParseNode(name);
        }

        private DtsProperty ParseProperty()
        {
            string name = Expect(DtsTokenType.Identifier).Lexeme;
            Expect(DtsTokenType.Equals);

            object value = null;

            if (Match(DtsTokenType.StringLiteral))
            {
                value = Previous().Lexeme;
            }
            else if (Match(DtsTokenType.LessThan))
            {
                var list = new List<uint>();
                while (!Match(DtsTokenType.GreaterThan))
                {
                    var token = Expect(DtsTokenType.Identifier);
                    list.Add(Convert.ToUInt32(token.Lexeme, 16)); // support <0x00 0x01>
                }
                value = list;
            }

            Expect(DtsTokenType.Semicolon);
            return new DtsProperty { Name = name, Value = value };
        }

        // ---- 辅助方法 ----

        private DtsToken Peek(int offset = 0) => _tokens[Math.Min(_position + offset, _tokens.Count - 1)];
        private DtsToken Previous() => _tokens[_position - 1];
        private DtsToken Expect(DtsTokenType type)
        {
            if (Peek().Type == type) return _tokens[_position++];
            throw new Exception($"Expected {type} but got {Peek().Type}");
        }
        private bool Match(DtsTokenType type)
        {
            if (Peek().Type == type)
            {
                _position++;
                return true;
            }
            return false;
        }
    }

}
