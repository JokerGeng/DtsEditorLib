using System;
using System.IO;
using System.Text;

namespace DtsParser
{
    /// <summary>
    /// DTS文件解析器主类
    /// 提供高级API来解析DTS文件
    /// </summary>
    public class DtsFileParser
    {
        /// <summary>
        /// 从文件路径解析DTS文件
        /// </summary>
        public static DtsNode ParseFile(string filePath)
        {
            if (!File.Exists(filePath))
                throw new FileNotFoundException($"DTS file not found: {filePath}");

            var content = File.ReadAllText(filePath, Encoding.UTF8);
            return ParseContent(content);
        }

        /// <summary>
        /// 从字符串内容解析DTS
        /// </summary>
        public static DtsNode ParseContent(string content)
        {
            try
            {
                // 词法分析
                var lexer = new DtsLexer(content);
                var tokens = lexer.Tokenize();

                // 语法分析
                var parser = new DtsParser(tokens);
                return parser.ParseDocument().RootNode;
            }
            catch (Exception ex)
            {
                throw new ParseException($"Failed to parse DTS content: {ex.Message}");
            }
        }
    }
}
