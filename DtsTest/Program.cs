using System;
using System.IO;
using System.Linq;
using DtsParser;

namespace DtsTest
{
    internal class Program
    {
        static void Main(string[] args)
        {
            // 示例1: 解析DTS文件
            Console.WriteLine("=== 解析DTS文件 ===");

            var dtsContent = File.ReadAllText("example.dts");

            try
            {
                // 词法分析
                var lexer = new DtsLexer(dtsContent);
                var tokens = lexer.Tokenize();

                Console.WriteLine("=== Lexical Analysis ===");
                foreach (var token in tokens.Take(20)) // 显示前20个token
                {
                    Console.WriteLine(token);
                }

                // 语法分析
                var parser = new DtsParser.DtsParser(tokens);
                var deviceTree = parser.Parse();

                Console.WriteLine("\n===DtsParser Parse Tree ===");
                PrintDeviceTree(deviceTree);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"DtsParser Error: {ex.Message}");
            }
            Console.ReadLine();

            #region 

            //var parser = new DeviceTreeParser();
            //var deviceTree = parser.ParseFile("example.dts");

            //Console.WriteLine($"解析完成，包含 {deviceTree.Root.Children.Count} 个顶级节点");
            //Console.WriteLine($"包含文件: {string.Join(", ", deviceTree.Includes)}");

            //Console.WriteLine("\n=== 生成DTS文件 ===");
            //var generator = new DeviceTreeGenerator();
            //var generatedContent0 = generator.Generate(deviceTree);
            //File.WriteAllText("generate_parse.dts", generatedContent0);
            //Console.WriteLine("已生成最终的 generate_parse.dts 文件");
            //Console.ReadLine();

            // 示例2: 编辑设备树
            //Console.WriteLine("\n=== 编辑设备树 ===");
            //var editor = new DeviceTreeEditor(deviceTree);

            //// 添加新节点
            //var newNode = editor.AddNode("/", "my-device", 0x1000000);
            //editor.AddProperty("/my-device", "compatible", "mycompany,my-device");
            //editor.AddProperty("/my-device", "reg", new int[] { 0x1000000, 0x1000 }, PropertyValueType.IntegerArray);
            //editor.AddProperty("/my-device", "status", "okay");

            //// 批量编辑
            //editor.BatchEdit(e =>
            //{
            //    e.AddProperty("/my-device", "interrupt-parent", "gic", PropertyValueType.LabelReference);
            //    e.AddProperty("/my-device", "interrupts", new int[] { 0, 42, 4 }, PropertyValueType.IntegerArray);
            //});


            //editor.AddProperty("/soc/i2c/eeprom", "eepromtest", "eepromtest");
            //editor.AddProperty("/clocks/scpi_clocks", "scpi_clktest", "scpi_clktest");
            //editor.UpdateProperty("/clocks/scpi_clocks", "clock-output-names", "cputest");

            //// 示例3: 验证设备树
            //Console.WriteLine("\n=== 验证设备树 ===");
            //var validator = new DeviceTreeValidator();
            //var validationResults = validator.Validate(deviceTree);

            //if (validationResults.Count == 0)
            //{
            //    Console.WriteLine("验证通过，没有发现问题");
            //}
            //else
            //{
            //    Console.WriteLine($"发现 {validationResults.Count} 个问题:");
            //    foreach (var result in validationResults)
            //    {
            //        Console.WriteLine($"  {result}");
            //    }
            //}

            //// 示例5: 查询和搜索
            //Console.WriteLine("\n=== 查询设备树 ===");

            ////按路径查找
            //var cpuNode = deviceTree.FindByPath("/soc/gic");
            //if (cpuNode != null)
            //{
            //    Console.WriteLine($"找到gic节点: {cpuNode.FullPath}");
            //}

            //// 按标签查找
            //var gicNode = deviceTree.FindByLabel("gic");
            //if (gicNode != null)
            //{
            //    Console.WriteLine($"找到GIC节点: {gicNode.FullPath}");
            //}

            //// 最终验证
            //Console.WriteLine("\n=== 最终验证 ===");
            //var finalResults = validator.Validate(deviceTree);
            //Console.WriteLine($"最终验证结果: {finalResults.Count} 个问题");

            //// 生成最终文件
            //generator.GenerateToFile(deviceTree, "final_output.dts");
            //Console.WriteLine("已生成最终的 final_output.dts 文件");
            //Console.ReadLine();

            #endregion
        }

        static void PrintDeviceTree(DeviceTreeNode deviceTree, int indent = 0)
        {
            var indentStr = new string(' ', indent * 2);

            // 打印预处理指令
            foreach (var include in deviceTree.Includes)
            {
                Console.WriteLine($"{indentStr}#include \"{include.FilePath}\"");
            }
            foreach (var define in deviceTree.Defines)
            {
                Console.WriteLine($"{indentStr}#define {define.Name} {define.Value}");
            }

            if (deviceTree.Includes.Count > 0 || deviceTree.Defines.Count > 0)
                Console.WriteLine();

            // 打印根节点
            PrintNode(deviceTree.RootNode, indent);
        }

        static void PrintNode(DtsNode node, int indent = 0)
        {
            var indentStr = new string(' ', indent * 2);

            // 打印标签
            if (!string.IsNullOrEmpty(node.Label))
            {
                Console.WriteLine($"{indentStr}{node.Label}:");
            }

            // 打印节点名
            Console.WriteLine($"{indentStr}{node.FullName} {{");

            // 打印删除指令
            foreach (var deletedProp in node.DeletedProperties)
            {
                Console.WriteLine($"{indentStr}  /delete-property/ {deletedProp};");
            }

            foreach (var deletedNode in node.DeletedNodes)
            {
                Console.WriteLine($"{indentStr}  /delete-node/ {deletedNode};");
            }

            // 打印属性
            foreach (var property in node.Properties)
            {
                PrintProperty(property, indent + 1);
            }

            // 打印子节点
            foreach (var child in node.Children)
            {
                Console.WriteLine();
                PrintNode(child, indent + 1);
            }

            Console.WriteLine($"{indentStr}}}");
        }

        static void PrintProperty(Property property, int indent)
        {
            var indentStr = new string(' ', indent * 2);

            if (property.Values.Count == 0)
            {
                Console.WriteLine($"{indentStr}{property.Name};");
            }
            else
            {
                Console.Write($"{indentStr}{property.Name} = ");

                if (property.Values.Count == 1)
                {
                    PrintPropertyValue(property.Values[0]);
                }
                else
                {
                    Console.Write("<");
                    for (int i = 0; i < property.Values.Count; i++)
                    {
                        if (i > 0) Console.Write(", ");
                        PrintPropertyValue(property.Values[i]);
                    }
                    Console.Write(">");
                }

                Console.WriteLine(";");
            }
        }

        static void PrintPropertyValue(PropertyValue value)
        {
            switch (value)
            {
                case StringValue str:
                    Console.Write($"\"{str.Value}\"");
                    break;
                case NumberValue num:
                    Console.Write(num.IsHex ? $"0x{num.Value:x}" : num.Value.ToString());
                    break;
                case ReferenceValue reference:
                    Console.Write(reference.Reference);
                    break;
                case ArrayValue array:
                    Console.Write("<");
                    for (int i = 0; i < array.Values.Count; i++)
                    {
                        if (i > 0) Console.Write(", ");
                        PrintPropertyValue(array.Values[i]);
                    }
                    Console.Write(">");
                    break;
            }
        }
    }
}
