namespace HulkCompiler.SemanticAnalizer.Types;


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

    public bool Equals(Type? other) => other is not null && Name == other?.Name;
    public override bool Equals(object? obj) => obj is Type t && Equals(t);
    public override int GetHashCode() => Name.GetHashCode();

    public override string ToString() => Name;
}

public class Protocol(string name, params Method[] methods) : IEquatable<Protocol>
{
    public string Name { get; private set; } = name;
    private readonly Dictionary<string, Method> Methods = methods.ToDictionary(m => m.Name);

    public Method? GetMethod(string name) => Methods.GetValueOrDefault(name);
    public void SetMethod(Method method) => Methods.TryAdd(method.Name, method);

    public bool IsImplementedBy(Type type)
    {
        foreach (var method in Methods.Values)
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

    public override string ToString() => $"Protocol {Name}:: {string.Join(", ", Methods.Values)}";

}



