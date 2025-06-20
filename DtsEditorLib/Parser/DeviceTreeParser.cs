using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using DtsEditorLib.Exceptions;
using DtsEditorLib.Models;

namespace DtsEditorLib.Parser
{
    public class DeviceTreeParser
    {
        private static readonly Regex NodeRegex = new Regex(@"^\s*(?:(\w+)\s*:\s*)?([\w\-]+)(@[0-9a-fA-F,]+)?\s*\{?\s*$");
        private static readonly Regex SingleNodeRegex = new Regex(@"^\s*(\w+)\s*:\s*([\w\-]+)(@[0-9a-fA-F,]+)?\s*\{\s*\}\s*;?\s*$");
        //除多行值之外,基本将属性值作为string类型处理,后续优化
        private static readonly Regex PropertyRegex = new Regex(@"^(#?\w+[-?,?\w]*)\s*(=\s*(.+))?\s*;?$");
        private static readonly Regex IncludeRegex = new Regex(@"#include\s+[""<]([^"">]+)["">\s*]");
        private static readonly Regex LabelRegex = new Regex(@"^(\w+):\s*(.+)$");
        //private static readonly Regex SharpPropertyRegex = new Regex(@"^\s*(#\w[\w\-]*)\s*=\s*(<[^>]*>)[\s;]*");

        public DeviceTree ParseFile(string filePath)
        {
            try
            {
                var content = File.ReadAllText(filePath);
                return Parse(content, Path.GetFileName(filePath));
            }
            catch (Exception ex)
            {
                throw new DeviceTreeParseException($"Failed to parse file '{filePath}': {ex.Message}", ex);
            }
        }

        public DeviceTree Parse(string content, string fileName = null)
        {
            var deviceTree = new DeviceTree { FileName = fileName };
            var lines = content.Split('\n');
            PreprocessHeaderCommonts(deviceTree, lines);
            var context = new ParseContext
            {
                Lines = PreprocessLinesRecursive(lines, 0, new List<ProcessedLine>(), false),
                DeviceTree = deviceTree,
                CurrentIndex = 0
            };

            // 递归解析根节点内容
            ParseNodeContent(deviceTree.Root, context);

            return deviceTree;
        }

        private void PreprocessHeaderCommonts(DeviceTree deviceTree, string[] lines)
        {
            var index = 0;
            var line = lines[index];
            while (line != "/{" && line.StartsWith("#") == false)
            {
                deviceTree.Comments.Add(line);
                index++;
                line = lines[index];
            }
        }

        // 递归预处理行（处理注释）
        private List<ProcessedLine> PreprocessLinesRecursive(string[] lines, int index, List<ProcessedLine> result, bool inMultiLineComment)
        {
            if (index >= lines.Length)
                return result;

            var line = lines[index].Trim();

            if (string.IsNullOrEmpty(line))
            {
                return PreprocessLinesRecursive(lines, index + 1, result, inMultiLineComment);
            }

            // 处理多行注释开始
            if (line.StartsWith("/*"))
            {
                inMultiLineComment = true;
                if (line.EndsWith("*/"))
                {
                    inMultiLineComment = false;
                }
                return PreprocessLinesRecursive(lines, index + 1, result, inMultiLineComment);
            }

            // 处理多行注释结束
            if (inMultiLineComment)
            {
                if (line.EndsWith("*/"))
                {
                    inMultiLineComment = false;
                }
                return PreprocessLinesRecursive(lines, index + 1, result, inMultiLineComment);
            }

            // 跳过单行注释
            if (line.StartsWith("//"))
            {
                return PreprocessLinesRecursive(lines, index + 1, result, inMultiLineComment);
            }

            // 移除行尾注释
            var commentIndex = line.IndexOf("//");
            if (commentIndex >= 0)
            {
                line = line.Substring(0, commentIndex).Trim();
            }

            if (!string.IsNullOrEmpty(line))
            {
                result.Add(new ProcessedLine { Content = line, LineNumber = index + 1 });
            }

            return PreprocessLinesRecursive(lines, index + 1, result, inMultiLineComment);
        }


