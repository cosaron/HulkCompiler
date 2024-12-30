namespace HulkCompiler.SemanticAnalizer;

using HulkCompiler.Parser.Ast;
using HulkCompiler.Utils;

public interface IAstTypeCollector
{
    public void Collect(AstNode node, Context context, ErrorStack errorStack);
    public void Collect(ProgramNode node, Context context, ErrorStack errorStack);
    public void Collect(FunctionDefinitionNode node, Context context, ErrorStack errorStack);
    public void Collect(ProtocolDefinitionNode node, Context context, ErrorStack errorStack);
    public void Collect(AttributeDefinitionNode node, Context context, ErrorStack errorStack);
    public void Collect(TypeDefinitionNode node, Context context, ErrorStack errorStack);
}


public class TypeCollector : IAstTypeCollector
{
    public void Collect(AstNode node, Context context, ErrorStack errorStack) { }
    public void Collect(ProgramNode node, Context context, ErrorStack errorStack)
    {
        Dictionary<string, TypeDefinitionNode> types = [];
        Dictionary<string, ProtocolDefinitionNode> protocols = [];
        Dictionary<TypeDefinitionNode, List<TypeDefinitionNode>> inheritsRelations = [];
        Dictionary<ProtocolDefinitionNode, List<ProtocolDefinitionNode>> extendsRelations = [];

        foreach (var definition in node.Definitions)
        {
            if (definition is TypeDefinitionNode typeDefinition)
            {
                if (types.ContainsKey(typeDefinition.Identifier.Value))
                    //TODO add error to the stack(type already defined)
                    throw new Exception();
                types.Add(typeDefinition.Identifier.Value, typeDefinition);
            }
            else if (definition is ProtocolDefinitionNode protocolDefinition)
            {
                if (protocols.ContainsKey(protocolDefinition.Identifier.Value))
                    //TODO add error to the stack(protocol already defined)
                    throw new Exception();
                protocols.Add(protocolDefinition.Identifier.Value, protocolDefinition);
            }
        }

        foreach (var definition in node.Definitions)
        {
            if (definition is TypeDefinitionNode typeDefinition)
            {
                if (typeDefinition.Inherits is not null)
                {
                    if (!types.TryGetValue(typeDefinition.Inherits.Identifier.Value, out TypeDefinitionNode? inherits))
                        //TODO add error to the stack(type not found)
                        throw new Exception();
                    if (!inheritsRelations.TryGetValue(inherits, out List<TypeDefinitionNode>? relations))
                        inheritsRelations.Add(inherits, []);
                    else
                        inheritsRelations[inherits].Add(typeDefinition);
                }
            }
            else if (definition is ProtocolDefinitionNode protocolDefinition && protocolDefinition.Extends is not null)
            {
                foreach (var extend in protocolDefinition.Extends)
                {
                    if (!protocols.TryGetValue(extend.Value, out ProtocolDefinitionNode? extends))
                        //TODO add error to the stack(protocol not found)
                        throw new Exception();
                    if (!extendsRelations.TryGetValue(extends, out List<ProtocolDefinitionNode>? relations))
                        extendsRelations.Add(extends, []);
                    else
                        extendsRelations[extends].Add(protocolDefinition);
                }
            }

        }

        TypeDefinitionNode[]? sortedTypes = Functions.TopologicalSort(inheritsRelations);
        if (sortedTypes is null)
            //TODO add error to the stack(circular inheritance)
            throw new Exception();

        ProtocolDefinitionNode[]? sortedProtocols = Functions.TopologicalSort(extendsRelations);
        if (sortedProtocols is null)
            //TODO add error to the stack(circular inheritance)
            throw new Exception();

        foreach (var type in sortedTypes)
            type.Collect(this, context, errorStack);

        foreach (var protocol in sortedProtocols)
            protocol.Collect(this, context, errorStack);


    }


