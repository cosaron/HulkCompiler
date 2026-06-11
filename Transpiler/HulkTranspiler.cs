// using HulkCompiler.Parser.Ast;
// using HulkCompiler.SemanticAnalizer;

// namespace HulkCompiler.Transpiler;


// public interface IHulkTranspiler
// {
//     AstNode Transpile(AstNode node, Context context, ErrorStack errorStack);
//     AstNode Transpile(LetNode node, Context context, ErrorStack errorStack);
//     AstNode Transpile(ForNode node, Context context, ErrorStack errorStack);
//     // AstNode Transpile(ComprehensionVectorNode node, Context context, ErrorStack errorStack);
// }


// public class HulkTranspiler : IHulkTranspiler
// {
//     public AstNode Transpile(AstNode node, Context context, ErrorStack errorStack)
//     {
//         throw new NotImplementedException();
//     }

//     public AstNode Transpile(LetNode node, Context context, ErrorStack errorStack)
//     {
//         LetNode transpiledNode = node;
//         if (transpiledNode.Declarations.Count > 1)
//         {
//             List<VariableDeclarationNode> otherDeclarations = transpiledNode.Declarations.GetRange(1, transpiledNode.Declarations.Count - 1);
//             Expression otherBody = transpiledNode.Body;
//             transpiledNode.Declarations = [transpiledNode.Declarations[0]];
//             transpiledNode.Body = new LetNode(otherDeclarations, otherBody, transpiledNode.Position);
//             transpiledNode.Body.Transpile(this, context, errorStack);
//         }

//         return transpiledNode;
//     }

//     public AstNode Transpile(ForNode node, Context context, ErrorStack errorStack)
//     {
//         LetNode transpiledNode = new(
//             declarations: [
//                 new VariableDeclarationNode(new IdentifierNode("iterable", node.Position), node.Iterable, node.Position)
//             ],
//             body: new WhileNode(
//                 condition: new FunctionCallNode(
//                     obj: new IdentifierNode("iterable", node.Position),
//                     invocation: new InvocationNode(
//                         identifier: new IdentifierNode("next", node.Position),
//                         arguments: [],
//                         position: node.Position,
//                         inferredType: BooleanType.Instance
//                     ),
//                     position: node.Position
//                 ),
//                 body: new LetNode(
//                     declarations: [
//                         new VariableDeclarationNode(
//                             identifier: node.IndexIdentifier,
//                             value: new FunctionCallNode(
//                                 obj: new IdentifierNode(
//                                     identifier: "iterable", position:
//                                     node.Position,
//                                     inferredType: node.Iterable.InferredType
//                                 ),
//                                 invocation: new InvocationNode(
//                                     identifier: new IdentifierNode("next", node.Position),
//                                     arguments: [],
//                                     position: node.Position,
//                                     inferredType: node.IterableType
//                                 ),
//                                 position: node.Position,
//                                 inferredType: node.Body.InferredType
//                             ),
//                             position: node.Position
//                         )
//                     ],
//                     body: node.Body,
//                     position: node.Position
//                 ),
//                 position: node.Position
//             ),
//             position: node.Position
//         );

//         return transpiledNode;
//     }

//     // public AstNode Transpile(ComprehensionVectorNode node, Context context, ErrorStack errorStack)
//     // {
//     //     LetNode transpiledNode = new(
//     //         declarations: [
//     //             new VariableDeclarationNode(
//     //                 identifier: new IdentifierNode("list", node.Position),
//     //                 value: new VectorNode(
//     //                     elements: from x in Enumerable.Range(node.Iterator.)
//     //                 )
//     //             )
//     //         ]
//     //     );
//     // }

// }