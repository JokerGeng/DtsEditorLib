using System;
using System.Collections.Generic;
using System.Text;

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
                    name += ":" + Consume(TokenType.Identifier, "Expected identifier after ':'").Value;
                }

                if (Check(TokenType.At))
                {
                    Advance(); // consume '@'
                    name += "@" + Consume(TokenType.Identifier, "Expected identifier after '@'").Value;
                }
            }

            var node = new DtsNode(name);

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
                        // 子节点
                        var childNode = ParseNode();
                        childNode.Parent = node;
                        node.AddChild(childNode);
                    }
                    else if (nextToken?.Type == TokenType.Equals)
                    {
                        // 属性
                        node.AddProperty(ParseProperty());
                    }
                    else
                    {
                        // 可能是没有值的属性
                        var propName = Advance().Value;
                        node.AddProperty(new DtsProperty(propName, new List<DtsPropertyValue>()));

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
            Consume(TokenType.Equals, "Expected '=' after property name");

            var values = new List<DtsPropertyValue>();

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

            return new DtsProperty(name, values);
        }

        private DtsPropertyValue ParsePropertyValue()
        {
            SkipNewlines();
            if (Check(TokenType.String))
            {
                return ParseString();
            }
            else if (Check(TokenType.LeftAngle))
            {
                return ParseCellArray();
            }
            else if (Check(TokenType.Ampersand))
            {
                return ParseReference();
            }
            else if (Check(TokenType.Comment))
            {
                Advance();
                return ParsePropertyValue();
            }
            else if (Check(TokenType.Bits) && PeekNext().Type == TokenType.Number)
            {
                return ParseBitsLineValue();
            }
            else if (Check(TokenType.LeftBracket))
            {
                return ParseBracket();
            }
            else
            {
                // 解析单个表达式
                var expr = ParseExpression();
                return new DtsPropertyValue(DtsPropertyValueType.Expression, expr);
            }
        }

        private DtsPropertyValue ParseBitsLineValue()
        {
            Consume(TokenType.Bits, "Expected 'bits'");
            var valueTemp = new DtsArrayValue();
            valueTemp.Values.Add(ParseBitsValue());
            while (!Check(TokenType.Semicolon))
            {
                SkipNewlines();
                var value = ParseCellArrayValue();
                valueTemp.Values.AddRange(value.Values);
                if (Peek().Type != TokenType.Comma)
                {
                    break;
                }
                SkipNewlines();
            }
            return new DtsPropertyValue(DtsPropertyValueType.Array, valueTemp);
        }

        private DtsPropertyValue ParseString()
        {
            SkipNewlines();
            var stringValue = Consume(TokenType.String, "Expected string value").Value;
            DtsPropertyValue dtsPropertyValue = new DtsPropertyValue(DtsPropertyValueType.String, stringValue);
            if (Peek().Type == TokenType.Comma)
            {
                var dtsArrayValue = new DtsArrayValue();
                dtsArrayValue.Values.Add(new DtsStringValue(stringValue));
                while (Check(TokenType.Semicolon) == false)
                {
                    SkipNewlines();
                    Consume(TokenType.Comma, "Expected ','");
                    var value = ParseStringValue();
                    dtsArrayValue.Values.Add(value);
                    SkipNewlines();
                }
                dtsPropertyValue = new DtsPropertyValue(DtsPropertyValueType.Array, dtsArrayValue);
            }
            return dtsPropertyValue;
        }

        private DtsPropertyValue ParseCellArray()
        {
            return new DtsPropertyValue(DtsPropertyValueType.Array, ParseCellArrayValue());
        }

        private DtsPropertyValue ParseReference()
        {
            return new DtsPropertyValue(DtsPropertyValueType.Reference, ParseReferenceValue());
        }

        private DtsPropertyValue ParseBracket()
        {
            return new DtsPropertyValue(DtsPropertyValueType.Bracket, ParseBracketValue());
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
                if (Check(TokenType.String))
                {
                    childValue = ParseStringValue();
                }
                else if (Check(TokenType.Ampersand))
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
                    childValue = new DtsStringValue("");
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
                if (Check(TokenType.Number) || Check(TokenType.Identifier))
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

        private DtsValue ParseReferenceValue()
        {
            Consume(TokenType.Ampersand, "Expected '&'");
            var refName = Consume(TokenType.Identifier, "Expected reference name").Value;
            return new DtsReferenceValue(refName);
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
            var dtsValue = new DtsStringValue(sb.ToString());
            return dtsValue;
        }

        private DtsValue ParseBitsValue()
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
            var value = Convert.ToUInt64(Consume(TokenType.HexNumber, "Expected hex number").Value.Substring(2), 16);
            return new DtsNumberValue(value, true);
        }

        private DtsExpression ParseExpression()
        {
            return ParseLogicalOr();
        }

        /// <summary>
        /// 解析逻辑或表达式 (||)
        /// </summary>
        private DtsExpression ParseLogicalOr()
        {
            var expr = ParseLogicalAnd();

            while (Match(TokenType.LogicalOr))
            {
                var op = Previous().Value;
                var right = ParseLogicalAnd();
                expr = new DtsBinaryExpression(expr, op, right);
            }

            return expr;
        }

        /// <summary>
        /// 解析逻辑与表达式 (&&)
        /// </summary>
        private DtsExpression ParseLogicalAnd()
        {
            var expr = ParseBitwiseOr();

            while (Match(TokenType.LogicalAnd))
            {
                var op = Previous().Value;
                var right = ParseBitwiseOr();
                expr = new DtsBinaryExpression(expr, op, right);
            }

            return expr;
        }

        /// <summary>
        /// 解析位或表达式 (|)
        /// </summary>
        private DtsExpression ParseBitwiseOr()
        {
            var expr = ParseBitwiseXor();

            while (Match(TokenType.Pipe))
            {
                var op = Previous().Value;
                var right = ParseBitwiseXor();
                expr = new DtsBinaryExpression(expr, op, right);
            }

            return expr;
        }

        /// <summary>
        /// 解析位异或表达式 (^)
        /// </summary>
        private DtsExpression ParseBitwiseXor()
        {
            var expr = ParseBitwiseAnd();

            while (Match(TokenType.Caret))
            {
                var op = Previous().Value;
                var right = ParseBitwiseAnd();
                expr = new DtsBinaryExpression(expr, op, right);
            }

            return expr;
        }

        /// <summary>
        /// 解析位与表达式 (&)
        /// </summary>
        private DtsExpression ParseBitwiseAnd()
        {
            var expr = ParseShift();

            while (Match(TokenType.Ampersand))
            {
                var op = Previous().Value;
                var right = ParseShift();
                expr = new DtsBinaryExpression(expr, op, right);
            }

            return expr;
        }

        /// <summary>
        /// 解析位移表达式 (<< >>)
        /// </summary>
        private DtsExpression ParseShift()
        {
            var expr = ParseAddition();

            while (Match(TokenType.LeftShift, TokenType.RightShift))
            {
                var op = Previous().Value;
                var right = ParseAddition();
                expr = new DtsBinaryExpression(expr, op, right);
            }

            return expr;
        }

        /// <summary>
        /// 解析加减表达式 (+ -)
        /// </summary>
        private DtsExpression ParseAddition()
        {
            var expr = ParseUnary();

            while (Match(TokenType.Plus, TokenType.Minus))
            {
                var op = Previous().Value;
                var right = ParseUnary();
                expr = new DtsBinaryExpression(expr, op, right);
            }

            return expr;
        }

        /// <summary>
        /// 解析一元表达式 (- ~ !)
        /// </summary>
        private DtsExpression ParseUnary()
        {
            if (Match(TokenType.Minus, TokenType.Tilde))
            {
                var op = Previous().Value;
                var expr = ParseUnary();
                return new DtsUnaryExpression(op, expr);
            }

            return ParsePrimary();
        }

        /// <summary>
        /// 解析基本表达式（数字、标识符、函数调用、括号表达式）
        /// </summary>
        private DtsExpression ParsePrimary()
        {
            // 数字
            if (Match(TokenType.Number))
            {
                return new DtsNumberExpression(Previous().Value, false);
            }

            // 十六进制数字
            if (Match(TokenType.HexNumber))
            {
                return new DtsNumberExpression(Previous().Value, true);
            }

            // 标识符或函数调用
            if (Match(TokenType.Identifier))
            {
                var name = Previous().Value;

                // 检查是否为函数调用
                if (Check(TokenType.LeftParen))
                {
                    return ParseFunctionCall(name);
                }
                else
                {
                    return new DtsIdentifierExpression(name);
                }
            }

            // 括号表达式
            if (Match(TokenType.LeftParen))
            {
                var expr = ParseExpression();
                Consume(TokenType.RightParen, "Expected ')' after expression");
                return expr;
            }

            throw new ParseException($"Unexpected token: {Peek().Value}", Peek().Line);
        }

        /// <summary>
        /// 解析函数调用
        /// </summary>
        private DtsFunctionCallExpression ParseFunctionCall(string functionName)
        {
            Consume(TokenType.LeftParen, "Expected '(' after function name");

            var arguments = new List<DtsExpression>();

            if (!Check(TokenType.RightParen))
            {
                do
                {
                    // Skip newlines in function arguments
                    SkipNewlines();

                    arguments.Add(ParseExpression());

                    SkipNewlines();
                }
                while (Match(TokenType.Comma));
            }

            Consume(TokenType.RightParen, "Expected ')' after function arguments");
            return new DtsFunctionCallExpression(functionName, arguments);
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
