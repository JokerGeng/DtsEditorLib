using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using DtsEditorLib.Models;

namespace DtsEditorLib.Generator
{
    public class DeviceTreeGenerator
    {
        private readonly StringBuilder _output;
        private int _indentLevel;
        private const string INDENT = "\t";

        public DeviceTreeGenerator()
        {
            _output = new StringBuilder();
            _indentLevel = 0;
        }

        public string GenerateDeviceTree(DeviceTreeDoc deviceTree)
        {
            _output.Clear();
            _indentLevel = 0;

            // 生成版本声明
            _output.AppendLine(deviceTree.Version);
            _output.AppendLine();

            // 生成包含文件
            foreach (var include in deviceTree.Includes)
            {
                _output.AppendLine($"#include \"{include}\"");
            }
            if (deviceTree.Includes.Any())
            {
                _output.AppendLine();
            }

            // 生成根节点
            GenerateNode(deviceTree.Root, isRoot: true);

            return _output.ToString();
        }

        private void GenerateNode(DeviceTreeNode node, bool isRoot = false)
        {
            if (!isRoot)
            {
                WriteIndent();

                // 写入标签（如果有）
                if (!string.IsNullOrEmpty(node.Label))
                {
                    _output.Append($"{node.Label}: ");
                }

                // 写入节点名和地址
                _output.Append(node.Name);
                if (node.Address.HasValue)
                {
                    _output.Append($"@{node.Address.Value:x}");
                }

                _output.AppendLine(" {");
                _indentLevel++;
            }

            // 生成属性
            foreach (var property in node.Properties.Values.OrderBy(p => p.Name))
            {
                WriteIndent();
                _output.AppendLine(property.ToString());
            }

            // 在属性和子节点之间添加空行
            if (node.Properties.Any() && node.Children.Any())
            {
                _output.AppendLine();
            }

            // 生成子节点
            foreach (var child in node.Children.Values.OrderBy(c => c.Name))
            {
                GenerateNode(child);
            }

            if (!isRoot)
            {
                _indentLevel--;
                WriteIndent();
                _output.AppendLine("};");

                // 在同级节点之间添加空行
                if (_indentLevel > 0)
                {
                    _output.AppendLine();
                }
            }
        }

        private void WriteIndent()
        {
            for (int i = 0; i < _indentLevel; i++)
            {
                _output.Append(INDENT);
            }
        }

        public void SaveToFile(DeviceTreeDoc deviceTree, string filePath)
        {
            var content = GenerateDeviceTree(deviceTree);
            File.WriteAllText(filePath, content);
        }
    }
}
