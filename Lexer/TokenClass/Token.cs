namespace Lexer.Token;
public enum TokenType { }



public class Token(string lex, TokenType type, int lineNumber, int columnStartNumber, int columnEndNumber)
{
    public string Lex { get; } = lex;
    public TokenType Type { get; } = type;
    public int LineNumber { get; } = lineNumber;
    public int ColumnStartNumber { get; } = columnStartNumber;
    public int ColumnEndNumber { get; } = columnEndNumber;
}