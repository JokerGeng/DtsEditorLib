﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace DtsParser
{
    // 词法分析器
    public class DtsLexer
    {
        private readonly string _source;
        private readonly List<Token> _tokens;
        private int _start = 0;
        private int _current = 0;
        private int _line = 1;
        private int _column = 1;

        public DtsLexer(string source)
        {
            _source = source ?? throw new ArgumentNullException(nameof(source));
            _tokens = new List<Token>();
        }

        /// <summary>
        /// 获取所有tokens
        /// </summary>
        public List<Token> Tokenize()
        {
            while (!IsAtEnd())
            {
                _start = _current;
                ScanToken();
            }

            _tokens.Add(new Token(TokenType.EOF, "", _line, _column));
            return _tokens;
        }

        private void ScanToken()
        {
            char c = Advance();

            switch (c)
            {
                case ' ':
                case '\r':
                case '\t':
                    _column++;
                    break;

                case '\n':
                    AddToken(TokenType.Newline);
                    _line++;
                    _column = 1;
                    break;

                case '/':
                    if (Match('/'))
                    {
                        // 单行注释
                        while (Peek() != '\n' && !IsAtEnd())
                            Advance();
                    }
                    else if (Match('*'))
                    {
                        // 多行注释
                        ScanBlockComment();
                    }
                    else
                    {
                        AddToken(TokenType.Slash);
                    }
                    break;

                case '{': AddToken(TokenType.LeftBrace); break;
                case '}': AddToken(TokenType.RightBrace); break;
                case '(': AddToken(TokenType.LeftParen); break;
                case ')': AddToken(TokenType.RightParen); break;
                case '<':
                    if (Match('<'))
                    {
                        AddToken(TokenType.LeftShift); // <<
                    }
                    else
                    {
                        AddToken(TokenType.LeftAngle); // <
                    }
                    break;
                case '>':
                    if (Match('>'))
                    {
                        AddToken(TokenType.RightShift); // >>
                    }
                    else
                    {
                        AddToken(TokenType.RightAngle); // >
                    }
                    break;
                case '[': AddToken(TokenType.LeftBracket); break;
                case ']': AddToken(TokenType.RightBracket); break;
                case ',': AddToken(TokenType.Comma); break;
                case ';': AddToken(TokenType.Semicolon); break;
                case ':': AddToken(TokenType.Colon); break;
                case '=': AddToken(TokenType.Equals); break;
                case '&':
                    if (Match('&'))
                    {
                        AddToken(TokenType.LogicalAnd); // &&
                    }
                    else
                    {
                        AddToken(TokenType.Ampersand); // &
                    }
                    break;
                case '-': AddToken(TokenType.Minus); break;
                case '+': AddToken(TokenType.Plus); break;
                case '.': AddToken(TokenType.Dot); break;
                case '|':
                    if (Match('|'))
                    {
                        AddToken(TokenType.LogicalOr); // ||
                    }
                    else
                    {
                        AddToken(TokenType.Pipe); // |
                    }
                    break;
                case '^': AddToken(TokenType.Caret); break;
                case '~': AddToken(TokenType.Tilde); break;

                case '#':
                    ScanPreprocessorDirective();
                    break;

                case '"':
                    ScanString();
                    break;

                case '\'':
                    ScanCharacter();
                    break;

                default:
                    if (IsDigit(c))
                    {
                        ScanNumber();
                    }
                    else if (IsAlpha(c) || c == '_')
                    {
                        ScanIdentifier();
                    }
                    else
                    {
                        throw new ParseException($"Unexpected character: {c}", _line);
                    }
                    break;
            }
        }

        private void ScanNumber()
        {
            // 检查十六进制 (0x 或 0X)
            if (_source[_start] == '0' && (Peek() == 'x' || Peek() == 'X'))
            {
                Advance(); // consume 'x' or 'X'
                while (IsHexDigit(Peek()))
                {
                    Advance();
                }
                AddToken(TokenType.HexNumber);
                return;
            }

            // 十进制数字
            while (IsDigit(Peek()))
            {
                Advance();
            }

            AddToken(TokenType.Number);
        }

        private void ScanIdentifier()
        {
            while (IsAlphaNumeric(Peek()) || Peek() == '_' || Peek() == '-' || Peek() == '@')
            {
                Advance();
            }

            var text = _source.Substring(_start, _current - _start);
            AddToken(TokenType.Identifier, text);
        }

        private void ScanPreprocessorDirective()
        {
            while (IsAlpha(Peek()))
            {
                Advance();
            }

            var directive = _source.Substring(_start, _current - _start);

            if (directive == "#include")
            {
                AddToken(TokenType.Include, directive);

                while (Peek() == ' ' || Peek() == '\t')
                {
                    Advance();
                }

                if (Peek() == '<')
                {
                    Advance();
                    AddToken(TokenType.LeftAngle, "<");
                    ScanIncludePath();

                    if (Peek() != '>')
                    {
                        throw new ParseException("Expected '>' after include path", _line);
                    }

                    Advance();
                    AddToken(TokenType.RightAngle, ">");
                }
                else if (Peek() == '"')
                {
                    ScanString();
                }
            }
            else
            {
                AddToken(TokenType.Preprocessor, directive);
            }
        }

        private void ScanIncludePath()
        {
            _start = _current;

            while (Peek() != '>' && !IsAtEnd())
            {
                char c = Peek();

                if (c == '/')
                {
                    if (_current > _start)
                    {
                        var identifier = _source.Substring(_start, _current - _start);
                        AddToken(TokenType.Identifier, identifier);
                    }

                    Advance();
                    AddToken(TokenType.Slash, "/");
                    _start = _current;
                }
                else if (c == '.')
                {
                    if (_current > _start)
                    {
                        var identifier = _source.Substring(_start, _current - _start);
                        AddToken(TokenType.Identifier, identifier);
                    }

                    Advance();
                    AddToken(TokenType.Dot, ".");
                    _start = _current;
                }
                else if (IsAlphaNumeric(c) || c == '_' || c == '-')
                {
                    Advance();
                }
                else
                {
                    break;
                }
            }

            if (_current > _start)
            {
                var identifier = _source.Substring(_start, _current - _start);
                AddToken(TokenType.Identifier, identifier);
            }
        }

        // 辅助方法
        private bool IsAtEnd() => _current >= _source.Length;
        private char Advance() => _source[_current++];
        private bool Match(char expected)
        {
            if (IsAtEnd()) return false;
            if (_source[_current] != expected) return false;
            _current++;
            return true;
        }
        private char Peek() => IsAtEnd() ? '\0' : _source[_current];
        private char PeekNext() => _current + 1 >= _source.Length ? '\0' : _source[_current + 1];
        private bool IsDigit(char c) => c >= '0' && c <= '9';
        private bool IsAlpha(char c) => (c >= 'a' && c <= 'z') || (c >= 'A' && c <= 'Z') || c == '_';
        private bool IsAlphaNumeric(char c) => IsAlpha(c) || IsDigit(c);
        private bool IsHexDigit(char c) => IsDigit(c) || (c >= 'a' && c <= 'f') || (c >= 'A' && c <= 'F');

        private void AddToken(TokenType type, string text = null)
        {
            text = text ?? _source.Substring(_start, _current - _start);
            _tokens.Add(new Token(type, text, _line, _column));
            _column += text.Length;
        }

        private void ScanString()
        {
            while (Peek() != '"' && !IsAtEnd())
            {
                if (Peek() == '\n') _line++;
                Advance();
            }

            if (IsAtEnd())
            {
                throw new ParseException("Unterminated string", _line);
            }

            Advance(); // consume closing "

            var value = _source.Substring(_start + 1, _current - _start - 2);
            AddToken(TokenType.String, value);
        }

        private void ScanCharacter()
        {
            while (Peek() != '\'' && !IsAtEnd())
            {
                if (Peek() == '\n') _line++;
                Advance();
            }

            if (IsAtEnd())
            {
                throw new ParseException("Unterminated character literal", _line);
            }

            Advance(); // consume closing '

            var value = _source.Substring(_start + 1, _current - _start - 2);
            AddToken(TokenType.Character, value);
        }

        private void ScanBlockComment()
        {
            while (!IsAtEnd())
            {
                if (Peek() == '*' && PeekNext() == '/')
                {
                    Advance();
                    Advance();
                    break;
                }

                if (Peek() == '\n')
                {
                    _line++;
                    _column = 0;
                }

                Advance();
            }
        }
    }
}
