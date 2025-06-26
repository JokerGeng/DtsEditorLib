using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DtsParser
{
    public class DtsComparator
    {
        public List<DeviceTreeDiff> Compare(DtsDocument oldTree, DtsDocument newTree)
        {
            var diffs = new List<DeviceTreeDiff>();
            CompareNodes(oldTree.RootNode, newTree.RootNode, diffs);
            return diffs;
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

        private void CompareNodes(DtsNode oldNode, DtsNode newNode, List<DeviceTreeDiff> diffs)
        {
            // 比较属性
            CompareProperties(oldNode, newNode, diffs);

            // 比较子节点
            CompareChildren(oldNode, newNode, diffs);
        }

        private void CompareProperties(DtsNode oldNode, DtsNode newNode, List<DeviceTreeDiff> diffs)
        {
            var oldProps = oldNode?.Properties;
            var newProps = newNode?.Properties;

            // 查找新增和修改的属性
            foreach (var newProp in newProps)
            {
                var find = oldProps.Find(t => t.Name == newProp.Name);
                if (find == null)
                {
                    diffs.Add(new DeviceTreeDiff
                    {
                        Type = DiffType.PropertyAdded,
                        Path = newNode.GetPath(),
                        PropertyName = newProp.Name,
                        NewValue = newProp.Values,
                        Description = $"Property '{newProp.Name}' added"
                    });
                }
                else if (!ValuesEqual(find, newProp.Values))
                {
                    diffs.Add(new DeviceTreeDiff
                    {
                        Type = DiffType.PropertyModified,
                        Path = newNode.GetPath(),
                        PropertyName = newProp.Name,
                        OldValue = find.Values,
                        NewValue = newProp.Values,
                        Description = $"Property '{newProp.Name}' modified"
                    });
                }
            }

            // 查找删除的属性
            foreach (var oldProp in oldProps)
            {
                var find = newProps.Find(t => t.Name == oldProp.Name);
                if (find == null)
                {
                    diffs.Add(new DeviceTreeDiff
                    {
                        Type = DiffType.PropertyRemoved,
                        Path = oldNode.GetPath(),
                        PropertyName = oldProp.Name,
                        OldValue = oldProp.Values,
                        Description = $"Property '{oldProp.Name}' removed"
                    });
                }
            }
        }

        private void CompareChildren(DtsNode oldNode, DtsNode newNode, List<DeviceTreeDiff> diffs)
        {
            var oldChildren = oldNode?.Children;
            var newChildren = newNode?.Children;

            // 查找新增和修改的子节点
            foreach (var newChild in newChildren)
            {
                var find = oldChildren.Find(t => t.Name == newChild.Name);
                if (find == null)
                {
                    diffs.Add(new DeviceTreeDiff
                    {
                        Type = DiffType.NodeAdded,
                        Path = newChild.GetPath(),
                        Description = $"Node '{newChild.Name}' added"
                    });

                    // 递归添加所有子属性和子节点作为新增项
                    AddAllDescendantsAsDiffs(newChild, DiffType.NodeAdded, diffs);
                }
                else
                {
                    // 递归比较子节点
                    CompareNodes(find, newChild, diffs);
                }
            }

            // 查找删除的子节点
            foreach (var oldChild in oldChildren)
            {
                var find = newChildren.Find(t => t.Name == oldChild.Name);
                if (find == null)
                {
                    diffs.Add(new DeviceTreeDiff
                    {
                        Type = DiffType.NodeRemoved,
                        Path = oldChild.GetPath(),
                        Description = $"Node '{oldChild.Name}' removed"
                    });
                }
            }
        }

        private void AddAllDescendantsAsDiffs(DtsNode node, DiffType diffType, List<DeviceTreeDiff> diffs)
        {
            // 添加所有属性
            foreach (var property in node.Properties)
            {
                diffs.Add(new DeviceTreeDiff
                {
                    Type = diffType == DiffType.NodeAdded ? DiffType.PropertyAdded : DiffType.PropertyRemoved,
                    Path = node.GetPath(),
                    PropertyName = property.Name,
                    NewValue = diffType == DiffType.NodeAdded ? property.Values : null,
                    OldValue = diffType == DiffType.NodeRemoved ? property.Values : null
                });
            }

            // 递归处理子节点
            foreach (var child in node.Children)
            {
                diffs.Add(new DeviceTreeDiff
                {
                    Type = diffType,
                    Path = child.GetPath()
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
    }
}
