namespace HulkCompiler.Parser.Ast;

using System.Collections;
using HulkCompiler.SemanticAnalizer;
using HulkCompiler.Transpiler;
using NodePosition = ((int Line, int Column) Start, (int Line, int Column) End);



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



public abstract class AstNode(NodePosition position)
{
    public NodePosition Position { get; } = position;

    public void Collect(IAstTypeCollector collector, Context context, ErrorStack errorStack) => collector.Collect(this, context, errorStack);
    public void CollectAttr(IAstTypeCollector collector, Context context, ErrorStack errorStack) => collector.Collect(this, context, errorStack);
    public void Infer(IAstTypeInferer inferer, Context context, ErrorStack errorStack) => inferer.Infer(this, context, errorStack);

    public void Transpile(IHulkTranspiler transpiler, Context context, ErrorStack errorStack) => transpiler.Transpile(this, context, errorStack);


}
public abstract class DefineStatement(NodePosition position) : AstNode(position);


public class ProgramNode : AstNode
{
    public List<DefineStatement> Definitions { get; }
    public Expression Statement { get; }

    public ProgramNode(AstNode? definitions, AstNode statement, NodePosition position) : base(position)
    {
        if (definitions is DefineStatementList _definitions)
            Definitions = _definitions.Statements;
        else if (definitions is null)
            Definitions = [];
        else
            throw new Exception();


        if (statement is Expression _statement)
            Statement = _statement;
        else
            throw new Exception();
    }
}


public class DefineStatementList : DefineStatement
{
    public List<DefineStatement> Statements { get; private set; }

    public DefineStatementList(IEnumerable<AstNode> statements) : base(((-1, -1), (-1, -1)))
    {
        if (statements is IEnumerable<DefineStatement> _statements)
            Statements = [.. _statements];
        else
            throw new Exception();
    }

    public void AppendStatement(AstNode statement)
    {
        if (statement is DefineStatement _statement)
            Statements.Add(_statement);
        else
            throw new Exception();
    }

    public void Extend(IEnumerable<DefineStatement> list) => Statements.AddRange(list);
}

public class TypeDeclarationNode(string type, NodePosition position) : DefineStatement(position)
{
    public string Value { get; } = type;
}


public class FunctionDefinitionList : DefineStatement
{
    public List<FunctionDefinitionNode> Functions { get; private set; }

    public FunctionDefinitionList(IEnumerable<AstNode> functions, NodePosition position) : base(position)
    {
        if (functions is IEnumerable<FunctionDefinitionNode> _functions)
            Functions = [.. _functions];
        else
            throw new Exception();
    }

    public void AppendFunction(AstNode function)
    {
        if (function is FunctionDefinitionNode _function)
            Functions.Add(_function);
        else
            throw new Exception();
    }
}
public class FunctionDefinitionNode : DefineStatement
{
    public IdentifierNode Identifier { get; }
    public List<ParameterNode> Parameters { get; }
    public Expression? Body { get; }
    public TypeDeclarationNode? StaticReturnType { get; }

    public FunctionDefinitionNode(IdentifierNode identifier, AstNode? body, AstNode? parameters, NodePosition position, TypeDeclarationNode? staticReturnType = null) : base(position)
    {
        if (body is Expression _body)
        {
            Identifier = identifier;
            Body = _body;
            StaticReturnType = staticReturnType;

            Parameters = parameters switch
            {
                ParameterListNode _parameters => _parameters.Parameters,
                null => [],
                _ => throw new Exception(),
            };
        }
        else
            throw new Exception();
    }
}

public class ProtocolDefinitionNode : DefineStatement
{
    public IdentifierNode Identifier { get; }
    public List<FunctionDefinitionNode> Functions { get; }
    public ExtendDeclarations? Extends { get; private set; }

