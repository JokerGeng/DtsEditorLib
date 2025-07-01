using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using DtsParser.AST;

namespace DtsParser.Models
{
    public class DtsDocument
    {
        public List<string> Comments { get; }
        public List<DtsIncludeDirective> Includes { get; }
        public DtsNode RootNode { get; set; }
        public string Version { get; set; }

        public Dtsmemreserve Dtsmemreserve { get; set; }

        public DtsDocument()
        {
            Includes = new List<DtsIncludeDirective>();
            Comments = new List<string>();
        }

        public DtsNode FindByName(string name)
        {
            var nodes = GetAllNodes();
            return nodes.FirstOrDefault(t => t.Name == name);
        }

        public DtsNode FindByLabel(string label)
        {
            var nodes = GetAllNodes();
            return nodes.FirstOrDefault(t => t.Label == label);
        }

        /// <param name="path">nodename@uintaddress</param>
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
