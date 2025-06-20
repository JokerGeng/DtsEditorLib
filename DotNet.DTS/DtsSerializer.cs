using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;
using System.Text.Json;

namespace DotNet.DTS
{
    public static class DtsSerializer
    {
        public static string ToJson(DtsNode node)
        {
            var json = JsonSerializer.Serialize(node, new JsonSerializerOptions
            {
                WriteIndented = true,
                Converters = { new JsonStringEnumConverter() }
            });
            return json;
        }

        public static DtsNode FromJson(string json)
        {
            return JsonSerializer.Deserialize<DtsNode>(json)!;
        }
    }

}
