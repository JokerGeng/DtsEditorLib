using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

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

        /// <summary>
        /// 解析DTS文件，返回根节点
        /// </summary>
        public DtsNode Parse()
        {
            var root = new DtsNode("/", null);

            while (!IsAtEnd())
            {
                if (Check(TokenType.Newline))
                {
                    Advance();
                    continue;
                }

                // 处理预处理指令
                if (Check(TokenType.Include))
                {
                    ParseIncludeDirective();
                    continue;
                }

                // 解析节点或属性
                var item = ParseTopLevelItem();
                if (item is DtsNode node)
                {
                    root.AddChild(node);
                }
                else if (item is DtsProperty property)
                {
                    root.AddProperty(property);
                }
            }

            return root;
        }

        /// <summary>
        /// 解析顶层项目（节点或属性）
        /// </summary>
        private object ParseTopLevelItem()
        {
            // 检查是否有标签
            string label = null;
            if (Check(TokenType.Identifier) && CheckNext(TokenType.Identifier) && Peek().Value == ":")
            {
                label = Advance().Value;
                Consume(TokenType.Identifier, "Expected ':' after label");
            }

            if (Check(TokenType.Identifier))
            {
                var name = Advance().Value;

                // 如果下一个token是'{'，这是一个节点
                if (Check(TokenType.LeftBrace))
                {
                    return ParseNode(name, label);
                }
                // 如果下一个token是'='或';'，这是一个属性
                else if (Check(TokenType.Equals) || Check(TokenType.Semicolon))
                {
                    return ParseProperty(name);
                }
            }

            throw new ParseException($"Unexpected token: {Current()}", Current().Line);
        }

        /// <summary>
        /// 解析节点
        /// </summary>
        private DtsNode ParseNode(string name, string label = null)
        {
            var node = new DtsNode(name, label)
            {
                Line = Previous().Line
            };

            Consume(TokenType.LeftBrace, "Expected '{' after node name");

            // 解析节点内容
            while (!Check(TokenType.RightBrace) && !IsAtEnd())
            {
                if (Check(TokenType.Newline))
                {
                    Advance();
                    continue;
                }

                // 检查是否有标签
                string childLabel = null;
                if (Check(TokenType.Identifier) && CheckNext(TokenType.Identifier) && PeekNext().Value == ":")
                {
                    childLabel = Advance().Value;
                    Consume(TokenType.Identifier, "Expected ':' after label");
                }

                if (Check(TokenType.Identifier))
                {
                    var itemName = Advance().Value;

                    if (Check(TokenType.LeftBrace))
                    {
                        // 子节点
                        var childNode = ParseNode(itemName, childLabel);
                        node.AddChild(childNode);
                    }
                    else if (Check(TokenType.Equals) || Check(TokenType.Semicolon))
                    {
                        // 属性
                        var property = ParseProperty(itemName);
                        node.AddProperty(property);
                    }
                    else
                    {
                        throw new ParseException($"Unexpected token after identifier: {Current()}", Current().Line);
                    }
                }
                else
                {
                    throw new ParseException($"Expected identifier in node body: {Current()}", Current().Line);
                }
            }

            Consume(TokenType.RightBrace, "Expected '}' after node body");

            // 节点定义后可能有分号
            if (Check(TokenType.Semicolon))
            {
                Advance();
            }

            return node;
        }

        /// <summary>
        /// 解析属性
        /// </summary>
        private DtsProperty ParseProperty(string name)
        {
            var line = Previous().Line;
            DtsValue value = null;

            if (Check(TokenType.Equals))
            {
                Advance(); // consume '='
                value = ParseValue();
            }

            Consume(TokenType.Semicolon, "Expected ';' after property");
            return new DtsProperty(name, value, line);
        }

        /// <summary>
        /// 解析属性值
        /// </summary>
        private DtsValue ParseValue()
        {
            // 字符串值
            if (Check(TokenType.String))
            {
                return new DtsStringValue(Advance().Value);
            }

            // 数字值
            if (Check(TokenType.Number))
            {
                var token = Advance();
                return new DtsNumberValue(long.Parse(token.Value));
            }

            // 十六进制数字值
            if (Check(TokenType.HexNumber))
            {
                var token = Advance();
                var hexValue = token.Value.Substring(2); // 去掉 "0x" 前缀
                return new DtsNumberValue(Convert.ToInt64(hexValue, 16), true);
            }

            // 引用值
            if (Check(TokenType.Ampersand))
            {
                Advance(); // consume '&'
                var reference = Consume(TokenType.Identifier, "Expected identifier after '&'").Value;
                return new DtsReferenceValue(reference);
            }

            // 数组值
            if (Check(TokenType.LeftAngle))
            {
                return ParseArrayValue();
            }

            // 标识符（可能是引用或枚举值）
            if (Check(TokenType.Identifier))
            {
                return new DtsStringValue(Advance().Value);
            }

            throw new ParseException($"Unexpected token in value: {Current()}", Current().Line);
        }

        /// <summary>
        /// 解析数组值
        /// </summary>
        private DtsArrayValue ParseArrayValue()
        {
            var array = new DtsArrayValue();

            Consume(TokenType.LeftAngle, "Expected '<'");

            while (!Check(TokenType.RightAngle) && !IsAtEnd())
            {
                array.Values.Add(ParseValue());

                if (Check(TokenType.Comma))
                {
                    Advance();
                }
                else if (!Check(TokenType.RightAngle))
                {
                    break;
                }
            }

            Consume(TokenType.RightAngle, "Expected '>' after array values");
            return array;
        }

        /// <summary>
        /// 解析include指令
        /// </summary>
        private void ParseIncludeDirective()
        {
            Consume(TokenType.Include, "Expected #include");

            if (Check(TokenType.String))
            {
                var includePath = Advance().Value;
                // 这里可以实现include文件的处理逻辑
                Console.WriteLine($"Include directive: {includePath}");
            }
            else if (Check(TokenType.LeftAngle))
            {
                Advance(); // consume '<'
                var includePath = Consume(TokenType.Identifier, "Expected include path").Value;
                Consume(TokenType.RightAngle, "Expected '>' after include path");
                Console.WriteLine($"Include directive: <{includePath}>");
            }
        }

        #region 辅助方法

        /// <summary>
        /// 检查当前token是否为指定类型
        /// </summary>
        private bool Check(TokenType type)
        {
            if (IsAtEnd()) return false;
            return Current().Type == type;
        }

        /// <summary>
        /// 检查下一个token是否为指定类型
        /// </summary>
        private bool CheckNext(TokenType type)
        {
            if (_position + 1 >= _tokens.Count) return false;
            return _tokens[_position + 1].Type == type;
        }

        /// <summary>
        /// 前进并返回当前token
        /// </summary>
        private Token Advance()
        {
            if (!IsAtEnd()) _position++;
            return Previous();
        }

        /// <summary>
        /// 检查是否到达token列表末尾
        /// </summary>
        private bool IsAtEnd()
        {
            return Current().Type == TokenType.EOF;
        }

        /// <summary>
        /// 获取当前token
        /// </summary>
        private Token Current()
        {
            return _tokens[_position];
        }

        /// <summary>
        /// 获取前一个token
        /// </summary>
        private Token Previous()
        {
            return _tokens[_position - 1];
        }

        /// <summary>
        /// 窥视下一个token
        /// </summary>
        private Token Peek()
        {
            if (_position + 1 >= _tokens.Count)
                return new Token(TokenType.EOF, "", 0, 0);
            return _tokens[_position + 1];
        }

        /// <summary>
        /// 窥视下下个token
        /// </summary>
        private Token PeekNext()
        {
            if (_position + 2 >= _tokens.Count)
                return new Token(TokenType.EOF, "", 0, 0);
            return _tokens[_position + 2];
        }

        /// <summary>
        /// 消费指定类型的token，如果不匹配则抛出异常
        /// </summary>
        private Token Consume(TokenType type, string message)
        {
            if (Check(type))
                return Advance();

            throw new ParseException($"{message}. Got: {Current()}", Current().Line);
        }

        #endregion
    }
}