    public void Collect(ProtocolDefinitionNode node, Context context, ErrorStack errorStack)
    {
        if (context.ContainsProtocol(node.Identifier.Value))
        {
            errorStack.AddError($"Protocol {node.Identifier} already defined", node.Position.Start, node.Position.End);
            return;
        }
        Protocol newProtocol = new(node.Identifier.Value);
        if (node.Extends is not null)
        {
            foreach (var extendsIdentifier in node.Extends)
            {
                Protocol? extendProtocol = context.GetProtocol(extendsIdentifier.Value);
                if (extendProtocol is not null)
                    errorStack.AddError($"Protocol {extendsIdentifier} is not defined", node.Extends.Position.Start, node.Extends.Position.End);
            }
        }

        context.DefineProtocol(newProtocol);
    }

    public void Collect(TypeDefinitionNode node, Context context, ErrorStack errorStack)
    {
        if (context.ContainsType(node.Identifier.Value))
            errorStack.AddError($"Type {node.Identifier} already defined", node.Position.Start, node.Position.End);

        Type newType = new(node.Identifier.Value);

        if (node.Inherits is not null)
        {
            Type? parent = context.GetType(node.Inherits.Identifier.Value);
            if (parent is null)
            {
                errorStack.AddError($"Type {node.Inherits.Identifier} is not defined", node.Inherits.Position.Start, node.Inherits.Position.End);
                return;
            }
            if (new Type[] { BooleanType.Instance, NumberType.Instance, StringType.Instance }.Contains(parent))
            {
                errorStack.AddError($"Type {node.Identifier} cannot inherit from {parent.Name}", node.Position.Start, node.Position.End);
                return;
            }
            if (node.Parameters.Any())
            {
                if (parent.Parameters.Count == 0)
                {
                    errorStack.AddError($"missing initialization expression for the parent type type {newType.Name}. hint: when you want to override the arguments of a type which inherits from another type you must provide initialization for the parent type", node.Position.Start, node.Inherits.Position.Start);
                    return;
                }
            }
            else
            {
                foreach (var param in parent.Parameters)
                    newType.SetParameter(param);
            }

            newType.SetParent(parent);
        }
        else
            newType.SetParent(ObjectType.Instance);

        context.DefineType(newType);
    }
    public void Collect(FunctionDefinitionNode node, Context context, ErrorStack errorStack) { }

    public void Collect(AttributeDefinitionNode node, Context context, ErrorStack errorStack) { }
}



public class TypeAttrCollector : IAstTypeCollector
{
    public void Collect(AstNode node, Context context, ErrorStack errorStack) { }
    public void Collect(ProgramNode node, Context context, ErrorStack errorStack)
    {
        foreach (var definition in node.Definitions)
            definition.CollectAttr(this, context, errorStack);
    }

    public void Collect(FunctionDefinitionNode node, Context context, ErrorStack errorStack)
    {
        throw new NotImplementedException();
    }

    public void Collect(ProtocolDefinitionNode node, Context context, ErrorStack errorStack)
    {
        Protocol protocol = context.GetProtocol(node.Identifier.Value) ?? throw new Exception(); //TODO analize why this can posibly return null
        foreach (var function in node.Functions)
        {
            if (protocol.GetMethod(function.Identifier.Value) is not null)
            {
                errorStack.AddError($"Function {function.Identifier} is already defined in protocol {protocol.Name}", function.Identifier.Position);
                return;
            }

            Type? returnType = context.GetTypeOrDefault(function.StaticReturnType?.Value);
            if (returnType is null)
            {
                errorStack.AddError($"Type {function.StaticReturnType} is not defined", function.Position);
                return;
            }

        }
    }

    public void Collect(AttributeDefinitionNode node, Context context, ErrorStack errorStack)
    {
        throw new NotImplementedException();
    }

    public void Collect(TypeDefinitionNode node, Context context, ErrorStack errorStack)
    {
        throw new NotImplementedException();
    }
}