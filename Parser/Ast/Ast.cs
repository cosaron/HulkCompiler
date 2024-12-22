namespace HulkCompiler.Parser.Ast;


using HulkCompiler.SemanticAnalizer;
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

}
public abstract class DefineStatement(NodePosition position) : AstNode(position);


public class ProgramNode : AstNode
{
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

    public DefineStatement[] Definitions { get; }
    public Expression Statement { get; }
}


public class DefineStatementList : DefineStatement
{
    public DefineStatementList(AstNode[] statements) : base(((-1, -1), (-1, -1)))
    {
        if (statements is DefineStatement[] _statements)
            Statements = _statements;
        else
            throw new Exception();
    }
    public DefineStatement[] Statements { get; private set; }

    public void AppendStatement(AstNode statement)
    {
        if (statement is DefineStatement _statement)
            Statements = new List<DefineStatement>(Statements) { _statement }.ToArray();
        else
            throw new Exception();
    }

    public void Extend(DefineStatement[] list)
    {
        Statements = new List<DefineStatement>(Statements).Concat(list).ToArray();
    }
}

public class TypeDeclaration(string type) : DefineStatement(((-1, -1), (-1, -1)))
{
    public string Type { get; } = type;
}


public class FunctionDefinitionList : DefineStatement
{
    public FunctionDefinitionList(AstNode[] functions, NodePosition position) : base(position)
    {
        if (functions is FunctionDefinitionNode[] _functions)
            Functions = _functions;
        else
            throw new Exception();
    }
    public FunctionDefinitionNode[] Functions { get; private set; }

    public void AppendFunction(AstNode function)
    {
        if (function is FunctionDefinitionNode _function)
            Functions = new List<FunctionDefinitionNode>(Functions) { _function }.ToArray();
        else
            throw new Exception();
    }
}
public class FunctionDefinitionNode : DefineStatement
{
    public FunctionDefinitionNode(string identifier, AstNode? body, AstNode? parameters, NodePosition position, string? staticReturnType = null) : base(position)
    {
        if (body is Expression _body)
        {
            Identifier = identifier;
            Body = _body;
            StaticReturnType = staticReturnType;
            if (parameters is ParameterListNode _parameters)
                Parameters = _parameters.Parameters;
            else if (parameters is null)
                Parameters = [];
            else
                throw new Exception();
        }
        else
            throw new Exception();
    }
    public string Identifier { get; }
    public ParameterNode[] Parameters { get; }
    public Expression? Body { get; }
    public string? StaticReturnType { get; }
}

public class ProtocolDefinitionNode : DefineStatement
{
    public ProtocolDefinitionNode(string identifier, AstNode? functions, NodePosition position, AstNode? extends) : base(position)
    {
        Identifier = identifier;

        if (extends is ExtendDeclarations _extends)
        {
            Extends = _extends.Extends;
        }
        else if (extends is null)
            Extends = [];
        else
            throw new Exception();


        if (functions is FunctionDefinitionList _functions)
        {
            Functions = _functions.Functions;
        }
        else if (functions is null)
            Functions = [];
        else
            throw new Exception();
    }

    public string Identifier { get; }

    public FunctionDefinitionNode[] Functions { get; }

    public string[] Extends { get; private set; }


}

public class ExtendDeclarations(string[] extends, NodePosition position) : AstNode(position)
{
    public string[] Extends { get; private set; } = extends;

    public void AddExtend(string extend) => Extends = new List<string>(Extends) { extend }.ToArray();
}

public class AttributeDefinitionNode : DefineStatement
{
    public AttributeDefinitionNode(string identifier, AstNode value, NodePosition position, string? staticType = null) : base(position)
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
    public string Identifier { get; }
    public Expression Value { get; }
    public string? StaticType { get; }
}

public class AttributeDefinitionListNode : DefineStatement
{
    public AttributeDefinitionListNode(AstNode? value, NodePosition position) : base(position)
    {
        if (value is AttributeDefinitionNode _value)
            Attributes = [_value];
        else if (value is null)
            Attributes = [];
        else
            throw new Exception();
    }
    public AttributeDefinitionNode[] Attributes { get; private set; }
    public void AppendAttribute(AstNode declaration)
    {
        if (declaration is AttributeDefinitionNode _declaration)
            Attributes = new List<AttributeDefinitionNode>(Attributes) { _declaration }.ToArray();
        else
            throw new Exception();
    }
}

