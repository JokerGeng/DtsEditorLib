using System;
using System.Collections.Generic;
using System.Text;

namespace DtsEditorLib.Models
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
}
