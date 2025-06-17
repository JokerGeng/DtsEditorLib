using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DtsEditorLib.Models;

namespace DtsEditorLib.Utils
{
    // 查询构建器
    public class DeviceTreeQueryBuilder
    {
        private readonly DeviceTree deviceTree;
        private readonly List<Func<DeviceTreeNode, bool>> conditions = new List<Func<DeviceTreeNode, bool>>();

        public DeviceTreeQueryBuilder(DeviceTree deviceTree)
        {
            this.deviceTree = deviceTree;
        }

        //public DeviceTreeQueryBuilder WithProperty(string propertyName)
        //{
        //    conditions.Add(node => node.Properties.ContainsKey(propertyName));
        //    return this;
        //}

        //public DeviceTreeQueryBuilder WithPropertyValue(string propertyName, object value)
        //{
        //    conditions.Add(node =>
        //        node.Properties.ContainsKey(propertyName) &&
        //        ValuesEqual(node.Properties[propertyName].Value, value));
        //    return this;
        //}

        public DeviceTreeQueryBuilder WithName(string name)
        {
            conditions.Add(node => node.Name == name);
            return this;
        }

        //public DeviceTreeQueryBuilder WithCompatible(string compatible)
        //{
        //    conditions.Add(node =>
        //        node.Properties.ContainsKey("compatible") &&
        //        node.Properties["compatible"].Value?.ToString()?.Contains(compatible) == true);
        //    return this;
        //}

        public DeviceTreeQueryBuilder UnderPath(string ancestorPath)
        {
            conditions.Add(node => node.FullPath.StartsWith(ancestorPath + "/"));
            return this;
        }

        public DeviceTreeQueryBuilder Where(Func<DeviceTreeNode, bool> predicate)
        {
            conditions.Add(predicate);
            return this;
        }

        public IEnumerable<DeviceTreeNode> Execute()
        {
            var query = new DeviceTreeQuery(deviceTree);
            var allNodes = FindAllNodes(deviceTree.Root);

            return allNodes.Where(node => conditions.All(condition => condition(node)));
        }

        private IEnumerable<DeviceTreeNode> FindAllNodes(DeviceTreeNode node)
        {
            yield return node;
            foreach (var child in node.Children)
            {
                foreach (var descendant in FindAllNodes(child))
                {
                    yield return descendant;
                }
            }
        }

        private bool ValuesEqual(object value1, object value2)
        {
            if (value1 == null && value2 == null) return true;
            if (value1 == null || value2 == null) return false;

            if (value1 is int[] intArray1 && value2 is int[] intArray2)
                return intArray1.SequenceEqual(intArray2);

            return value1.Equals(value2);
        }
    }
}
