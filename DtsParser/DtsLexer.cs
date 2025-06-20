using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace DtsParser
{
    // 词法分析器
    public class DtsLexer
    {
        private readonly string _input;
        private int _position;
        private int _line;
        private int _column;

        // 正则表达式模式
        private static readonly Dictionary<TokenType, Regex> TokenPatterns = new Dictionary<TokenType, Regex>
    {
        { TokenType.COMMENT, new Regex(@"^(//.*?$|/\*.*?\*/)", RegexOptions.Multiline | RegexOptions.Singleline) },
        { TokenType.INCLUDE, new Regex(@"^#include\s*[<""]([^>""]+)[>""]") },
        { TokenType.DEFINE, new Regex(@"^#define\s+(\w+)(?:\s+(.*))?") },
        { TokenType.DELETE_NODE, new Regex(@"^/delete-node/") },
        { TokenType.DELETE_PROP, new Regex(@"^/delete-property/") },
        { TokenType.STRING, new Regex(@"^""([^""\\]|\\.)*""") },
        { TokenType.REFERENCE, new Regex(@"^&(\w+|\{[^}]+\})") },
        { TokenType.LABEL, new Regex(@"^(\w+):") },
        { TokenType.IDENTIFIER, new Regex(@"^[a-zA-Z_][a-zA-Z0-9_\-,]*") },
        { TokenType.NUMBER, new Regex(@"^(0x[0-9a-fA-F]+|0[0-7]+|\d+)") },
        { TokenType.WHITESPACE, new Regex(@"^[ \t]+") },
        { TokenType.NEWLINE, new Regex(@"^[\r\n]+") }
    };

        // 单字符token映射
        private static readonly Dictionary<char, TokenType> SingleCharTokens = new Dictionary<char, TokenType>
    {
        { '{', TokenType.LBRACE },
        { '}', TokenType.RBRACE },
        { ';', TokenType.SEMICOLON },
        { '=', TokenType.EQUALS },
        { ',', TokenType.COMMA },
        { '(', TokenType.LPAREN },
        { ')', TokenType.RPAREN },
        { '<', TokenType.LANGLE },
        { '>', TokenType.RANGLE },
        { '/', TokenType.SLASH }
    };

        public DtsLexer(string input)
        {
            _input = input;
            _position = 0;
            _line = 1;
            _column = 1;
        }

        public List<Token> Tokenize()
        {
            var tokens = new List<Token>();

            while (_position < _input.Length)
            {
                var token = NextToken();
                if (token != null && token.Type != TokenType.WHITESPACE/* && token.Type != TokenType.COMMENT*/)
                {
                    tokens.Add(token);
                }
            }

            tokens.Add(new Token(TokenType.EOF, "", _line, _column));
            return tokens;
        }

        private Token NextToken()
        {
            if (_position >= _input.Length)
                return null;

            // 尝试匹配多字符token
            var remaining = _input.Substring(_position);

            foreach (var pattern in TokenPatterns)
            {
                var match = pattern.Value.Match(remaining);
                if (match.Success)
                {
                    var value = match.Value;
                    var token = new Token(pattern.Key, value, _line, _column);
                    Advance(value);
                    return token;
                }
            }

            // 尝试匹配单字符token
            var currentChar = _input[_position];
            if (SingleCharTokens.ContainsKey(currentChar))
            {
                var token = new Token(SingleCharTokens[currentChar], currentChar.ToString(), _line, _column);
                Advance(currentChar.ToString());
                return token;
            }

            // 未知字符，跳过
            Advance(currentChar.ToString());
            return NextToken();
        }

        private void Advance(string text)
        {
            foreach (var c in text)
            {
                _position++;
                if (c == '\n')
                {
                    _line++;
                    _column = 1;
                }
                else
                {
                    _column++;
                }
            }
        }
    }
}
