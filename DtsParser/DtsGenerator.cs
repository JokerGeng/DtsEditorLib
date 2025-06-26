using System.IO;
using System.Linq;
using System.Text;

namespace DtsParser
{
    public class DtsGenerator
    {
        private int indentLevel = 0;
        private const string IndentString = "\t";
        private readonly DtsDocument dtsDocument;

        public DtsGenerator(DtsDocument dtsDocument)
        {
            this.dtsDocument = dtsDocument;
        }

        public string Generate()
        {
            var sb = new StringBuilder();

            // 添加文件头注释
            if (dtsDocument.Comments.Any())
            {
                foreach (var comment in dtsDocument.Comments)
                {
                    sb.AppendLine(comment);
                }
                sb.AppendLine();
            }

            // 生成版本声明
            sb.AppendLine(dtsDocument.Version);
            sb.AppendLine();

            // 生成包含文件
            foreach (var include in dtsDocument.Includes)
            {
                sb.AppendLine(include.ToString());
            }

            if (dtsDocument.Includes.Any())
                sb.AppendLine();

            // 生成根节点
            GenerateNode(sb, dtsDocument.RootNode, true);

            return sb.ToString();
        }

        public void GenerateToFile(string filePath)
        {
            var content = Generate();
            File.WriteAllText(filePath, content);
        }

        private void GenerateNode(StringBuilder sb, DtsNode node, bool isRoot = false)
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
                // 添加节点名称、标签和单元地址
                sb.Append($"{indent}{node.ToString()}");
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
            var childNodes = node.Children;
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

        private void GenerateProperty(StringBuilder sb, DtsProperty property)
        {
            var indent = GetIndent(indentLevel);
            sb.Append($"{indent}{property.ToString(indent + indent)}");
            sb.AppendLine();
        }

        private string GetIndent(int count)
        {
            return new string('\t', count);
        }
    }
}