    public ProtocolDefinitionNode(IdentifierNode identifier, AstNode? functions, NodePosition position, AstNode? extends) : base(position)
    {
        Identifier = identifier;

        Extends = extends switch
        {
            ExtendDeclarations _extends => _extends,
            null => null,
            _ => throw new Exception(),
        };

        Functions = functions switch
        {
            FunctionDefinitionList _functions => _functions.Functions,
            null => [],
            _ => throw new Exception(),
        };
    }
}

public class ExtendDeclarations(List<IdentifierNode> extends, NodePosition position) : AstNode(position), IEnumerable<IdentifierNode>
{
    public List<IdentifierNode> Extends { get; private set; } = extends;

    public void AddExtend(IdentifierNode extend) => Extends.Add(extend);


    public IEnumerator<IdentifierNode> GetEnumerator() => Extends.GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}

public class AttributeDefinitionNode : DefineStatement
{
    public IdentifierNode Identifier { get; }
    public Expression Value { get; }
    public TypeDeclarationNode? StaticType { get; }

    public AttributeDefinitionNode(IdentifierNode identifier, AstNode value, NodePosition position, TypeDeclarationNode? staticType = null) : base(position)
    {
        if (value is Expression _value)
        {
            Identifier = identifier;
            Value = _value;
            StaticType = staticType;
        }
        else
            throw new Exception();
    }
}

public class AttributeDefinitionListNode : DefineStatement
{
    public List<AttributeDefinitionNode> Attributes { get; private set; }
    public AttributeDefinitionListNode(AstNode? value, NodePosition position) : base(position)
    {
        Attributes = value switch
        {
            AttributeDefinitionNode _value => [_value],
            null => [],
            _ => throw new Exception(),
        };
    }

    public void AppendAttribute(AstNode declaration)
    {
        if (declaration is AttributeDefinitionNode _declaration)
            Attributes.Add(_declaration);
        else
            throw new Exception();
    }
}

public class InheritsNode : DefineStatement
{
    public IdentifierNode Identifier { get; }
    public List<Expression> Arguments { get; }

    public InheritsNode(IdentifierNode identifier, AstNode? arguments, NodePosition position) : base(position)
    {
        Identifier = identifier;

        Arguments = arguments switch
        {
            ExpressionBlockNode _arguments => _arguments.Expressions,
            null => [],
            _ => throw new Exception(),
        };
    }
}

public class TypeDefinitionNode : DefineStatement
{
    public IdentifierNode Identifier { get; }
    public ParameterListNode Parameters { get; }
    public AttributeDefinitionNode[] Attributes { get; }
    public FunctionDefinitionNode[] Functions { get; }
    public InheritsNode? Inherits { get; }

    public TypeDefinitionNode(IdentifierNode identifier, AstNode? parameters, AstNode? definitions, NodePosition position, AstNode? inherits = null) : base(position)
    {
        Identifier = identifier;

        List<AttributeDefinitionNode> attributes = [];
        List<FunctionDefinitionNode> functions = [];

        if (definitions is DefineStatementList _definitions)
        {
            foreach (var definition in _definitions.Statements)
            {
                if (definition is AttributeDefinitionNode _attribute)
                    attributes.Add(_attribute);
                else if (definition is FunctionDefinitionNode _function)
                    functions.Add(_function);
                else
                    throw new Exception();

            }

            Attributes = [.. attributes];
            Functions = [.. functions];
        }
        else if (definitions is null)
        {
            Attributes = [];
            Functions = [];
        }
        else
            throw new Exception();



        Inherits = inherits switch
        {
            InheritsNode _inherits => _inherits,
            null => null,
            _ => throw new Exception(),
        };

        Parameters = parameters switch
        {
            ParameterListNode _parameters => _parameters,
            null => new ParameterListNode([], ((-1, -1), (-1, -1))),
            _ => throw new Exception(),
        };
    }
}


public abstract class Expression(NodePosition position, Type? inferredType) : AstNode(position)
{
    public Type? InferredType { get; set; } = inferredType;

}


