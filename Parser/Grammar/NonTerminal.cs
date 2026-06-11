using HulkCompiler.Lexer;
using HulkCompiler.Parser.Ast;


namespace HulkCompiler.Parser.Grammar;



public class NonTerminal(string value, Grammar grammar) : Symbol(value, grammar)
{
    public static NonTerminal operator %(NonTerminal self, (Sentence, Func<Token[], AstNode[], AstNode>) attributation)
    {
        AttributedSentence attributedSentence = new(attributation.Item1, attributation.Item2);

        if (self._grammar.Productions.TryGetValue(self, out SentenceList? value))
        {
            value.Append(attributedSentence);
        }
        else
        {
            self._grammar.Productions[self] = new([attributedSentence]);

        }

        return self;
    }

    public static NonTerminal operator %(NonTerminal self, (Symbol, Func<Token[], AstNode[], AstNode>) attributation)
    {
        AttributedSentence attributedSentence = new(new Sentence([attributation.Item1]), attributation.Item2);
        if (self._grammar.Productions.TryGetValue(self, out SentenceList? value))
        {
            value.Append(attributedSentence);
        }
        else
        {
            self._grammar.Productions[self] = new([attributedSentence]);

        }

        return self;
    }

    public override string ToString()
    {
        return _value.ToString();
    }
}