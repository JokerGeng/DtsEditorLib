using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DtsParser
{
    public abstract class DtsExpression
    {
        public abstract string ToString();
    }

    /// <summary>
    /// 标识符表达式（包括宏）
    /// </summary>
    public class DtsIdentifierExpression : DtsExpression
    {
        public string Name { get; }

        public DtsIdentifierExpression(string name)
        {
            Name = name ?? throw new ArgumentNullException(nameof(name));
        }

        public override string ToString() => Name;
    }

    /// <summary>
    /// 数值表达式
    /// </summary>
    public class DtsNumberExpression : DtsExpression
    {
        public string Value { get; }
        public bool IsHex { get; }

        public DtsNumberExpression(string value, bool isHex = false)
        {
            Value = value ?? throw new ArgumentNullException(nameof(value));
            IsHex = isHex;
        }

        public override string ToString() => Value;
    }

    /// <summary>
    /// 函数调用表达式
    /// </summary>
    public class DtsFunctionCallExpression : DtsExpression
    {
        public string FunctionName { get; }
        public List<DtsExpression> Arguments { get; }

        public DtsFunctionCallExpression(string functionName, List<DtsExpression> arguments)
        {
            FunctionName = functionName ?? throw new ArgumentNullException(nameof(functionName));
            Arguments = arguments ?? new List<DtsExpression>();
        }

        public override string ToString()
        {
            var args = string.Join(", ", Arguments.Select(a => a.ToString()));
            return $"{FunctionName}({args})";
        }
    }

    /// <summary>
    /// 二元运算表达式
    /// </summary>
    public class DtsBinaryExpression : DtsExpression
    {
        public DtsExpression Left { get; }
        public string Operator { get; }
        public DtsExpression Right { get; }

        public DtsBinaryExpression(DtsExpression left, string op, DtsExpression right)
        {
            Left = left ?? throw new ArgumentNullException(nameof(left));
            Operator = op ?? throw new ArgumentNullException(nameof(op));
            Right = right ?? throw new ArgumentNullException(nameof(right));
        }

        public override string ToString()
        {
            return $"({Left} {Operator} {Right})";
        }
    }

    /// <summary>
    /// 一元运算表达式
    /// </summary>
    public class DtsUnaryExpression : DtsExpression
    {
        public string Operator { get; }
        public DtsExpression Operand { get; }

        public DtsUnaryExpression(string op, DtsExpression operand)
        {
            Operator = op ?? throw new ArgumentNullException(nameof(op));
            Operand = operand ?? throw new ArgumentNullException(nameof(operand));
        }

        public override string ToString()
        {
            return $"{Operator}{Operand}";
        }
    }
}