public class LiteralNode(string value, NodePosition position, Type? inferredType = null) : Expression(position, inferredType)
{
    public string Value { get; } = value;

}

public class NegativeNode : Expression
{
    public Expression Expression { get; }
    public NegativeNode(AstNode expression, NodePosition position, Type? inferredType = null) : base(position, inferredType)
    {
        if (expression is Expression ex)
            Expression = ex;
        else
            throw new Exception();

    }
}

public class PositiveNode : Expression
{
    public Expression Expression { get; }
    public PositiveNode(AstNode expression, NodePosition position, Type? inferredType = null) : base(position, inferredType)
    {
        if (expression is Expression ex)
            Expression = ex;
        else
            throw new Exception();
    }
}

public class NotNode : Expression
{
    public Expression Expression { get; }
    public NotNode(AstNode expression, NodePosition position, Type? inferredType = null) : base(position, inferredType)
    {
        if (expression is Expression ex)
            Expression = ex;
        else
            throw new Exception();
    }
}

public class BinaryExpressionNode : Expression
{
    public Expression Left { get; }
    public Operator Operator { get; }
    public Expression Right { get; }
    public BinaryExpressionNode(AstNode left, Operator op, AstNode right, NodePosition position, Type? inferredType = null) : base(position, inferredType)
    {
        if (left is Expression l && right is Expression r)
        {
            Left = l;
            Right = r;
        }
        else
            throw new Exception();
        Operator = op;
    }
}

public class IndexNode : Expression
{
    public Expression Obj { get; }
    public Expression Index { get; }
    public IndexNode(AstNode obj, AstNode index, NodePosition position, Type? inferredType = null) : base(position, inferredType)
    {
        if (obj is Expression _obj && index is Expression _index)
        {
            Obj = _obj;
            Index = _index;
        }
        else
            throw new Exception();
    }

}

public class IdentifierNode(string identifier, NodePosition position, Type? inferredType = null) : Expression(position, inferredType)
{
    public string Value { get; } = identifier;

    public override string ToString() => Value;

}

public class ParameterNode : Expression
{
    public IdentifierNode Identifier { get; }
    public TypeDeclarationNode? StaticType { get; }

    public ParameterNode(AstNode identifier, NodePosition position, Type? inferredType = null, AstNode? staticType = null) : base(position, inferredType)
    {
        if (identifier is IdentifierNode _identifier)
        {
            Identifier = _identifier;
            if (staticType is TypeDeclarationNode _staticType)
                StaticType = _staticType;
            else if (staticType is null)
                StaticType = null;
            else
                throw new Exception();

        }
        else
            throw new Exception();
    }
}

public class ParameterListNode : AstNode, IEnumerable<ParameterNode>
{
    public List<ParameterNode> Parameters { get; private set; }
    public ParameterListNode(IEnumerable<AstNode> parameters, NodePosition position) : base(position)
    {
        if (parameters is IEnumerable<ParameterNode> _parameters)
            Parameters = [.. _parameters];
        else
            throw new Exception();
    }

    public void AppendParameter(AstNode expression)
    {
        if (expression is ParameterNode _parameter)
            Parameters.Add(_parameter);
        else
            throw new Exception();
    }

    public IEnumerator<ParameterNode> GetEnumerator() => Parameters.GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}

public class ExpressionBlockNode : Expression
{
    public List<Expression> Expressions { get; private set; }
    public ExpressionBlockNode(IEnumerable<AstNode> expressions, NodePosition position, Type? inferredType = null) : base(position, inferredType)
    {
        if (expressions is IEnumerable<Expression> _expressions)
            Expressions = [.. _expressions];
        else
            throw new Exception();
    }

    public void AppendExpression(AstNode expression)
    {
        if (expression is Expression _expression)
            Expressions.Add(_expression);
        else
            throw new Exception();
    }
}
public class ForNode : Expression
{
    public IdentifierNode IndexIdentifier { get; }
    public Expression Iterable { get; }
    public Expression Body { get; }
    public Type? IterableType { get; set; }

