using System;
using System.Collections.Generic;
using System.Text;
using DtsParser.AST;

namespace DtsParser.Models
{
    public class DtsDocument
    {
        public List<string> Comments { get; }
        public List<DtsIncludeDirective> Includes { get; }
        public DtsNode RootNode { get; set; }
        public string Version { get; set; }

        public DtsDocument()
        {
            Includes = new List<DtsIncludeDirective>();
            Comments = new List<string>();
        }

        public void AddInclude(DtsIncludeDirective include)
        {
            Includes.Add(include);
        }

        public DtsNode FindByPath(string path)
        {
            if (path == "/")
            {
                return RootNode;
            }
            return FindChild(RootNode, path);
        }

        public DtsNode FindChild(DtsNode currNode, string path)
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
        }

        public DtsNode FindByLabel(string label)
        {
            var nodes = GetAllNodes();
            foreach (var node in nodes)
            {
                if (node.Label == label)
                {
                    return node;
                }
            }
            return null;
        }

        private IEnumerable<DtsNode> GetAllNodes()
        {
            return GetAllNodesRecursive(RootNode);
        }

        private IEnumerable<DtsNode> GetAllNodesRecursive(DtsNode node)
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
