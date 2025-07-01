namespace DtsParser.AST
{
    /// <summary>
    /// 字符串值
    /// </summary>
    public class DtsStringValue : DtsValue
    {
        public string Value { get; }

        public bool IsContainerQuot { get; }

        public DtsStringValue(string value, bool isContainerQuot = true)
        {
            Value = value;
            IsContainerQuot = isContainerQuot;
        }

        public override string ToString()
        {
            if (IsContainerQuot)
            {
                return $"\"{Value}\"";
            }
            else
            {
                return $"{Value}";
            }
        }
    }
}
