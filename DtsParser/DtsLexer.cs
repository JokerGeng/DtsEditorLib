using System;
using System.Collections.Generic;
using System.Text;
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

        public DtsLexer(string input)
        {
            _input = input ?? throw new ArgumentNullException(nameof(input));
            _position = 0;
            _line = 1;
            _column = 1;
        }

        /// <summary>
        /// 获取所有tokens
        /// </summary>
        public List<Token> Tokenize()
        {
            var tokens = new List<Token>();
            Token token;

            do
            {
                token = NextToken();
                if (token.Type != TokenType.Comment) // 过滤掉注释
                {
                    tokens.Add(token);
                }
            } while (token.Type != TokenType.EOF);

            return tokens;
        }

        /// <summary>
        /// 获取下一个token
        /// </summary>
        private Token NextToken()
        {
            SkipWhitespace();

            if (_position >= _input.Length)
                return new Token(TokenType.EOF, "", _line, _column);

            var currentChar = _input[_position];
            var currentLine = _line;
            var currentColumn = _column;

            // 处理单字符token
            switch (currentChar)
            {
                case '{':
                    Advance();
                    return new Token(TokenType.LeftBrace, "{", currentLine, currentColumn);
                case '}':
                    Advance();
                    return new Token(TokenType.RightBrace, "}", currentLine, currentColumn);
                case '(':
                    Advance();
                    return new Token(TokenType.LeftParen, "(", currentLine, currentColumn);
                case ')':
                    Advance();
                    return new Token(TokenType.RightParen, ")", currentLine, currentColumn);
                case '[':
                    Advance();
                    return new Token(TokenType.LeftBracket, "[", currentLine, currentColumn);
                case ']':
                    Advance();
                    return new Token(TokenType.RightBracket, "]", currentLine, currentColumn);
                case '<':
                    Advance();
                    return new Token(TokenType.LeftAngle, "<", currentLine, currentColumn);
                case '>':
                    Advance();
                    return new Token(TokenType.RightAngle, ">", currentLine, currentColumn);
                case ';':
                    Advance();
                    return new Token(TokenType.Semicolon, ";", currentLine, currentColumn);
                case ',':
                    Advance();
                    return new Token(TokenType.Comma, ",", currentLine, currentColumn);
                case '=':
                    Advance();
                    return new Token(TokenType.Equals, "=", currentLine, currentColumn);
                case '&':
                    Advance();
                    return new Token(TokenType.Ampersand, "&", currentLine, currentColumn);
                case '\n':
                    Advance();
                    return new Token(TokenType.Newline, "\n", currentLine, currentColumn);
            }

            // 处理注释
            if (currentChar == '/' && Peek() == '/')
            {
                return ReadLineComment();
            }
            if (currentChar == '/' && Peek() == '*')
            {
                return ReadBlockComment();
            }

            // 处理字符串
            if (currentChar == '"')
            {
                return ReadString();
            }

            // 处理预处理指令
            if (currentChar == '#')
            {
                return ReadPreprocessorDirective();
            }

            // 处理数字
            if (char.IsDigit(currentChar) || (currentChar == '0' && Peek() == 'x'))
            {
                return ReadNumber();
            }

            // 处理标识符
            if (char.IsLetter(currentChar) || currentChar == '_')
            {
                return ReadIdentifier();
            }

            // 未知字符
            var unknownChar = currentChar.ToString();
            Advance();
            return new Token(TokenType.Unknown, unknownChar, currentLine, currentColumn);
        }

        /// <summary>
        /// 跳过空白字符（除了换行符）
        /// </summary>
        private void SkipWhitespace()
        {
            while (_position < _input.Length &&
                   char.IsWhiteSpace(_input[_position]) &&
                   _input[_position] != '\n')
            {
                Advance();
            }
        }

        /// <summary>
        /// 读取行注释
        /// </summary>
        private Token ReadLineComment()
        {
            var startLine = _line;
            var startColumn = _column;
            var sb = new StringBuilder();

            while (_position < _input.Length && _input[_position] != '\n')
            {
                sb.Append(_input[_position]);
                Advance();
            }

            return new Token(TokenType.Comment, sb.ToString(), startLine, startColumn);
        }

        /// <summary>
        /// 读取块注释
        /// </summary>
        private Token ReadBlockComment()
        {
            var startLine = _line;
            var startColumn = _column;
            var sb = new StringBuilder();

            Advance(); // skip '/'
            Advance(); // skip '*'

            while (_position < _input.Length - 1)
            {
                if (_input[_position] == '*' && _input[_position + 1] == '/')
                {
                    sb.Append("*/");
                    Advance();
                    Advance();
                    break;
                }
                sb.Append(_input[_position]);
                Advance();
            }

            return new Token(TokenType.Comment, sb.ToString(), startLine, startColumn);
        }

        /// <summary>
        /// 读取字符串字面量
        /// </summary>
        private Token ReadString()
        {
            var startLine = _line;
            var startColumn = _column;
            var sb = new StringBuilder();

            Advance(); // skip opening quote

            while (_position < _input.Length && _input[_position] != '"')
            {
                if (_input[_position] == '\\' && _position + 1 < _input.Length)
                {
                    Advance(); // skip backslash
                    var escapeChar = _input[_position];
                    switch (escapeChar)
                    {
                        case 'n': sb.Append('\n'); break;
                        case 't': sb.Append('\t'); break;
                        case 'r': sb.Append('\r'); break;
                        case '\\': sb.Append('\\'); break;
                        case '"': sb.Append('"'); break;
                        default: sb.Append(escapeChar); break;
                    }
                }
                else
                {
                    sb.Append(_input[_position]);
                }
                Advance();
            }

            if (_position < _input.Length)
                Advance(); // skip closing quote

            return new Token(TokenType.String, sb.ToString(), startLine, startColumn);
        }

        /// <summary>
        /// 读取预处理指令
        /// </summary>
        private Token ReadPreprocessorDirective()
        {
            var startLine = _line;
            var startColumn = _column;
            var sb = new StringBuilder();

            while (_position < _input.Length &&
                   (char.IsLetterOrDigit(_input[_position]) || _input[_position] == '#' || _input[_position] == '_'))
            {
                sb.Append(_input[_position]);
                Advance();
            }

            var directive = sb.ToString();
            TokenType type = directive switch
            {
                "#include" => TokenType.Include,
                "#define" => TokenType.Define,
                _ => TokenType.Unknown
            };

            return new Token(type, directive, startLine, startColumn);
        }

        /// <summary>
        /// 读取数字
        /// </summary>
        private Token ReadNumber()
        {
            var startLine = _line;
            var startColumn = _column;
            var sb = new StringBuilder();
            var isHex = false;

            // 检查是否为十六进制
            if (_input[_position] == '0' && _position + 1 < _input.Length &&
                char.ToLower(_input[_position + 1]) == 'x')
            {
                isHex = true;
                sb.Append(_input[_position++]);
                sb.Append(_input[_position++]);
                _column += 2;
            }

            while (_position < _input.Length)
            {
                var ch = _input[_position];
                if (isHex ? char.IsDigit(ch) || (ch >= 'a' && ch <= 'f') || (ch >= 'A' && ch <= 'F')
                         : char.IsDigit(ch))
                {
                    sb.Append(ch);
                    Advance();
                }
                else
                {
                    break;
                }
            }

            var tokenType = isHex ? TokenType.HexNumber : TokenType.Number;
            return new Token(tokenType, sb.ToString(), startLine, startColumn);
        }

        /// <summary>
        /// 读取标识符
        /// </summary>
        private Token ReadIdentifier()
        {
            var startLine = _line;
            var startColumn = _column;
            var sb = new StringBuilder();

            while (_position < _input.Length &&
                   (char.IsLetterOrDigit(_input[_position]) || _input[_position] == '_' ||
                    _input[_position] == '-' || _input[_position] == '.'))
            {
                sb.Append(_input[_position]);
                Advance();
            }

            return new Token(TokenType.Identifier, sb.ToString(), startLine, startColumn);
        }

        /// <summary>
        /// 前进一个字符位置
        /// </summary>
        private void Advance()
        {
            if (_position < _input.Length && _input[_position] == '\n')
            {
                _line++;
                _column = 1;
            }
            else
            {
                _column++;
            }
            _position++;
        }

        /// <summary>
        /// 窥视下一个字符但不前进位置
        /// </summary>
        private char Peek()
        {
            return _position + 1 < _input.Length ? _input[_position + 1] : '\0';
        }
    }
}
