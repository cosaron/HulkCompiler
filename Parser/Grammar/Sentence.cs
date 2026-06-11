namespace HulkCompiler.Parser.Grammar;

public class Sentence(List<Symbol> symbols)
{
    private readonly List<Symbol> _symbols = symbols;

    public int Length { get => _symbols.Count; }

    public Symbol this[int index]
    {
        get
        {
            if (index >= _symbols.Count || index <= 0) throw new IndexOutOfRangeException();

            return _symbols[index];
        }
    }

    public static Sentence operator +(Sentence self, Symbol other)
    {
        self._symbols.Add(other);
        return self;
    }

    public override string ToString() => string.Join<Symbol>(" + ", [.. _symbols]);



}