using System.Collections.Generic;
using System.Linq;

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
        internal void AddProperty(string name, object value, PropertyValueType type = PropertyValueType.String)
        {
            Properties.Add(new DeviceTreeProperty(name)
            {
                Value = value,
                ValueType = type
            });
        }

        internal void AddProperty(DeviceTreeProperty property)
        {
            Properties.Add(property);
        }

        // 添加子节点
        internal DeviceTreeNode AddChild(string name, uint? address, string label = null)
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


        internal void AddChild(DeviceTreeNode child)
        {
            child.Parent = this;
            Children.Add(child);
        }
    }
}
