namespace HulkCompiler.SemanticAnalizer;

public static class HulkBuiltIn
{
    private static readonly Protocol Iterable = new(
        name: "Iterable",
        new Method("next", BooleanType.Instance),
        new Method("current", ObjectType.Instance)
    );

    private static readonly Method Sqrt = new(
        name: "sqrt",
        returnType: NumberType.Instance,
        parameters: new Variable("value", NumberType.Instance)
    );
    private static readonly Method Sin = new(
        name: "sin",
        returnType: NumberType.Instance,
        parameters: new Variable("value", NumberType.Instance)
    );
    private static readonly Method Cos = new(
        name: "cos",
        returnType: NumberType.Instance,
        parameters: new Variable("value", NumberType.Instance)
    );
    private static readonly Method Log = new(
        name: "log",
        returnType: NumberType.Instance,
        new Variable("value", NumberType.Instance),
        new Variable("base", NumberType.Instance)
    );
    private static readonly Method Exp = new(
        name: "exp",
        returnType: NumberType.Instance,
        new Variable("value", NumberType.Instance),
        new Variable("base", NumberType.Instance)
    );
    private static readonly Method Rand = new(
        name: "rand",
        returnType: NumberType.Instance
    );
    private static readonly Method Print = new(
        name: "print",
        returnType: StringType.Instance,
        new Variable("object", ObjectType.Instance)
    );
    private static readonly Method Range = new(
        name: "range",
        returnType: RangeType.Instance,
        new Variable("start", NumberType.Instance),
        new Variable("end", NumberType.Instance)
    );

    public static Protocol[] Protocols => [Iterable];
    public static Method[] Methods => [Sqrt, Sin, Cos, Log, Exp, Rand, Print, Range];
    public static Type[] Types => [ObjectType.Instance, NumberType.Instance, StringType.Instance, BooleanType.Instance, RangeType.Instance];

}