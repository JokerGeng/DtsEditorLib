using System.IO;
using System.Linq;
using System.Text;
using DtsParser.AST;
using DtsParser.Models;

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

            // generate version
            if (!string.IsNullOrWhiteSpace(dtsDocument.Version))
            {
                sb.AppendLine(dtsDocument.Version);
                sb.AppendLine();
            }

            //generate memreserve
            if (dtsDocument.Dtsmemreserve != null)
            {
                sb.AppendLine(dtsDocument.Dtsmemreserve.ToString());
                sb.AppendLine();
            }

            // generate include
            if (dtsDocument.Includes.Any())
            {
                foreach (var include in dtsDocument.Includes)
                {
                    sb.AppendLine(include.ToString());
                }
                sb.AppendLine();
            }

            // generate comment
            if (dtsDocument.Comments.Any())
            {
                foreach (var comment in dtsDocument.Comments)
                {
                    sb.AppendLine(comment);
                }
                sb.AppendLine();
            }

            // generate root node
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
                var indent = GetIndent(indentLevel);
                //generate node、label and address
                sb.Append($"{indent}{node.ToString()}");
                sb.AppendLine(" {");
                indentLevel++;
            }

            // generate property
            foreach (var property in node.Properties)
            {
                GenerateProperty(sb, property);
            }

            // add new line between property and child node
            if (node.Properties.Any() && node.Children.Any())
            {
                sb.AppendLine();
            }

            // generate childe node
            var childNodes = node.Children;
            for (int i = 0; i < childNodes.Count; i++)
            {
                GenerateNode(sb, childNodes[i]);

                // add new line between child node
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
