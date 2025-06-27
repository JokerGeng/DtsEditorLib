using System;
using System.Collections.Generic;
using System.Text;
using DtsParser.AST;
using DtsParser.Models;

namespace DtsParser
{
    /// <summary>
    /// DTS语法分析器
    /// 负责将Token序列解析为抽象语法树
    /// </summary>
    public class DtsParser
    {
        private readonly List<Token> _tokens;
        private int _position;

        public DtsParser(List<Token> tokens)
        {
            _tokens = tokens ?? throw new ArgumentNullException(nameof(tokens));
            _position = 0;
        }

        public DtsDocument ParseDocument()
        {
            var document = new DtsDocument();
            ParseHeaderComment(document);
            while (!IsAtEnd())
            {
                SkipNewlines();

                if (IsAtEnd())
                    break;

                if (Check(TokenType.Slash) && PeekNext()?.Type == TokenType.Identifier && PeekNext()?.Value.Contains("dts-v") == true)
                {
                    document.Version = ParseVersionDirective();
                }
                else if (Check(TokenType.Include))
                {
                    document.AddInclude(ParseIncludeDirective());
                }
                else if (Check(TokenType.Slash))
                {
                    document.RootNode = ParseNode();
                    break;
                }
                else
                {
                    Advance(); // Skip unexpected tokens
                }
            }

            return document;
        }

        private string ParseVersionDirective()
        {
            var sb = new StringBuilder();

            Consume(TokenType.Slash, "Expected '/'");
            sb.Append("/");

            var identifier = Consume(TokenType.Identifier, "Expected 'dts-v1'");
            if (identifier.Value != "dts-v1")
            {
                throw new ParseException("Expected 'dts-v1'", identifier.Line);
            }
            sb.Append(identifier.Value);

            Consume(TokenType.Slash, "Expected '/' after 'dts-v1'");
            sb.Append("/");

            Consume(TokenType.Semicolon, "Expected ';' after version declaration");
            sb.Append(";");

            return sb.ToString();
        }

        public DtsIncludeDirective ParseIncludeDirective()
        {
            var includeLine = Peek().Line;
            Consume(TokenType.Include, "Expected '#include'");

            bool isSystemInclude = false;
            string path = "";

            if (Check(TokenType.LeftAngle))
            {
                isSystemInclude = true;
                Advance(); // consume '<'

                var pathBuilder = new StringBuilder();
                while (!Check(TokenType.RightAngle) && !IsAtEnd())
                {
                    if (Check(TokenType.Identifier))
                    {
                        pathBuilder.Append(Advance().Value);
                    }
                    else if (Check(TokenType.Slash))
                    {
                        pathBuilder.Append(Advance().Value);
                    }
                    else if (Check(TokenType.Dot))
                    {
                        pathBuilder.Append(Advance().Value);
                    }
                    else if (Check(TokenType.Minus))
                    {
                        pathBuilder.Append(Advance().Value);
                    }
                    else if (Check(TokenType.Comma))
                    {
                        pathBuilder.Append(Advance().Value);
                    }
                    else
                    {
                        break;
                    }
                }

                path = pathBuilder.ToString();
                Consume(TokenType.RightAngle, "Expected '>' after include path");
            }
            else if (Check(TokenType.String))
            {
                isSystemInclude = false;
                path = Advance().Value;
            }
            else
            {
                throw new ParseException("Expected '<' or '\"' after #include", Peek().Line);
            }

            return new DtsIncludeDirective(path, isSystemInclude, includeLine);
        }

        private void ParseHeaderComment(DtsDocument document)
        {
            while (Check(TokenType.Comment) || Check(TokenType.Newline))
            {
                var curr = Peek();
                if (Check(TokenType.Comment))
                {
                    document.Comments.Add(curr.Value);
                    Consume(TokenType.Comment, "Expected comment");
                }
                else
                {
                    Consume(TokenType.Newline, "Expected newLine");
                }
            }
        }