public class InheritsNode : DefineStatement
{
    public InheritsNode(string identifier, AstNode? arguments, NodePosition position) : base(position)
    {
        Identifier = identifier;

        if (arguments is ExpressionBlockNode _arguments)
            Arguments = _arguments.Expressions;
        else if (arguments is null)
            Arguments = [];
        else
            throw new Exception();
    }
    public string Identifier { get; }
    public Expression[] Arguments { get; }
}

public class TypeDefinitionNode : DefineStatement
{

    public TypeDefinitionNode(string identifier, AstNode? parameters, AstNode? definitions, NodePosition position, AstNode? inherits = null) : base(position)
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



        if (inherits is InheritsNode _inherits)
            Inherits = _inherits;
        else if (inherits is null)
            Inherits = null;
        else
            throw new Exception();
        if (parameters is ParameterListNode _parameters)
        {
            Parameters = _parameters;
        }
        else if (parameters is null)
            Parameters = new ParameterListNode([], ((-1, -1), (-1, -1)));
        else
            throw new Exception();
    }
    public string Identifier { get; }
    public ParameterListNode Parameters { get; }
    public AttributeDefinitionNode[] Attributes { get; }
    public FunctionDefinitionNode[] Functions { get; }
    public InheritsNode? Inherits { get; }
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
    public NegativeNode(AstNode expression, NodePosition position, Type? inferredType = null) : base(position, inferredType)
    {
        if (expression is Expression ex)
            Expression = ex;
        else
            throw new Exception();

    }
    public Expression Expression { get; }
}

public class PositiveNode : Expression
{
    public PositiveNode(AstNode expression, NodePosition position, Type? inferredType = null) : base(position, inferredType)
    {
        if (expression is Expression ex)
            Expression = ex;
        else
            throw new Exception();
    }
    public Expression Expression { get; }
}

public class NotNode : Expression
{
    public NotNode(AstNode expression, NodePosition position, Type? inferredType = null) : base(position, inferredType)
    {
        if (expression is Expression ex)
            Expression = ex;
        else
            throw new Exception();
    }
    public Expression Expression { get; }
}

public class BinaryExpressionNode : Expression
{
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


    public Expression Left { get; }
    public Operator Operator { get; }
    public Expression Right { get; }

}

public class IndexNode : Expression
{
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
    public Expression Obj { get; }
    public Expression Index { get; }

}

public class IdentifierNode(string identifier, NodePosition position, Type? inferredType = null) : Expression(position, inferredType)
{
    public string Identifier { get; } = identifier;

}

public class ParameterNode(string identifier, NodePosition position, Type? inferredType = null, string? staticType = null) : Expression(position, inferredType)
{
    public string Identifier { get; } = identifier;
    public string? StaticType { get; } = staticType;
}

public class ParameterListNode : AstNode
{
    public ParameterListNode(AstNode[] parameters, NodePosition position) : base(position)
    {
        if (parameters is ParameterNode[] _parameters)
            Parameters = _parameters;
        else
            throw new Exception();
    }
    public ParameterNode[] Parameters { get; private set; }

    public void AppendParameter(AstNode expression)
    {
        if (expression is ParameterNode _parameter)
            Parameters = new List<ParameterNode>(Parameters) { _parameter }.ToArray();
        else
            throw new Exception();
    }
}

public class ExpressionBlockNode : Expression
{
    public ExpressionBlockNode(AstNode[] expressions, NodePosition position, Type? inferredType = null) : base(position, inferredType)
    {
        if (expressions is Expression[] _expressions)
            Expressions = _expressions;
        else
            throw new Exception();
    }
    public Expression[] Expressions { get; private set; }

    public void AppendExpression(AstNode expression)
    {
        if (expression is Expression _expression)
            Expressions = new List<Expression>(Expressions) { _expression }.ToArray();
        else
            throw new Exception();
    }
}
public class ForNode : Expression
{
    public ForNode(string indexIdentifier, AstNode iterable, AstNode body, NodePosition position, Type? inferredType = null, Type? indexType = null) : base(position, inferredType)
    {
        if (iterable is Expression _iterable && body is Expression _body)
        {
            IndexIdentifier = indexIdentifier;
            Iterable = _iterable;
            Body = _body;
            IterableType = indexType;
        }
        else
            throw new Exception();
    }
    public string IndexIdentifier { get; }
    public Expression Iterable { get; }
    public Expression Body { get; }
    public Type? IterableType { get; set; }
}