    public ForNode(AstNode indexIdentifier, AstNode iterable, AstNode body, NodePosition position, Type? inferredType = null, Type? indexType = null) : base(position, inferredType)
    {
        if (indexIdentifier is IdentifierNode _identifier && iterable is Expression _iterable && body is Expression _body)
        {
            IndexIdentifier = _identifier;
            Iterable = _iterable;
            Body = _body;
            IterableType = indexType;
        }
        else
            throw new Exception();
    }

}

public class WhileNode : Expression
{
    public Expression Condition { get; }
    public Expression Body { get; }

    public WhileNode(AstNode condition, AstNode body, NodePosition position, Type? inferredType = null) : base(position, inferredType)
    {
        if (condition is Expression _condition && body is Expression _body)
        {
            Condition = _condition;
            Body = _body;
        }
        else
            throw new Exception();
    }

}

public class IfNode : Expression
{
    public Expression Condition { get; }
    public Expression Body { get; }
    public List<ElifNode> ElifClauses { get; }
    public Expression? ElseBody { get; }

    public IfNode(AstNode condition, AstNode body, AstNode? elifClauses, AstNode? elseBody, NodePosition position, Type? inferredType = null) : base(position, inferredType)
    {
        if (condition is Expression _condition && body is Expression _body)
        {
            Condition = _condition;
            Body = _body;


            if (elifClauses is ElifClausesNodes _elifClauses)
                ElifClauses = _elifClauses.ElifClauses;
            else if (elifClauses is null)
                ElifClauses = [];
            else
                throw new Exception();

            if (elseBody is Expression _elseBody)
                ElseBody = _elseBody;
            else if (elseBody is null)
                ElseBody = null;
            else
                throw new Exception();
        }
        else
            throw new Exception();

    }

}

public class ElifClausesNodes : AstNode
{
    public List<ElifNode> ElifClauses { get; private set; }
    public ElifClausesNodes(IEnumerable<AstNode> elifClauses, NodePosition position) : base(position)
    {
        if (elifClauses is IEnumerable<ElifNode> _elifClauses)
            ElifClauses = [.. _elifClauses];
        else
            throw new Exception();
    }

    public void AppendElif(AstNode elif)
    {
        if (elif is ElifNode _elif)
            ElifClauses.Add(_elif);
        else
            throw new Exception();
    }
}

public class ElifNode : Expression
{
    public Expression Condition { get; }
    public Expression Body { get; }

    public ElifNode(AstNode condition, AstNode body, NodePosition position, Type? inferredType = null) : base(position, inferredType)
    {
        if (condition is Expression _condition && body is Expression _body)
        {
            Condition = _condition;
            Body = _body;
        }
        else
            throw new Exception();
    }

}

public class VariableDeclarationNode : DefineStatement
{
    public IdentifierNode Identifier { get; }
    public Expression Value { get; }
    public TypeDeclarationNode? StaticType { get; }

    public VariableDeclarationNode(AstNode identifier, AstNode value, NodePosition position, TypeDeclarationNode? staticType = null) : base(position)
    {
        if (identifier is IdentifierNode _identifier && value is Expression _value)
        {
            Identifier = _identifier;
            Value = _value;
            StaticType = staticType;
        }
        else
            throw new Exception();
    }
}

public class VariableDeclarationListNode : DefineStatement
{
    public List<VariableDeclarationNode> Declarations { get; private set; }

    public VariableDeclarationListNode(AstNode value, NodePosition position) : base(position)
    {
        if (value is VariableDeclarationNode _value)
            Declarations = [_value];
        else
            throw new Exception();
    }
    public void AppendDeclaration(AstNode declaration)
    {
        if (declaration is VariableDeclarationNode _declaration)
            Declarations.Add(_declaration);
        else
            throw new Exception();
    }
}

public class LetNode : Expression
{
    public List<VariableDeclarationNode> Declarations { get; set; }
    public Expression Body { get; set; }

