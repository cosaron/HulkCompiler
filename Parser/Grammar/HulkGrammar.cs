using Utils;

namespace Parser.Grammar;

public static class HulkGrammar
{
    public static Grammar GetGrammar()
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


        program %= (headProgram + statement, (_) => new());
        program %= (statement, (_) => new());

        headProgram %= (headProgram + defineStatement, (_) => new());
        headProgram %= (defineStatement, (_) => new());

        defineStatement %= (functionTerminal + functionDefinition, (_) => new());
        defineStatement %= (typeDefinition, (_) => new());
        defineStatement %= (protocolDefinition, (_) => new());

        typeDefinition %= (typeTerminal + identifier + typeArguments + typeInherits + openBrace + typeBody + closeBrace, (_) => new());
        typeDefinition %= (typeTerminal + identifier + typeArguments + typeInherits + openBrace + closeBrace, (_) => new());
        typeDefinition %= (typeTerminal + identifier + typeArguments + openBrace + typeBody + closeBrace, (_) => new());
        typeDefinition %= (typeTerminal + identifier + typeArguments + openBrace + closeBrace, (_) => new());
        typeDefinition %= (typeTerminal + identifier + typeInherits + openBrace + typeBody + closeBrace, (_) => new());
        typeDefinition %= (typeTerminal + identifier + typeInherits + openBrace + closeBrace, (_) => new());
        typeDefinition %= (typeTerminal + identifier + openBrace + typeBody + closeBrace, (_) => new());
        typeDefinition %= (typeTerminal + identifier + openBrace + closeBrace, (_) => new());

        typeBody %= (typeBody + attributeDefinition, (_) => new());
        typeBody %= (attributeDefinition, (_) => new());
        typeBody %= (typeBody + functionDefinition, (_) => new());
        typeBody %= (functionDefinition, (_) => new());

        attributeDefinition %= (identifier + typeDeclaration + assignmentTerminal + expression + semicolon, (_) => new());
        attributeDefinition %= (identifier + assignmentTerminal + expression + semicolon, (_) => new());

        typeArguments %= (openParenthesis + argumentListDefinition + closeParenthesis, (_) => new());

        typeInherits %= (inheritsTerminal + identifier + inheritsDeclaration, (_) => new());
        typeInherits %= (inheritsTerminal + identifier, (_) => new());

        typeDeclaration %= (colon + identifier, (_) => new());
        typeDeclaration %= (colon + numberTypeDeclaration, (_) => new());
        typeDeclaration %= (colon + StringTypeDeclaration, (_) => new());
        typeDeclaration %= (colon + boolTypeDeclaration, (_) => new());
        typeDeclaration %= (colon + objectTypeDeclaration, (_) => new());

        inheritsDeclaration %= (openParenthesis + argumentList + closeParenthesis, (_) => new());

        functionDefinition %= (inlineFunction, (_) => new());
        functionDefinition %= (blockFunction, (_) => new());

        inlineFunction %= (identifier + openParenthesis + argumentListDefinition + closeParenthesis + typeDeclaration + inline + statement, (_) => new());
        inlineFunction %= (identifier + openParenthesis + argumentListDefinition + closeParenthesis + inline + statement, (_) => new());
        inlineFunction %= (identifier + openParenthesis + closeParenthesis + typeDeclaration + inline + statement, (_) => new());
        inlineFunction %= (identifier + openParenthesis + closeParenthesis + inline + statement, (_) => new());

        blockFunction %= (identifier + openParenthesis + argumentListDefinition + closeParenthesis + typeDeclaration + expressionBlock, (_) => new());
        blockFunction %= (identifier + openParenthesis + argumentListDefinition + closeParenthesis + expressionBlock, (_) => new());
        blockFunction %= (identifier + openParenthesis + closeParenthesis + typeDeclaration + expressionBlock, (_) => new());
        blockFunction %= (identifier + openParenthesis + closeParenthesis + expressionBlock, (_) => new());

        argumentListDefinition %= (argumentListDefinition + comma + identifier + typeDeclaration, (_) => new());
        argumentListDefinition %= (argumentListDefinition + comma + identifier, (_) => new());
        argumentListDefinition %= (identifier + typeDeclaration, (_) => new());
        argumentListDefinition %= (identifier, (_) => new());

        protocolDefinition %= (protocolTerminal + identifier + extendsDefinition + openBrace + protocolBody + closeBrace, (_) => new());
        protocolDefinition %= (protocolTerminal + identifier + extendsDefinition + openBrace + closeBrace, (_) => new());
        protocolDefinition %= (protocolTerminal + identifier + openBrace + protocolBody + closeBrace, (_) => new());
        protocolDefinition %= (protocolTerminal + identifier + openBrace + closeBrace, (_) => new());

