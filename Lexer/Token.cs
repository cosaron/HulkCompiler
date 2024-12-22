namespace HulkCompiler.Lexer;

public class Token(string lex, TokenType type, int lineNumber = -1, int columnStartNumber = -1, int columnEndNumber = -1)
{
    private (int LineNumber, int ColumnStartNumber) Start { get; } = (lineNumber, columnStartNumber);
    private (int LineNumber, int ColumnEndNumber) End { get; } = (lineNumber, columnEndNumber);

    public string Lex { get; } = lex;
    public TokenType Type { get; } = type;
    public ((int, int) Start, (int, int) End) Position => (Start, End);
}

public enum TokenType

{
    AND,
    OR,
    NOT,
    IF,
    ELSE,
    ELIF,
    WHILE,
    FOR,
    LET,
    IN,
    ASSIGNMENT,
    DESTRUCTIVE_ASSIGNMENT,
    NEW,
    AS,
    IDENTIFIER,

    STRING,
    NUMBER,
    BOOLEAN,
    FUNCTION,
    TYPE,
    INHERITS,
    PROTOCOL,
    EXTENDS,

    STRING_LITERAL,
    NUMBER_LITERAL,
    TRUE_LITERAL,
    FALSE_LITERAL,

    PLUS,
    MINUS,
    TIMES,
    DIVIDE,
    MOD,
    POW,

    EQUAL,
    NOT_EQUAL,
    LESS_THAN,
    GREATER_THAN,
    LESS_THAN_EQUAL,
    GREATER_THAN_EQUAL,
    IS,

    DOT,
    COMMA,
    COLON,
    SEMICOLON,
    LEFT_PARENTHESIS,
    RIGHT_PARENTHESIS,
    LEFT_BRACKET,
    RIGHT_BRACKET,
    LEFT_BRACE,
    RIGHT_BRACE,

    PI,
    E,

    CONCAT,
    DOUBLE_CONCAT,
    ARROW_OP,

    LINE_COMMENT,
    MULTI_LINE_COMMENT_START,
    MULTI_LINE_COMMENT_END,
    DOUBLE_PIPE,
    EOF,
}
