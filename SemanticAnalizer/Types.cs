namespace HulkCompiler.SemanticAnalizer;


public class Variable(string name, Type type) : IEquatable<Variable>
{
    public string Name { get; private set; } = name;
    public Type Type { get; private set; } = type;

    public bool Equals(Variable? other) => other != null && Name == other.Name;

    public override bool Equals(object? obj) => obj is Variable v && Equals(v);

    public override int GetHashCode() => Name.GetHashCode();

    public override string ToString() => $"{Name}: {Type}";
}


public class Method(string name, Type returnType, params Variable[] parameters) : IEquatable<Method>
{
    public string Name { get; private set; } = name;
    public Type ReturnType { get; private set; } = returnType;
    public readonly List<Variable> Parameters = [.. parameters];


    public Variable? GetParameter(string name) => Parameters.Find(p => p.Name == name);

    public bool Equals(Method? other) => (
        other != null
        && Name == other.Name
        && Parameters.Count == other.Parameters.Count
        && ReturnType == other.ReturnType
    );

    public override bool Equals(object? obj) => obj is Method m && Equals(m);
    public override int GetHashCode() => ToString().GetHashCode();
    public override string ToString() => $"{Name} ({string.Join(", ", Parameters)}) -> {ReturnType}";

}


public class Type(
    string name,
    Type? parent = null,
    Variable[]? parameters = null,
    Variable[]? attributes = null,
    Method[]? methods = null
) : IEquatable<Type>
{
    public string Name { get; private set; } = name;
    public Type? Parent { get; private set; } = parent;
    public readonly List<Variable> Attributes = attributes?.ToList() ?? [];
    public readonly List<Variable> Parameters = parameters?.ToList() ?? [];
    public readonly List<Method> Methods = methods?.ToList() ?? [];


    static private Type _getLowerAncestor(Type type1, Type type2)
    {
        if (type1.ConformsTo(type2)) return type2;
        if (type2.ConformsTo(type1)) return type1;

        if (type1.Parent is not null) return _getLowerAncestor(type1.Parent, type2);
        if (type2.Parent is not null) return _getLowerAncestor(type1, type2.Parent);

        return type1;
    }


    public bool ConformsTo(Type other)
    {
        if (other is UnknownType) return true;
        if (this == other) return true;
        if (Parent != null) return Parent.ConformsTo(other);

        return false;
    }

    public Method? GetMethod(string name) => Methods.Find(m => m.Name == name) ?? Parent?.GetMethod(name);
    public void SetMethod(Method method) => Methods.Add(method);

    public Variable? GetParameter(string name) => Parameters.Find(p => p.Name == name);
    public void SetParameter(Variable parameter) => Parameters.Add(parameter);

    public Variable? GetAttribute(string name) => Attributes.Find(a => a.Name == name) ?? Parent?.GetAttribute(name);
    public void SetAttribute(Variable attribute) => Attributes.Add(attribute);

    public void SetParent(Type parent) => Parent = parent;

    public bool Equals(Type? other) => other is not null && Name == other?.Name;
    public override bool Equals(object? obj) => obj is Type t && Equals(t);
    public override int GetHashCode() => Name.GetHashCode();

    public override string ToString() => Name;
}

public class Protocol(string name, params Method[] methods) : IEquatable<Protocol>
{
    public string Name { get; private set; } = name;
    private readonly Dictionary<string, Method> _methods = methods.ToDictionary(m => m.Name);

    public Method[] Methods => [.. _methods.Values];
    public Method? GetMethod(string name) => _methods.GetValueOrDefault(name);
    public void SetMethod(Method method) => _methods.TryAdd(method.Name, method);

    public bool IsImplementedBy(Type type)
    {
        foreach (var method in _methods.Values)
        {
            Method? typeMethod = type.GetMethod(method.Name);
            if (typeMethod is null) return false;
            if (!typeMethod.ReturnType.ConformsTo(method.ReturnType)) return false;
            if (typeMethod.Parameters.Count != method.Parameters.Count) return false;

            foreach (var (p1, p2) in method.Parameters.Zip(typeMethod.Parameters))
                if (!p1.Type.ConformsTo(p2.Type)) return false;

        }

        return true;
    }

    public bool Equals(Protocol? other) => other is not null && Name == other.Name;
    public override bool Equals(object? obj) => obj is Protocol p && Equals(p);
    public override int GetHashCode() => Name.GetHashCode();

    public override string ToString() => $"Protocol {Name}:: {string.Join(", ", _methods.Values)}";

}


public class UnknownType : Type
{
    static public UnknownType Instance { get; private set; } = new();
    private UnknownType() : base("Unknown") { }
}

public class ObjectType : Type
{
    static public ObjectType Instance { get; private set; } = new();
    private ObjectType() : base("Object") { }

}

public class NumberType : Type
{
    static public NumberType Instance { get; private set; } = new();
    private NumberType() : base("Number", ObjectType.Instance) { }
}

public class StringType : Type
{
    static public StringType Instance { get; private set; } = new();
    private StringType() : base("String", ObjectType.Instance) { }
}

public class BooleanType : Type
{
    static public BooleanType Instance { get; private set; } = new();
    private BooleanType() : base("Boolean", ObjectType.Instance) { }
}

public class RangeType : Type
{
    static public RangeType Instance { get; private set; } = new();
    private RangeType() : base(
        name: "Range",
        parent: ObjectType.Instance,
        parameters: [new("min", NumberType.Instance), new("max", NumberType.Instance)],
        methods: [new("current", NumberType.Instance), new("next", BooleanType.Instance)],
        attributes: [new("min", NumberType.Instance), new("max", NumberType.Instance), new("current", NumberType.Instance)]
    )
    { }
}

public class VectorType(Type itemsType) : Type(
    name: "Vector",
    parent: ObjectType.Instance,
    parameters: [],
    methods: [new("current", itemsType), new("next", BooleanType.Instance), new("size", NumberType.Instance)],
    attributes: []
    )
{
    public Type ItemsType { get; private set; } = itemsType;
}