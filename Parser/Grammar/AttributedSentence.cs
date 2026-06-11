using HulkCompiler.Lexer;
using HulkCompiler.Parser.Ast;

namespace HulkCompiler.Parser.Grammar;

public class AttributedSentence(Sentence sentence, Func<Token[], AstNode[], AstNode> attributation)
{
    private readonly Sentence _sentence = sentence;

    private readonly Func<Token[], AstNode[], AstNode> _attributation = attributation;

    public int Length { get => _sentence.Length; }

    public Symbol this[int index] { get => _sentence[index]; }
    public Symbol First { get => _sentence[0]; }

    public override string ToString()
    {
        return _sentence.ToString() + _attributation.ToString();
    }

    public AstNode Attributate(Token[] tokens, AstNode[] nodes) => _attributation(tokens, nodes);

}