namespace DtsParser
{
    // Token类型枚举
    public enum TokenType
    {
        // 基本符号
        /// <summary>
        /// {
        /// </summary>
        LeftBrace,
        /// <summary>
        /// }
        /// </summary>
        RightBrace,
        /// <summary>
        /// (
        /// </summary>
        LeftParen,
        /// <summary>
        /// )
        /// </summary>
        RightParen,
        /// <summary>
        /// <
        /// </summary>
        LeftAngle,
        /// <summary>
        /// >
        /// </summary>
        RightAngle,
        /// <summary>
        /// [
        /// </summary>
        LeftBracket,
        /// <summary>
        /// ]
        /// </summary>
        RightBracket,
        /// <summary>
        /// ,
        /// </summary>
        Comma, 
        /// <summary>
        /// ;
        /// </summary>
        Semicolon, 
        /// <summary>
        /// :
        /// </summary>
        Colon,    
        /// <summary>
        /// =
        /// </summary>
        Equals, 
        /// <summary>
        /// &
        /// </summary>
        Ampersand, 
        /// <summary>
        /// -
        /// </summary>
        Minus, 
        /// <summary>
        /// +
        /// </summary>
        Plus, 
        /// <summary>
        /// .
        /// </summary>
        Dot,    
        /// <summary>
        /// /
        /// </summary>
        Slash,
        /// <summary>
        /// @
        /// </summary>
        At,
        // 位运算符和逻辑运算符
        Pipe,                       // | (位或)
        Caret,                      // ^ (位异或)
        Tilde,                      // ~ (位取反)
        LeftShift,                  // <<
        RightShift,                 // >>
        LogicalAnd,                 // &&
        LogicalOr,                  // ||

        // 字面量
        Identifier, String, Number, HexNumber, Character,

        // 关键字和指令
        Include, Preprocessor, Define,

        //注释
        Comment,

        // 特殊
        Newline, EOF,
    }
}
