namespace DtsParser.Models
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
        /// <summary>
        /// | (位或)
        /// </summary>
        Pipe,
        /// <summary>
        ///  ^ (位异或)
        /// </summary>
        Caret,
        /// <summary>
        /// ~ (位取反)
        /// </summary>
        Tilde,
        /// <summary>
        /// <<
        /// </summary>
        LeftShift,
        /// <summary>
        /// >>
        /// </summary>
        RightShift,                 
        /// <summary>
        /// &&
        /// </summary>
        LogicalAnd,                 
        /// <summary>
        /// ||
        /// </summary>
        LogicalOr,    

        Bits,
        Memreserve,

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
