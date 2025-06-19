using System;
using System.IO;
using DtsEditorLib.Editor;
using DtsEditorLib.Generator;
using DtsEditorLib.Models;
using DtsEditorLib.Parser;
using DtsEditorLib.Validator;

namespace DtsTest
{
    internal class Program
    {
        static void Main(string[] args)
        {
            // 示例1: 解析DTS文件
            Console.WriteLine("=== 解析DTS文件 ===");

            var parser = new DeviceTreeParser();
            var deviceTree = parser.ParseFile("example.dts");

            Console.WriteLine($"解析完成，包含 {deviceTree.Root.Children.Count} 个顶级节点");
            Console.WriteLine($"包含文件: {string.Join(", ", deviceTree.Includes)}");

            Console.WriteLine("\n=== 生成DTS文件 ===");
            var generator = new DeviceTreeGenerator();
            var generatedContent0 = generator.Generate(deviceTree);
            File.WriteAllText("generate.dts", generatedContent0);

            // 示例2: 编辑设备树
            Console.WriteLine("\n=== 编辑设备树 ===");
            var editor = new DeviceTreeEditor(deviceTree);

            // 添加新节点
            var newNode = editor.AddNode("/", "my-device", 0x1000000);
            editor.AddProperty("/my-device", "compatible", "mycompany,my-device");
            editor.AddProperty("/my-device", "reg", new int[] { 0x1000000, 0x1000 }, PropertyValueType.IntegerArray);
            editor.AddProperty("/my-device", "status", "okay");

            // 批量编辑
            editor.BatchEdit(e =>
            {
                e.AddProperty("/my-device", "interrupt-parent", "gic", PropertyValueType.LabelReference);
                e.AddProperty("/my-device", "interrupts", new int[] { 0, 42, 4 }, PropertyValueType.IntegerArray);
            });

            Console.WriteLine("添加了新的设备节点");
            var generatedContent1 = generator.Generate(deviceTree);
            File.WriteAllText("generate_edit.dts", generatedContent1);

            editor.AddProperty("/soc/i2c/eeprom", "eepromtest", "eepromtest");
            editor.AddProperty("/clocks/scpi_clocks", "scpi_clktest", "scpi_clktest");
            editor.UpdateProperty("/clocks/scpi_clocks", "clock-output-names", "cputest");

            // 示例3: 验证设备树
            Console.WriteLine("\n=== 验证设备树 ===");
            var validator = new DeviceTreeValidator();
            var validationResults = validator.Validate(deviceTree);

            if (validationResults.Count == 0)
            {
                Console.WriteLine("验证通过，没有发现问题");
            }
            else
            {
                Console.WriteLine($"发现 {validationResults.Count} 个问题:");
                foreach (var result in validationResults)
                {
                    Console.WriteLine($"  {result}");
                }
            }

            // 示例5: 查询和搜索
            Console.WriteLine("\n=== 查询设备树 ===");

            //按路径查找
            var cpuNode = deviceTree.FindByPath("/soc/gic");
            if (cpuNode != null)
            {
                Console.WriteLine($"找到gic节点: {cpuNode.FullPath}");
            }

            // 按标签查找
            var gicNode = deviceTree.FindByLabel("gic");
            if (gicNode != null)
            {
                Console.WriteLine($"找到GIC节点: {gicNode.FullPath}");
            }

            // 示例6: 高级编辑操作
            Console.WriteLine("\n=== 高级编辑操作 ===");

            // 复制节点
            //var copiedNode = editor.CopyNode("/my-device", "/", "my-device-copy@2000000");
            //if (copiedNode != null)
            //{
            //    editor.UpdateProperty("/my-device-copy", "reg", new int[] { 0x2000000, 0x1000 });
            //    Console.WriteLine("复制并修改了设备节点");
            //};

            // 最终验证
            Console.WriteLine("\n=== 最终验证 ===");
            var finalResults = validator.Validate(deviceTree);
            Console.WriteLine($"最终验证结果: {finalResults.Count} 个问题");

            // 生成最终文件
            generator.GenerateToFile(deviceTree, "final_output.dts");
            Console.WriteLine("已生成最终的 final_output.dts 文件");
            Console.ReadLine();
        }

    }
}
