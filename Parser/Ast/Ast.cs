using SemanticAnalizer.Types;
using NodePosition = (int line, int columnStart, int columnEnd);

namespace Parser.Ast;


public enum Operator
{
    ADD,
    SUB,
    MUL,
    DIV,
    MOD,
    POW,
    AND,
    OR,
    EQ,
    NEQ,
    LT,
    GT,
    LTE,
    GTE,
    CONCAT,
    DCONCAT,
    IS,
    AS,
}


public class AstNode
{
    public NodePosition Position { get; }

}


public abstract class Expression : AstNode
{
    public TypeClass InferredType { get; set; }

}

public abstract class DefineStatement;


public class LiteralNode(string value, TypeClass inferredType, NodePosition position) : Expression
{
    public string Value { get; } = value;
    public NodePosition Position { get; } = position;

}

public class NegativeNode(Expression expression, TypeClass inferredType, NodePosition position) : Expression
{
    public Expression Expression { get; } = expression;
    public TypeClass InferredType { get; set; } = inferredType;
    public NodePosition Position { get; } = position;
}