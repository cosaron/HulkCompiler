namespace HulkCompiler.Parser.Grammar;

using HulkCompiler.Lexer.TokenClass;
using HulkCompiler.Parser.Ast;
using Utils;


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


        program %= (headProgram + statement, (_, nodes) => new Program(nodes[0], nodes[1], nodes[1].Position));
        program %= (statement, (_, nodes) => new Program(null, nodes[0], nodes[0].Position));

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

        typeDefinition %= (typeTerminal + identifier + typeArguments + typeInherits + openBrace + typeBody + closeBrace, (tokens, nodes) => new TypeDefinitionNode(tokens[1].Lex, nodes[0], nodes[2], (tokens[0].Position.LineNumber, tokens[0].Position.ColumnStartNumber, tokens[3].Position.ColumnEndNumber), nodes[1]));
        typeDefinition %= (typeTerminal + identifier + typeArguments + typeInherits + openBrace + closeBrace, (tokens, nodes) => new TypeDefinitionNode(tokens[1].Lex, nodes[0], null, (tokens[0].Position.LineNumber, tokens[0].Position.ColumnStartNumber, tokens[3].Position.ColumnEndNumber), nodes[1]));
        typeDefinition %= (typeTerminal + identifier + typeArguments + openBrace + typeBody + closeBrace, (tokens, nodes) => new TypeDefinitionNode(tokens[1].Lex, nodes[0], null, (tokens[0].Position.LineNumber, tokens[0].Position.ColumnStartNumber, tokens[3].Position.ColumnEndNumber), nodes[0]));
        typeDefinition %= (typeTerminal + identifier + typeArguments + openBrace + closeBrace, (tokens, nodes) => new TypeDefinitionNode(tokens[1].Lex, nodes[0], null, (tokens[0].Position.LineNumber, tokens[0].Position.ColumnStartNumber, tokens[3].Position.ColumnEndNumber)));
        typeDefinition %= (typeTerminal + identifier + typeInherits + openBrace + typeBody + closeBrace, (tokens, nodes) => new TypeDefinitionNode(tokens[1].Lex, null, nodes[1], (tokens[0].Position.LineNumber, tokens[0].Position.ColumnStartNumber, tokens[3].Position.ColumnEndNumber), nodes[0]));
        typeDefinition %= (typeTerminal + identifier + typeInherits + openBrace + closeBrace, (tokens, nodes) => new TypeDefinitionNode(tokens[1].Lex, null, null, (tokens[0].Position.LineNumber, tokens[0].Position.ColumnStartNumber, tokens[3].Position.ColumnEndNumber), nodes[0]));
        typeDefinition %= (typeTerminal + identifier + openBrace + typeBody + closeBrace, (tokens, nodes) => new TypeDefinitionNode(tokens[1].Lex, null, nodes[0], (tokens[0].Position.LineNumber, tokens[0].Position.ColumnStartNumber, tokens[3].Position.ColumnEndNumber)));
        typeDefinition %= (typeTerminal + identifier + openBrace + closeBrace, (tokens, _) => new TypeDefinitionNode(tokens[1].Lex, null, null, (tokens[0].Position.LineNumber, tokens[0].Position.ColumnStartNumber, tokens[3].Position.ColumnEndNumber)));

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

        attributeDefinition %= (identifier + typeDeclaration + assignmentTerminal + expression + semicolon, (tokens, nodes) => new AttributeDefinitionNode(tokens[0].Lex, nodes[0], (tokens[0].Position.LineNumber, tokens[0].Position.ColumnStartNumber, tokens[2].Position.ColumnEndNumber), tokens[1].Lex));
        attributeDefinition %= (identifier + assignmentTerminal + expression + semicolon, (tokens, nodes) => new AttributeDefinitionNode(tokens[0].Lex, nodes[0], (tokens[0].Position.LineNumber, tokens[0].Position.ColumnStartNumber, tokens[2].Position.ColumnEndNumber)));

        typeArguments %= (openParenthesis + argumentListDefinition + closeParenthesis, (_, nodes) => nodes[0]);

        typeInherits %= (inheritsTerminal + identifier + inheritsDeclaration, (tokens, nodes) => new InheritsNode(tokens[0].Lex, nodes[0], (tokens[0].Position.LineNumber, tokens[0].Position.ColumnStartNumber, nodes[0].Position.ColumnEnd)));
        typeInherits %= (inheritsTerminal + identifier, (tokens, _) => new InheritsNode(tokens[1].Lex, null, (tokens[0].Position.LineNumber, tokens[0].Position.ColumnStartNumber, tokens[1].Position.ColumnEndNumber)));

        typeDeclaration %= (colon + identifier, (tokens, _) => new TypeDeclaration(tokens[0].Lex));
        typeDeclaration %= (colon + numberTypeDeclaration, (tokens, _) => new TypeDeclaration(tokens[0].Lex));
        typeDeclaration %= (colon + StringTypeDeclaration, (tokens, _) => new TypeDeclaration(tokens[0].Lex));
        typeDeclaration %= (colon + boolTypeDeclaration, (tokens, _) => new TypeDeclaration(tokens[0].Lex));
        typeDeclaration %= (colon + objectTypeDeclaration, (tokens, _) => new TypeDeclaration(tokens[0].Lex));

        inheritsDeclaration %= (openParenthesis + argumentList + closeParenthesis, (_, nodes) => nodes[0]);

        functionDefinition %= (inlineFunction, (_, nodes) => nodes[0]);
        functionDefinition %= (blockFunction, (_, nodes) => nodes[0]);

        inlineFunction %= (identifier + openParenthesis + argumentListDefinition + closeParenthesis + typeDeclaration + inline + statement, (tokens, nodes) => new FunctionDefinitionNode(tokens[0].Lex, nodes[1], nodes[0], (tokens[0].Position.LineNumber, tokens[0].Position.ColumnStartNumber, nodes[1].Position.ColumnEnd), tokens[3].Lex));
        inlineFunction %= (identifier + openParenthesis + argumentListDefinition + closeParenthesis + inline + statement, (tokens, nodes) => new FunctionDefinitionNode(tokens[0].Lex, nodes[1], nodes[0], (tokens[0].Position.LineNumber, tokens[0].Position.ColumnStartNumber, nodes[1].Position.ColumnEnd)));
        inlineFunction %= (identifier + openParenthesis + closeParenthesis + typeDeclaration + inline + statement, (tokens, nodes) => new FunctionDefinitionNode(tokens[0].Lex, nodes[0], null, (tokens[0].Position.LineNumber, tokens[0].Position.ColumnStartNumber, nodes[1].Position.ColumnEnd), tokens[3].Lex));
        inlineFunction %= (identifier + openParenthesis + closeParenthesis + inline + statement, (tokens, nodes) => new FunctionDefinitionNode(tokens[0].Lex, nodes[0], null, (tokens[0].Position.LineNumber, tokens[0].Position.ColumnStartNumber, nodes[0].Position.ColumnEnd)));

        blockFunction %= (identifier + openParenthesis + argumentListDefinition + closeParenthesis + typeDeclaration + expressionBlock, (tokens, nodes) => new FunctionDefinitionNode(tokens[0].Lex, nodes[1], nodes[0], (tokens[0].Position.LineNumber, tokens[0].Position.ColumnStartNumber, nodes[1].Position.ColumnEnd), tokens[3].Lex));
        blockFunction %= (identifier + openParenthesis + argumentListDefinition + closeParenthesis + expressionBlock, (tokens, nodes) => new FunctionDefinitionNode(tokens[0].Lex, nodes[1], nodes[0], (tokens[0].Position.LineNumber, tokens[0].Position.ColumnStartNumber, nodes[1].Position.ColumnEnd)));
        blockFunction %= (identifier + openParenthesis + closeParenthesis + typeDeclaration + expressionBlock, (tokens, nodes) => new FunctionDefinitionNode(tokens[0].Lex, nodes[0], null, (tokens[0].Position.LineNumber, tokens[0].Position.ColumnStartNumber, nodes[0].Position.ColumnEnd), tokens[3].Lex));
        blockFunction %= (identifier + openParenthesis + closeParenthesis + expressionBlock, (tokens, nodes) => new FunctionDefinitionNode(tokens[0].Lex, nodes[0], null, (tokens[0].Position.LineNumber, tokens[0].Position.ColumnStartNumber, nodes[0].Position.ColumnEnd)));

        argumentListDefinition %= (argumentListDefinition + comma + identifier + typeDeclaration, (tokens, nodes) =>
        {
            var parameters = nodes[0] as ParameterListNode ?? throw new Exception();
            parameters.AppendParameter(new ParameterNode(tokens[0].Lex, tokens[0].Position, staticType: tokens[1].Lex));
            return parameters;
        }
        );
        argumentListDefinition %= (argumentListDefinition + comma + identifier, (tokens, nodes) =>
        {
            var parameters = nodes[0] as ParameterListNode ?? throw new Exception();
            parameters.AppendParameter(new ParameterNode(tokens[0].Lex, tokens[0].Position));
            return parameters;
        }
        );
        argumentListDefinition %= (identifier + typeDeclaration, (tokens, _) => new ParameterListNode([new ParameterNode(tokens[0].Lex, tokens[0].Position, staticType: tokens[1].Lex)], tokens[0].Position));
        argumentListDefinition %= (identifier, (tokens, _) => new ParameterListNode([new ParameterNode(tokens[0].Lex, tokens[0].Position)], tokens[0].Position));

        protocolDefinition %= (protocolTerminal + identifier + extendsDefinition + openBrace + protocolBody + closeBrace, (tokens, nodes) => new ProtocolDefinitionNode(tokens[1].Lex, nodes[0], (tokens[0].Position.LineNumber, tokens[0].Position.ColumnStartNumber, tokens[3].Position.ColumnEndNumber), nodes[1]));
        protocolDefinition %= (protocolTerminal + identifier + extendsDefinition + openBrace + closeBrace, (tokens, nodes) => new ProtocolDefinitionNode(tokens[1].Lex, null, (tokens[0].Position.LineNumber, tokens[0].Position.ColumnStartNumber, tokens[3].Position.ColumnEndNumber), nodes[0]));
        protocolDefinition %= (protocolTerminal + identifier + openBrace + protocolBody + closeBrace, (tokens, nodes) => new ProtocolDefinitionNode(tokens[1].Lex, nodes[0], (tokens[0].Position.LineNumber, tokens[0].Position.ColumnStartNumber, tokens[3].Position.ColumnEndNumber), null));
        protocolDefinition %= (protocolTerminal + identifier + openBrace + closeBrace, (tokens, nodes) => new ProtocolDefinitionNode(tokens[1].Lex, null, (tokens[0].Position.LineNumber, tokens[0].Position.ColumnStartNumber, tokens[3].Position.ColumnEndNumber), null));

        extendsDefinition %= (extendsTerminal + identifier + extendsMultipleIdentifier, (tokens, nodes) =>
        {
            var extendsDeclaration = nodes[0] as ExtendDeclarations ?? throw new Exception();
            extendsDeclaration.AddExtend(tokens[0].Lex);
            return extendsDeclaration;
        }
        );
        extendsDefinition %= (extendsTerminal + identifier, (tokens, _) => new ExtendDeclarations([tokens[0].Lex], tokens[0].Position));

        extendsMultipleIdentifier %= (comma + identifier + extendsMultipleIdentifier, (tokens, nodes) =>
        {
            var extendsDeclaration = nodes[0] as ExtendDeclarations ?? throw new Exception();
            extendsDeclaration.AddExtend(tokens[0].Lex);
            return extendsDeclaration;
        }
        );
        extendsMultipleIdentifier %= (comma + identifier, (tokens, _) => new ExtendDeclarations([tokens[0].Lex], tokens[0].Position));

        protocolBody %= (protocolBody + identifier + openParenthesis + protocolArgumentsDefinition + closeParenthesis + typeDeclaration + semicolon, (tokens, nodes) =>
        {
            var functionList = nodes[0] as FunctionDefinitionList ?? throw new Exception();
            functionList.AppendFunction(new FunctionDefinitionNode(tokens[0].Lex, null, nodes[1], (tokens[0].Position.LineNumber, tokens[0].Position.ColumnStartNumber, tokens[3].Position.ColumnEndNumber), tokens[3].Lex));
            return functionList;
        }
        );
        protocolBody %= (protocolBody + identifier + openParenthesis + closeParenthesis + typeDeclaration + semicolon, (tokens, nodes) =>
        {
            var functionList = nodes[0] as FunctionDefinitionList ?? throw new Exception();
            functionList.AppendFunction(new FunctionDefinitionNode(tokens[0].Lex, null, null, (tokens[0].Position.LineNumber, tokens[0].Position.ColumnStartNumber, tokens[3].Position.ColumnEndNumber), tokens[3].Lex));
            return functionList;
        }
        );
        protocolBody %= (identifier + openParenthesis + protocolArgumentsDefinition + closeParenthesis + typeDeclaration + semicolon, (tokens, nodes) => new FunctionDefinitionList([new FunctionDefinitionNode(tokens[0].Lex, null, nodes[0], (tokens[0].Position.LineNumber, tokens[0].Position.ColumnStartNumber, tokens[3].Position.ColumnEndNumber), tokens[3].Lex)], (tokens[0].Position.LineNumber, tokens[0].Position.ColumnStartNumber, tokens[3].Position.ColumnEndNumber)));
        protocolBody %= (identifier + openParenthesis + closeParenthesis + typeDeclaration + semicolon, (tokens, nodes) => new FunctionDefinitionList([new FunctionDefinitionNode(tokens[0].Lex, null, null, (tokens[0].Position.LineNumber, tokens[0].Position.ColumnStartNumber, tokens[3].Position.ColumnEndNumber), tokens[3].Lex)], (tokens[0].Position.LineNumber, tokens[0].Position.ColumnStartNumber, tokens[3].Position.ColumnEndNumber)));

        protocolArgumentsDefinition %= (identifier + typeDeclaration + protocolMultipleArgumentsDefinition, (tokens, nodes) =>
        {
            var parameters = nodes[0] as ParameterListNode ?? throw new Exception();
            parameters.AppendParameter(new ParameterNode(tokens[0].Lex, tokens[0].Position, staticType: tokens[2].Lex));
            return parameters;
        }
        );
        protocolArgumentsDefinition %= (identifier + typeDeclaration, (tokens, _) => new ParameterListNode([new ParameterNode(tokens[0].Lex, tokens[0].Position, staticType: tokens[1].Lex)], tokens[0].Position));

        protocolMultipleArgumentsDefinition %= (comma + identifier + typeDeclaration + protocolMultipleArgumentsDefinition, (tokens, nodes) =>
        {
            var parameters = nodes[0] as ParameterListNode ?? throw new Exception();
            parameters.AppendParameter(new ParameterNode(tokens[0].Lex, tokens[0].Position, staticType: tokens[2].Lex));
            return parameters;
        }
        );
        protocolMultipleArgumentsDefinition %= (comma + identifier + typeDeclaration, (tokens, _) => new ParameterListNode([new ParameterNode(tokens[1].Lex, tokens[1].Position, staticType: tokens[2].Lex)], tokens[1].Position));

        expressionBlock %= (openBrace + statementList + closeBrace, (_, nodes) => nodes[0]);

        statementList %= (statementList + statement, (_, nodes) =>
        {
            var expressionBlock = nodes[0] as ExpressionBlock ?? throw new Exception();
            expressionBlock.AppendExpression(nodes[1]);
            return expressionBlock;
        }
        );
        statementList %= (statement, (_, nodes) => new ExpressionBlock([nodes[0]], nodes[0].Position));

        statement %= (expressionBlock + semicolon, (_, nodes) => nodes[0]);
        statement %= (expressionBlock, (_, nodes) => nodes[0]);
        statement %= (orExpression + semicolon, (_, nodes) => nodes[0]);
        statement %= (destructiveAssignment + semicolon, (_, nodes) => nodes[0]);
        statement %= (controlStatement + semicolon, (_, nodes) => nodes[0]);

        controlStatement %= (ifStatement, (_, nodes) => nodes[0]);
        controlStatement %= (whileHeader + statement, (tokens, nodes) => new WhileNode(nodes[0], nodes[1], (nodes[0].Position.Line, nodes[0].Position.ColumnStart, nodes[1].Position.ColumnEnd)));
        controlStatement %= (forHeader + statement, (tokens, nodes) =>
        {
            var declaration = nodes[0] as VariableDeclarationNode ?? throw new Exception();
            return new ForNode(declaration.Identifier, declaration.Value, nodes[1], (declaration.Position.Line, declaration.Position.ColumnStart, nodes[1].Position.ColumnEnd));
        }
        );
        controlStatement %= (letHeader + statement, (_, nodes) => new LetNode(nodes[0], nodes[1], (nodes[0].Position.Line, nodes[0].Position.ColumnStart, nodes[1].Position.ColumnEnd)));

        expression %= (expressionBlock, (_, nodes) => nodes[0]);
        expression %= (destructiveAssignment, (_, nodes) => nodes[0]);
        expression %= (orExpression, (_, nodes) => nodes[0]);
        expression %= (controlExpression, (_, nodes) => nodes[0]);

        controlExpression %= (ifExpression, (_, nodes) => nodes[0]);
        controlExpression %= (whileHeader + expression, (_, nodes) => new WhileNode(nodes[0], nodes[1], (nodes[0].Position.Line, nodes[0].Position.ColumnStart, nodes[1].Position.ColumnEnd)));
        controlExpression %= (forHeader + expression, (_, nodes) =>
        {
            var declaration = (VariableDeclarationNode)nodes[0];
            return new ForNode(declaration.Identifier, declaration.Value, nodes[1], (declaration.Position.Line, declaration.Position.ColumnStart, nodes[1].Position.ColumnEnd));
        }
        );
        controlExpression %= (letHeader + expression, (_, nodes) => new LetNode(nodes[0], nodes[1], (nodes[0].Position.Line, nodes[0].Position.ColumnStart, nodes[1].Position.ColumnEnd)));

        ifStatement %= (ifTerminal + openParenthesis + expression + closeParenthesis + expression + elifStatement + elseTerminal + statement, (tokens, nodes) => new IfNode(nodes[0], nodes[1], nodes[2], nodes[3], (tokens[0].Position.LineNumber, tokens[0].Position.ColumnStartNumber, nodes[3].Position.ColumnEnd)));
        ifStatement %= (ifTerminal + openParenthesis + expression + closeParenthesis + expression + elseTerminal + statement, (tokens, nodes) => new IfNode(nodes[0], nodes[1], null, nodes[2], (tokens[0].Position.LineNumber, tokens[0].Position.ColumnStartNumber, nodes[2].Position.ColumnEnd)));

        elifStatement %= (elifStatement + elifTerminal + openParenthesis + expression + closeParenthesis + statement, (tokens, nodes) =>
        {
            var elifClauses = nodes[0] as ElifClausesNodes ?? throw new Exception();
            elifClauses.AppendElif(new ElifNode(nodes[1], nodes[2], (tokens[0].Position.LineNumber, tokens[0].Position.ColumnStartNumber, nodes[2].Position.ColumnEnd)));
            return elifClauses;
        }
        );
        elifStatement %= (elifTerminal + openParenthesis + expression + closeParenthesis + statement, (tokens, nodes) => new ElifClausesNodes([new ElifNode(nodes[0], nodes[1], (tokens[0].Position.LineNumber, tokens[0].Position.ColumnStartNumber, nodes[1].Position.ColumnEnd))], (tokens[0].Position.LineNumber, tokens[0].Position.ColumnStartNumber, nodes[1].Position.ColumnEnd)));

        whileHeader %= (whileTerminal + openParenthesis + expression + closeParenthesis, (_, nodes) => nodes[0]);

        forHeader %= (forTerminal + openParenthesis + identifier + typeDeclaration + inTerminal + expression + closeParenthesis, (tokens, nodes) => new VariableDeclarationNode(tokens[2].Lex, nodes[0], (tokens[0].Position.LineNumber, tokens[0].Position.ColumnStartNumber, tokens[5].Position.ColumnEndNumber), tokens[4].Lex));
        forHeader %= (forTerminal + openParenthesis + identifier + inTerminal + expression + closeParenthesis, (tokens, nodes) => new VariableDeclarationNode(tokens[2].Lex, nodes[0], (tokens[0].Position.LineNumber, tokens[0].Position.ColumnStartNumber, tokens[4].Position.ColumnEndNumber)));

        letHeader %= (letTerminal + identifier + typeDeclaration + assignmentTerminal + expression + multipleDeclaration + inTerminal, (tokens, nodes) =>
        {
            var declarationList = (VariableDeclarationListNode)nodes[1];
            declarationList.AppendDeclaration(new VariableDeclarationNode(tokens[1].Lex, nodes[0], (tokens[0].Position.LineNumber, tokens[0].Position.ColumnStartNumber, nodes[0].Position.ColumnEnd)));
            return declarationList;
        }
        );
        letHeader %= (letTerminal + identifier + typeDeclaration + assignmentTerminal + expression + inTerminal, (tokens, nodes) => new VariableDeclarationListNode(new VariableDeclarationNode(tokens[1].Lex, nodes[0], (tokens[0].Position.LineNumber, tokens[0].Position.ColumnStartNumber, nodes[0].Position.ColumnEnd), tokens[2].Lex), (tokens[0].Position.LineNumber, tokens[0].Position.ColumnStartNumber, tokens[4].Position.ColumnEndNumber
        )));
        letHeader %= (letTerminal + identifier + assignmentTerminal + expression + multipleDeclaration + inTerminal, (tokens, nodes) =>
        {
            var declarationList = (VariableDeclarationListNode)nodes[1];
            declarationList.AppendDeclaration(new VariableDeclarationNode(tokens[1].Lex, nodes[0], (tokens[0].Position.LineNumber, tokens[0].Position.ColumnStartNumber, nodes[0].Position.ColumnEnd)));
            return declarationList;
        }
        );
        letHeader %= (letTerminal + identifier + assignmentTerminal + expression + inTerminal, (tokens, nodes) => new VariableDeclarationListNode(new VariableDeclarationNode(tokens[1].Lex, nodes[0], (tokens[0].Position.LineNumber, tokens[0].Position.ColumnStartNumber, nodes[0].Position.ColumnEnd)), (tokens[0].Position.LineNumber, tokens[0].Position.ColumnStartNumber, tokens[3].Position.ColumnEndNumber)));

        multipleDeclaration %= (comma + identifier + typeDeclaration + assignmentTerminal + expression + multipleDeclaration, (tokens, nodes) =>
        {
            var declarationList = nodes[1] as VariableDeclarationListNode ?? throw new Exception();
            declarationList.AppendDeclaration(new VariableDeclarationNode(tokens[1].Lex, nodes[0], (tokens[0].Position.LineNumber, tokens[0].Position.ColumnStartNumber, nodes[0].Position.ColumnEnd), tokens[2].Lex));
            return declarationList;
        }
        );
        multipleDeclaration %= (comma + identifier + typeDeclaration + assignmentTerminal + expression, (tokens, nodes) => new VariableDeclarationListNode(new VariableDeclarationNode(tokens[1].Lex, nodes[0], (tokens[0].Position.LineNumber, tokens[0].Position.ColumnStartNumber, nodes[0].Position.ColumnEnd), tokens[2].Lex), (tokens[0].Position.LineNumber, tokens[0].Position.ColumnStartNumber, nodes[0].Position.ColumnEnd)));
        multipleDeclaration %= (comma + identifier + assignmentTerminal + expression + multipleDeclaration, (tokens, nodes) =>
        {
            var declarationList = nodes[1] as VariableDeclarationListNode ?? throw new Exception();
            declarationList.AppendDeclaration(new VariableDeclarationNode(tokens[1].Lex, nodes[0], (tokens[0].Position.LineNumber, tokens[0].Position.ColumnStartNumber, nodes[0].Position.ColumnEnd)));
            return declarationList;
        }
        );
        multipleDeclaration %= (comma + identifier + assignmentTerminal + expression, (tokens, nodes) => new VariableDeclarationListNode(new VariableDeclarationNode(tokens[1].Lex, nodes[0], (tokens[0].Position.LineNumber, tokens[0].Position.ColumnStartNumber, nodes[0].Position.ColumnEnd)), (tokens[0].Position.LineNumber, tokens[0].Position.ColumnStartNumber, nodes[0].Position.ColumnEnd)));

        ifExpression %= (ifTerminal + openParenthesis + expression + closeParenthesis + expression + elifExpression + elseTerminal + expression, (tokens, nodes) => new IfNode(nodes[0], nodes[1], null, nodes[2], (tokens[0].Position.LineNumber, tokens[0].Position.ColumnStartNumber, nodes[2].Position.ColumnEnd)));
        ifExpression %= (ifTerminal + openParenthesis + expression + closeParenthesis + expression + elseTerminal + expression, (tokens, nodes) => new IfNode(nodes[0], nodes[1], nodes[2], nodes[3], (tokens[0].Position.LineNumber, tokens[0].Position.ColumnStartNumber, nodes[3].Position.ColumnEnd)));

        elifExpression %= (elifExpression + elifTerminal + openParenthesis + expression + closeParenthesis + statement, (_, nodes) =>
        {
            var elifNodes = nodes[0] as ElifClausesNodes ?? throw new Exception();
            elifNodes.AppendElif(nodes[1]);
            return elifNodes;
        }
        );
        elifExpression %= (elifTerminal + openParenthesis + expression + closeParenthesis + statement, (tokens, nodes) => new ElifClausesNodes([new ElifNode(nodes[0], nodes[1], (tokens[0].Position.LineNumber, tokens[0].Position.ColumnStartNumber, nodes[1].Position.ColumnEnd))], nodes[0].Position));


        destructiveAssignment %= (identifier + destructiveAssignmentTerminal + expression, (tokens, nodes) => new DestructiveAssignmentNode(new IdentifierNode(tokens[0].Lex, tokens[0].Position), nodes[0], (tokens[0].Position.LineNumber, tokens[0].Position.ColumnStartNumber, nodes[0].Position.ColumnEnd)));
        destructiveAssignment %= (memberAccess + destructiveAssignmentTerminal + expression, (_, nodes) => new DestructiveAssignmentNode(nodes[0], nodes[1], (nodes[0].Position.Line, nodes[0].Position.ColumnStart, nodes[1].Position.ColumnEnd)));

        orExpression %= (orExpression + orTerminal + andExpression, (_, nodes) => new BinaryExpressionNode(nodes[0], Operator.OR, nodes[1], (nodes[0].Position.Line, nodes[0].Position.ColumnStart, nodes[1].Position.ColumnEnd)));
        orExpression %= (andExpression, (_, nodes) => nodes[0]);

        andExpression %= (andExpression + andTerminal + equalityExpression, (_, nodes) => new BinaryExpressionNode(nodes[0], Operator.AND, nodes[1], (nodes[0].Position.Line, nodes[0].Position.ColumnStart, nodes[1].Position.ColumnEnd)));
        andExpression %= (equalityExpression, (_, nodes) => nodes[0]);

        equalityExpression %= (equalityExpression + equalTerminal + relationalExpression, (_, nodes) => new BinaryExpressionNode(nodes[0], Operator.EQ, nodes[1], (nodes[0].Position.Line, nodes[0].Position.ColumnStart, nodes[1].Position.ColumnEnd)));
        equalityExpression %= (equalityExpression + notEqualTerminal + relationalExpression, (_, nodes) => new BinaryExpressionNode(nodes[0], Operator.NEQ, nodes[1], (nodes[0].Position.Line, nodes[0].Position.ColumnStart, nodes[1].Position.ColumnEnd)));
        equalityExpression %= (relationalExpression, (_, nodes) => nodes[0]);

        relationalExpression %= (relationalExpression + less + concatExpression, (_, nodes) => new BinaryExpressionNode(nodes[0], Operator.LTE, nodes[1], (nodes[0].Position.Line, nodes[0].Position.ColumnStart, nodes[1].Position.ColumnEnd)));
        relationalExpression %= (relationalExpression + lessEqual + concatExpression, (_, nodes) => new BinaryExpressionNode(nodes[0], Operator.LT, nodes[1], (nodes[0].Position.Line, nodes[0].Position.ColumnStart, nodes[1].Position.ColumnEnd)));
        relationalExpression %= (relationalExpression + greater + concatExpression, (_, nodes) => new BinaryExpressionNode(nodes[0], Operator.GT, nodes[1], (nodes[0].Position.Line, nodes[0].Position.ColumnStart, nodes[1].Position.ColumnEnd)));
        relationalExpression %= (relationalExpression + greaterEqual + concatExpression, (_, nodes) => new BinaryExpressionNode(nodes[0], Operator.GTE, nodes[1], (nodes[0].Position.Line, nodes[0].Position.ColumnStart, nodes[1].Position.ColumnEnd)));
        relationalExpression %= (relationalExpression + isTerminal + identifier, (tokens, nodes) => new BinaryExpressionNode(nodes[0], Operator.IS, new IdentifierNode(tokens[0].Lex, tokens[0].Position), (nodes[0].Position.Line, nodes[0].Position.ColumnStart, nodes[1].Position.ColumnEnd)));
        relationalExpression %= (relationalExpression + asTerminal + identifier, (tokens, nodes) => new BinaryExpressionNode(nodes[0], Operator.AS, new IdentifierNode(tokens[0].Lex, tokens[0].Position), (nodes[0].Position.Line, nodes[0].Position.ColumnStart, nodes[1].Position.ColumnEnd)));
        relationalExpression %= (concatExpression, (_, nodes) => nodes[0]);

        concatExpression %= (concatExpression + concat + aritmeticExpression, (_, nodes) => new BinaryExpressionNode(nodes[0], Operator.CONCAT, nodes[1], (nodes[0].Position.Line, nodes[0].Position.ColumnStart, nodes[1].Position.ColumnEnd)));
        concatExpression %= (concatExpression + doubleConcat + aritmeticExpression, (_, nodes) => new BinaryExpressionNode(nodes[0], Operator.DCONCAT, nodes[1], (nodes[0].Position.Line, nodes[0].Position.ColumnStart, nodes[1].Position.ColumnEnd)));
        concatExpression %= (aritmeticExpression, (_, nodes) => nodes[0]);

        aritmeticExpression %= (aritmeticExpression + plus + multExpression, (_, nodes) => new BinaryExpressionNode(nodes[0], Operator.ADD, nodes[1], (nodes[0].Position.Line, nodes[0].Position.ColumnStart, nodes[1].Position.ColumnEnd)));
        aritmeticExpression %= (aritmeticExpression + minus + multExpression, (_, nodes) => new BinaryExpressionNode(nodes[0], Operator.SUB, nodes[1], (nodes[0].Position.Line, nodes[0].Position.ColumnStart, nodes[1].Position.ColumnEnd)));
        aritmeticExpression %= (multExpression, (_, nodes) => nodes[0]);

        multExpression %= (multExpression + times + exponentialExpression, (_, nodes) => new BinaryExpressionNode(nodes[0], Operator.MUL, nodes[1], (nodes[0].Position.Line, nodes[0].Position.ColumnStart, nodes[1].Position.ColumnEnd)));
        multExpression %= (multExpression + divide + exponentialExpression, (_, nodes) => new BinaryExpressionNode(nodes[0], Operator.DIV, nodes[1], (nodes[0].Position.Line, nodes[0].Position.ColumnStart, nodes[1].Position.ColumnEnd)));
        multExpression %= (multExpression + mod + exponentialExpression, (_, nodes) => new BinaryExpressionNode(nodes[0], Operator.MOD, nodes[1], (nodes[0].Position.Line, nodes[0].Position.ColumnStart, nodes[1].Position.ColumnEnd)));
        multExpression %= (exponentialExpression, (_, nodes) => nodes[0]);

        exponentialExpression %= (unaryExpression + power + exponentialExpression, (_, nodes) => new BinaryExpressionNode(nodes[0], Operator.POW, nodes[1], (nodes[0].Position.Line, nodes[0].Position.ColumnStart, nodes[1].Position.ColumnEnd)));
        exponentialExpression %= (unaryExpression, (_, nodes) => nodes[0]);

        unaryExpression %= (plus + primaryExpression, (tokens, nodes) => new PositiveNode(nodes[0], (tokens[0].Position.LineNumber, tokens[0].Position.ColumnStartNumber, nodes[0].Position.ColumnEnd)));
        unaryExpression %= (minus + primaryExpression, (tokens, nodes) => new NegativeNode(nodes[0], (tokens[0].Position.LineNumber, tokens[0].Position.ColumnStartNumber, nodes[0].Position.ColumnEnd)));
        unaryExpression %= (notOperator + primaryExpression, (tokens, nodes) => new NotNode(nodes[0], (tokens[0].Position.LineNumber, tokens[0].Position.ColumnStartNumber, nodes[0].Position.ColumnEnd)));
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

        invocationExpression %= (identifier + openParenthesis + argumentList + closeParenthesis, (tokens, nodes) => new InvocationNode(tokens[0].Lex, ((ExpressionBlock)nodes[0]).Expressions, (tokens[0].Position.LineNumber, tokens[0].Position.ColumnStartNumber, tokens[3].Position.ColumnEndNumber)));
        invocationExpression %= (identifier + openParenthesis + closeParenthesis, (tokens, _) => new InvocationNode(tokens[0].Lex, [], (tokens[0].Position.LineNumber, tokens[0].Position.ColumnStartNumber, tokens[2].Position.ColumnEndNumber)));

        argumentList %= (argumentList + comma + expression, (_, nodes) =>
        {
            var expressionList = nodes[0] as ExpressionBlock ?? throw new Exception();
            expressionList.AppendExpression(nodes[1]);
            return expressionList;
        }
        );
        argumentList %= (expression, (_, nodes) => new ExpressionBlock([nodes[0]], nodes[0].Position));

        vector %= (openBracket + vectorElement + closeBracket, (tokens, nodes) => new VectorNode(((ExpressionBlock)nodes[0]).Expressions, (tokens[0].Position.LineNumber, tokens[0].Position.ColumnStartNumber, tokens[1].Position.ColumnEndNumber)));

        vectorElement %= (vectorElement + comma + expression, (_, nodes) =>
        {
            var expressionList = nodes[0] as ExpressionBlock ?? throw new Exception();
            expressionList.AppendExpression(nodes[1]);
            return expressionList;
        }
        );
        vectorElement %= (expression, (_, nodes) => new ExpressionBlock([nodes[0]], nodes[0].Position));

        comprehensionVector %= (openBracket + expression + doublePipe + identifier + inTerminal + expression + closeBracket, (tokens, nodes) => new ComprehensionVectorNode(nodes[0], tokens[2].Lex, nodes[1], (tokens[0].Position.LineNumber, tokens[0].Position.ColumnStartNumber, tokens[5].Position.ColumnEndNumber)));

        indexedValue %= (primaryExpression + openBracket + expression + closeBracket, (_, nodes) => new IndexNode(nodes[0], nodes[1], (nodes[0].Position.Line, nodes[0].Position.ColumnStart, nodes[1].Position.ColumnEnd)));

        memberAccess %= (primaryExpression + dot + identifier, (tokens, nodes) => new AttributeCallNode(nodes[0], tokens[1].Lex, (nodes[0].Position.Line, nodes[0].Position.ColumnStart, tokens[1].Position.ColumnEndNumber)));
        memberAccess %= (primaryExpression + dot + invocationExpression, (_, nodes) => new FunctionCallNode(nodes[0], nodes[1], (nodes[0].Position.Line, nodes[0].Position.ColumnStart, nodes[1].Position.ColumnEnd)));

        instatiation %= (newTerminal + invocationExpression, (_, nodes) =>
        {
            var invocation = nodes[0] as InvocationNode ?? throw new Exception();
            return new InstanciateNode(invocation.Identifier, invocation.Arguments, invocation.Position);
        }
        );

        literal %= (numberLiteral, (tokens, _) => new LiteralNode(tokens[0].Lex, tokens[0].Position));
        literal %= (stringLiteral, (tokens, _) => new LiteralNode(tokens[0].Lex, tokens[0].Position));
        literal %= (trueTerminal, (tokens, _) => new LiteralNode(tokens[0].Lex, tokens[0].Position));
        literal %= (falseTerminal, (tokens, _) => new LiteralNode(tokens[0].Lex, tokens[0].Position));

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