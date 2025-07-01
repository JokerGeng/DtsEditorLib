using System;
using System.Collections.Generic;
using System.IO;
using DtsParser;
using DtsParser.AST;

namespace DtsTest
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("=== Parse Start ===");

            var dtsContent = File.ReadAllText("example.dts");

            try
            {
                // 词法分析
                Console.WriteLine("=== Lexical Analysis ===");
                var lexer = new DtsLexer(dtsContent);
                var tokens = lexer.Tokenize();

                // 语法分析
                Console.WriteLine("=== Semantic Analysis ===");
                var parser = new DtsParser.DtsParser(tokens);
                var deviceTree = parser.ParseDocument();

                //Console.WriteLine("=== Print Node Tree ===");
                //DtsPrinter.PrintTree(deviceTree);

                Console.WriteLine("=== Generator dts ===");
                var genertrtor = new DtsGenerator(deviceTree);
                var content = genertrtor.Generate();
                File.WriteAllText("generate1.dts", content);
                Console.WriteLine("=== Parse end ===");

                var node = deviceTree.FindByPath("/amba_apu@0/serial@2000a000");
                var compatible = node.FindProperty("compatible");

                Console.WriteLine("=== Edit dts tree ===");
                var editor = new DtsEditor(deviceTree);
                var newNode = editor.AddNode("/", "my-device", null, "0x1000000");

                var clockValue = new DtsListValue();
                clockValue.Values.Add(new DtsStringValue("snps"));
                clockValue.Values.Add(new DtsStringValue("dw-apb-uart"));
                editor.AddProperty("/my-device@0x1000000", "clock-names", new List<DtsValue>() { clockValue });

                editor.AddProperty("/my-device@0x1000000", "compatible", new List<DtsValue>() { new DtsStringValue("snps,dw-apb-uart") });

                var regValue = new DtsCellValue();
                regValue.Values.Add(new DtsNumberValue(0x1000000, true, 6));
                regValue.Values.Add(new DtsNumberValue(0x1000, true, 4));
                editor.AddProperty("/my-device@0x1000000", "reg", new List<DtsValue>() { regValue });

                var interruptValue = new DtsCellValue();
                interruptValue.Values.Add(new DtsReferenceValue("gic400"));
                editor.BatchEdit(e =>
                {
                    e.AddProperty("/my-device@0x1000000", "interrupt-parent", new List<DtsValue>() { interruptValue });
                    e.AddProperty("/my-device@0x1000000", "status", new List<DtsValue>() { new DtsStringValue("ok") });
                });
                content = genertrtor.Generate();
                File.WriteAllText("generate2.dts", content);
                Console.WriteLine("=== Edit end ===");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"DtsParser Error: {ex.Message}");
            }
            Console.ReadLine();

        }
    }
}
