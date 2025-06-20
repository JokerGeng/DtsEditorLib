using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace DtsParser
{
    public class DtsParser
    {
        private readonly List<Token> _tokens;
        private int _position;
        private Token CurrentToken => _position < _tokens.Count ? _tokens[_position] : null;

        public DtsParser(List<Token> tokens)
        {
            _tokens = tokens;
            _position = 0;
        }

        public DeviceTreeNode Parse()
        {
            var deviceTree = new DeviceTreeNode();

            // 解析预处理指令
            while (CurrentToken?.Type == TokenType.INCLUDE || CurrentToken?.Type == TokenType.DEFINE)
            {
                if (CurrentToken.Type == TokenType.INCLUDE)
                {
                    deviceTree.Includes.Add(ParseInclude());
                }
                else if (CurrentToken.Type == TokenType.DEFINE)
                {
                    deviceTree.Defines.Add(ParseDefine());
                }
            }

            // 解析根节点
            deviceTree.RootNode = ParseNode();

            return deviceTree;
        }

        private IncludeDirective ParseInclude()
        {
            var include = new IncludeDirective();
            var token = Consume(TokenType.INCLUDE);

            // 从#include token的值中提取文件路径
            var match = Regex.Match(token.Value, @"#include\s*[<""]([^>""]+)[>""]");
            if (match.Success)
            {
                include.FilePath = match.Groups[1].Value;
            }

            return include;
        }

        private DefineDirective ParseDefine()
        {
            var define = new DefineDirective();
            var token = Consume(TokenType.DEFINE);

            // 从#define token的值中提取名称和值
            var match = Regex.Match(token.Value, @"#define\s+(\w+)(?:\s+(.*))?");
            if (match.Success)
            {
                define.Name = match.Groups[1].Value;
                define.Value = match.Groups.Count > 2 ? match.Groups[2].Value : "";
            }

            return define;
        }

        private DtsNode ParseNode()
        {
            var node = new DtsNode();

            // 解析标签（可选）
            if (CurrentToken?.Type == TokenType.LABEL)
            {
                var labelToken = Consume(TokenType.LABEL);
                node.Label = labelToken.Value.TrimEnd(':');
            }

            // 解析节点名
            if (CurrentToken?.Type == TokenType.IDENTIFIER)
            {
                var nameToken = Consume(TokenType.IDENTIFIER);
                var nameParts = nameToken.Value.Split('@');
                node.Name = nameParts[0];
                if (nameParts.Length > 1)
                {
                    node.UnitAddress = nameParts[1];
                }
            }
            else if (CurrentToken?.Type == TokenType.SLASH)
            {
                // 根节点
                Consume(TokenType.SLASH);
                node.Name = "/";
            }

            Consume(TokenType.LBRACE);

            // 解析节点内容
            while (CurrentToken?.Type != TokenType.RBRACE && CurrentToken?.Type != TokenType.EOF)
            {
                if (CurrentToken.Type == TokenType.DELETE_NODE)
                {
                    Consume(TokenType.DELETE_NODE);
                    var nodeToDelete = Consume(TokenType.IDENTIFIER).Value;
                    node.DeletedNodes.Add(nodeToDelete);
                    Consume(TokenType.SEMICOLON);
                }
                else if (CurrentToken.Type == TokenType.DELETE_PROP)
                {
                    Consume(TokenType.DELETE_PROP);
                    var propToDelete = Consume(TokenType.IDENTIFIER).Value;
                    node.DeletedProperties.Add(propToDelete);
                    Consume(TokenType.SEMICOLON);
                }
                else if (IsNodeStart())
                {
                    node.Children.Add(ParseNode());
                }
                else
                {
                    node.Properties.Add(ParseProperty());
                }
            }

            Consume(TokenType.RBRACE);

            // 可选的分号
            if (CurrentToken?.Type == TokenType.SEMICOLON)
            {
                Consume(TokenType.SEMICOLON);
            }

            return node;
        }

        private bool IsNodeStart()
        {
            var currentPos = _position;

            // 检查是否是标签
            if (CurrentToken?.Type == TokenType.LABEL)
                currentPos++;

            // 检查后面是否跟着标识符或根节点标识
            if (currentPos < _tokens.Count)
            {
                var nextToken = _tokens[currentPos];
                return nextToken.Type == TokenType.IDENTIFIER || nextToken.Type == TokenType.SLASH;
            }

            return false;
        }

        private Property ParseProperty()
        {
            var property = new Property();

            property.Name = Consume(TokenType.IDENTIFIER).Value;

            if (CurrentToken?.Type == TokenType.EQUALS)
            {
                Consume(TokenType.EQUALS);
                property.Values = ParsePropertyValues();
            }

            Consume(TokenType.SEMICOLON);

            return property;
        }

        private List<PropertyValue> ParsePropertyValues()
        {
            var values = new List<PropertyValue>();

            if (CurrentToken?.Type == TokenType.LANGLE)
            {
                // 解析数组 <value1, value2, ...>
                Consume(TokenType.LANGLE);

                if (CurrentToken?.Type != TokenType.RANGLE)
                {
                    values.Add(ParseSinglePropertyValue());

                    while (CurrentToken?.Type == TokenType.COMMA)
                    {
                        Consume(TokenType.COMMA);
                        values.Add(ParseSinglePropertyValue());
                    }
                }

                Consume(TokenType.RANGLE);
            }
            else
            {
                // 单个值
                values.Add(ParseSinglePropertyValue());
            }

            return values;
        }

        private PropertyValue ParseSinglePropertyValue()
        {
            if (CurrentToken?.Type == TokenType.STRING)
            {
                var token = Consume(TokenType.STRING);
                return new StringValue { Value = token.Value.Trim('"') };
            }
            else if (CurrentToken?.Type == TokenType.NUMBER)
            {
                var token = Consume(TokenType.NUMBER);
                var isHex = token.Value.StartsWith("0x");
                var value = isHex ? Convert.ToInt64(token.Value, 16) : Convert.ToInt64(token.Value);
                return new NumberValue { Value = value, IsHex = isHex };
            }
            else if (CurrentToken?.Type == TokenType.REFERENCE)
            {
                var token = Consume(TokenType.REFERENCE);
                return new ReferenceValue { Reference = token.Value };
            }

            throw new ParseException($"Unexpected token: {CurrentToken}");
        }

        private Token Consume(TokenType expectedType)
        {
            if (CurrentToken?.Type != expectedType)
            {
                throw new ParseException($"Expected {expectedType}, got {CurrentToken?.Type}");
            }

            var token = CurrentToken;
            _position++;
            return token;
        }
    }
}
