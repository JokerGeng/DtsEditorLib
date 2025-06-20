﻿using System;
using System.Collections.Generic;
using System.Linq;
using DtsEditorLib.Models;

namespace DtsEditorLib.Editor
{
    public class DeviceTreeEditor
    {
        private DeviceTree deviceTree;

        public DeviceTreeEditor(DeviceTree deviceTree)
        {
            this.deviceTree = deviceTree;
        }

        // 添加节点
        public DeviceTreeNode AddNode(string parentPath, string nodeName, uint? unitAddress)
        {
            var parent = deviceTree.FindByPath(parentPath);
            if (parent == null)
                throw new ArgumentException($"Parent node not found: {parentPath}");

            var newNode = new DeviceTreeNode(nodeName)
            {
                UnitAddress = unitAddress
            };

            parent.AddChild(newNode);
            return newNode;
        }

        // 删除节点
        public bool RemoveNode(string nodePath)
        {
            var node = deviceTree.FindByPath(nodePath);
            if (node?.Parent == null)
                return false;

            var nodeName = node.Name;
            node.Parent.Children.Remove(node);

            // 清理标签引用
            var labelsToRemove = deviceTree.Labels.Where(kvp => kvp.Value == node).Select(kvp => kvp.Key).ToList();
            foreach (var label in labelsToRemove)
            {
                deviceTree.Labels.Remove(label);
            }

            return true;
        }

        // 添加属性
        public void AddProperty(string nodePath, string propertyName, object value, PropertyValueType valueType = PropertyValueType.String)
        {
            var node = deviceTree.FindByPath(nodePath);
            if (node == null)
                throw new ArgumentException($"Node not found: {nodePath}");

            var property = new DeviceTreeProperty(propertyName)
            {
                Value = value,
                ValueType = valueType
            };
            if(valueType == PropertyValueType.String)
            {
                property.Value = $"\"{value}\";";
            }

            node.AddProperty(property);
        }

        // 更新属性
        public bool UpdateProperty(string nodePath, string propertyName, object newValue)
        {
            var node = deviceTree.FindByPath(nodePath);
            var find = node?.Properties.Find(p => p.Name == propertyName);
            if (find == null)
            {
                return false;
            }

            find.Value = newValue;
            if (find.ValueType == PropertyValueType.String)
            {
                find.Value = $"\"{newValue}\";";
            }
            return true;
        }

        // 删除属性
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

        // 移动节点
        public bool MoveNode(string sourcePath, string targetParentPath)
        {
            var sourceNode = deviceTree.FindByPath(sourcePath);
            var targetParent = deviceTree.FindByPath(targetParentPath);

            if (sourceNode?.Parent == null || targetParent == null)
                return false;

            // 从原父节点移除
            sourceNode.Parent.Children.Remove(sourceNode);

            // 添加到新父节点
            targetParent.AddChild(sourceNode);

            return true;
        }

        // 复制节点
        public DeviceTreeNode CopyNode(string sourcePath, string targetParentPath, string newNodeName = null)
        {
            var sourceNode = deviceTree.FindByPath(sourcePath);
            var targetParent = deviceTree.FindByPath(targetParentPath);

            if (sourceNode == null || targetParent == null)
                return null;

            var copiedNode = CloneNode(sourceNode);
            if (!string.IsNullOrEmpty(newNodeName))
            {
                copiedNode.Name = newNodeName;
            }

            targetParent.AddChild(copiedNode);
            return copiedNode;
        }

        // 克隆节点
        private DeviceTreeNode CloneNode(DeviceTreeNode source)
        {
            var cloned = new DeviceTreeNode(source.Name)
            {
                UnitAddress = source.UnitAddress,
                Label = source.Label
            };

            // 复制属性
            foreach (var property in source.Properties)
            {
                var clonedProperty = new DeviceTreeProperty(property.Name)
                {
                    ValueType = property.ValueType,
                    Value = CloneValue(property.Value),
                    RawValue = property.RawValue
                };
                cloned.AddProperty(clonedProperty);
            }

            // 递归复制子节点
            foreach (var child in source.Children)
            {
                var clonedChild = CloneNode(child);
                cloned.AddChild(clonedChild);
            }

            return cloned;
        }

        private object CloneValue(object value)
        {
            switch (value)
            {
                case int[] intArray:
                    return (int[])intArray.Clone();
                case byte[] byteArray:
                    return (byte[])byteArray.Clone();
                case string[] stringArray:
                    return (string[])stringArray.Clone();
                default:
                    return value;
            }
        }

        // 批量操作
        public void BatchEdit(Action<DeviceTreeEditor> editAction)
        {
            editAction(this);
        }

        // 查找和替换
        private void FindAndReplaceInNode(DeviceTreeNode node, string propertyName, object oldValue, object newValue, ref int count)
        {
            var find = node.Properties.Find(p => p.Name == propertyName);
            if (find != null)
            {
                if (Equals(find.Value, oldValue))
                {
                    find.Value = newValue;
                    count++;
                }
            }

            foreach (var child in node.Children)
            {
                FindAndReplaceInNode(child, propertyName, oldValue, newValue, ref count);
            }
        }
    }
}
