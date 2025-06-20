using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DotNet.DTS
{
    public class DtsPrettyPrinter
    {
        public static string ToDtsText(DtsNode node, int indent = 0)
        {
            var sb = new StringBuilder();
            string pad = new string(' ', indent);

            sb.AppendLine($"{pad}{node.Name} {{");

            foreach (var prop in node.Properties.Values)
            {
                string valStr = prop.Value switch
                {
                    string s => $"\"{s}\"",
                    List<uint> list => "<" + string.Join(" ", list.Select(x => $"0x{x:X}")) + ">",
                    _ => prop.Value?.ToString() ?? ""
                };

                sb.AppendLine($"{pad}  {prop.Name} = {valStr};");
            }

            foreach (var child in node.Children)
            {
                sb.Append(ToDtsText(child, indent + 2));
            }

            sb.AppendLine($"{pad}}};");
            return sb.ToString();
        }
    }

}
