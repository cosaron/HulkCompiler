namespace HulkCompiler.Lexer;

public interface ILexeable
{
    public Token[] Tokenize(string input);
}