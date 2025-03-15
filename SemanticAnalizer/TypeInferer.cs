using HulkCompiler.Parser.Ast;

namespace HulkCompiler.SemanticAnalizer
{
    public interface IAstTypeInferer
    {
        void Infer(AstNode node, Context context, ErrorStack errorStack);
        void Infer(ProgramNode node, Context context, ErrorStack errorStack);
        void Infer(TypeDefinitionNode node, Context context, ErrorStack errorStack);
        void Infer(ProtocolDefinitionNode node, Context context, ErrorStack errorStack);
        void Infer(FunctionDefinitionNode node, Context context, ErrorStack errorStack);
        void Infer(AttributeDefinitionNode node, Context context, ErrorStack errorStack);
        void Infer(InheritsNode node, Context context, ErrorStack errorStack);
        void Infer(ForNode node, Context context, ErrorStack errorStack);
        void Infer(WhileNode node, Context context, ErrorStack errorStack);
        void Infer(IfNode node, Context context, ErrorStack errorStack);
        void Infer(ElifNode node, Context context, ErrorStack errorStack);
        void Infer(ExpressionBlockNode node, Context context, ErrorStack errorStack);
        void Infer(VariableDeclarationNode node, Context context, ErrorStack errorStack);
        void Infer(LetNode node, Context context, ErrorStack errorStack);
        void Infer(DestructiveAssignmentNode node, Context context, ErrorStack errorStack);
        void Infer(FunctionCallNode node, Context context, ErrorStack errorStack);
        void Infer(InstanciateNode node, Context context, ErrorStack errorStack);
        void Infer(AttributeCallNode node, Context context, ErrorStack errorStack);
        void Infer(IdentifierNode node, Context context, ErrorStack errorStack);
        void Infer(VectorNode node, Context context, ErrorStack errorStack);
        void Infer(ComprehensionVectorNode node, Context context, ErrorStack errorStack);
        void Infer(IndexNode node, Context context, ErrorStack errorStack);
        void Infer(BinaryExpressionNode node, Context context, ErrorStack errorStack);
        void Infer(PositiveNode node, Context context, ErrorStack errorStack);
        void Infer(NegativeNode node, Context context, ErrorStack errorStack);
        void Infer(NotNode node, Context context, ErrorStack errorStack);
        void Infer(LiteralNode node, Context context, ErrorStack errorStack);
    }

    public class TypeInferer : IAstTypeInferer
    {
        public void Infer(AstNode node, Context context, ErrorStack errorStack) { }

        public void Infer(ProgramNode node, Context context, ErrorStack errorStack)
        {
            throw new NotImplementedException();
        }

        public void Infer(TypeDefinitionNode node, Context context, ErrorStack errorStack)
        {
            throw new NotImplementedException();
        }

        public void Infer(ProtocolDefinitionNode node, Context context, ErrorStack errorStack)
        {
            throw new NotImplementedException();
        }

        public void Infer(FunctionDefinitionNode node, Context context, ErrorStack errorStack)
        {
            throw new NotImplementedException();
        }

        public void Infer(AttributeDefinitionNode node, Context context, ErrorStack errorStack)
        {
            throw new NotImplementedException();
        }

        public void Infer(InheritsNode node, Context context, ErrorStack errorStack)
        {
            throw new NotImplementedException();
        }

        public void Infer(ForNode node, Context context, ErrorStack errorStack)
        {
            throw new NotImplementedException();
        }

        public void Infer(WhileNode node, Context context, ErrorStack errorStack)
        {
            throw new NotImplementedException();
        }

        public void Infer(IfNode node, Context context, ErrorStack errorStack)
        {
            throw new NotImplementedException();
        }

        public void Infer(ElifNode node, Context context, ErrorStack errorStack)
        {
            throw new NotImplementedException();
        }

        public void Infer(ExpressionBlockNode node, Context context, ErrorStack errorStack)
        {
            throw new NotImplementedException();
        }

        public void Infer(VariableDeclarationNode node, Context context, ErrorStack errorStack)
        {
            throw new NotImplementedException();
        }

        public void Infer(LetNode node, Context context, ErrorStack errorStack)
        {
            throw new NotImplementedException();
        }

        public void Infer(DestructiveAssignmentNode node, Context context, ErrorStack errorStack)
        {
            node.Identifier.Infer(this, context, errorStack);
            node.Expression.Infer(this, context, errorStack);

            node.InferredType = node.Expression.InferredType;
        }

