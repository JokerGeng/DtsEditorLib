using System;
using System.Collections.Generic;
using System.Text;

namespace DotNet.DTS
{
    public class DtsLabelResolver
    {
        private readonly Dictionary<string, DtsNode> _labelMap = new Dictionary<string, DtsNode>();

        public void RegisterLabels(DtsNode node)
        {
            if (node.Properties.TryGetValue("label", out var labelProp))
            {
                _labelMap[labelProp.Value.ToString()!] = node;
            }

            foreach (var child in node.Children)
                RegisterLabels(child);
        }

        public void ResolvePhandles(DtsNode node)
        {
            foreach (var prop in node.Properties.Values)
            {
                if (prop.Value is string str && str.StartsWith("&"))
                {
                    string label = str[1..];
                    if (_labelMap.TryGetValue(label, out var target))
                    {
                        prop.Value = target; // 可替换为 phandle ID 或引用
                    }
                }
            }

            foreach (var child in node.Children)
                ResolvePhandles(child);
        }
    }

}
