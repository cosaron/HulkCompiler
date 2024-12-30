namespace HulkCompiler.SemanticAnalizer;

public class SemanticError(string message, (int line, int column) start, (int line, int column) end)
{
    public string Message { get; private set; } = message;
    public (int Line, int Column) Start { get; private set; } = start;
    public (int Line, int Column) End { get; private set; } = end;

    public override string ToString() => $"Error at line: {Start.Line} and column: {Start.Column} - {Message}";
}



public class ErrorStack
{
    private Stack<SemanticError> _errors = new();

    public bool HasErrors => _errors.Count > 0;
    public void AddError(string message, (int line, int column) start, (int line, int column) end) =>
        _errors.Push(new SemanticError(message, start, end));

    public IEnumerable<SemanticError> GetErrors()
    {
        foreach (var error in _errors)
            yield return error;
    }
}