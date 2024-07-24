namespace Parser.Grammar;

using System.Collections;
using System.Text;
using Parser.Ast;

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

public class NonTerminal(string value, Grammar grammar) : Symbol(value, grammar)
{
    public static NonTerminal operator %(NonTerminal self, (Sentence, Func<Sentence, AstNode>) attributation)
    {
        AttributedSentence attributedSentence = new(attributation.Item1, attributation.Item2);

        if (self._grammar.Productions.TryGetValue(self, out SentenceList? value))
        {
            value.Append(attributedSentence);
        }
        else
        {
            self._grammar.Productions[self] = new([attributedSentence]);

        }

        return self;
    }

    public static NonTerminal operator %(NonTerminal self, (Symbol, Func<Sentence, AstNode>) attributation)
    {
        AttributedSentence attributedSentence = new(new Sentence([attributation.Item1]), attributation.Item2);
        if (self._grammar.Productions.TryGetValue(self, out SentenceList? value))
        {
            value.Append(attributedSentence);
        }
        else
        {
            self._grammar.Productions[self] = new([attributedSentence]);

        }

        return self;
    }

    public override string ToString()
    {
        return _value.ToString();
    }
}



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

public class AttributedSentence(Sentence sentence, Func<Sentence, AstNode> attributation)
{
    private readonly Sentence _sentence = sentence;

    private readonly Func<Sentence, AstNode> _attributation = attributation;

    public int Length { get => _sentence.Length; }

    public Symbol this[int index] { get => _sentence[index]; }
    public Symbol First { get => _sentence[0]; }

    public override string ToString()
    {
        return _sentence.ToString() + _attributation.ToString();
    }

    public AstNode Attributate() => _attributation(_sentence);

}


public class SentenceList(List<AttributedSentence> sentences) : IEnumerable<AttributedSentence>
{
    private readonly List<AttributedSentence> _sentences = sentences;

    public void Append(AttributedSentence sentence)
    {
        this._sentences.Add(sentence);
    }

    public IEnumerator<AttributedSentence> GetEnumerator()
    {
        return _sentences.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return _sentences.GetEnumerator();
    }
}

/// <summary>
/// Class <c>Grammar</c> represents a Free Context Grammar
/// </summary>

public class Grammar
{

    public NonTerminal? Seed { get; private set; }
    /// <value>
    /// Property <c>Terminals</c> represents the Terminals defined in the Grammar.
    /// A Terminal is a Symbol that can't be produced by the combination of another Symbols.
    /// </value>
    public HashSet<Symbol> Terminals { get; private set; }


    /// <value>
    /// Property <c>NonTerminals</c> represent the set of NonTerminals in the Grammar. A NonTerminal is 
    /// a Symbol that can be produced by a combination of Terminals and NonTerminals and can derive in that Symbols
    /// </value>
    public HashSet<NonTerminal> NonTerminals { get; private set; }

    /// <value>
    /// Property <c>Productions</c> represent the relationship between the NonTerminals and the Symbols on which it can derive.
    /// <c>Productions</c> is a <c>Dictionary NonTerminal,SentenceList </c> where the NonTerminal key 
    /// correspond to the list of sentences on which it can derive.
    /// </value>
    public Dictionary<NonTerminal, SentenceList> Productions { get; private set; }


    public IEnumerable<Symbol> Symbols { get => NonTerminals.Union(Terminals); }

    /// <value>
    /// Property <c>Eof</c> represent a special Symbol in the Grammar. The symbol mean is the end of a string that belongs to the Grammar
    /// </value>
    public Symbol Eof { get; private set; }

    /// <summary>
    /// Initialize a new empty instance of the <c>Grammar</c> class
    /// </summary>
    public Grammar()
    {
        Terminals = [];
        NonTerminals = [];
        Eof = SetTerminals("$")[0];
        Productions = [];
    }

    public void SetSeed(NonTerminal seed) => Seed = seed;

    /// <summary>
    /// This method given an array of strings creates an array of Symbol, adds them to the Grammar and returns.
    /// These Symbols can be decontructed with the tuple syntax for a more readable code with a limit of 6 Symbols for call.
    /// </summary>
    /// <param name="nonTerminals">
    /// An array of <c>string</c> that represent the values for create the NonTerminals
    /// </param>
    /// <returns>
    /// An array of NonTerminals
    /// </returns>
    /// <code>
    /// Grammar grammar = new();
    /// var (E,T,F,i) = grammar.NonTerminals("Ex","Term","Factor","Number");
    /// </code>
    public NonTerminal[] SetNonTerminals(params string[] nonTerminals)
    {
        List<NonTerminal> newNonTerminals = [];
        foreach (var str in nonTerminals)
        {
            NonTerminal newNonTerminal = new(str, this);
            NonTerminals.Add(newNonTerminal);
            newNonTerminals.Add(newNonTerminal);
        }

        return [.. newNonTerminals];

    }

    public Symbol[] SetTerminals(params string[] terminals)
    {
        List<Symbol> newTerminals = [];
        foreach (var str in terminals)
        {
            Symbol newTerminal = new(str, this);
            this.Terminals.Add(newTerminal);
            newTerminals.Add(newTerminal);
        }

        return [.. newTerminals];
    }


    public Dictionary<Symbol, HashSet<Symbol>> GetFirst()
    {
        Dictionary<Symbol, HashSet<Symbol>> firstSet = [];

        foreach (var terminal in Terminals)
        {
            firstSet[terminal] = [terminal];
        }

        foreach (var nonTerminal in NonTerminals)
        {
            firstSet[nonTerminal] = [];
        }

        bool hasChanged = true;

        do
        {
            hasChanged = false;
            foreach (var (head, body) in Productions)
            {
                foreach (var sentence in body)
                {
                    int oldLength = firstSet[head].Count;
                    HashSet<Symbol> newFirst = [];

                    foreach (var first in firstSet[sentence.First])
                    {
                        newFirst.Add(first);
                    }

                    firstSet[head] = firstSet[head].Union(newFirst).ToHashSet();
                    hasChanged = oldLength != firstSet[head].Count;
                }
            }

        } while (hasChanged);

        return firstSet;
    }

    public HashSet<Item> GetClousure(HashSet<Item> items, Dictionary<Symbol, HashSet<Symbol>> firsts)
    {
        bool hasChanged = true;
        do
        {
            int oldLength = items.Count;
            HashSet<Item> newItems = [];
            foreach (var item in items)
            {
                if (!item.CanReduce)
                {
                    Symbol itemNextProduction = item.NextSymbol;
                    if (!itemNextProduction.IsTerminal)
                    {
                        NonTerminal? nextProduction = itemNextProduction as NonTerminal;
                        //TODO make a error catcher for the posible null reference in the downcast operation
                        foreach (var (head, body) in Productions)
                        {
                            foreach (var first in firsts[item.CanReduce ? item.NextSymbol : item.LookAhead])
                            {
                                foreach (var sentence in body)
                                {
                                    newItems.Add(new Item(nextProduction!, sentence, 0, first));
                                }
                            }
                        }
                    }
                }
            }
            items.UnionWith(newItems);
            hasChanged = items.Count != oldLength;

        } while (hasChanged);

        return items;
    }


    public override string ToString()
    {
        StringBuilder message = new();
        message.Append("Grammar Productions :\n");
        foreach (var item in Productions)
        {
            message.Append(item.Key);
            message.Append(" --> ");
            foreach (var sentence in item.Value)
            {
                message.Append(sentence);
                message.Append("\n|");
            }
            message.Append('\n');
        }

        return message.ToString();
    }

}


