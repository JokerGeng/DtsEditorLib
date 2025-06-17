using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DtsEditorLib.Models;

namespace DtsEditorLib.Comparator
{
    public class DeviceTreeComparator
    {
        public List<DeviceTreeDiff> Compare(DeviceTree oldTree, DeviceTree newTree)
        {
            var diffs = new List<DeviceTreeDiff>();
            CompareNodes(oldTree.Root, newTree.Root, diffs);
            return diffs;
        }

        private void CompareNodes(DeviceTreeNode oldNode, DeviceTreeNode newNode, List<DeviceTreeDiff> diffs)
        {
            // 比较属性
            CompareProperties(oldNode, newNode, diffs);

            // 比较子节点
            CompareChildren(oldNode, newNode, diffs);
        }

        private void CompareProperties(DeviceTreeNode oldNode, DeviceTreeNode newNode, List<DeviceTreeDiff> diffs)
        {
            var oldProps = oldNode?.Properties ?? new Dictionary<string, DeviceTreeProperty>();
            var newProps = newNode?.Properties ?? new Dictionary<string, DeviceTreeProperty>();

            // 查找新增和修改的属性
            foreach (var newProp in newProps)
            {
                if (!oldProps.ContainsKey(newProp.Key))
                {
                    diffs.Add(new DeviceTreeDiff
                    {
                        Type = DiffType.PropertyAdded,
                        Path = newNode.FullPath,
                        PropertyName = newProp.Key,
                        NewValue = newProp.Value.Value,
                        Description = $"Property '{newProp.Key}' added"
                    });
                }
                else if (!ValuesEqual(oldProps[newProp.Key].Value, newProp.Value.Value))
                {
                    diffs.Add(new DeviceTreeDiff
                    {
                        Type = DiffType.PropertyModified,
                        Path = newNode.FullPath,
                        PropertyName = newProp.Key,
                        OldValue = oldProps[newProp.Key].Value,
                        NewValue = newProp.Value.Value,
                        Description = $"Property '{newProp.Key}' modified"
                    });
                }
            }

            // 查找删除的属性
            foreach (var oldProp in oldProps)
            {
                if (!newProps.ContainsKey(oldProp.Key))
                {
                    diffs.Add(new DeviceTreeDiff
                    {
                        Type = DiffType.PropertyRemoved,
                        Path = oldNode.FullPath,
                        PropertyName = oldProp.Key,
                        OldValue = oldProp.Value.Value,
                        Description = $"Property '{oldProp.Key}' removed"
                    });
                }
            }
        }

        private void CompareChildren(DeviceTreeNode oldNode, DeviceTreeNode newNode, List<DeviceTreeDiff> diffs)
        {
            var oldChildren = oldNode?.Children ?? new Dictionary<string, DeviceTreeNode>();
            var newChildren = newNode?.Children ?? new Dictionary<string, DeviceTreeNode>();

            // 查找新增和修改的子节点
            foreach (var newChild in newChildren)
            {
                if (!oldChildren.ContainsKey(newChild.Key))
                {
                    diffs.Add(new DeviceTreeDiff
                    {
                        Type = DiffType.NodeAdded,
                        Path = newChild.Value.FullPath,
                        Description = $"Node '{newChild.Key}' added"
                    });

                    // 递归添加所有子属性和子节点作为新增项
                    AddAllDescendantsAsDiffs(newChild.Value, DiffType.NodeAdded, diffs);
                }
                else
                {
                    // 递归比较子节点
                    CompareNodes(oldChildren[newChild.Key], newChild.Value, diffs);
                }
            }

            // 查找删除的子节点
            foreach (var oldChild in oldChildren)
            {
                if (!newChildren.ContainsKey(oldChild.Key))
                {
                    diffs.Add(new DeviceTreeDiff
                    {
                        Type = DiffType.NodeRemoved,
                        Path = oldChild.Value.FullPath,
                        Description = $"Node '{oldChild.Key}' removed"
                    });
                }
            }
        }

        private void AddAllDescendantsAsDiffs(DeviceTreeNode node, DiffType diffType, List<DeviceTreeDiff> diffs)
        {
            // 添加所有属性
            foreach (var property in node.Properties.Values)
            {
                diffs.Add(new DeviceTreeDiff
                {
                    Type = diffType == DiffType.NodeAdded ? DiffType.PropertyAdded : DiffType.PropertyRemoved,
                    Path = node.FullPath,
                    PropertyName = property.Name,
                    NewValue = diffType == DiffType.NodeAdded ? property.Value : null,
                    OldValue = diffType == DiffType.NodeRemoved ? property.Value : null
                });
            }

            // 递归处理子节点
            foreach (var child in node.Children.Values)
            {
                diffs.Add(new DeviceTreeDiff
                {
                    Type = diffType,
                    Path = child.FullPath
                });
                AddAllDescendantsAsDiffs(child, diffType, diffs);
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

        public string GenerateDiffReport(List<DeviceTreeDiff> diffs)
        {
            var report = new System.Text.StringBuilder();
            report.AppendLine("Device Tree Comparison Report");
            report.AppendLine("===========================");
            report.AppendLine($"Total changes: {diffs.Count}");
            report.AppendLine();

            var grouped = diffs.GroupBy(d => d.Type).ToList();

            foreach (var group in grouped)
            {
                report.AppendLine($"{group.Key} ({group.Count()}):");
                foreach (var diff in group)
                {
                    report.AppendLine($"  {diff}");
                }
                report.AppendLine();
            }

            return report.ToString();
        }
    }
}