        private DtsNode ParseNode()
        {
            var name = "/";
            string label = null;

            if (Check(TokenType.Slash))
            {
                Advance(); // consume '/'
            }
            else if (Check(TokenType.Identifier))
            {
                name = Advance().Value;

                if (Check(TokenType.Colon))
                {
                    Advance(); // consume ':'
                    label = name;
                    name = Consume(TokenType.Identifier, "Expected identifier after ':'").Value;
                }

                if (Check(TokenType.At))
                {
                    Advance(); // consume '@'
                    name += "@" + Consume(TokenType.Identifier, "Expected identifier after '@'").Value;
                }
            }

            var node = new DtsNode(name, Peek().Line, label);

            Consume(TokenType.LeftBrace, "Expected '{' after node name");
            SkipNewlines();

            while (!Check(TokenType.RightBrace) && !IsAtEnd())
            {
                SkipNewlines();

                if (Check(TokenType.RightBrace))
                    break;

                if (Check(TokenType.Identifier))
                {
                    var nextToken = PeekNext();
                    if (nextToken?.Type == TokenType.LeftBrace ||
                        nextToken?.Type == TokenType.Colon ||
                        nextToken?.Type == TokenType.At)
                    {
                        var childNode = ParseNode();
                        childNode.Parent = node;
                        node.AddChild(childNode);
                    }
                    else if (nextToken?.Type == TokenType.Equals || nextToken?.Type == TokenType.Comma)
                    {
                        node.AddProperty(ParseProperty());
                    }
                    else
                    {
                        // maybe property not have value
                        var propName = Advance().Value;
                        node.AddProperty(new DtsProperty(propName));

                        if (Check(TokenType.Semicolon))
                        {
                            Advance();
                        }
                    }
                }
                else
                {
                    Advance(); // Skip unexpected tokens
                }

                SkipNewlines();
            }

            Consume(TokenType.RightBrace, "Expected '}' after node body");

            if (Check(TokenType.Semicolon))
            {
                Advance(); // consume optional semicolon
            }

            return node;
        }

        private DtsProperty ParseProperty()
        {
            var name = Consume(TokenType.Identifier, "Expected property name").Value;
            SkipNewlines();
            while (!Check(TokenType.Equals))
            {
                Consume(TokenType.Comma, "Expected ',' after property name");
                SkipNewlines();
                var othName = Consume(TokenType.Identifier, "Expected property name after ','").Value;
                name += "," + othName;
                SkipNewlines();
                if (Check(TokenType.Semicolon))
                {
                    //no property value
                    return new DtsProperty(name);
                }
            }
            Consume(TokenType.Equals, "Expected '=' after property name");

            var values = new List<DtsValue>();

            // Skip newlines after '='
            SkipNewlines();

            do
            {
                SkipNewlines();
                //每一对<>一个值
                var value = ParsePropertyValue();
                if (value != null)
                {
                    values.Add(value);
                }
                SkipNewlines();
            }
            while (Match(TokenType.Comma));

            Consume(TokenType.Semicolon, "Expected ';' after property");

            var property = new DtsProperty(name);
            property.Values.AddRange(values);
            return property;
        }

        private DtsValue ParsePropertyValue()
        {
            SkipNewlines();
            if (Check(TokenType.String))
            {
                //string and string array
                return ParseString();
            }
            else if (Check(TokenType.LeftAngle))
            {
                //<>
                return ParseCellArrayValue();
            }
            else if (Check(TokenType.Ampersand))
            {
                //&
                return ParseReferenceValue();
            }
            else if (Check(TokenType.Comment))
            {
                Advance();
                return ParsePropertyValue();
            }
            else if (Check(TokenType.Bits) && PeekNext().Type == TokenType.Number)
            {
                //bits
                return ParseBitsLineValue();
            }
            else if (Check(TokenType.LeftBracket))
            {
                //[]
                return ParseBracketValue();
            }
            else
            {
                throw new ParseException("UnSupport token type", Peek().Line);
            }
        }

        private DtsValue ParseString()
        {
            SkipNewlines();
            var valueStr = Consume(TokenType.String, "Expected string value").Value;
            DtsValue stringValue;
            stringValue = new DtsStringValue(valueStr);
            SkipNewlines();
            //avoid line breaks in the property for string array
            if (Peek().Type == TokenType.Comma)
            {
                var dtsArrayValue = new DtsArrayStringValue();
                dtsArrayValue.Values.Add(new DtsStringValue(valueStr));
                while (Check(TokenType.Semicolon) == false)
                {
                    SkipNewlines();
                    Consume(TokenType.Comma, "Expected ','");
                    var value = ParseStringValue();
                    dtsArrayValue.Values.Add(value);
                    SkipNewlines();
                }
                stringValue = dtsArrayValue;
            }
            return stringValue;
        }

        private DtsArrayValue ParseCellArrayValue()
        {
            Consume(TokenType.LeftAngle, "Expected '<'");

            var valueTemp = new DtsArrayValue();
            SkipNewlines();
            while (!Check(TokenType.RightAngle) && !IsAtEnd())
            {
                SkipNewlines();
                DtsValue childValue;
                if (Check(TokenType.Ampersand))
                {
                    childValue = ParseReferenceValue();
                }
                else if (Check(TokenType.HexNumber))
                {
                    childValue = ParseHexNumberValue();
                }
                else if (Check(TokenType.Number))
                {
                    childValue = ParseNumberValue();
                }
                else if (Check(TokenType.Identifier))
                {
                    //identify
                    //ragard as string value
                    childValue = ParseMultiLine();
                }
                else if (Check(TokenType.Comment))
                {
                    //是否保存注释
                    Advance();
                    SkipNewlines();
                    continue;
                }
                else
                {
                    throw new ParseException("Expected property value", Peek().Line);
                }
                SkipNewlines();
                valueTemp.Values.Add(childValue);
            }
            Consume(TokenType.RightAngle, "Expected '>'");
            return valueTemp;
        }

