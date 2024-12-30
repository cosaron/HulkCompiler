namespace HulkCompiler.Parser.Grammar;

using HulkCompiler.Lexer;
using HulkCompiler.Parser.Ast;
using HulkCompiler.SemanticAnalizer;
using HulkCompiler.Utils;


public static class HulkGrammar
{
    public static (Grammar, Dictionary<TokenType, Symbol>) GetGrammar()
    {
        Grammar grammar = new();

        var (program, defineStatement, statement, typeDefinition, functionDefinition, protocolDefinition) = grammar.SetNonTerminals("program", "statement", "typeDefinition", "functionDefinition", "protocolDefinition");
        var (expressionBlock, expression, statementList, ifExpression, invocationExpression, aritmeticExpression) = grammar.SetNonTerminals("expressionBlock", "expression", "statementList", "ifExpression", "invocationExpression", "aritmeticExpression");
        var (multExpression, exponentialExpression, unaryExpression, primaryExpression, literal, vector) = grammar.SetNonTerminals("multExpression", "exponentialExpression", "unaryExpression", "primaryExpression", "literal", "vector");
        var (comprehensionVector, indexedValue, argumentList, destructiveAssignment, memberAccess, multipleDeclaration) = grammar.SetNonTerminals("comprehensionVector", "indexedValue", "argumentList", "destructiveAssignment", "memberAccess", "multipleDeclaration");
        var (argumentListDefinition, orExpression, andExpression, equalityExpression, relationalExpression, ifStatement) = grammar.SetNonTerminals("argumentListDefinition", "orExpression", "andExpression", "equalityExpression", "relationalExpression", "ifStatement");
        var (elifStatement, attributeDefinition, typeInherits, inheritsDeclaration, typeBody, typeArguments) = grammar.SetNonTerminals("elifStatement", "attributeDefinition", "typeInherits", "inheritsDeclaration", "typeBody", "typeArguments");
        var (instatiation, extendsDefinition, protocolBody, protocolArgumentsDefinition, extendsMultipleIdentifier, protocolMultipleArgumentsDefinition) = grammar.SetNonTerminals("instatiation", "extendsDefinition", "protocolBody", "protocolArgumentsDefinition", "extendsMultipleIdentifier", "protocolMultipleArgumentsDefinition");
        var (vectorElement, headProgram, elifExpression, concatExpression, controlStatement, whileHeader) = grammar.SetNonTerminals("vectorElement", "headProgram", "elifExpression", "concatExpression", "controlStatement", "whileHeader");
        var (forHeader, letHeader, controlExpression, inlineFunction, blockFunction, typeDeclaration) = grammar.SetNonTerminals("forHeader", "letHeader", "controlExpression", "inlineFunction", "blockFunction", "typeDeclaration");
        var (typeAttributes, typeFunctions) = grammar.SetNonTerminals("typeAttributes", "typeFunctions");

        var (openBrace, closeBrace, semicolon, plus, minus, times) = grammar.SetTerminals("{", "}", ";", "+", "-", "*");
        var (divide, power, mod, openParenthesis, closeParenthesis, comma) = grammar.SetTerminals("/", "^", "%", "(", ")", ",");
        var (concat, doubleConcat, dot, assignmentTerminal, destructiveAssignmentTerminal, inline) = grammar.SetTerminals("@", "@@", ".", "=", ":=", "=>");
        var (colon, notOperator, orTerminal, andTerminal, equalTerminal, notEqualTerminal) = grammar.SetTerminals(":", "!", "|", "&", "==", "!=");
        var (less, lessEqual, greater, greaterEqual, openBracket, closeBracket) = grammar.SetTerminals("<", "<=", ">", ">=", "[", "]");
        var (identifier, doublePipe, letTerminal, inTerminal, functionTerminal, numberLiteral) = grammar.SetTerminals("identifier", "||", "let", "in", "function", "number");
        var (stringLiteral, trueTerminal, falseTerminal, isTerminal, asTerminal) = grammar.SetTerminals("stringLiteral", "true", "false", "is", "as");
        var (ifTerminal, elifTerminal, elseTerminal, whileTerminal, forTerminal, typeTerminal) = grammar.SetTerminals("if", "elif", "else", "while", "for", "type");
        var (inheritsTerminal, newTerminal, protocolTerminal, extendsTerminal, numberTypeDeclaration, StringTypeDeclaration) = grammar.SetTerminals("inherits", "new", "protocol", "extends", "number", "string");
        var (boolTypeDeclaration, objectTypeDeclaration) = grammar.SetTerminals("bool", "object");

        grammar.SetSeed(program);


        program %= (headProgram + statement, (_, nodes) => new ProgramNode(nodes[0], nodes[1], nodes[1].Position));
        program %= (statement, (_, nodes) => new ProgramNode(null, nodes[0], nodes[0].Position));

        headProgram %= (headProgram + defineStatement, (_, nodes) =>
        {
            var defineStatementList = nodes[0] as DefineStatementList ?? throw new Exception();
            defineStatementList.AppendStatement(nodes[1]);
            return defineStatementList;
        }
        );
        headProgram %= (defineStatement, (_, nodes) => new DefineStatementList([nodes[0]]));

        defineStatement %= (functionTerminal + functionDefinition, (_, nodes) => nodes[0]);
        defineStatement %= (typeDefinition, (_, nodes) => nodes[0]);
        defineStatement %= (protocolDefinition, (_, nodes) => nodes[0]);

        typeDefinition %= (typeTerminal + identifier + typeArguments + typeInherits + openBrace + typeBody + closeBrace, (tokens, nodes) => new TypeDefinitionNode(new IdentifierNode(tokens[1].Lex, tokens[1].Position), nodes[0], nodes[2], (tokens[0].Position.Start, tokens[3].Position.End), nodes[1]));
        typeDefinition %= (typeTerminal + identifier + typeArguments + typeInherits + openBrace + closeBrace, (tokens, nodes) => new TypeDefinitionNode(new IdentifierNode(tokens[1].Lex, tokens[1].Position), nodes[0], null, (tokens[0].Position.Start, tokens[3].Position.End), nodes[1]));
        typeDefinition %= (typeTerminal + identifier + typeArguments + openBrace + typeBody + closeBrace, (tokens, nodes) => new TypeDefinitionNode(new IdentifierNode(tokens[1].Lex, tokens[1].Position), nodes[0], null, (tokens[0].Position.Start, tokens[3].Position.End), nodes[0]));
        typeDefinition %= (typeTerminal + identifier + typeArguments + openBrace + closeBrace, (tokens, nodes) => new TypeDefinitionNode(new IdentifierNode(tokens[1].Lex, tokens[1].Position), nodes[0], null, (tokens[0].Position.Start, tokens[3].Position.End)));
        typeDefinition %= (typeTerminal + identifier + typeInherits + openBrace + typeBody + closeBrace, (tokens, nodes) => new TypeDefinitionNode(new IdentifierNode(tokens[1].Lex, tokens[1].Position), null, nodes[1], (tokens[0].Position.Start, tokens[3].Position.End), nodes[0]));
        typeDefinition %= (typeTerminal + identifier + typeInherits + openBrace + closeBrace, (tokens, nodes) => new TypeDefinitionNode(new IdentifierNode(tokens[1].Lex, tokens[1].Position), null, null, (tokens[0].Position.Start, tokens[3].Position.End), nodes[0]));
        typeDefinition %= (typeTerminal + identifier + openBrace + typeBody + closeBrace, (tokens, nodes) => new TypeDefinitionNode(new IdentifierNode(tokens[1].Lex, tokens[1].Position), null, nodes[0], (tokens[0].Position.Start, tokens[3].Position.End)));
        typeDefinition %= (typeTerminal + identifier + openBrace + closeBrace, (tokens, _) => new TypeDefinitionNode(new IdentifierNode(tokens[1].Lex, tokens[1].Position), null, null, (tokens[0].Position.Start, tokens[3].Position.End)));

        typeBody %= (typeAttributes + typeFunctions, (_, nodes) =>
        {
            var attributes = nodes[0] as AttributeDefinitionListNode ?? throw new Exception();
            var functions = nodes[1] as FunctionDefinitionList ?? throw new Exception();
            DefineStatementList definitions = new(attributes.Attributes);
            definitions.Extend(functions.Functions);
            return definitions;
        }
        );
        typeBody %= (typeAttributes, (_, nodes) =>
        {
            var attributes = nodes[0] as AttributeDefinitionListNode ?? throw new Exception();
            return new DefineStatementList(attributes.Attributes);
        }
        );
        typeBody %= (typeFunctions, (_, nodes) =>
        {
            var functions = nodes[0] as FunctionDefinitionList ?? throw new Exception();
            return new DefineStatementList(functions.Functions);
        }
        );


        typeAttributes %= (typeAttributes + attributeDefinition, (_, nodes) =>
        {
            var attributeList = nodes[0] as AttributeDefinitionListNode ?? throw new Exception();
            attributeList.AppendAttribute(nodes[1]);
            return attributeList;
        }
        );
        typeAttributes %= (attributeDefinition, (_, nodes) => new AttributeDefinitionListNode(nodes[0], nodes[0].Position));
        typeFunctions %= (typeFunctions + functionDefinition, (_, nodes) =>
        {
            var functionList = nodes[0] as FunctionDefinitionList ?? throw new Exception();
            functionList.AppendFunction(nodes[1]);
            return functionList;
        }
        );
        typeFunctions %= (functionDefinition, (_, nodes) => new FunctionDefinitionList([nodes[0]], nodes[0].Position));

        attributeDefinition %= (identifier + typeDeclaration + assignmentTerminal + expression + semicolon, (tokens, nodes) => new AttributeDefinitionNode(new IdentifierNode(tokens[0].Lex, tokens[0].Position), nodes[0], (tokens[0].Position.Start, tokens[2].Position.End), tokens[1].Lex));
        attributeDefinition %= (identifier + assignmentTerminal + expression + semicolon, (tokens, nodes) => new AttributeDefinitionNode(new IdentifierNode(tokens[0].Lex, tokens[0].Position), nodes[0], (tokens[0].Position.Start, tokens[2].Position.End)));

        typeArguments %= (openParenthesis + argumentListDefinition + closeParenthesis, (_, nodes) => nodes[0]);

        typeInherits %= (inheritsTerminal + identifier + inheritsDeclaration, (tokens, nodes) => new InheritsNode(new IdentifierNode(tokens[0].Lex, tokens[0].Position), nodes[0], (tokens[0].Position.Start, nodes[0].Position.End)));
        typeInherits %= (inheritsTerminal + identifier, (tokens, _) => new InheritsNode(new IdentifierNode(tokens[1].Lex, tokens[1].Position), null, (tokens[0].Position.Start, tokens[1].Position.End)));

        typeDeclaration %= (colon + identifier, (tokens, _) => new TypeDeclarationNode(tokens[0].Lex, tokens[0].Position));
        typeDeclaration %= (colon + numberTypeDeclaration, (tokens, _) => new TypeDeclarationNode(tokens[0].Lex, tokens[0].Position));
        typeDeclaration %= (colon + StringTypeDeclaration, (tokens, _) => new TypeDeclarationNode(tokens[0].Lex, tokens[0].Position));
        typeDeclaration %= (colon + boolTypeDeclaration, (tokens, _) => new TypeDeclarationNode(tokens[0].Lex, tokens[0].Position));
        typeDeclaration %= (colon + objectTypeDeclaration, (tokens, _) => new TypeDeclarationNode(tokens[0].Lex, tokens[0].Position));

        inheritsDeclaration %= (openParenthesis + argumentList + closeParenthesis, (_, nodes) => nodes[0]);

        functionDefinition %= (inlineFunction, (_, nodes) => nodes[0]);
        functionDefinition %= (blockFunction, (_, nodes) => nodes[0]);

        inlineFunction %= (identifier + openParenthesis + argumentListDefinition + closeParenthesis + typeDeclaration + inline + statement, (tokens, nodes) => new FunctionDefinitionNode(new IdentifierNode(tokens[0].Lex, tokens[0].Position), nodes[1], nodes[0], position: (tokens[0].Position.Start, nodes[1].Position.End), staticReturnType: new TypeDeclarationNode(tokens[3].Lex, tokens[3].Position)));
        inlineFunction %= (identifier + openParenthesis + argumentListDefinition + closeParenthesis + inline + statement, (tokens, nodes) => new FunctionDefinitionNode(new IdentifierNode(tokens[0].Lex, tokens[0].Position), nodes[1], nodes[0], (tokens[0].Position.Start, nodes[1].Position.End)));
        inlineFunction %= (identifier + openParenthesis + closeParenthesis + typeDeclaration + inline + statement, (tokens, nodes) => new FunctionDefinitionNode(new IdentifierNode(tokens[0].Lex, tokens[0].Position), nodes[0], null, position: (tokens[0].Position.Start, nodes[1].Position.End), staticReturnType: new TypeDeclarationNode(tokens[3].Lex, tokens[3].Position)));
        inlineFunction %= (identifier + openParenthesis + closeParenthesis + inline + statement, (tokens, nodes) => new FunctionDefinitionNode(new IdentifierNode(tokens[0].Lex, tokens[0].Position), nodes[0], null, (tokens[0].Position.Start, nodes[0].Position.End)));

        blockFunction %= (identifier + openParenthesis + argumentListDefinition + closeParenthesis + typeDeclaration + expressionBlock, (tokens, nodes) => new FunctionDefinitionNode(new IdentifierNode(tokens[0].Lex, tokens[0].Position), nodes[1], nodes[0], position: (tokens[0].Position.Start, nodes[1].Position.End), staticReturnType: new TypeDeclarationNode(tokens[3].Lex, tokens[3].Position)));
        blockFunction %= (identifier + openParenthesis + argumentListDefinition + closeParenthesis + expressionBlock, (tokens, nodes) => new FunctionDefinitionNode(new IdentifierNode(tokens[0].Lex, tokens[0].Position), nodes[1], nodes[0], (tokens[0].Position.Start, nodes[1].Position.End)));
        blockFunction %= (identifier + openParenthesis + closeParenthesis + typeDeclaration + expressionBlock, (tokens, nodes) => new FunctionDefinitionNode(new IdentifierNode(tokens[0].Lex, tokens[0].Position), nodes[0], null, position: (tokens[0].Position.Start, nodes[0].Position.End), staticReturnType: new TypeDeclarationNode(tokens[3].Lex, tokens[3].Position)));
        blockFunction %= (identifier + openParenthesis + closeParenthesis + expressionBlock, (tokens, nodes) => new FunctionDefinitionNode(new IdentifierNode(tokens[0].Lex, tokens[0].Position), nodes[0], null, (tokens[0].Position.Start, nodes[0].Position.End)));

        argumentListDefinition %= (argumentListDefinition + comma + identifier + typeDeclaration, (tokens, nodes) =>
        {
            var parameters = nodes[0] as ParameterListNode ?? throw new Exception();
            parameters.AppendParameter(new ParameterNode(new IdentifierNode(tokens[0].Lex, tokens[0].Position), tokens[0].Position, staticType: new TypeDeclarationNode(tokens[1].Lex, tokens[1].Position)));
            return parameters;
        }
        );
        argumentListDefinition %= (argumentListDefinition + comma + identifier, (tokens, nodes) =>
        {
            var parameters = nodes[0] as ParameterListNode ?? throw new Exception();
            parameters.AppendParameter(new ParameterNode(new IdentifierNode(tokens[0].Lex, tokens[0].Position), tokens[0].Position));
            return parameters;
        }
        );
        argumentListDefinition %= (identifier + typeDeclaration, (tokens, _) => new ParameterListNode([new ParameterNode(new IdentifierNode(tokens[0].Lex, tokens[0].Position), tokens[0].Position, staticType: new TypeDeclarationNode(tokens[1].Lex, tokens[1].Position))], tokens[0].Position));
        argumentListDefinition %= (identifier, (tokens, _) => new ParameterListNode([new ParameterNode(new IdentifierNode(tokens[0].Lex, tokens[0].Position), tokens[0].Position)], tokens[0].Position));

        protocolDefinition %= (protocolTerminal + identifier + extendsDefinition + openBrace + protocolBody + closeBrace, (tokens, nodes) => new ProtocolDefinitionNode(new IdentifierNode(tokens[1].Lex, tokens[1].Position), nodes[0], (tokens[0].Position.Start, tokens[3].Position.End), nodes[1]));
        protocolDefinition %= (protocolTerminal + identifier + extendsDefinition + openBrace + closeBrace, (tokens, nodes) => new ProtocolDefinitionNode(new IdentifierNode(tokens[1].Lex, tokens[1].Position), null, (tokens[0].Position.Start, tokens[3].Position.End), nodes[0]));
        protocolDefinition %= (protocolTerminal + identifier + openBrace + protocolBody + closeBrace, (tokens, nodes) => new ProtocolDefinitionNode(new IdentifierNode(tokens[1].Lex, tokens[1].Position), nodes[0], (tokens[0].Position.Start, tokens[3].Position.End), null));
        protocolDefinition %= (protocolTerminal + identifier + openBrace + closeBrace, (tokens, nodes) => new ProtocolDefinitionNode(new IdentifierNode(tokens[1].Lex, tokens[1].Position), null, (tokens[0].Position.Start, tokens[3].Position.End), null));

        extendsDefinition %= (extendsTerminal + identifier + extendsMultipleIdentifier, (tokens, nodes) =>
        {
            var extendsDeclaration = nodes[0] as ExtendDeclarations ?? throw new Exception();
            extendsDeclaration.AddExtend(new IdentifierNode(tokens[0].Lex, tokens[0].Position));
            return extendsDeclaration;
        }
        );
        extendsDefinition %= (extendsTerminal + identifier, (tokens, _) => new ExtendDeclarations([new IdentifierNode(tokens[0].Lex, tokens[0].Position)], tokens[0].Position));

        extendsMultipleIdentifier %= (comma + identifier + extendsMultipleIdentifier, (tokens, nodes) =>
        {
            var extendsDeclaration = nodes[0] as ExtendDeclarations ?? throw new Exception();
            extendsDeclaration.AddExtend(new IdentifierNode(tokens[0].Lex, tokens[0].Position));
            return extendsDeclaration;
        }
        );
        extendsMultipleIdentifier %= (comma + identifier, (tokens, _) => new ExtendDeclarations([new IdentifierNode(tokens[0].Lex, tokens[0].Position)], tokens[0].Position));

        protocolBody %= (protocolBody + identifier + openParenthesis + protocolArgumentsDefinition + closeParenthesis + typeDeclaration + semicolon, (tokens, nodes) =>
        {
            var functionList = nodes[0] as FunctionDefinitionList ?? throw new Exception();
            functionList.AppendFunction(new FunctionDefinitionNode(new IdentifierNode(tokens[0].Lex, tokens[0].Position), null, nodes[1], position: (tokens[0].Position.Start, tokens[3].Position.End), staticReturnType: new TypeDeclarationNode(tokens[3].Lex, tokens[3].Position)));
            return functionList;
        }
        );
        protocolBody %= (protocolBody + identifier + openParenthesis + closeParenthesis + typeDeclaration + semicolon, (tokens, nodes) =>
        {
            var functionList = nodes[0] as FunctionDefinitionList ?? throw new Exception();
            functionList.AppendFunction(new FunctionDefinitionNode(new IdentifierNode(tokens[0].Lex, tokens[0].Position), null, null, position: (tokens[0].Position.Start, tokens[3].Position.End), staticReturnType: new TypeDeclarationNode(tokens[3].Lex, tokens[3].Position)));
            return functionList;
        }
        );
        protocolBody %= (identifier + openParenthesis + protocolArgumentsDefinition + closeParenthesis + typeDeclaration + semicolon, (tokens, nodes) => new FunctionDefinitionList([new FunctionDefinitionNode(new IdentifierNode(tokens[0].Lex, tokens[0].Position), null, nodes[0], position: (tokens[0].Position.Start, tokens[3].Position.End), staticReturnType: new TypeDeclarationNode(tokens[3].Lex, tokens[3].Position))], position: (tokens[0].Position.Start, tokens[3].Position.End)));
        protocolBody %= (identifier + openParenthesis + closeParenthesis + typeDeclaration + semicolon, (tokens, nodes) => new FunctionDefinitionList([new FunctionDefinitionNode(new IdentifierNode(tokens[0].Lex, tokens[0].Position), null, null, position: (tokens[0].Position.Start, tokens[3].Position.End), staticReturnType: new TypeDeclarationNode(tokens[3].Lex, tokens[3].Position))], position: (tokens[0].Position.Start, tokens[3].Position.End)));

        protocolArgumentsDefinition %= (identifier + typeDeclaration + protocolMultipleArgumentsDefinition, (tokens, nodes) =>
        {
            var parameters = nodes[0] as ParameterListNode ?? throw new Exception();
            parameters.AppendParameter(new ParameterNode(new IdentifierNode(tokens[0].Lex, tokens[0].Position), tokens[0].Position, staticType: new TypeDeclarationNode(tokens[2].Lex, tokens[2].Position)));
            return parameters;
        }
        );
        protocolArgumentsDefinition %= (identifier + typeDeclaration, (tokens, _) => new ParameterListNode([new ParameterNode(new IdentifierNode(tokens[0].Lex, tokens[0].Position), tokens[0].Position, staticType: new TypeDeclarationNode(tokens[1].Lex, tokens[1].Position))], tokens[0].Position));

        protocolMultipleArgumentsDefinition %= (comma + identifier + typeDeclaration + protocolMultipleArgumentsDefinition, (tokens, nodes) =>
        {
            var parameters = nodes[0] as ParameterListNode ?? throw new Exception();
            parameters.AppendParameter(new ParameterNode(new IdentifierNode(tokens[0].Lex, tokens[0].Position), tokens[0].Position, staticType: new TypeDeclarationNode(tokens[2].Lex, tokens[2].Position)));
            return parameters;
        }
        );
        protocolMultipleArgumentsDefinition %= (comma + identifier + typeDeclaration, (tokens, _) => new ParameterListNode([new ParameterNode(new IdentifierNode(tokens[1].Lex, tokens[1].Position), tokens[1].Position, staticType: new TypeDeclarationNode(tokens[2].Lex, tokens[2].Position))], tokens[1].Position));

        expressionBlock %= (openBrace + statementList + closeBrace, (_, nodes) => nodes[0]);

        statementList %= (statementList + statement, (_, nodes) =>
        {
            var expressionBlock = nodes[0] as ExpressionBlockNode ?? throw new Exception();
            expressionBlock.AppendExpression(nodes[1]);
            return expressionBlock;
        }
        );
        statementList %= (statement, (_, nodes) => new ExpressionBlockNode([nodes[0]], nodes[0].Position));

        statement %= (expressionBlock + semicolon, (_, nodes) => nodes[0]);
        statement %= (expressionBlock, (_, nodes) => nodes[0]);
        statement %= (orExpression + semicolon, (_, nodes) => nodes[0]);
        statement %= (destructiveAssignment + semicolon, (_, nodes) => nodes[0]);
        statement %= (controlStatement + semicolon, (_, nodes) => nodes[0]);

        controlStatement %= (ifStatement, (_, nodes) => nodes[0]);
        controlStatement %= (whileHeader + statement, (tokens, nodes) => new WhileNode(nodes[0], nodes[1], (nodes[0].Position.Start, nodes[1].Position.End)));
        controlStatement %= (forHeader + statement, (tokens, nodes) =>
        {
            var declaration = nodes[0] as VariableDeclarationNode ?? throw new Exception();
            return new ForNode(declaration.Identifier, declaration.Value, nodes[1], (declaration.Position.Start, nodes[1].Position.End));
        }
        );
        controlStatement %= (letHeader + statement, (_, nodes) => new LetNode(nodes[0], nodes[1], (nodes[0].Position.Start, nodes[1].Position.End)));

        expression %= (expressionBlock, (_, nodes) => nodes[0]);
        expression %= (destructiveAssignment, (_, nodes) => nodes[0]);
        expression %= (orExpression, (_, nodes) => nodes[0]);
        expression %= (controlExpression, (_, nodes) => nodes[0]);

        controlExpression %= (ifExpression, (_, nodes) => nodes[0]);
        controlExpression %= (whileHeader + expression, (_, nodes) => new WhileNode(nodes[0], nodes[1], (nodes[0].Position.Start, nodes[1].Position.End)));
        controlExpression %= (forHeader + expression, (_, nodes) =>
        {
            var declaration = (VariableDeclarationNode)nodes[0];
            return new ForNode(declaration.Identifier, declaration.Value, nodes[1], (declaration.Position.Start, nodes[1].Position.End));
        }
        );
        controlExpression %= (letHeader + expression, (_, nodes) => new LetNode(nodes[0], nodes[1], (nodes[0].Position.Start, nodes[1].Position.End)));

        ifStatement %= (ifTerminal + openParenthesis + expression + closeParenthesis + expression + elifStatement + elseTerminal + statement, (tokens, nodes) => new IfNode(nodes[0], nodes[1], nodes[2], nodes[3], (tokens[0].Position.Start, nodes[3].Position.End)));
        ifStatement %= (ifTerminal + openParenthesis + expression + closeParenthesis + expression + elseTerminal + statement, (tokens, nodes) => new IfNode(nodes[0], nodes[1], null, nodes[2], (tokens[0].Position.Start, nodes[2].Position.End)));

        elifStatement %= (elifStatement + elifTerminal + openParenthesis + expression + closeParenthesis + statement, (tokens, nodes) =>
        {
            var elifClauses = nodes[0] as ElifClausesNodes ?? throw new Exception();
            elifClauses.AppendElif(new ElifNode(nodes[1], nodes[2], (tokens[0].Position.Start, nodes[2].Position.End)));
            return elifClauses;
        }
        );
        elifStatement %= (elifTerminal + openParenthesis + expression + closeParenthesis + statement, (tokens, nodes) => new ElifClausesNodes([new ElifNode(nodes[0], nodes[1], (tokens[0].Position.Start, nodes[1].Position.End))], (tokens[0].Position.Start, nodes[1].Position.End)));

        whileHeader %= (whileTerminal + openParenthesis + expression + closeParenthesis, (_, nodes) => nodes[0]);

        forHeader %= (forTerminal + openParenthesis + identifier + typeDeclaration + inTerminal + expression + closeParenthesis, (tokens, nodes) => new VariableDeclarationNode(new IdentifierNode(tokens[2].Lex, tokens[2].Position), nodes[0], (tokens[0].Position.Start, tokens[5].Position.End), new TypeDeclarationNode(tokens[4].Lex, tokens[4].Position)));
        forHeader %= (forTerminal + openParenthesis + identifier + inTerminal + expression + closeParenthesis, (tokens, nodes) => new VariableDeclarationNode(new IdentifierNode(tokens[2].Lex, tokens[2].Position), nodes[0], (tokens[0].Position.Start, tokens[4].Position.End)));

        letHeader %= (letTerminal + identifier + typeDeclaration + assignmentTerminal + expression + multipleDeclaration + inTerminal, (tokens, nodes) =>
        {
            var declarationList = nodes[1] as VariableDeclarationListNode ?? throw new Exception();
            declarationList.AppendDeclaration(new VariableDeclarationNode(new IdentifierNode(tokens[1].Lex, tokens[1].Position), nodes[0], (tokens[0].Position.Start, nodes[0].Position.End)));
            return declarationList;
        }
        );
        letHeader %= (letTerminal + identifier + typeDeclaration + assignmentTerminal + expression + inTerminal, (tokens, nodes) => new VariableDeclarationListNode(new VariableDeclarationNode(new IdentifierNode(tokens[1].Lex, tokens[1].Position), nodes[0], (tokens[0].Position.Start, nodes[0].Position.End), new TypeDeclarationNode(tokens[2].Lex, tokens[2].Position)), (tokens[0].Position.Start, tokens[4].Position.End)));
        letHeader %= (letTerminal + identifier + assignmentTerminal + expression + multipleDeclaration + inTerminal, (tokens, nodes) =>
        {
            var declarationList = (VariableDeclarationListNode)nodes[1];
            declarationList.AppendDeclaration(new VariableDeclarationNode(new IdentifierNode(tokens[1].Lex, tokens[1].Position), nodes[0], (tokens[0].Position.Start, nodes[0].Position.End)));
            return declarationList;
        }
        );
        letHeader %= (letTerminal + identifier + assignmentTerminal + expression + inTerminal, (tokens, nodes) => new VariableDeclarationListNode(new VariableDeclarationNode(new IdentifierNode(tokens[1].Lex, tokens[1].Position), nodes[0], (tokens[0].Position.Start, nodes[0].Position.End)), (tokens[0].Position.Start, tokens[3].Position.End)));

        multipleDeclaration %= (comma + identifier + typeDeclaration + assignmentTerminal + expression + multipleDeclaration, (tokens, nodes) =>
        {
            var declarationList = nodes[1] as VariableDeclarationListNode ?? throw new Exception();
            declarationList.AppendDeclaration(new VariableDeclarationNode(new IdentifierNode(tokens[1].Lex, tokens[1].Position), nodes[0], (tokens[0].Position.Start, nodes[0].Position.End), new TypeDeclarationNode(tokens[2].Lex, tokens[2].Position)));
            return declarationList;
        }
        );
        multipleDeclaration %= (comma + identifier + typeDeclaration + assignmentTerminal + expression, (tokens, nodes) => new VariableDeclarationListNode(new VariableDeclarationNode(new IdentifierNode(tokens[1].Lex, tokens[1].Position), nodes[0], (tokens[0].Position.Start, nodes[0].Position.End), new TypeDeclarationNode(tokens[2].Lex, tokens[2].Position)), (tokens[0].Position.Start, nodes[0].Position.End)));
        multipleDeclaration %= (comma + identifier + assignmentTerminal + expression + multipleDeclaration, (tokens, nodes) =>
        {
            var declarationList = nodes[1] as VariableDeclarationListNode ?? throw new Exception();
            declarationList.AppendDeclaration(new VariableDeclarationNode(new IdentifierNode(tokens[1].Lex, tokens[1].Position), nodes[0], (tokens[0].Position.Start, nodes[0].Position.End)));
            return declarationList;
        }
        );
        multipleDeclaration %= (comma + identifier + assignmentTerminal + expression, (tokens, nodes) => new VariableDeclarationListNode(new VariableDeclarationNode(new IdentifierNode(tokens[1].Lex, tokens[1].Position), nodes[0], (tokens[0].Position.Start, nodes[0].Position.End)), (tokens[0].Position.Start, nodes[0].Position.End)));

        ifExpression %= (ifTerminal + openParenthesis + expression + closeParenthesis + expression + elifExpression + elseTerminal + expression, (tokens, nodes) => new IfNode(nodes[0], nodes[1], null, nodes[2], (tokens[0].Position.Start, nodes[2].Position.End)));
        ifExpression %= (ifTerminal + openParenthesis + expression + closeParenthesis + expression + elseTerminal + expression, (tokens, nodes) => new IfNode(nodes[0], nodes[1], nodes[2], nodes[3], (tokens[0].Position.Start, nodes[3].Position.End)));

        elifExpression %= (elifExpression + elifTerminal + openParenthesis + expression + closeParenthesis + statement, (_, nodes) =>
        {
            var elifNodes = nodes[0] as ElifClausesNodes ?? throw new Exception();
            elifNodes.AppendElif(nodes[1]);
            return elifNodes;
        }
        );
        elifExpression %= (elifTerminal + openParenthesis + expression + closeParenthesis + statement, (tokens, nodes) => new ElifClausesNodes([new ElifNode(nodes[0], nodes[1], (tokens[0].Position.Start, nodes[1].Position.End))], nodes[0].Position));


        destructiveAssignment %= (identifier + destructiveAssignmentTerminal + expression, (tokens, nodes) => new DestructiveAssignmentNode(new IdentifierNode(tokens[0].Lex, tokens[0].Position), nodes[0], (tokens[0].Position.Start, nodes[0].Position.End)));
        destructiveAssignment %= (memberAccess + destructiveAssignmentTerminal + expression, (_, nodes) => new DestructiveAssignmentNode(nodes[0], nodes[1], (nodes[0].Position.Start, nodes[1].Position.End)));

        orExpression %= (orExpression + orTerminal + andExpression, (_, nodes) => new BinaryExpressionNode(nodes[0], Operator.OR, nodes[1], (nodes[0].Position.Start, nodes[1].Position.End)));
        orExpression %= (andExpression, (_, nodes) => nodes[0]);

        andExpression %= (andExpression + andTerminal + equalityExpression, (_, nodes) => new BinaryExpressionNode(nodes[0], Operator.AND, nodes[1], (nodes[0].Position.Start, nodes[1].Position.End)));
        andExpression %= (equalityExpression, (_, nodes) => nodes[0]);

        equalityExpression %= (equalityExpression + equalTerminal + relationalExpression, (_, nodes) => new BinaryExpressionNode(nodes[0], Operator.EQ, nodes[1], (nodes[0].Position.Start, nodes[1].Position.End)));
        equalityExpression %= (equalityExpression + notEqualTerminal + relationalExpression, (_, nodes) => new BinaryExpressionNode(nodes[0], Operator.NEQ, nodes[1], (nodes[0].Position.Start, nodes[1].Position.End)));
        equalityExpression %= (relationalExpression, (_, nodes) => nodes[0]);

        relationalExpression %= (relationalExpression + less + concatExpression, (_, nodes) => new BinaryExpressionNode(nodes[0], Operator.LTE, nodes[1], (nodes[0].Position.Start, nodes[1].Position.End)));
        relationalExpression %= (relationalExpression + lessEqual + concatExpression, (_, nodes) => new BinaryExpressionNode(nodes[0], Operator.LT, nodes[1], (nodes[0].Position.Start, nodes[1].Position.End)));
        relationalExpression %= (relationalExpression + greater + concatExpression, (_, nodes) => new BinaryExpressionNode(nodes[0], Operator.GT, nodes[1], (nodes[0].Position.Start, nodes[1].Position.End)));
        relationalExpression %= (relationalExpression + greaterEqual + concatExpression, (_, nodes) => new BinaryExpressionNode(nodes[0], Operator.GTE, nodes[1], (nodes[0].Position.Start, nodes[1].Position.End)));
        relationalExpression %= (relationalExpression + isTerminal + identifier, (tokens, nodes) => new BinaryExpressionNode(nodes[0], Operator.IS, new IdentifierNode(tokens[0].Lex, tokens[0].Position), (nodes[0].Position.Start, nodes[1].Position.End)));
        relationalExpression %= (relationalExpression + asTerminal + identifier, (tokens, nodes) => new BinaryExpressionNode(nodes[0], Operator.AS, new IdentifierNode(tokens[0].Lex, tokens[0].Position), (nodes[0].Position.Start, nodes[1].Position.End)));
        relationalExpression %= (concatExpression, (_, nodes) => nodes[0]);

        concatExpression %= (concatExpression + concat + aritmeticExpression, (_, nodes) => new BinaryExpressionNode(nodes[0], Operator.CONCAT, nodes[1], (nodes[0].Position.Start, nodes[1].Position.End)));
        concatExpression %= (concatExpression + doubleConcat + aritmeticExpression, (_, nodes) => new BinaryExpressionNode(nodes[0], Operator.DCONCAT, nodes[1], (nodes[0].Position.Start, nodes[1].Position.End)));
        concatExpression %= (aritmeticExpression, (_, nodes) => nodes[0]);

        aritmeticExpression %= (aritmeticExpression + plus + multExpression, (_, nodes) => new BinaryExpressionNode(nodes[0], Operator.ADD, nodes[1], (nodes[0].Position.Start, nodes[1].Position.End)));
        aritmeticExpression %= (aritmeticExpression + minus + multExpression, (_, nodes) => new BinaryExpressionNode(nodes[0], Operator.SUB, nodes[1], (nodes[0].Position.Start, nodes[1].Position.End)));
        aritmeticExpression %= (multExpression, (_, nodes) => nodes[0]);

        multExpression %= (multExpression + times + exponentialExpression, (_, nodes) => new BinaryExpressionNode(nodes[0], Operator.MUL, nodes[1], (nodes[0].Position.Start, nodes[1].Position.End)));
        multExpression %= (multExpression + divide + exponentialExpression, (_, nodes) => new BinaryExpressionNode(nodes[0], Operator.DIV, nodes[1], (nodes[0].Position.Start, nodes[1].Position.End)));
        multExpression %= (multExpression + mod + exponentialExpression, (_, nodes) => new BinaryExpressionNode(nodes[0], Operator.MOD, nodes[1], (nodes[0].Position.Start, nodes[1].Position.End)));
        multExpression %= (exponentialExpression, (_, nodes) => nodes[0]);

        exponentialExpression %= (unaryExpression + power + exponentialExpression, (_, nodes) => new BinaryExpressionNode(nodes[0], Operator.POW, nodes[1], (nodes[0].Position.Start, nodes[1].Position.End)));
        exponentialExpression %= (unaryExpression, (_, nodes) => nodes[0]);

        unaryExpression %= (plus + primaryExpression, (tokens, nodes) => new PositiveNode(nodes[0], (tokens[0].Position.Start, nodes[0].Position.End)));
        unaryExpression %= (minus + primaryExpression, (tokens, nodes) => new NegativeNode(nodes[0], (tokens[0].Position.Start, nodes[0].Position.End)));
        unaryExpression %= (notOperator + primaryExpression, (tokens, nodes) => new NotNode(nodes[0], (tokens[0].Position.Start, nodes[0].Position.End)));
        unaryExpression %= (primaryExpression, (_, nodes) => nodes[0]);

        primaryExpression %= (literal, (_, nodes) => nodes[0]);
        primaryExpression %= (invocationExpression, (_, nodes) => nodes[0]);
        primaryExpression %= (identifier, (tokens, _) => new IdentifierNode(tokens[0].Lex, tokens[0].Position));
        primaryExpression %= (vector, (_, nodes) => nodes[0]);
        primaryExpression %= (comprehensionVector, (_, nodes) => nodes[0]);
        primaryExpression %= (indexedValue, (_, nodes) => nodes[0]);
        primaryExpression %= (memberAccess, (_, nodes) => nodes[0]);
        primaryExpression %= (openParenthesis + expression + closeParenthesis, (_, nodes) => nodes[0]);
        primaryExpression %= (instatiation, (_, nodes) => nodes[0]);

        invocationExpression %= (identifier + openParenthesis + argumentList + closeParenthesis, (tokens, nodes) => new InvocationNode(new IdentifierNode(tokens[0].Lex, tokens[0].Position), ((ExpressionBlockNode)nodes[0]).Expressions, (tokens[0].Position.Start, tokens[3].Position.End)));
        invocationExpression %= (identifier + openParenthesis + closeParenthesis, (tokens, _) => new InvocationNode(new IdentifierNode(tokens[0].Lex, tokens[0].Position), [], (tokens[0].Position.Start, tokens[2].Position.End)));

        argumentList %= (argumentList + comma + expression, (_, nodes) =>
        {
            var expressionList = nodes[0] as ExpressionBlockNode ?? throw new Exception();
            expressionList.AppendExpression(nodes[1]);
            return expressionList;
        }
        );
        argumentList %= (expression, (_, nodes) => new ExpressionBlockNode([nodes[0]], nodes[0].Position));

        vector %= (openBracket + vectorElement + closeBracket, (tokens, nodes) => new VectorNode(((ExpressionBlockNode)nodes[0]).Expressions, (tokens[0].Position.Start, tokens[1].Position.End)));

        vectorElement %= (vectorElement + comma + expression, (_, nodes) =>
        {
            var expressionList = nodes[0] as ExpressionBlockNode ?? throw new Exception();
            expressionList.AppendExpression(nodes[1]);
            return expressionList;
        }
        );
        vectorElement %= (expression, (_, nodes) => new ExpressionBlockNode([nodes[0]], nodes[0].Position));

        comprehensionVector %= (openBracket + expression + doublePipe + identifier + inTerminal + expression + closeBracket, (tokens, nodes) => new ComprehensionVectorNode(nodes[0], new IdentifierNode(tokens[2].Lex, tokens[2].Position), nodes[1], (tokens[0].Position.Start, tokens[5].Position.End)));

        indexedValue %= (primaryExpression + openBracket + expression + closeBracket, (_, nodes) => new IndexNode(nodes[0], nodes[1], (nodes[0].Position.Start, nodes[1].Position.End)));

        memberAccess %= (primaryExpression + dot + identifier, (tokens, nodes) => new AttributeCallNode(nodes[0], tokens[1].Lex, (nodes[0].Position.Start, tokens[1].Position.End)));
        memberAccess %= (primaryExpression + dot + invocationExpression, (_, nodes) => new FunctionCallNode(nodes[0], nodes[1], (nodes[0].Position.Start, nodes[1].Position.End)));

        instatiation %= (newTerminal + invocationExpression, (_, nodes) =>
        {
            var invocation = nodes[0] as InvocationNode ?? throw new Exception();
            return new InstanciateNode(invocation.Identifier, invocation.Arguments, invocation.Position);
        }
        );

        literal %= (numberLiteral, (tokens, _) => new LiteralNode(tokens[0].Lex, tokens[0].Position, NumberType.Instance));
        literal %= (stringLiteral, (tokens, _) => new LiteralNode(tokens[0].Lex, tokens[0].Position, StringType.Instance));
        literal %= (trueTerminal, (tokens, _) => new LiteralNode(tokens[0].Lex, tokens[0].Position, BooleanType.Instance));
        literal %= (falseTerminal, (tokens, _) => new LiteralNode(tokens[0].Lex, tokens[0].Position, BooleanType.Instance));

        var mapping = new Dictionary<TokenType, Symbol>
        {
            { TokenType.AND, andTerminal },
            { TokenType.OR,  orTerminal },
            { TokenType.NOT, notOperator },
            { TokenType.IF, ifTerminal },
            { TokenType.ELSE, elseTerminal },
            { TokenType.ELIF, elifTerminal },
            { TokenType.WHILE, whileTerminal },
            { TokenType.NEW, newTerminal },
            { TokenType.PROTOCOL, protocolTerminal },
            { TokenType.TYPE, typeTerminal },
            { TokenType.IDENTIFIER, identifier },
            { TokenType.STRING_LITERAL, stringLiteral },
            { TokenType.NUMBER_LITERAL, numberLiteral },
            { TokenType.PLUS, plus },
            { TokenType.TIMES, times},
            { TokenType.ASSIGNMENT, assignmentTerminal },
            { TokenType.DESTRUCTIVE_ASSIGNMENT, destructiveAssignment },
            { TokenType.DOT, dot },
            { TokenType.CONCAT, concat },
            { TokenType.MINUS, minus },
            { TokenType.AS, asTerminal },
            { TokenType.LET, letTerminal},
            { TokenType.IN, inTerminal},
            { TokenType.DOUBLE_PIPE, doublePipe },
            { TokenType.ARROW_OP, inline },
            { TokenType.COLON, colon },
            { TokenType.SEMICOLON, semicolon },
            { TokenType.COMMA, comma },
            { TokenType.DIVIDE, divide},
            { TokenType.DOUBLE_CONCAT, doubleConcat },
            { TokenType.EXTENDS, extendsTerminal },
            { TokenType.FALSE_LITERAL, falseTerminal },
            { TokenType.TRUE_LITERAL, trueTerminal },
            { TokenType.FOR, forTerminal },
            { TokenType.FUNCTION, functionTerminal },
            { TokenType.EQUAL,equalTerminal },
            { TokenType.NOT_EQUAL, notEqualTerminal },
            { TokenType.GREATER_THAN, greater },
            { TokenType.LESS_THAN,less },
            { TokenType.GREATER_THAN_EQUAL, greaterEqual },
            { TokenType.LESS_THAN_EQUAL, lessEqual },
            { TokenType.LEFT_PARENTHESIS,openParenthesis },
            { TokenType.RIGHT_PARENTHESIS,closeParenthesis },
            { TokenType.LEFT_BRACKET,openBracket },
            { TokenType.RIGHT_BRACKET,closeBracket },
            { TokenType.LEFT_BRACE,openBrace },
            { TokenType.RIGHT_BRACE,closeBrace },
            { TokenType.MOD, mod },
            { TokenType.POW, power },
            { TokenType.IS, isTerminal },
            { TokenType.STRING, StringTypeDeclaration },
            { TokenType.NUMBER, numberTypeDeclaration },
            { TokenType.BOOLEAN, boolTypeDeclaration },
            { TokenType.INHERITS, inheritsTerminal },
            { TokenType.EOF, grammar.Eof },
        };

        return (grammar, mapping);
    }
}