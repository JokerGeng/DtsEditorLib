using System;
using System.Collections.Generic;

namespace DtsParser
{
    public class DtsEditor
    {
        private DtsDocument deviceTree;

        public DtsEditor(DtsDocument deviceTree)
        {
            this.deviceTree = deviceTree;
        }

        public DtsNode AddNode(string parentPath, string nodeName, uint? unitAddress)
        {
            var parent = deviceTree.FindByPath(parentPath);
            if (parent == null)
                throw new ArgumentException($"Parent node not found: {parentPath}");

            var newNode = new DtsNode(nodeName, 0);

            parent.AddChild(newNode);
            return newNode;
        }

        public bool RemoveNode(string nodePath)
        {
            var node = deviceTree.FindByPath(nodePath);
            if (node?.Parent == null)
                return false;

            var nodeName = node.Name;
            node.Parent.Children.Remove(node);

            return true;
        }

        public void AddProperty(string nodePath, string propertyName, List<DtsPropertyValue> values)
        {
            var node = deviceTree.FindByPath(nodePath);
            if (node == null)
                throw new ArgumentException($"Node not found: {nodePath}");

            var property = new DtsProperty(propertyName);
            property.Values.AddRange(values);

            node.AddProperty(property);
        }

        public bool UpdateProperty(string nodePath, string propertyName, object newValue)
        {
            var node = deviceTree.FindByPath(nodePath);
            var find = node?.Properties.Find(p => p.Name == propertyName);
            if (find == null)
            {
                return false;
            }
            return true;
        }

        public bool RemoveProperty(string nodePath, string propertyName)
        {
            var node = deviceTree.FindByPath(nodePath);
            var find = node?.Properties.Find(p => p.Name == propertyName);
            if (find == null)
            {
                return false;
            }
            return node.Properties.Remove(find);
        }

        public bool MoveNode(string sourcePath, string targetParentPath)
        {
            var sourceNode = deviceTree.FindByPath(sourcePath);
            var targetParent = deviceTree.FindByPath(targetParentPath);

            if (sourceNode?.Parent == null || targetParent == null)
                return false;

            sourceNode.Parent.Children.Remove(sourceNode);

            targetParent.AddChild(sourceNode);

            return true;
        }

        public void BatchEdit(Action<DtsEditor> editAction)
        {
            editAction(this);
        }
    }
}