        // 相应的修改ParseNodeContent方法
        private void ParseNodeContent(DeviceTreeNode currentNode, ParseContext context)
        {
            try
            {
                if (context.CurrentIndex >= context.Lines.Count)
                    return;

                var line = context.Lines[context.CurrentIndex];
                var contentLine = line.Content;

                // 处理版本声明
                if (contentLine.StartsWith("/dts-v"))
                {
                    context.DeviceTree.Version = contentLine;
                    context.CurrentIndex++;
                    ParseNodeContent(currentNode, context); // 递归继续处理
                }

                // 处理包含文件
                var includeMatch = IncludeRegex.Match(contentLine);
                if (includeMatch.Success)
                {
                    context.DeviceTree.Includes.Add(includeMatch.Groups[1].Value);
                    context.CurrentIndex++;
                    ParseNodeContent(currentNode, context); // 递归继续处理
                    return;
                }

                // 处理节点结束
                if (contentLine.StartsWith("}"))
                {
                    context.CurrentIndex++;
                    return; // 递归返回
                }

                // 处理标签
                var labelMatch = LabelRegex.Match(contentLine);
                //if (labelMatch.Success)
                //{
                //    var labelName = labelMatch.Groups[1].Value;
                //    contentLine = labelMatch.Groups[2].Value;
                //}

                // 处理节点开始
                var singleNodeMatch = SingleNodeRegex.Match(contentLine);
                if (singleNodeMatch.Success)
                {
                    var childNode = ParseNodeDeclaration(singleNodeMatch, line.LineNumber, labelMatch, context.DeviceTree);
                    currentNode.AddChild(childNode);

                    context.CurrentIndex++;
                    ParseNodeContent(currentNode, context); // 递归继续处理同级节点
                    return;
                }

                var nodeMatch = NodeRegex.Match(contentLine);
                if (nodeMatch.Success)
                {
                    var childNode = ParseNodeDeclaration(nodeMatch, line.LineNumber, labelMatch, context.DeviceTree);
                    currentNode.AddChild(childNode);

                    context.CurrentIndex++;

                    if (contentLine.EndsWith("{"))
                    {
                        ParseNodeContent(childNode, context); // 递归解析子节点
                    }

                    ParseNodeContent(currentNode, context); // 递归继续处理同级节点
                    return;
                }

                // 处理属性
                var propertyMatch = PropertyRegex.Match(contentLine);
                if (propertyMatch.Success)
                {
                    var property = ParseProperty(propertyMatch.Groups[1].Value,
                                               propertyMatch.Groups[3].Value,
                                               line.LineNumber);
                    MultiLineProperty(context, property);
                    currentNode.AddProperty(property);
                    context.CurrentIndex++;
                    ParseNodeContent(currentNode, context); // 递归继续处理
                    return;
                }

                // 跳过无法识别的行
                context.CurrentIndex++;
                ParseNodeContent(currentNode, context); // 递归继续处理
            }
            catch (Exception ex)
            {
                Console.WriteLine(context.Lines[context.CurrentIndex].ToString());
                throw ex;
            }
            
        }

        // 解析节点声明
        private DeviceTreeNode ParseNodeDeclaration(Match nodeMatch, int lineNumber, Match labelMatch, DeviceTree deviceTree)
        {
            var nodeName = nodeMatch.Groups[2].Value;
            var address = nodeMatch.Groups[3].Success ?
                         Convert.ToUInt32(nodeMatch.Groups[3].Value.TrimStart('@'), 16) : (uint?)null;

            var node = new DeviceTreeNode(nodeName)
            {
                UnitAddress = address,
                LineNumber = lineNumber
            };

            // 处理标签
            if (labelMatch.Success)
            {
                deviceTree.AddLabel(labelMatch.Groups[1].Value, node);
            }

            return node;
        }

