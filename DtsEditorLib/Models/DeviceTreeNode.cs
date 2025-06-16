using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace DtsEditorLib.Models
{
    // 设备树节点
    public class DeviceTreeNode
    {
        public string Name { get; set; }
        public string Label { get; set; }
        public uint? Address { get; set; }
        public Dictionary<string, DeviceTreeProperty> Properties { get; set; } = new Dictionary<string, DeviceTreeProperty>();
        public Dictionary<string, DeviceTreeNode> Children { get; set; } = new Dictionary<string, DeviceTreeNode>();
        public DeviceTreeNode Parent { get; set; }
        public List<string> Comments { get; set; } = new List<string>();

        // 完整路径
        public string FullPath
        {
            get
            {
                if (Parent == null) return "/";
                var path = Parent.FullPath;
                if (path == "/") return $"/{Name}";
                return $"{path}/{Name}";
            }
        }

        // 添加属性
        public void AddProperty(string name, object value, PropertyValueType type = PropertyValueType.String)
        {
            Properties[name] = new DeviceTreeProperty
            {
                Name = name,
                Value = value,
                ValueType = type
            };
        }

        // 添加子节点
        public DeviceTreeNode AddChild(string name, uint? address = null, string label = null)
        {
            var child = new DeviceTreeNode
            {
                Name = name,
                Address = address,
                Label = label,
                Parent = this
            };
            Children[name] = child;
            return child;
        }

        // 查找节点
        public DeviceTreeNode FindNode(string path)
        {
            if (path.StartsWith("/"))
            {
                // 从根节点开始查找
                var root = this;
                while (root.Parent != null) root = root.Parent;
                return root.FindNode(path.Substring(1));
            }

            var parts = path.Split('/');
            var current = this;

            foreach (var part in parts)
            {
                if (string.IsNullOrEmpty(part)) continue;
                if (!current.Children.ContainsKey(part))
                    return null;
                current = current.Children[part];
            }

            return current;
        }

        // 验证节点
        public List<ValidationResult> Validate()
        {
            var results = new List<ValidationResult>();

            // 检查必需属性
            if (Name == "memory" && !Properties.ContainsKey("reg"))
            {
                results.Add(new ValidationResult(ValidationLevel.Error,
                    $"Memory node '{FullPath}' must have 'reg' property"));
            }

            // 递归验证子节点
            foreach (var child in Children.Values)
            {
                results.AddRange(child.Validate());
            }

            return results;
        }
    }
}
