namespace HulkCompiler.SemanticAnalizer;


public class Context(Context? father)
{
    private Dictionary<string, Type> _types = [];
    private Dictionary<string, Variable> _variables = [];
    private Dictionary<string, Protocol> _protocols = [];
    private List<Method> _methods = [];
    private Context? _father = father;

    public Protocol IterableProtocol => GetProtocol("Iterable");

    private bool ContainsType(string name) =>
        _types.ContainsKey(name) || (_father?.ContainsType(name) ?? false);
    public Type GetType(string name) =>
    _types.GetValueOrDefault(name)
    ?? _father?.GetType(name)
    ?? throw new Exception($"Type {name} not found");
    public void DefineType(Type type)
    {
        if (ContainsType(type.Name))
            throw new Exception($"Type {type.Name} already defined");

        _types.Add(type.Name, type);
    }


    private bool ContainsVariable(string name) =>
        _variables.ContainsKey(name) || (_father?.ContainsVariable(name) ?? false);
    public Variable GetVariable(string name) =>
        _variables.GetValueOrDefault(name)
        ?? _father?.GetVariable(name)
        ?? throw new Exception($"Variable {name} not found");
    public Type GetVariableType(string name) => GetVariable(name).Type;
    public void DefineVariable(Variable variable)
    {
        if (ContainsVariable(variable.Name))
            throw new Exception($"Variable {variable.Name} already defined");

        _variables.Add(variable.Name, variable);
    }

    private bool ContainsProtocol(string name) =>
        _protocols.ContainsKey(name) || (_father?.ContainsProtocol(name) ?? false);
    public Protocol GetProtocol(string name) =>
        _protocols.GetValueOrDefault(name)
        ?? _father?.GetProtocol(name)
        ?? throw new Exception($"Protocol {name} not found");
    public void DefineProtocol(Protocol protocol)
    {
        if (ContainsProtocol(protocol.Name))
            throw new Exception($"Protocol {protocol.Name} already defined");

        _protocols.Add(protocol.Name, protocol);
    }

    public bool ContainsMethod(string name) =>
        _methods.Any(m => m.Name == name) || (_father?.ContainsMethod(name) ?? false);
    public Method GetMethod(string name, int paramsCount) =>
        _methods.Find((m) => m.Name == name && m.Parameters.Count == paramsCount)
        ?? _father?.GetMethod(name, paramsCount)
        ?? throw new Exception($"Method {name} with {paramsCount} parameters not found");
    public void DefineMethod(Method method)
    {
        if (ContainsMethod(method.Name))
            throw new Exception($"Method {method.Name} already defined");

        _methods.Add(method);
    }


    public void DefineBuiltIns()
    {
        foreach (var protocol in HulkBuiltIn.Protocols)
            DefineProtocol(protocol);

        foreach (var method in HulkBuiltIn.Methods)
            DefineMethod(method);
    }

    public Context CreateChildContext() => new(this);

}