public class WhileNode : Expression
{
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
    public Expression Condition { get; }
    public Expression Body { get; }
}

public class IfNode : Expression
{
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
    public Expression Condition { get; }
    public Expression Body { get; }
    public ElifNode[] ElifClauses { get; }
    public Expression? ElseBody { get; }
}

public class ElifClausesNodes : AstNode
{
    public ElifClausesNodes(AstNode[] elifClauses, NodePosition position) : base(position)
    {
        if (elifClauses is ElifNode[] _elifClauses)
            ElifClauses = _elifClauses;
        else
            throw new Exception();
    }
    public ElifNode[] ElifClauses { get; private set; }

    public void AppendElif(AstNode elif)
    {
        if (elif is ElifNode _elif)
            ElifClauses = new List<ElifNode>(ElifClauses) { _elif }.ToArray();
        else
            throw new Exception();
    }
}

public class ElifNode : Expression
{
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
    public Expression Condition { get; }
    public Expression Body { get; }
}

public class VariableDeclarationNode : DefineStatement
{
    public VariableDeclarationNode(string identifier, AstNode value, NodePosition position, string? staticType = null) : base(position)
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
    public string Identifier { get; }
    public Expression Value { get; }
    public string? StaticType { get; }
}

public class VariableDeclarationListNode : DefineStatement
{
    public VariableDeclarationListNode(AstNode value, NodePosition position) : base(position)
    {
        if (value is VariableDeclarationNode _value)
            Declarations = [_value];
        else
            throw new Exception();
    }
    public VariableDeclarationNode[] Declarations { get; private set; }
    public void AppendDeclaration(AstNode declaration)
    {
        if (declaration is VariableDeclarationNode _declaration)
            Declarations = new List<VariableDeclarationNode>(Declarations) { _declaration }.ToArray();
        else
            throw new Exception();
    }
}

public class LetNode : Expression
{
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
    public VariableDeclarationNode[] Declarations { get; }
    public Expression Body { get; }
}

public class DestructiveAssignmentNode : Expression
{
    public DestructiveAssignmentNode(AstNode identifier, AstNode expression, NodePosition position, Type? inferredType = null) : base(position, inferredType)
    {
        if (identifier is Expression _identifier && expression is Expression _expression)
        {
            Identifier = _identifier;
            Expression = _expression;
        }
        else
            throw new Exception();
    }
    public Expression Identifier { get; }
    public Expression Expression { get; }
}



public class InstanciateNode : Expression
{
    public InstanciateNode(string identifier, AstNode[] parameters, NodePosition position, Type? inferredType = null) : base(position, inferredType)
    {
        if (parameters is Expression[] _parameters)
        {
            Identifier = identifier;
            Parameters = _parameters;
        }
        else
            throw new Exception();
    }
    public string Identifier { get; }
    public Expression[] Parameters { get; }

}

public class InvocationNode : Expression
{
    public InvocationNode(string identifier, AstNode[] arguments, NodePosition position, Type? inferredType = null) : base(position, inferredType)
    {
        if (arguments is Expression[] _arguments)
        {
            Identifier = identifier;
            Arguments = _arguments;
        }
        else
            throw new Exception();
    }
    public string Identifier { get; }
    public Expression[] Arguments { get; }
}

public class AttributeCallNode : Expression
{
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
    public Expression Obj { get; }
    public string Identifier { get; }
}

public class FunctionCallNode : Expression
{
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
    public Expression Obj { get; }
    public InvocationNode Invocation { get; }
}

public class VectorNode : Expression
{
    public VectorNode(AstNode[] elements, NodePosition position, Type? inferredType = null) : base(position, inferredType)
    {
        if (elements is Expression[] _elements)
            Elements = _elements;
        else
            throw new Exception();
    }
    public Expression[] Elements { get; }
}

public class ComprehensionVectorNode : Expression
{
    public ComprehensionVectorNode(AstNode generator, string identifier, AstNode iterator, NodePosition position, Type? inferredType = null) : base(position, inferredType)
    {
        if (generator is Expression _generator && iterator is Expression _iterator)
        {
            Generator = _generator;
            Identifier = identifier;
            Iterator = _iterator;
        }
        else
            throw new Exception();
    }
    public Expression Generator { get; }
    public string Identifier { get; }
    public Expression Iterator { get; }
}