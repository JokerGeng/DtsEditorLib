using System;
using System.Collections.Generic;
using System.Text;

namespace DtsParser
{
    // Token类型枚举
    public enum TokenType
    {
        // 基本符号
        LeftBrace,      // {
        RightBrace,     // }
        LeftParen,      // (
        RightParen,     // )
        LeftBracket,    // [
        RightBracket,   // ]
        LeftAngle,      // <
        RightAngle,     // >
        Semicolon,      // ;
        Comma,          // ,
        Equals,         // =
        Ampersand,      // &
        Slash,          // /

        // 数据类型
        Identifier,     // 标识符
        String,         // 字符串字面量
        Number,         // 数字
        HexNumber,      // 十六进制数字

        // 关键字和指令
        Include,        // #include
        Define,         // #define

        // 特殊
        Comment,        // 注释
        Newline,        // 换行
        EOF,            // 文件结束
        Unknown         // 未知token
    }
}
