using System;
using System.Collections.Generic;

namespace DtsEditorLib.Models
{
    public class DeviceTree
    {
        /// <summary>
        /// 根节点
        /// </summary>
        public DeviceTreeNode Root { get; set; }
        /// <summary>
        /// 头文件
        /// </summary>
        public List<string> Includes { get; set; }
        /// <summary>
        /// 标签引用节点
        /// </summary>
        public Dictionary<string, DeviceTreeNode> Labels { get; set; }
        /// <summary>
        /// 注释
        /// </summary>
        public List<string> Comments { get; set; }
        /// <summary>
        /// 文件
        /// </summary>
        public string FileName { get; set; }
        /// <summary>
        /// 版本
        /// </summary>
        public string Version { get; set; } = "/dts-v1/";

        public DeviceTree()
        {
            Root = new DeviceTreeNode("/");
            Includes = new List<string>();
            Labels = new Dictionary<string, DeviceTreeNode>();
            Comments = new List<string>();
        }

        public void AddLabel(string label, DeviceTreeNode node)
        {
            Labels[label] = node;
            node.Label = label;
        }

        /// <summary>
        /// 根节点开始/A/B/C
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public DeviceTreeNode FindByPath(string path)
        {
            if (path == "/")
            {
                return Root;
            }
            return FindChild(Root, path);
        }

        public DeviceTreeNode FindChild(DeviceTreeNode currNode, string path)
        {
            if (currNode.Name == path)
            {
                return currNode;
            }
            var newPath = path.TrimStart(currNode.Name.ToCharArray());
            newPath = newPath.TrimStart('/');
            foreach (var child in currNode.Children)
            {
                if (newPath.StartsWith(child.Name))
                {
                    return FindChild(child, newPath);
                }
            }
            return null;
            //return null;
        }

        public DeviceTreeNode FindByLabel(string label)
        {
            return Labels.ContainsKey(label) ? Labels[label] : null;
        }

        // 获取所有节点
        private IEnumerable<DeviceTreeNode> GetAllNodes()
        {
            return GetAllNodesRecursive(Root);
        }

        private IEnumerable<DeviceTreeNode> GetAllNodesRecursive(DeviceTreeNode node)
        {
            yield return node;
            foreach (var child in node.Children)
            {
                foreach (var descendant in GetAllNodesRecursive(child))
                {
                    yield return descendant;
                }
            }
        }
    }
}
