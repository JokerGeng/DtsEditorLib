using System;
using System.Collections.Generic;
using System.Text;

namespace DtsEditorLib.Exceptions
{
    public class DeviceTreeParseException : Exception
    {
        public DeviceTreeParseException(string message) : base(message) { }
        public DeviceTreeParseException(string message, Exception innerException) : base(message, innerException) { }
    }
}