        private void MultiLineProperty(ParseContext context, DeviceTreeProperty property)
        {
            int count = context.CurrentIndex + 1;
            var contentLine = context.Lines[count].Content;
            while (contentLine.StartsWith("<") && (contentLine.EndsWith(">,") || contentLine.EndsWith(">;")))
            {
                var numbers = contentLine.Substring(1, contentLine.Length - 3)
                  .Split(new[] { ' ', '\t', ',' }, StringSplitOptions.RemoveEmptyEntries)
                  .Select(s => ParseInteger(s.Trim()))
                  .ToArray();
                if (property.Value is List<int[]>)
                {
                    ((List<int[]>)property.Value).Add(numbers);
                }
                count++;
                contentLine = context.Lines[count].Content;
            }
            context.CurrentIndex = count - 1;
        }

        private DeviceTreeProperty ParseProperty(string name, string value, int lineNumber)
        {
            var property = new DeviceTreeProperty(name)
            {
                LineNumber = lineNumber,
                RawValue = value
            };

            if (string.IsNullOrEmpty(value))
            {
                property.ValueType = PropertyValueType.Empty;
                return property;
            }

            // 解析字符串值
            if (value.StartsWith("\"") && value.EndsWith("\""))
            {
                property.ValueType = PropertyValueType.String;
                property.Value = value.Substring(1, value.Length - 2);
                return property;
            }

            // 解析引用
            if (value.StartsWith("&"))
            {
                property.ValueType = PropertyValueType.LabelReference;
                property.Value = value.Substring(1, value.Length - 2);
                return property;
            }

            // 解析引用
            if (value.StartsWith("<&") && value.EndsWith(">;"))
            {
                property.ValueType = PropertyValueType.ValueReference;
                property.Value = value.Substring(2, value.Length - 4);
                return property;
            }

            // 解析整数数组
            if (value.StartsWith("<") && value.EndsWith(">"))
            {
                var numbers = value.Substring(1, value.Length - 2)
                    .Split(new[] { ' ', '\t', ',' }, StringSplitOptions.RemoveEmptyEntries)
                    .Select(s => ParseInteger(s.Trim()))
                    .ToArray();

                property.ValueType = PropertyValueType.IntegerArray;
                property.Value = numbers;
                return property;
            }

            //解析整数数组
            if (value.StartsWith("<") && value.EndsWith(">,"))
            {
                var numbers = value.Substring(1, value.Length - 3)
                    .Split(new[] { ' ', '\t', ',' }, StringSplitOptions.RemoveEmptyEntries)
                    .Select(s => ParseInteger(s.Trim()))
                    .ToArray();

                property.ValueType = PropertyValueType.MultiIntegerArray;
                property.Value = new List<int[]>() { numbers };
                return property;
            }

            // 解析字节数组
            if (value.StartsWith("[") && value.EndsWith("]"))
            {
                var bytes = value.Substring(1, value.Length - 2)
                    .Split(new[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries)
                    .Select(s => Convert.ToByte(s.Trim(), 16))
                    .ToArray();

                property.ValueType = PropertyValueType.ByteArray;
                property.Value = bytes;
                return property;
            }

            // 解析单个整数
            if (int.TryParse(value, out int intValue) || value.StartsWith("0x"))
            {
                property.ValueType = PropertyValueType.Integer;
                property.Value = ParseInteger(value);
                return property;
            }

            // 默认作为字符串处理
            property.ValueType = PropertyValueType.String;
            property.Value = value;
            return property;
        }

        private int ParseInteger(string value)
        {
            if (value.StartsWith("0x"))
                return Convert.ToInt32(value, 16);
            return int.Parse(value);
        }

        private string RemoveComments(string line)
        {
            var commentIndex = line.IndexOf("//");
            if (commentIndex >= 0)
                return line.Substring(0, commentIndex).Trim();

            commentIndex = line.IndexOf("/*");
            if (commentIndex >= 0)
                return line.Substring(0, commentIndex).Trim();

            return line;
        }
    }
}
