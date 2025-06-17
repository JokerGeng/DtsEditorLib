using System;
using System.Collections.Generic;
using System.Reflection.Metadata.Ecma335;

namespace DtsEditorLib.Models
{
    // 设备树节点
    public class DeviceTreeNode
    {
        public string Name { get; set; }
        public string Label { get; set; }
        public uint? UnitAddress { get; set; }
        public List<DeviceTreeProperty> Properties { get; set; }
        public List<DeviceTreeNode> Children { get; set; }
        public DeviceTreeNode Parent { get; set; }
        public int LineNumber { get; set; }
        public List<string> Comments { get; set; }
        public List<string> Labels { get; set; } = new List<string>();

        public DeviceTreeNode(string name)
        {
            Name = name;
            Properties = new List<DeviceTreeProperty>();
            Children = new List<DeviceTreeNode>();
            Comments = new List<string>();
        }

        public string FullPath
        {
            get
            {
                if (Parent == null) return "/";
                var path = Parent.FullPath;
                return path == "/" ? $"/{Name}" : $"{path}/{Name}";
            }
        }

        // 添加属性
        public void AddProperty(string name, object value, PropertyValueType type = PropertyValueType.String)
        {
            Properties.Add(new DeviceTreeProperty(name)
            {
                Value = value,
                ValueType = type
            });
        }

        public void AddProperty(DeviceTreeProperty property)
        {
            Properties.Add(property);
        }

        // 添加子节点
        public DeviceTreeNode AddChild(string name, uint? address, string label = null)
        {
            var child = new DeviceTreeNode(name)
            {
                UnitAddress = address,
                Label = label,
                Parent = this
            };
            Children.Add(child);    
            return child;
        }


        public void AddChild(DeviceTreeNode child)
        {
            child.Parent = this;
            Children.Add(child);
        }

        public DeviceTreeNode FindChild(string path)
        {
            if (string.IsNullOrWhiteSpace(path))
            {
                return null;
            }

            if (path.StartsWith("/"))
            {
                // 从根节点开始查找
                var root = this;
                while (root.Parent != null) root = root.Parent;
                return root.FindChild(path.Substring(1));
            }

            var parts = path.Split('/', StringSplitOptions.RemoveEmptyEntries);
            var current = this;

            foreach (var part in parts)
            {
                if (string.IsNullOrEmpty(part)) continue;
                var find= current.Children.Find(n=>n.Name==part);
                if(find==null)
                {
                    return null ;
                }
                current= find;
            }

            return current;
        }
    }
}
