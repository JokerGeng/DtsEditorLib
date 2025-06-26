using System;
using System.IO;
using DtsParser;

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
                //DtsTreePrinter.PrintTree(deviceTree);

                Console.WriteLine("=== Generator dts ===");
                var genertrtor = new DtsGenerator(deviceTree);
                var content = genertrtor.Generate();
                File.WriteAllText("generate.dts", content);
                Console.WriteLine("=== Parse end ===");

                //Console.WriteLine("\n=== Edit dts tree ===");
                //var editor = new DtsEditor(deviceTree);
                //var newNode = editor.AddNode("/", "my-device", 0x1000000);
                //editor.AddProperty("/my-device", "compatible", default);
                //editor.AddProperty("/my-device", "reg", new int[] { 0x1000000, 0x1000 }, PropertyValueType.IntegerArray);
                //editor.AddProperty("/my-device", "status", "okay");

                //editor.BatchEdit(e =>
                //{
                //    e.AddProperty("/my-device", "interrupt-parent", "gic", PropertyValueType.LabelReference);
                //    e.AddProperty("/my-device", "interrupts", new int[] { 0, 42, 4 }, PropertyValueType.IntegerArray);
                //});
            }
            catch (Exception ex)
            {
                Console.WriteLine($"DtsParser Error: {ex.Message}");
            }
            Console.ReadLine();

        }
    }
}
