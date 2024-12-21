namespace HulkCompiler.Lexer;



public class LexerGenerator(Dictionary<string, TokenType>? patterns = null) : ILexeable
{
    public Token[] Tokenize(string input)
    {
        throw new NotImplementedException();
    }
}