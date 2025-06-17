using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DtsEditorLib.Models;

namespace DtsEditorLib.Utils
{
    public class DeviceTreeQuery
    {
        private readonly DeviceTree deviceTree;

        public DeviceTreeQuery(DeviceTree deviceTree)
        {
            this.deviceTree = deviceTree;
        }

        // 按路径查询
        public DeviceTreeNode ByPath(string path)
        {
            return deviceTree.FindByPath(path);
        }

        // 按标签查询
        public DeviceTreeNode ByLabel(string label)
        {
            return deviceTree.FindByLabel(label);
        }

        // 按属性查询
        public IEnumerable<DeviceTreeNode> WithProperty(string propertyName)
        {
            return FindNodesWithProperty(deviceTree.Root, propertyName);
        }

        // 按属性值查询
        public IEnumerable<DeviceTreeNode> WithPropertyValue(string propertyName, object value)
        {
            return WithProperty(propertyName).Where(node =>
            {
                var find = node.Properties.Find(p => p.Name == propertyName);
                return ValuesEqual(find.Value, value);
            });
        }

        // 按兼容字符串查询
        public IEnumerable<DeviceTreeNode> WithCompatible(string compatible)
        {
            return WithProperty("compatible").Where(node =>
            {
                var compatibleProp = node.Properties.Find(p => p.Name == "compatible");
                if (compatibleProp.Value is string compatibleStr)
                {
                    return compatibleStr.Contains(compatible);
                }
                return false;
            });
        }

        // 按节点名称查询
        public IEnumerable<DeviceTreeNode> WithName(string name)
        {
            return FindAllNodes(deviceTree.Root).Where(node => node.Name == name);
        }

        // 按正则表达式查询节点名称
        public IEnumerable<DeviceTreeNode> WithNamePattern(string pattern)
        {
            var regex = new System.Text.RegularExpressions.Regex(pattern);
            return FindAllNodes(deviceTree.Root).Where(node => regex.IsMatch(node.Name));
        }

        // 查找所有中断控制器
        public IEnumerable<DeviceTreeNode> InterruptControllers()
        {
            return WithProperty("interrupt-controller");
        }

        // 查找所有时钟提供者
        public IEnumerable<DeviceTreeNode> ClockProviders()
        {
            return WithProperty("#clock-cells");
        }

        // 查找所有GPIO控制器
        public IEnumerable<DeviceTreeNode> GpioControllers()
        {
            return WithProperty("gpio-controller");
        }

        // 查找所有总线节点
        public IEnumerable<DeviceTreeNode> Buses()
        {
            return WithProperty("compatible").Where(node =>
            {
                var compatible = node.Properties.Find(p => p.Name == "compatible");
                return compatible != null && (
                    compatible.Value.ToString().Contains("simple-bus") ||
                    compatible.Value.ToString().Contains("i2c") ||
                    compatible.Value.ToString().Contains("spi") ||
                    compatible.Value.ToString().Contains("pci")
                );
            });
        }

        // 复杂查询：支持条件组合
        public IEnumerable<DeviceTreeNode> Where(Func<DeviceTreeNode, bool> predicate)
        {
            return FindAllNodes(deviceTree.Root).Where(predicate);
        }

        // 查找父子关系
        public IEnumerable<DeviceTreeNode> ChildrenOf(string parentPath)
        {
            var parent = ByPath(parentPath);
            return parent?.Children ?? Enumerable.Empty<DeviceTreeNode>();
        }

        public IEnumerable<DeviceTreeNode> DescendantsOf(string ancestorPath)
        {
            var ancestor = ByPath(ancestorPath);
            if (ancestor == null) yield break;

            foreach (var child in ancestor.Children)
            {
                yield return child;
                foreach (var descendant in DescendantsOf(child.FullPath))
                {
                    yield return descendant;
                }
            }
        }

        // 统计查询
        public Dictionary<string, int> PropertyStatistics()
        {
            var stats = new Dictionary<string, int>();
            var allNodes = FindAllNodes(deviceTree.Root);

            foreach (var node in allNodes)
            {
                foreach (var property in node.Properties)
                {
                    stats[property.Name] = stats.ContainsKey(property.Name) ? stats[property.Name] + 1 : 1;
                }
            }

            return stats.OrderByDescending(kvp => kvp.Value).ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
        }

        public Dictionary<string, int> CompatibleStatistics()
        {
            var stats = new Dictionary<string, int>();
            var nodesWithCompatible = WithProperty("compatible");

            foreach (var node in nodesWithCompatible)
            {
                var compatible = node.Properties.Find(p => p.Name == "compatible");
                if (!string.IsNullOrEmpty(compatible?.Value.ToString()))
                {
                    stats[compatible.Name] = stats.ContainsKey(compatible.Name) ? stats[compatible.Name] + 1 : 1;
                }
            }

            return stats.OrderByDescending(kvp => kvp.Value).ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
        }

        // 私有辅助方法
        private IEnumerable<DeviceTreeNode> FindNodesWithProperty(DeviceTreeNode node, string propertyName)
        {
            foreach(var property in node.Properties)
            {
                if(property.Name == propertyName)
                {
                    yield return node;
                }
            }
            foreach (var child in node.Children)
            {
                foreach (var result in FindNodesWithProperty(child, propertyName))
                {
                    yield return result;
                }
            }
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

            if (value1 is byte[] byteArray1 && value2 is byte[] byteArray2)
                return byteArray1.SequenceEqual(byteArray2);

            if (value1 is string[] stringArray1 && value2 is string[] stringArray2)
                return stringArray1.SequenceEqual(stringArray2);

            return value1.Equals(value2);
        }
    }
}
