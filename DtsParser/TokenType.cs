using System;
using System.Collections.Generic;
using System.Text;

namespace DtsParser
{
    // Token类型枚举
    public enum TokenType
    {
        // 基本符号
        LBRACE,         // {
        RBRACE,         // }
        SEMICOLON,      // ;
        EQUALS,         // =
        COMMA,          // ,
        LPAREN,         // (
        RPAREN,         // )
        LANGLE,         // <
        RANGLE,         // >
        SLASH,          // /

        // 字面量
        IDENTIFIER,     // 标识符
        STRING,         // 字符串字面量
        NUMBER,         // 数字

        // 关键字
        DELETE_NODE,    // /delete-node/
        DELETE_PROP,    // /delete-property/

        // 预处理指令
        INCLUDE,        // #include
        DEFINE,         // #define

        // 特殊
        REFERENCE,      // &label 或 &{/path}
        LABEL,          // label:
        NEWLINE,
        EOF,
        COMMENT,
        WHITESPACE
    }
}
