using System;
using System.Collections.Generic;
using System.Text;

namespace DtsEditorLib.Models
{
    public class DeviceTreeDoc
    {
        public DeviceTreeNode Root { get; set; }
        public Dictionary<string, DeviceTreeNode> Labels { get; set; } = new Dictionary<string, DeviceTreeNode>();
        public List<string> Includes { get; set; } = new List<string>();
        public string Version { get; set; } = "/dts-v1/";

        public DeviceTreeDoc()
        {
            Root = new DeviceTreeNode { Name = "/" };
        }

        // 通过标签查找节点
        public DeviceTreeNode FindByLabel(string label)
        {
            return Labels.ContainsKey(label) ? Labels[label] : null;
        }

        // 验证整个设备树
        public List<ValidationResult> Validate()
        {
            var results = Root.Validate();

            // 检查引用的标签是否存在
            foreach (var node in GetAllNodes())
            {
                foreach (var prop in node.Properties.Values)
                {
                    if (prop.ValueType == PropertyValueType.Reference)
                    {
                        var refLabel = prop.Value.ToString();
                        if (!Labels.ContainsKey(refLabel))
                        {
                            results.Add(new ValidationResult(ValidationLevel.Error,
                                $"Reference to undefined label '{refLabel}'", node.FullPath));
                        }
                    }
                }
            }

            return results;
        }

        // 获取所有节点
        private IEnumerable<DeviceTreeNode> GetAllNodes()
        {
            return GetAllNodesRecursive(Root);
        }

        private IEnumerable<DeviceTreeNode> GetAllNodesRecursive(DeviceTreeNode node)
        {
            yield return node;
            foreach (var child in node.Children.Values)
            {
                foreach (var descendant in GetAllNodesRecursive(child))
                {
                    yield return descendant;
                }
            }
        }
    }
}
