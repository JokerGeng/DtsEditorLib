using System;
using System.Collections.Generic;
using System.Text;

namespace DtsParser
{
    /// <summary>
    /// DTS节点
    /// </summary>
    public class DtsNode
    {
        public string Name { get; set; }
        public string Label { get; set; }  // 节点标签
        public List<DtsProperty> Properties { get; set; }
        public List<DtsNode> Children { get; set; }
        public DtsNode Parent { get; set; }
        public int Line { get; set; }

        public DtsNode(string name = null, string label = null)
        {
            Name = name;
            Label = label;
            Properties = new List<DtsProperty>();
            Children = new List<DtsNode>();
        }

        /// <summary>
        /// 添加子节点
        /// </summary>
        public void AddChild(DtsNode child)
        {
            child.Parent = this;
            Children.Add(child);
        }

        /// <summary>
        /// 添加属性
        /// </summary>
        public void AddProperty(DtsProperty property)
        {
            Properties.Add(property);
        }

        /// <summary>
        /// 根据名称查找子节点
        /// </summary>
        public DtsNode FindChild(string name)
        {
            return Children.Find(child => child.Name == name);
        }

        /// <summary>
        /// 根据名称查找属性
        /// </summary>
        public DtsProperty FindProperty(string name)
        {
            return Properties.Find(prop => prop.Name == name);
        }

        /// <summary>
        /// 获取节点的完整路径
        /// </summary>
        public string GetPath()
        {
            if (Parent == null)
                return "/";

            var path = Parent.GetPath();
            if (path == "/")
                return $"/{Name}";
            return $"{path}/{Name}";
        }

        public override string ToString()
        {
            var labelPart = !string.IsNullOrEmpty(Label) ? $"{Label}: " : "";
            return $"{labelPart}{Name ?? ""}";
        }
    }
}
