using System;
using System.Collections.Generic;
using System.Text;

namespace DtsParser.Models
{
    public enum DiffType
    {
        Added,
        Removed,
        Modified,
        Unchanged,
        NodeAdded,
        NodeRemoved,
        PropertyAdded,
        PropertyRemoved,
        PropertyModified
    }

    public class DeviceTreeDiff
    {
        public DiffType Type { get; set; }
        public string Path { get; set; }
        public string PropertyName { get; set; }
        public object OldValue { get; set; }
        public object NewValue { get; set; }
        public string Description { get; set; }

        public override string ToString()
        {
            var location = !string.IsNullOrEmpty(PropertyName) ? $"{Path}.{PropertyName}" : Path;

            return Type switch
            {
                DiffType.NodeAdded => $"+ Node added: {Path}",
                DiffType.NodeRemoved => $"- Node removed: {Path}",
                DiffType.PropertyAdded => $"+ Property added: {location} = {NewValue}",
                DiffType.PropertyRemoved => $"- Property removed: {location} = {OldValue}",
                DiffType.PropertyModified => $"* Property modified: {location}: {OldValue} -> {NewValue}",
                _ => $"{Type}: {location}"
            };
        }
    }
}