        extendsDefinition %= (extendsTerminal + identifier + extendsMultipleIdentifier, (_) => new());
        extendsDefinition %= (extendsTerminal + identifier, (_) => new());

        extendsMultipleIdentifier %= (comma + identifier + extendsMultipleIdentifier, (_) => new());
        extendsMultipleIdentifier %= (comma + identifier, (_) => new());

        protocolBody %= (protocolBody + identifier + openParenthesis + protocolArgumentsDefinition + closeParenthesis + typeDeclaration + semicolon, (_) => new());
        protocolBody %= (protocolBody + identifier + openParenthesis + closeParenthesis + typeDeclaration + semicolon, (_) => new());
        protocolBody %= (identifier + openParenthesis + protocolArgumentsDefinition + closeParenthesis + typeDeclaration + semicolon, (_) => new());
        protocolBody %= (identifier + openParenthesis + closeParenthesis + typeDeclaration + semicolon, (_) => new());

        protocolArgumentsDefinition %= (identifier + typeDeclaration + protocolArgumentsDefinition, (_) => new());
        protocolArgumentsDefinition %= (identifier + typeDeclaration, (_) => new());

        protocolMultipleArgumentsDefinition %= (comma + identifier + protocolMultipleArgumentsDefinition, (_) => new());
        protocolMultipleArgumentsDefinition %= (comma + identifier, (_) => new());

        statement %= (expressionBlock + semicolon, (_) => new());
        statement %= (expressionBlock, (_) => new());
        statement %= (orExpression + semicolon, (_) => new());
        statement %= (destructiveAssignment + semicolon, (_) => new());
        statement %= (controlStatement + semicolon, (_) => new());

        controlStatement %= (ifStatement, (_) => new());
        controlStatement %= (whileHeader + statement, (_) => new());
        controlStatement %= (forHeader + statement, (_) => new());
        controlStatement %= (letHeader + statement, (_) => new());

        ifStatement %= (ifTerminal + openParenthesis + expression + closeParenthesis + expression + elifStatement + elseTerminal + statement, (_) => new());
        ifStatement %= (ifTerminal + openParenthesis + expression + closeParenthesis + expression + elseTerminal + statement, (_) => new());

        elifStatement %= (elifStatement + elifTerminal + openParenthesis + expression + closeParenthesis + statement, (_) => new());
        elifStatement %= (elifTerminal + openParenthesis + expression + closeParenthesis + statement, (_) => new());

        whileHeader %= (whileTerminal + openParenthesis + expression + closeParenthesis, (_) => new());

        forHeader %= (forTerminal + openParenthesis + identifier + typeDeclaration + inTerminal + expression + closeParenthesis, (_) => new());
        forHeader %= (forTerminal + openParenthesis + identifier + inTerminal + expression + closeParenthesis, (_) => new());

        letHeader %= (letTerminal + identifier + typeDeclaration + assignmentTerminal + expression + multipleDeclaration + inTerminal, (_) => new());
        letHeader %= (letTerminal + identifier + typeDeclaration + assignmentTerminal + expression + inTerminal, (_) => new());
        letHeader %= (letTerminal + identifier + assignmentTerminal + expression + multipleDeclaration + inTerminal, (_) => new());
        letHeader %= (letTerminal + identifier + assignmentTerminal + expression + inTerminal, (_) => new());

        multipleDeclaration %= (comma + identifier + typeDeclaration + assignmentTerminal + expression + multipleDeclaration, (_) => new());
        multipleDeclaration %= (comma + identifier + typeDeclaration + assignmentTerminal + expression, (_) => new());
        multipleDeclaration %= (comma + identifier + assignmentTerminal + expression + multipleDeclaration, (_) => new());
        multipleDeclaration %= (comma + identifier + assignmentTerminal + expression, (_) => new());

        expression %= (expressionBlock, (_) => new());
        expression %= (destructiveAssignment, (_) => new());
        expression %= (orExpression, (_) => new());
        expression %= (controlExpression, (_) => new());

        controlExpression %= (ifExpression, (_) => new());
        controlExpression %= (whileHeader + expression, (_) => new());
        controlExpression %= (forHeader + expression, (_) => new());
        controlExpression %= (letHeader + expression, (_) => new());

        ifExpression %= (ifTerminal + openParenthesis + expression + closeParenthesis + expression + elifExpression + elseTerminal + expression, (_) => new());
        ifExpression %= (ifTerminal + openParenthesis + expression + closeParenthesis + expression + elseTerminal + expression, (_) => new());

