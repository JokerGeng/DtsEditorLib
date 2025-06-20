using System;
using System.Collections.Generic;
using System.Text;

namespace DtsParser
{
    /// <summary>
    /// DTS查询工具
    /// </summary>
    public static class DtsQuery
    {
        /// <summary>
        /// 根据路径查找节点
        /// </summary>
        public static DtsNode FindNodeByPath(DtsNode root, string path)
        {
            if (string.IsNullOrEmpty(path) || path == "/")
                return root;

            var parts = path.Split('/', StringSplitOptions.RemoveEmptyEntries);
            var current = root;

            foreach (var part in parts)
            {
                current = current.FindChild(part);
                if (current == null)
                    return null;
            }

            return current;
        }

        /// <summary>
        /// 查找所有具有指定属性的节点
        /// </summary>
        public static List<DtsNode> FindNodesWithProperty(DtsNode root, string propertyName)
        {
            var result = new List<DtsNode>();
            FindNodesWithPropertyRecursive(root, propertyName, result);
            return result;
        }

        private static void FindNodesWithPropertyRecursive(DtsNode node, string propertyName, List<DtsNode> result)
        {
            if (node.FindProperty(propertyName) != null)
            {
                result.Add(node);
            }

            foreach (var child in node.Children)
            {
                FindNodesWithPropertyRecursive(child, propertyName, result);
            }
        }

        /// <summary>
        /// 查找所有具有指定名称的节点
        /// </summary>
        public static List<DtsNode> FindNodesByName(DtsNode root, string nodeName)
        {
            var result = new List<DtsNode>();
            FindNodesByNameRecursive(root, nodeName, result);
            return result;
        }

        private static void FindNodesByNameRecursive(DtsNode node, string nodeName, List<DtsNode> result)
        {
            if (node.Name == nodeName)
            {
                result.Add(node);
            }

            foreach (var child in node.Children)
            {
                FindNodesByNameRecursive(child, nodeName, result);
            }
        }
    }
}
