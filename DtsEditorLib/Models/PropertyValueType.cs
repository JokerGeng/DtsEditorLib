using System;
using System.Collections.Generic;
using System.Text;

namespace DtsEditorLib.Models
{
    // 设备树属性值类型
    public enum PropertyValueType
    {
        String,
        Integer,
        IntegerArray,
        MultiIntegerArray,
        ByteArray,
        Boolean,
        LabelReference,//&uart0
        ValueReference,//<&scpi_clk 0>;
        Empty
    }
}
