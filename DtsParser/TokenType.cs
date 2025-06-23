namespace DtsParser
{
    // Token类型枚举
    public enum TokenType
    {
        // 基本符号
        LeftBrace, RightBrace,      // { }
        LeftParen, RightParen,      // ( )
        LeftAngle, RightAngle,      // < >
        LeftBracket, RightBracket,  // [ ]
        Comma, Semicolon, Colon,    // , ; :
        Equals, Ampersand,          // = &
        Minus, Plus, Dot,           // - + .
        Slash,                      // /

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
        At
    }
}
