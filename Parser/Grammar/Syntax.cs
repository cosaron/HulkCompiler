using HulkCompiler.Lexer;

namespace HulkCompiler.Parser.Grammar;



abstract class Syntax
{
    public abstract (Grammar, Dictionary<TokenType, Symbol>) BuildSyntax();
}