        public void Infer(FunctionCallNode node, Context context, ErrorStack errorStack)
        {
            node.Obj.Infer(this, context, errorStack);

            Method? method = node.Obj.InferredType?.GetMethod(node.Invocation.Identifier.Value);

            if (method is null)
            {
                errorStack.AddError($"The type {node.Obj.InferredType?.Name} does not have a method called {node.Invocation.Identifier.Value}", node.Position);
                return;
            }

            node.InferredType = method.ReturnType;

            foreach (var arg in node.Invocation.Arguments)
            {
                arg.Infer(this, context, errorStack);
            }


        }

        public void Infer(InstanciateNode node, Context context, ErrorStack errorStack)
        {
            Type? type = context.GetType(node.Identifier.Value);
            if (type is not null)
                node.InferredType = type;
            else
            {
                errorStack.AddError($"Type {node.Identifier.Value} is not defined", node.Position);
                return;
            }
        }

        public void Infer(AttributeCallNode node, Context context, ErrorStack errorStack)
        {
            node.Obj.Infer(this, context, errorStack);
            node.InferredType = node.Obj.InferredType?.GetAttribute(node.Identifier)?.Type;
        }

        public void Infer(IdentifierNode node, Context context, ErrorStack errorStack)
        {
            Type? variableType = context.GetVariableType(node.Value);
            if (variableType is null)
            {
                errorStack.AddError($"Variable {node.Value} is not defined", node.Position);
                return;
            }

            node.InferredType = variableType;
        }

        public void Infer(VectorNode node, Context context, ErrorStack errorStack)
        {
            foreach (var element in node.Elements)
            {
                element.Infer(this, context, errorStack);
            }
            node.InferredType = new VectorType(node.Elements[0].InferredType ?? UnknownType.Instance);
        }

        public void Infer(ComprehensionVectorNode node, Context context, ErrorStack errorStack)
        {
            node.Iterator.Infer(this, context, errorStack);

            if (context.IterableProtocol.IsImplementedBy(node.Iterator.InferredType ?? UnknownType.Instance))
            {
                Context child = context.CreateChildContext();
                Method currentMethod = node.Iterator.InferredType?.GetMethod("current") ?? throw new Exception("The iterator must have a method called current");

                child.DefineVariable(variable: new Variable(name: node.Identifier.Value, type: currentMethod.ReturnType));
                node.Generator.Infer(this, child, errorStack);
                node.InferredType = new VectorType(node.Generator.InferredType ?? UnknownType.Instance);
            }

        }

        public void Infer(IndexNode node, Context context, ErrorStack errorStack)
        {
            node.Obj.Infer(this, context, errorStack);
            node.Index.Infer(this, context, errorStack);

            if (node.Obj.InferredType is VectorType)
                node.InferredType = node.Obj.InferredType.GetMethod("current")?.ReturnType;

        }

        public void Infer(BinaryExpressionNode node, Context context, ErrorStack errorStack)
        {
            node.Left.Infer(this, context, errorStack);
            node.Right.Infer(this, context, errorStack);

            switch (node.Operator)
            {
                case Operator.ADD or Operator.SUB or Operator.MUL or Operator.DIV or Operator.POW or Operator.MOD:
                    node.InferredType = NumberType.Instance;
                    break;

                case Operator.AND or Operator.OR:
                    node.InferredType = BooleanType.Instance;
                    break;

                case Operator.EQ or Operator.NEQ or Operator.GT or Operator.GTE or Operator.LT or Operator.LTE:
                    node.InferredType = BooleanType.Instance;
                    break;

                case Operator.IS:
                    node.InferredType = BooleanType.Instance;
                    break;

                case Operator.AS:
                    node.InferredType = node.Right.InferredType;
                    break;

                case Operator.CONCAT or Operator.DCONCAT:
                    node.InferredType = StringType.Instance;
                    break;

                default:
                    throw new NotImplementedException();

            }
        }
        public void Infer(PositiveNode node, Context context, ErrorStack errorStack)
            => node.InferredType = NumberType.Instance;

        public void Infer(NegativeNode node, Context context, ErrorStack errorStack)
            => node.InferredType = NumberType.Instance;

        public void Infer(NotNode node, Context context, ErrorStack errorStack)
            => node.InferredType = BooleanType.Instance;

        public void Infer(LiteralNode node, Context context, ErrorStack errorStack) { }
    }

}