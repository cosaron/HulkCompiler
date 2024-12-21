namespace HulkCompiler.Lexer;

using System.Text.RegularExpressions;

public class LexerRegex(Dictionary<string, TokenType>? patterns = null) : ILexeable
{
    private Dictionary<Regex, TokenType> _patterns { get; } = (patterns is null ? HulkTokenPatter.Patterns : patterns).ToDictionary(
            pair => new Regex(pair.Key),
            pair => pair.Value
        );

    public Token[] Tokenize(string input)
    {
        List<Token> tokens = [];
        int line = 0;
        int column = 0;

        int i = 0;
        while (i < input.Length)
        {
            switch (input[i])
            {
                case '\n':
                    i++;
                    line++;
                    column = 0;
                    continue;
                case '\r':
                    i++;
                    if (i < input.Length && input[i] == '\n')
                    {
                        i++;
                    }
                    line++;
                    column = 0;
                    continue;
                case ' ':
                    i++;
                    column++;
                    continue;
                case '\t':
                    i++;
                    column += 4;
                    continue;
                default:
                    break;
            }
            bool finded = false;
            foreach (var (pattern, type) in _patterns)
            {
                Match match = pattern.Match(input, i);
                if (match.Success && match.Index == i)
                {
                    finded = true;
                    string lex = match.Value;

                    tokens.Add(new Token(
                        lex: lex,
                        type: type,
                        lineNumber: line,
                        columnStartNumber: column,
                        columnEndNumber: column + lex.Length
                    ));

                    i += lex.Length;
                    column += lex.Length;
                    break;
                }
            }
            if (!finded)
            {
                throw new Exception($"Unexpected character{input[i..]} at line {line}, column {column}");
            }

        }

        return [.. tokens, new Token(
            lex: "$",
            type: TokenType.EOF,
            lineNumber: line,
            column,
            column
        )];
    }

}