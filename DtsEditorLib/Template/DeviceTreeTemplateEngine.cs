using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DtsEditorLib.Models;
using DtsEditorLib.Parser;
using System.Text.RegularExpressions;
using System.IO;

namespace DtsEditorLib.Template
{
    public class DeviceTreeTemplateEngine
    {
        private readonly Dictionary<string, DeviceTreeTemplate> templates = new Dictionary<string, DeviceTreeTemplate>();
        private readonly Regex templateVariableRegex = new Regex(@"\{\{(\w+)\}\}");

        public void LoadTemplate(string templateFile)
        {
            var content = File.ReadAllText(templateFile);
            var template = ParseTemplate(content);
            templates[template.Name] = template;
        }

        public void SaveTemplate(DeviceTreeTemplate template, string templateFile)
        {
            var content = SerializeTemplate(template);
            File.WriteAllText(templateFile, content);
        }

        public string GenerateFromTemplate(string templateName, Dictionary<string, object> parameters)
        {
            if (!templates.ContainsKey(templateName))
                throw new ArgumentException($"Template '{templateName}' not found");

            var template = templates[templateName];
            ValidateParameters(template, parameters);

            var content = template.Content;
            var allParameters = new Dictionary<string, object>(template.DefaultValues);

            foreach (var param in parameters)
            {
                allParameters[param.Key] = param.Value;
            }

            return ReplaceTemplateVariables(content, allParameters);
        }

        public DeviceTree GenerateDeviceTreeFromTemplate(string templateName, Dictionary<string, object> parameters)
        {
            var dtsContent = GenerateFromTemplate(templateName, parameters);
            var parser = new DeviceTreeParser();
            return parser.Parse(dtsContent);
        }

        private DeviceTreeTemplate ParseTemplate(string content)
        {
            var template = new DeviceTreeTemplate();
            var lines = content.Split('\n');
            var inHeader = true;
            var contentBuilder = new StringBuilder();

            foreach (var line in lines)
            {
                if (inHeader && line.StartsWith("/*") && line.Contains("TEMPLATE"))
                {
                    continue; // Skip template header start
                }
                else if (inHeader && line.StartsWith(" * Name:"))
                {
                    template.Name = line.Substring(8).Trim();
                }
                else if (inHeader && line.StartsWith(" * Description:"))
                {
                    template.Description = line.Substring(15).Trim();
                }
                else if (inHeader && line.StartsWith(" * Parameter:"))
                {
                    var paramInfo = line.Substring(13).Trim();
                    template.Parameters.Add(ParseParameterInfo(paramInfo));
                }
                else if (inHeader && line.Trim() == "*/")
                {
                    inHeader = false;
                }
                else if (!inHeader)
                {
                    contentBuilder.AppendLine(line);
                }
            }

            template.Content = contentBuilder.ToString().Trim();
            return template;
        }

        private TemplateParameter ParseParameterInfo(string paramInfo)
        {
            // 解析参数信息，格式: "name:type:description:required:default"
            var parts = paramInfo.Split(':');
            return new TemplateParameter
            {
                Name = parts.Length > 0 ? parts[0].Trim() : "",
                Type = parts.Length > 1 ? parts[1].Trim() : "string",
                Description = parts.Length > 2 ? parts[2].Trim() : "",
                Required = parts.Length > 3 ? bool.Parse(parts[3].Trim()) : false,
                DefaultValue = parts.Length > 4 ? parts[4].Trim() : null
            };
        }

        private string SerializeTemplate(DeviceTreeTemplate template)
        {
            var builder = new StringBuilder();
            builder.AppendLine("/* TEMPLATE");
            builder.AppendLine($" * Name: {template.Name}");
            builder.AppendLine($" * Description: {template.Description}");

            foreach (var param in template.Parameters)
            {
                builder.AppendLine($" * Parameter: {param.Name}:{param.Type}:{param.Description}:{param.Required}:{param.DefaultValue}");
            }

            builder.AppendLine(" */");
            builder.AppendLine();
            builder.AppendLine(template.Content);

            return builder.ToString();
        }

        private void ValidateParameters(DeviceTreeTemplate template, Dictionary<string, object> parameters)
        {
            foreach (var templateParam in template.Parameters)
            {
                if (templateParam.Required && !parameters.ContainsKey(templateParam.Name) &&
                    !template.DefaultValues.ContainsKey(templateParam.Name))
                {
                    throw new ArgumentException($"Required parameter '{templateParam.Name}' is missing");
                }

                if (parameters.ContainsKey(templateParam.Name))
                {
                    var value = parameters[templateParam.Name];
                    if (!ValidateParameterType(value, templateParam.Type))
                    {
                        throw new ArgumentException($"Parameter '{templateParam.Name}' has invalid type");
                    }

                    if (templateParam.ValidValues != null && templateParam.ValidValues.Length > 0)
                    {
                        if (!Array.Exists(templateParam.ValidValues, v => v == value?.ToString()))
                        {
                            throw new ArgumentException($"Parameter '{templateParam.Name}' has invalid value");
                        }
                    }
                }
            }
        }

        private bool ValidateParameterType(object value, string expectedType)
        {
            return expectedType.ToLower() switch
            {
                "string" => value is string,
                "int" => value is int,
                "bool" => value is bool,
                "array" => value is Array,
                _ => true
            };
        }

        private string ReplaceTemplateVariables(string content, Dictionary<string, object> parameters)
        {
            return templateVariableRegex.Replace(content, match =>
            {
                var varName = match.Groups[1].Value;
                if (parameters.ContainsKey(varName))
                {
                    var value = parameters[varName];
                    return FormatValue(value);
                }
                return match.Value; // 保持原样如果找不到参数
            });
        }

        private string FormatValue(object value)
        {
            return value switch
            {
                int[] intArray => $"<{string.Join(" ", intArray)}>",
                string[] stringArray => string.Join(" ", stringArray.Select(s => $"\"{s}\"")),
                string str => $"\"{str}\"",
                bool boolean => boolean ? "1" : "0",
                _ => value?.ToString() ?? ""
            };
        }

        public List<string> GetTemplateNames()
        {
            return new List<string>(templates.Keys);
        }

        public DeviceTreeTemplate GetTemplate(string name)
        {
            return templates.ContainsKey(name) ? templates[name] : null;
        }
    }
}
