using System.Collections.Generic;

namespace DtsParser.AST
{
    /// <summary>
    /// DTS节点
    /// </summary>
    public class DtsNode
    {
        public string Name { get; }
        public string Label { get; }  // 节点标签
        public List<DtsProperty> Properties { get; }
        public List<DtsNode> Children { get; }
        public DtsNode Parent { get; set; }
        public int Line { get; }

        public string Path => GetPath();

        public DtsNode(string name, int line, string label = null, string uintAddress = null)
        {
            Name = name;
            if (uintAddress != null)
            {
                Name = Name + "@" + uintAddress;
            }
            Line = line;
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
        private string GetPath()
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
            var labelPart = !string.IsNullOrWhiteSpace(Label) ? $"{Label}: " : "";
            return $"{labelPart}{Name ?? ""}";
        }
    }
}