        elifExpression %= (elifExpression + elifTerminal + openParenthesis + expression + closeParenthesis + statement, (_) => new());
        elifExpression %= (elifTerminal + openParenthesis + expression + closeParenthesis + statement, (_) => new());

        expressionBlock %= (openBrace + statementList + closeBrace, (_) => new());

        statementList %= (statementList + statement, (_) => new());
        statementList %= (statement, (_) => new());

        destructiveAssignment %= (identifier + destructiveAssignmentTerminal + expression, (_) => new());
        destructiveAssignment %= (memberAccess + destructiveAssignmentTerminal + expression, (_) => new());

        orExpression %= (orExpression + orTerminal + andExpression, (_) => new());
        orExpression %= (andExpression, (_) => new());

        andExpression %= (andExpression + andTerminal + equalityExpression, (_) => new());
        andExpression %= (equalityExpression, (_) => new());

        equalityExpression %= (equalityExpression + equalTerminal + relationalExpression, (_) => new());
        equalityExpression %= (equalityExpression + notEqualTerminal + relationalExpression, (_) => new());
        equalityExpression %= (relationalExpression, (_) => new());

        relationalExpression %= (relationalExpression + less + concatExpression, (_) => new());
        relationalExpression %= (relationalExpression + lessEqual + concatExpression, (_) => new());
        relationalExpression %= (relationalExpression + greater + concatExpression, (_) => new());
        relationalExpression %= (relationalExpression + greaterEqual + concatExpression, (_) => new());
        relationalExpression %= (relationalExpression + isTerminal + identifier, (_) => new());
        relationalExpression %= (relationalExpression + asTerminal + identifier, (_) => new());
        relationalExpression %= (concatExpression, (_) => new());

        concatExpression %= (concatExpression + concat + aritmeticExpression, (_) => new());
        concatExpression %= (concatExpression + doubleConcat + aritmeticExpression, (_) => new());
        concatExpression %= (aritmeticExpression, (_) => new());

        aritmeticExpression %= (aritmeticExpression + plus + multExpression, (_) => new());
        aritmeticExpression %= (aritmeticExpression + minus + multExpression, (_) => new());
        aritmeticExpression %= (multExpression, (_) => new());

        multExpression %= (multExpression + times + exponentialExpression, (_) => new());
        multExpression %= (multExpression + divide + exponentialExpression, (_) => new());
        multExpression %= (multExpression + mod + exponentialExpression, (_) => new());
        multExpression %= (exponentialExpression, (_) => new());

        exponentialExpression %= (unaryExpression + power + exponentialExpression, (_) => new());
        exponentialExpression %= (unaryExpression, (_) => new());

        unaryExpression %= (plus + primaryExpression, (_) => new());
        unaryExpression %= (minus + primaryExpression, (_) => new());
        unaryExpression %= (notOperator + primaryExpression, (_) => new());
        unaryExpression %= (primaryExpression, (_) => new());

        primaryExpression %= (literal, (_) => new());
        primaryExpression %= (invocationExpression, (_) => new());
        primaryExpression %= (identifier, (_) => new());
        primaryExpression %= (vector, (_) => new());
        primaryExpression %= (comprehensionVector, (_) => new());
        primaryExpression %= (indexedValue, (_) => new());
        primaryExpression %= (memberAccess, (_) => new());
        primaryExpression %= (openParenthesis + expression + closeParenthesis, (_) => new());
        primaryExpression %= (instatiation, (_) => new());

        invocationExpression %= (identifier + openParenthesis + argumentList + closeParenthesis, (_) => new());
        invocationExpression %= (identifier + openParenthesis + closeParenthesis, (_) => new());

        argumentList %= (argumentList + comma + expression, (_) => new());
        argumentList %= (expression, (_) => new());

        vector %= (openBracket + vectorElement + closeBracket, (_) => new());

        vectorElement %= (vectorElement + comma + expression, (_) => new());
        vectorElement %= (expression, (_) => new());

        comprehensionVector %= (openBracket + expression + doublePipe + identifier + inTerminal + expression + closeBracket, (_) => new());

        indexedValue %= (primaryExpression + openBracket + expression + closeBracket, (_) => new());

        memberAccess %= (primaryExpression + dot + identifier, (_) => new());
        memberAccess %= (primaryExpression + dot + invocationExpression, (_) => new());

        instatiation %= (newTerminal + invocationExpression, (_) => new());

        literal %= (numberLiteral, (_) => new());
        literal %= (stringLiteral, (_) => new());
        literal %= (trueTerminal, (_) => new());
        literal %= (falseTerminal, (_) => new());

        return grammar;
    }
}