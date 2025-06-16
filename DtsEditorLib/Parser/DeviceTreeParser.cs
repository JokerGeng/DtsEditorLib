using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using DtsEditorLib.Models;
using System.Text.RegularExpressions;
using DtsEditorLib.Exceptions;

namespace DtsEditorLib.Parser
{
    public class DeviceTreeParser
    {
        private readonly Regex _nodeRegex = new Regex(@"^(?:(\w+):\s*)?(\w+)(?:@([0-9a-fA-F]+))?\s*\{");
        private readonly Regex _propertyRegex = new Regex(@"^(\w+)(?:\s*=\s*(.+?))?;");
        private readonly Regex _includeRegex = new Regex(@"^#include\s+[""<]([^"">]+)["">\s]");

        public DeviceTreeDoc ParseFile(string filePath)
        {
            try
            {
                var content = File.ReadAllText(filePath);
                return ParseContent(content, Path.GetDirectoryName(filePath));
            }
            catch (Exception ex)
            {
                throw new DeviceTreeParseException($"Failed to parse file '{filePath}': {ex.Message}", ex);
            }
        }

        public DeviceTreeDoc ParseContent(string content, string basePath = null)
        {
            var deviceTree = new DeviceTreeDoc();
            var lines = content.Split('\n').Select((line, index) => new { Content = line.Trim(), LineNumber = index + 1 }).ToArray();
            var currentNode = deviceTree.Root;
            var nodeStack = new Stack<DeviceTreeNode>();

            for (int i = 0; i < lines.Length; i++)
            {
                var line = lines[i];
                var content_line = line.Content;

                // 跳过空行和注释
                if (string.IsNullOrWhiteSpace(content_line) || content_line.StartsWith("//"))
                    continue;

                try
                {
                    // 处理版本声明
                    if (content_line.StartsWith("/dts-v"))
                    {
                        deviceTree.Version = content_line;
                        continue;
                    }

                    // 处理包含文件
                    var includeMatch = _includeRegex.Match(content_line);
                    if (includeMatch.Success)
                    {
                        var includePath = includeMatch.Groups[1].Value;
                        deviceTree.Includes.Add(includePath);

                        // 如果提供了基路径，递归解析包含的文件
                        if (!string.IsNullOrEmpty(basePath))
                        {
                            var fullIncludePath = Path.Combine(basePath, includePath);
                            if (File.Exists(fullIncludePath))
                            {
                                var includedTree = ParseFile(fullIncludePath);
                                MergeDeviceTrees(deviceTree, includedTree);
                            }
                        }
                        continue;
                    }

                    // 处理节点开始
                    var nodeMatch = _nodeRegex.Match(content_line);
                    if (nodeMatch.Success)
                    {
                        var label = nodeMatch.Groups[1].Success ? nodeMatch.Groups[1].Value : null;
                        var nodeName = nodeMatch.Groups[2].Value;
                        var address = nodeMatch.Groups[3].Success ?
                            Convert.ToUInt32(nodeMatch.Groups[3].Value, 16) : (uint?)null;

                        nodeStack.Push(currentNode);
                        currentNode = currentNode.AddChild(nodeName, address, label);

                        if (!string.IsNullOrEmpty(label))
                        {
                            deviceTree.Labels[label] = currentNode;
                        }
                        continue;
                    }

                    // 处理节点结束
                    if (content_line == "}")
                    {
                        if (nodeStack.Count > 0)
                        {
                            currentNode = nodeStack.Pop();
                        }
                        continue;
                    }

                    // 处理属性
                    var propertyMatch = _propertyRegex.Match(content_line);
                    if (propertyMatch.Success)
                    {
                        var propName = propertyMatch.Groups[1].Value;
                        var propValue = propertyMatch.Groups[2].Success ? propertyMatch.Groups[2].Value.Trim() : null;

                        var property = ParseProperty(propName, propValue);
                        currentNode.Properties[propName] = property;
                        continue;
                    }
                }
                catch (Exception ex)
                {
                    throw new DeviceTreeParseException($"Parse error at line {line.LineNumber}: {ex.Message}", ex);
                }
            }

            return deviceTree;
        }

        private DeviceTreeProperty ParseProperty(string name, string value)
        {
            var property = new DeviceTreeProperty
            {
                Name = name,
                RawValue = value
            };

            if (string.IsNullOrEmpty(value))
            {
                property.ValueType = PropertyValueType.Empty;
                return property;
            }

            // 字符串值
            if (value.StartsWith("\"") && value.EndsWith("\""))
            {
                property.ValueType = PropertyValueType.String;
                property.Value = value.Substring(1, value.Length - 2);
                return property;
            }

            // 引用值
            if (value.StartsWith("<&") && value.EndsWith(">"))
            {
                property.ValueType = PropertyValueType.Reference;
                property.Value = value.Substring(2, value.Length - 3);
                return property;
            }

            // 整数或整数数组
            if (value.StartsWith("<") && value.EndsWith(">"))
            {
                var intContent = value.Substring(1, value.Length - 2).Trim();
                var intValues = intContent.Split(new[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries)
                                        .Select(s => ParseInteger(s))
                                        .ToArray();

                if (intValues.Length == 1)
                {
                    property.ValueType = PropertyValueType.Integer;
                    property.Value = intValues[0];
                }
                else
                {
                    property.ValueType = PropertyValueType.IntegerArray;
                    property.Value = intValues;
                }
                return property;
            }

            // 字节数组
            if (value.StartsWith("[") && value.EndsWith("]"))
            {
                var byteContent = value.Substring(1, value.Length - 2).Trim();
                var byteValues = byteContent.Split(new[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries)
                                          .Select(s => Convert.ToByte(s, 16))
                                          .ToArray();

                property.ValueType = PropertyValueType.ByteArray;
                property.Value = byteValues;
                return property;
            }

            // 默认作为字符串处理
            property.ValueType = PropertyValueType.String;
            property.Value = value;
            return property;
        }

        private int ParseInteger(string value)
        {
            if (value.StartsWith("0x") || value.StartsWith("0X"))
                return Convert.ToInt32(value, 16);
            return Convert.ToInt32(value);
        }

        private void MergeDeviceTrees(DeviceTreeDoc target, DeviceTreeDoc source)
        {
            // 简单合并逻辑，实际实现可能需要更复杂的合并策略
            foreach (var include in source.Includes)
            {
                if (!target.Includes.Contains(include))
                    target.Includes.Add(include);
            }

            foreach (var label in source.Labels)
            {
                target.Labels[label.Key] = label.Value;
            }
        }
    }
