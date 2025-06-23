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
                        var childNode= ParseNode();
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

        // <summary>
        /// 解析属性值
        /// </summary>
        private DtsPropertyValue ParsePropertyValue()
        {
            SkipNewlines();
            if (Check(TokenType.String))
            {
                return ParseString();
            }
            else if (Check(TokenType.LeftAngle) || Check(TokenType.String))
            {
                return ParseCellArray();
            }
            else if (Check(TokenType.Ampersand))
            {
                return ParseReference();
            }
            else
            {
                // 解析单个表达式
                var expr = ParseExpression();
                return new DtsPropertyValue(DtsPropertyValueType.Expression, expr);
            }
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
                while (Check(TokenType.Semicolon)==false)
                {
                    Consume(TokenType.Comma, "Expected ','");
                    var value = new DtsStringValue(Consume(TokenType.String, "Expected string value").Value);
                    dtsArrayValue.Values.Add(value);
                }
                dtsPropertyValue = new DtsPropertyValue(DtsPropertyValueType.Array, dtsArrayValue);
            }
            return dtsPropertyValue;
        }

        private DtsPropertyValue ParseCellArray()
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
                    var stringValue = Advance().Value;
                    childValue = new DtsStringValue(stringValue);
                }
                else if (Check(TokenType.Ampersand))
                {
                    childValue = ParseReferenceValue();
                }
                else if (Check(TokenType.HexNumber))
                {
                    var value = Convert.ToUInt64(Consume(TokenType.HexNumber, "Expected hex number").Value.Substring(2), 16);
                    childValue = new DtsNumberValue(value, true);
                }
                else if (Check(TokenType.Number))
                {
                    var value = Convert.ToUInt64(Consume(TokenType.Number, "Expected number").Value);
                    childValue = new DtsNumberValue(value);
                }
                else if (Check(TokenType.Identifier))
                {
                    //identify
                    //ragard as string value
                    childValue = new DtsStringValue("");
                }
                else
                {
                    throw new ParseException("Expected property value", Peek().Line);
                }
                SkipNewlines();
                valueTemp.Values.Add(childValue);
            }
            Consume(TokenType.RightAngle, "Expected '>'");
            return new DtsPropertyValue(DtsPropertyValueType.Array, valueTemp);
        }

        private DtsReferenceValue ParseReferenceValue()
        {
            Consume(TokenType.Ampersand, "Expected '&'");
            var refName = Consume(TokenType.Identifier, "Expected reference name").Value;
            return new DtsReferenceValue(refName);
        }

        private DtsPropertyValue ParseArrayValue()
        {
            Consume(TokenType.LeftAngle, "Expected '<'");

            var values = new List<DtsPropertyValue>();

            // Skip newlines at the beginning
            SkipNewlines();

            while (!Check(TokenType.RightAngle) && !IsAtEnd())
            {
                // Skip newlines between values
                SkipNewlines();

                if (Check(TokenType.RightAngle))
                    break;

                var expr = ParseExpression();
                values.Add(new DtsPropertyValue(DtsPropertyValueType.Expression, expr));

                // Skip trailing newlines and whitespace
                SkipNewlines();

                // Check if we have more values (no comma needed in DTS arrays)
                if (Check(TokenType.RightAngle))
                    break;
            }

            Consume(TokenType.RightAngle, "Expected '>'");
            return new DtsPropertyValue(DtsPropertyValueType.Array, values);
        }

        /// <summary>
        /// 解析表达式（支持复杂的位运算和函数调用）
        /// </summary>
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

        /// <summary>
        /// 跳过换行符
        /// </summary>
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
            if (Check(type)) return Advance();
            throw new ParseException(message, Peek().Line);
        }

        private Token PeekNext()
        {
            if (_position + 1 >= _tokens.Count)
                return null;
            return _tokens[_position + 1];
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
                values.Add(ParsePropertyValue());
                SkipNewlines();
            }
            while (Match(TokenType.Comma));

            Consume(TokenType.Semicolon, "Expected ';' after property");

            return new DtsProperty(name, values);
        }

        private DtsPropertyValue ParseReference()
        {
            Consume(TokenType.Ampersand, "Expected '&'");
            var refName = Consume(TokenType.Identifier, "Expected reference name").Value;
            return new DtsPropertyValue(DtsPropertyValueType.Reference, "&" + refName);
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

    }
}
