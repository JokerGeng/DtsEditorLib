using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Linq;
using DtsParser.AST;
using DtsParser.Models;

namespace DtsParser
{
    /// <summary>
    /// DTS树的打印工具
    /// </summary>
    public static class DtsPrinter
    {
        /// <summary>
        /// 打印DTS节点树
        /// </summary>
        public static void PrintTree(DtsDocument dtsDocument, int indent = 0)
        {
            var indentStr = new string(' ', indent * 2);
            
            //打印version
            Console.WriteLine($"{indentStr}{dtsDocument.Version}");

            foreach (var item in dtsDocument.Comments)
            {
                // 打印注释
                Console.WriteLine($"{indentStr}{item}");
            }

            // 打印include
            foreach (var include in dtsDocument.Includes)
            {
                Console.WriteLine($"{indentStr}  {include}");
            }

            PrintNode(dtsDocument.RootNode, indent);
        }

        public static void PrintNode(DtsNode node, int indent = 0)
        {
            var indentStr = new string(' ', indent * 2);

            // 打印节点信息
            Console.WriteLine($"{indentStr}{node}");

            // 打印属性
            foreach (var property in node.Properties)
            {
                Console.WriteLine($"{indentStr}  {property}");
            }

            // 递归打印子节点
            foreach (var child in node.Children)
            {
                PrintNode(child, indent + 1);
            }
        }

        /// <summary>
        /// 将DTS树转换为字符串表示
        /// </summary>
        public static string TreeToString(DtsNode node, int indent = 0)
        {
            var sb = new StringBuilder();
            var indentStr = new string(' ', indent * 2);

            // 节点信息
            sb.AppendLine($"{indentStr}{node}");

            // 属性
            foreach (var property in node.Properties)
            {
                sb.AppendLine($"{indentStr}  {property}");
            }

            // 子节点
            foreach (var child in node.Children)
            {
                sb.Append(TreeToString(child, indent + 1));
            }

            return sb.ToString();
        }
    }
}
