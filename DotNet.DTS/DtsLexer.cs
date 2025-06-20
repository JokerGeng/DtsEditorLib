using System;
using System.Collections.Generic;
using System.Text;

namespace DotNet.DTS
{
    public class DtsLexer
    {
        private readonly string _text;
        private int _position;

        public DtsLexer(string text) => _text = text;

        public IEnumerable<DtsToken> Tokenize()
        {
            while (_position < _text.Length)
            {
                char c = _text[_position];

                if (char.IsWhiteSpace(c)) { _position++; continue; }
                if (char.IsLetter(c) || c == '_')
                {
                    yield return ReadIdentifier();
                }
                else if (c == '"')
                {
                    yield return ReadStringLiteral();
                }
                else if (c == '<')
                {
                    yield return new DtsToken(DtsTokenType.LessThan, "<"); _position++;
                }
                else if (c == '>')
                {
                    yield return new DtsToken(DtsTokenType.GreaterThan, ">"); _position++;
                }
                else if (c == '{')
                {
                    yield return new DtsToken(DtsTokenType.LeftBrace, "{"); _position++;
                }
                else if (c == '}')
                {
                    yield return new DtsToken(DtsTokenType.RightBrace, "}"); _position++;
                }
                else if (c == ';')
                {
                    yield return new DtsToken(DtsTokenType.Semicolon, ";"); _position++;
                }
                else if (c == '=')
                {
                    yield return new DtsToken(DtsTokenType.Equals, "="); _position++;
                }
                else if (c == '@')
                {
                    yield return new DtsToken(DtsTokenType.At, "@"); _position++;
                }
                else if (c == '/')
                {
                    yield return new DtsToken(DtsTokenType.Slash, "/"); _position++;
                }
                else
                {
                    throw new Exception($"Unknown character: {c}");
                }
            }

            yield return new DtsToken(DtsTokenType.EndOfFile, "");
        }

        private DtsToken ReadIdentifier()
        {
            int start = _position;
            while (_position < _text.Length &&
                   (char.IsLetterOrDigit(_text[_position]) || _text[_position] == '_' || _text[_position] == '-'))
            {
                _position++;
            }
            return new DtsToken(DtsTokenType.Identifier, _text[start.._position]);
        }

        private DtsToken ReadStringLiteral()
        {
            int start = ++_position;
            while (_position < _text.Length && _text[_position] != '"') _position++;
            string value = _text[start.._position];
            _position++; // skip closing "
            return new DtsToken(DtsTokenType.StringLiteral, value);
        }
    }

}