        private DtsBitsValue ParseBitsValue()
        {
            var token = Advance();
            if (token.Type == TokenType.Number &&
                (token.Value == "8" || token.Value == "16" ||
                token.Value == "32" || token.Value == "64"))
            {
                var value = Convert.ToUInt16(token.Value);
                return new DtsBitsValue(value);
            }
            throw new ParseException("Expected number after /bits/: must be 8, 16, 32, or 64");

        }

        private DtsValue ParseReferenceValue()
        {
            Consume(TokenType.Ampersand, "Expected '&'");
            var refName = Consume(TokenType.Identifier, "Expected reference name").Value;
            return new DtsReferenceValue(refName);
        }

        private DtsValue ParseBitsLineValue()
        {
            Consume(TokenType.Bits, "Expected 'bits'");
            DtsBitsValue bitsValue = ParseBitsValue();
            while (!Check(TokenType.Semicolon))
            {
                SkipNewlines();
                DtsArrayValue value = ParseCellArrayValue();
                bitsValue.Values.Add(value);
                if (Peek().Type == TokenType.Comma)
                {
                    Consume(TokenType.Comma, "Expected ','");
                }
                SkipNewlines();
            }
            return bitsValue;
        }

        //mac-address property
        // byte array
        private DtsByteArrayValue ParseBracketValue()
        {
            Consume(TokenType.LeftBracket, "Expected '['");

            var valueTemp = new DtsByteArrayValue();
            SkipNewlines();
            while (!Check(TokenType.RightBracket))
            {
                SkipNewlines();
                if (Check(TokenType.Number) || Check(TokenType.HexNumber) || Check(TokenType.Identifier))
                {
                    var stringValue = Consume(Peek().Type, "Expected mac address value").Value;
                    valueTemp.Values.Add(stringValue);
                }
                else
                {
                    throw new ParseException("Expected byte array", Peek().Line);
                }
                SkipNewlines();
            }
            Consume(TokenType.RightBracket, "Expected ']'");
            return valueTemp;
        }


        private DtsValue ParseMultiLine()
        {
            StringBuilder sb = new StringBuilder();
            while (!Check(TokenType.RightAngle))
            {
                SkipNewlines();
                var value = Advance().Value;
                sb.Append(" ");
                sb.Append(value);
                SkipNewlines();
            }
            var dtsValue = new DtsCellStringValue(sb.ToString());
            return dtsValue;
        }

        private DtsValue ParseStringValue()
        {
            var stringValue = Consume(TokenType.String, "Expected string value").Value;
            return new DtsStringValue(stringValue);
        }


        private DtsValue ParseNumberValue()
        {
            var value = Convert.ToUInt64(Consume(TokenType.Number, "Expected number").Value);
            return new DtsNumberValue(value);
        }

        private DtsValue ParseHexNumberValue()
        {
            var valueStr = Consume(TokenType.HexNumber, "Expected hex number").Value.Substring(2);
            var value = Convert.ToUInt64(valueStr, 16);
            return new DtsNumberValue(value, true, valueStr.Length);
        }

        #region [aiding method]

        private void SkipNewlines()
        {
            while (Match(TokenType.Newline))
            {
                // 继续跳过
            }
        }

        // 辅助方法
        private bool Match(params TokenType[] types)
        {
            foreach (var type in types)
            {
                if (Check(type))
                {
                    Advance();
                    return true;
                }
            }
            return false;
        }

        private bool Check(TokenType type)
        {
            if (IsAtEnd()) return false;
            return Peek().Type == type;
        }

        private Token Advance()
        {
            if (!IsAtEnd()) _position++;
            return Previous();
        }

        private bool IsAtEnd()
        {
            return Peek().Type == TokenType.EOF;
        }

        private Token Peek()
        {
            return _tokens[_position];
        }

        private Token Previous()
        {
            return _tokens[_position - 1];
        }

        private Token Consume(TokenType type, string message)
        {
            if (type != TokenType.Newline)
            {
                SkipNewlines();
            }
            if (Check(type)) return Advance();
            message += $" on line:{Peek().Line}";
            throw new ParseException(message, Peek().Line);
        }

        private Token PeekNext()
        {
            if (_position + 1 >= _tokens.Count)
                return null;
            return _tokens[_position + 1];
        }

        #endregion[aiding method]

    }
}
