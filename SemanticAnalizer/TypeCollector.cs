using HulkCompiler.Parser.Ast;
using HulkCompiler.Utils;

namespace HulkCompiler.SemanticAnalizer
{


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


            //TODO improve this
            foreach (var definition in node.Definitions)
            {
                if (definition is TypeDefinitionNode typeDefinition)
                {
                    if (!types.TryAdd(typeDefinition.Identifier.Value, typeDefinition))
                        errorStack.AddError($"Type {typeDefinition.Identifier} already defined", typeDefinition.Identifier.Position.Start, typeDefinition.Identifier.Position.End);
                }
                else if (definition is ProtocolDefinitionNode protocolDefinition)
                {
                    if (!protocols.TryAdd(protocolDefinition.Identifier.Value, protocolDefinition))
                        errorStack.AddError($"Protocol {protocolDefinition.Identifier} already defined", protocolDefinition.Identifier.Position.Start, protocolDefinition.Identifier.Position.End);
                }
            }

            foreach (var definition in node.Definitions)
            {
                if (definition is TypeDefinitionNode typeDefinition)
                {
                    if (typeDefinition.Inherits is not null)
                    {
                        if (!types.TryGetValue(typeDefinition.Inherits.Identifier.Value, out TypeDefinitionNode? inherits))
                        {
                            errorStack.AddError($"Type {typeDefinition.Inherits} is not defined", typeDefinition.Inherits.Position.Start, typeDefinition.Inherits.Position.End);
                            continue;
                        }
                        if (!inheritsRelations.TryGetValue(inherits, out List<TypeDefinitionNode>? _))
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
                        {
                            errorStack.AddError($"Protocol {extend} is not defined", extend.Position.Start, extend.Position.End);
                            continue;
                        }
                        if (!extendsRelations.TryGetValue(extends, out List<ProtocolDefinitionNode>? _))
                            extendsRelations.Add(extends, []);
                        else
                            extendsRelations[extends].Add(protocolDefinition);
                    }
                }

            }

            TypeDefinitionNode[]? sortedTypes = Functions.TopologicalSort(inheritsRelations);
            if (sortedTypes is null)
            {
                errorStack.AddError("Circular inheritance detected", (-1, -1), (-1, -1));
                return;
            }
            ProtocolDefinitionNode[]? sortedProtocols = Functions.TopologicalSort(extendsRelations);
            if (sortedProtocols is null)
            {
                errorStack.AddError("Circular protocol extension detected", (-1, -1), (-1, -1));
                return;
            }

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
        public void Collect(AttributeDefinitionNode node, Context context, ErrorStack errorStack) { }
        public void Collect(TypeDefinitionNode node, Context context, ErrorStack errorStack)
        {
            Type type = context.GetType(node.Identifier.Value) ?? throw new Exception(); //TODO analize why this can posibly return null

            foreach (var param in node.Parameters)
            {
                Type? paramType = context.GetTypeOrDefault(param.StaticType?.Value);
                if (paramType is null)
                {
                    errorStack.AddError($"Type {param.StaticType!.Value} is not defined", param.StaticType!.Position);
                    return;
                }
                if (type.GetParameter(param.Identifier.Value) is not null)
                {
                    errorStack.AddError($"Parameter {param.Identifier} is already defined in type {type.Name}", param.Identifier.Position);
                    return;
                }
                type.SetParameter(new Variable(param.Identifier.Value, paramType));
            }

            foreach (var attr in node.Attributes)
            {
                Type? attrType = context.GetTypeOrDefault(attr.StaticType?.Value);
                if (attrType is null)
                {
                    errorStack.AddError($"Type {attr.StaticType!.Value} is not defined", attr.StaticType!.Position);
                    return;
                }
                if (type.GetAttribute(attr.Identifier.Value) is not null)
                {
                    errorStack.AddError($"Attribute {attr.Identifier} is already defined in type {type.Name}", attr.Identifier.Position);
                }
                type.SetAttribute(new Variable(attr.Identifier.Value, attrType));
            }

            foreach (var method in node.Functions)
            {
                if (type.GetMethod(method.Identifier.Value) is not null)
                {
                    errorStack.AddError($"The methods overload is not supported, duplicated method {method.Identifier.Value}", method.Identifier.Position);
                    return;
                }

                Type? returnType = context.GetTypeOrDefault(method.StaticReturnType?.Value);
                if (returnType is null)
                {
                    errorStack.AddError($"Type {method.StaticReturnType!.Value} is not defined", method.StaticReturnType.Position);
                    return;
                }

                List<Variable> createdParams = [];
                HashSet<string> visitedParams = [];

                foreach (var param in method.Parameters)
                {
                    Type? paramType = context.GetTypeOrDefault(param.StaticType?.Value);
                    if (paramType is null)
                    {
                        errorStack.AddError($"Type {param.StaticType!.Value} is not defined", param.StaticType!.Position);
                        return;
                    }
                    if (visitedParams.Contains(param.Identifier.Value))
                    {
                        errorStack.AddError($"Duplicated parameter {param.Identifier.Value} in method {method.Identifier.Value}", param.Identifier.Position);
                        return;
                    }
                    createdParams.Add(new Variable(param.Identifier.Value, paramType));
                    visitedParams.Add(param.Identifier.Value);
                }

                type.SetMethod(new Method(method.Identifier.Value, returnType, [.. createdParams]));
            }
        }
    }
}