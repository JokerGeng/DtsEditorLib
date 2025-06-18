using System.IO;
using System.Linq;
using System.Text;
using DtsEditorLib.Models;

namespace DtsEditorLib.Generator
{
    public class DeviceTreeGenerator
    {
        private int indentLevel = 0;
        private const string IndentString = "\t";

        public string Generate(DeviceTree deviceTree)
        {
            var sb = new StringBuilder();

            // 添加文件头注释
            if (deviceTree.Comments.Any())
            {
                foreach (var comment in deviceTree.Comments)
                {
                    sb.AppendLine($"/* {comment} */");
                }
                sb.AppendLine();
            }

            // 生成版本声明
            sb.AppendLine(deviceTree.Version);
            sb.AppendLine();

            // 生成包含文件
            foreach (var include in deviceTree.Includes)
            {
                sb.AppendLine($"#include \"{include}\"");
            }

            if (deviceTree.Includes.Any())
                sb.AppendLine();

            // 生成根节点
            GenerateNode(sb, deviceTree.Root, true);

            return sb.ToString();
        }

        public void GenerateToFile(DeviceTree deviceTree, string filePath)
        {
            var content = Generate(deviceTree);
            File.WriteAllText(filePath, content);
        }

        private void GenerateNode(StringBuilder sb, DeviceTreeNode node, bool isRoot = false)
        {
            if (isRoot)
            {
                sb.AppendLine("/{");
                indentLevel++;
            }

            if (!isRoot)
            {
                // 添加节点标签
                var indent = GetIndent(indentLevel);
                if (!string.IsNullOrEmpty(node.Label))
                {
                    sb.Append($"{indent}{node.Label}: ");
                }
                else
                {
                    sb.Append(indent);
                }

                // 添加节点名称和单元地址
                sb.Append(node.Name);
                if (node.UnitAddress.HasValue)
                {
                    sb.Append($"@{node.UnitAddress.Value:x}");
                }
                sb.AppendLine(" {");
                indentLevel++;
            }

            // 生成属性
            foreach (var property in node.Properties)
            {
                GenerateProperty(sb, property);
            }

            // 添加子节点之间的空行
            if (node.Properties.Any() && node.Children.Any())
            {
                sb.AppendLine();
            }

            // 生成子节点
            var childNodes = node.Children.ToList();//.OrderBy(n => n.Name).ToList();
            for (int i = 0; i < childNodes.Count; i++)
            {
                GenerateNode(sb, childNodes[i]);

                // 在子节点之间添加空行
                if (i < childNodes.Count - 1)
                {
                    sb.AppendLine();
                }
            }

            //if (!isRoot)
            {
                indentLevel--;
                sb.AppendLine($"{GetIndent(indentLevel)}}};");
            }
        }

        private void GenerateProperty(StringBuilder sb, DeviceTreeProperty property)
        {
            var indent = GetIndent(indentLevel);
            sb.Append($"{indent}{property.Name}");

            if (property.ValueType == PropertyValueType.Empty)
            {
                sb.AppendLine(";");
                return;
            }

            sb.Append(" = ");

            switch (property.ValueType)
            {
                case PropertyValueType.String:
                    sb.Append($"{property.Value}");
                    break;

                case PropertyValueType.Integer:
                    sb.Append($"<{property.Value}>");
                    break;

                case PropertyValueType.IntegerArray:
                    var intArray = property.GetIntegerArray();
                    sb.Append($"<{string.Join(" ", intArray)}>;");
                    break;

                case PropertyValueType.MultiIntegerArray:
                    var listArray = property.GetListArray();
                    sb.AppendLine($"<{string.Join(" ", listArray[0].Select(b => "0x" + b.ToString("x2")))}>,");
                    for (int i = 1; i < listArray.Count; i++)
                    {
                        var listValues = listArray[i].Select(b => "0x" + b.ToString("x2"));
                        if (i == listArray.Count - 1)
                        {
                            sb.AppendLine($"{indent}{indent}<{string.Join(" ", listValues)}>;");
                        }
                        else
                        {
                            sb.AppendLine($"{indent}{indent}<{string.Join(" ", listValues)}>,");
                        }
                    }
                    break;

                case PropertyValueType.ByteArray:
                    var byteArray = property.GetByteArray();
                    var hexValues = byteArray.Select(b => b.ToString("X2"));
                    sb.Append($"[{string.Join(" ", hexValues)}]");
                    break;

                case PropertyValueType.LabelReference:
                    sb.Append($"&{property.Value};");
                    break;

                case PropertyValueType.ValueReference:
                    sb.Append($"<&{property.Value}>;");
                    break;

                case PropertyValueType.Boolean:
                    // 布尔属性通常没有值
                    break;

                default:
                    sb.Append(property.RawValue ?? property.Value?.ToString());
                    break;
            }

            sb.AppendLine();
        }

        private string GetIndent(int count)
        {
            return new string('\t', count);
        }
    }
}
