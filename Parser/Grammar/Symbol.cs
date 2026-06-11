namespace HulkCompiler.Parser.Grammar;

public class Symbol(string value, Grammar grammar)
{
    protected readonly string _value = value;

    protected readonly Grammar _grammar = grammar;
    public bool IsTerminal { get => _grammar.Terminals.Contains(this); }

    public static Sentence operator +(Symbol self, Symbol other) => new([self, other]);

    public static bool operator ==(Symbol self, Symbol other) => self._value == other._value;

    public static bool operator !=(Symbol self, Symbol other) => !(self == other);



    public override bool Equals(object? obj)
    {

        if (ReferenceEquals(this, obj))
        {
            Symbol? other = obj as Symbol;
            return this == (other ?? new Symbol("$", _grammar));
        }

        return false;
    }

    public override int GetHashCode() => _value.GetHashCode();

    public override string ToString()
    {
        return _value.ToString();
    }

}