    public LetNode(AstNode declarations, AstNode body, NodePosition position, Type? inferredType = null) : base(position, inferredType)
    {
        if (declarations is VariableDeclarationListNode _declarations && body is Expression _body)
        {
            Declarations = _declarations.Declarations;
            Body = _body;
        }
        else
            throw new Exception();
    }

    public LetNode(List<VariableDeclarationNode> declarations, AstNode body, NodePosition position, Type? inferredType = null) : base(position, inferredType)
    {
        Declarations = declarations;
        if (body is Expression _body)
            Body = _body;
        else
            throw new Exception();
    }
}

public class DestructiveAssignmentNode : Expression
{
    public IdentifierNode Identifier { get; }
    public Expression Expression { get; }

    public DestructiveAssignmentNode(AstNode identifier, AstNode expression, NodePosition position, Type? inferredType = null) : base(position, inferredType)
    {
        if (identifier is IdentifierNode _identifier && expression is Expression _expression)
        {
            Identifier = _identifier;
            Expression = _expression;
        }
        else
            throw new Exception();
    }
}



public class InstanciateNode : Expression
{
    public IdentifierNode Identifier { get; }
    public Expression[] Parameters { get; }

    public InstanciateNode(AstNode identifier, IEnumerable<AstNode> parameters, NodePosition position, Type? inferredType = null) : base(position, inferredType)
    {
        if (identifier is IdentifierNode _identifier && parameters is IEnumerable<Expression> _parameters)
        {
            Identifier = _identifier;
            Parameters = [.. _parameters];
        }
        else
            throw new Exception();
    }
}

public class InvocationNode : Expression
{
    public IdentifierNode Identifier { get; }
    public Expression[] Arguments { get; }

    public InvocationNode(IdentifierNode identifier, IEnumerable<AstNode> arguments, NodePosition position, Type? inferredType = null) : base(position, inferredType)
    {
        if (identifier is IdentifierNode _identifier && arguments is IEnumerable<Expression> _arguments)
        {
            Identifier = _identifier;
            Arguments = [.. _arguments];
        }
        else
            throw new Exception();
    }
}

public class AttributeCallNode : Expression
{
    public Expression Obj { get; }
    public string Identifier { get; }

    public AttributeCallNode(AstNode obj, string identifier, NodePosition position, Type? inferredType = null) : base(position, inferredType)
    {
        if (obj is Expression _expression)
        {
            Obj = _expression;
            Identifier = identifier;
        }
        else
            throw new Exception();
    }
}

public class FunctionCallNode : Expression
{
    public Expression Obj { get; }
    public InvocationNode Invocation { get; }

    public FunctionCallNode(AstNode obj, AstNode invocation, NodePosition position, Type? inferredType = null) : base(position, inferredType)
    {
        if (obj is Expression _obj && invocation is InvocationNode _invocation)
        {
            Obj = _obj;
            Invocation = _invocation;
        }
        else
            throw new Exception();
    }
}

public class VectorNode : Expression
{
    public Expression[] Elements { get; }

    public VectorNode(IEnumerable<AstNode> elements, NodePosition position, Type? inferredType = null) : base(position, inferredType)
    {
        if (elements is IEnumerable<Expression> _elements)
            Elements = [.. _elements];
        else
            throw new Exception();
    }
}

public class ComprehensionVectorNode : Expression
{
    public Expression Generator { get; }
    public IdentifierNode Identifier { get; }
    public Expression Iterator { get; }

    public ComprehensionVectorNode(AstNode generator, AstNode identifier, AstNode iterator, NodePosition position, Type? inferredType = null) : base(position, inferredType)
    {
        if (generator is Expression _generator && iterator is Expression _iterator && identifier is IdentifierNode _identifier)
        {
            Generator = _generator;
            Identifier = _identifier;
            Iterator = _iterator;
        }
        else
            throw new Exception();
    }
}