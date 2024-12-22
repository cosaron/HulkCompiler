namespace HulkCompiler.Parser.Grammar;

using System.Diagnostics;
using System.Text;

public class Item : IEquatable<Item>
{
    public bool CanReduce { get => DotPosition == Body.Length; }


    public Symbol NextSymbol => Body[DotPosition];

    public NonTerminal Head { get; set; }

    public AttributedSentence Body { get; set; }

    public int DotPosition { get; set; }

    public Symbol LookAhead { get; set; }

    public Item(NonTerminal head, AttributedSentence body, int dotPosition, Symbol lookAhead)
    {
        Debug.Assert(0 <= dotPosition && dotPosition <= body.Length, "");

        Head = head;
        Body = body;
        DotPosition = dotPosition;
        LookAhead = lookAhead;
    }

    public Item MoveDot()
    {
        Debug.Assert(DotPosition <= Body.Length, "");
        return new Item(Head, Body, DotPosition++, LookAhead);
    }

    public override string ToString()
    {
        StringBuilder sb = new();
        sb.Append(Head);
        sb.Append(" -> ");
        for (int i = 0; i < Body.Length; i++)
        {
            if (i == DotPosition)
            {
                sb.Append('.');
            }
            sb.Append(Body[i]);
            sb.Append(' ');
        }
        if (DotPosition == Body.Length)
        {
            sb.Append('.');
        }
        sb.Append(", ");
        sb.Append(LookAhead);
        return sb.ToString();
    }

    public bool Equals(Item? other) =>
        other is not null
        && Head == other.Head
        && Body == other.Body
        && DotPosition == other.DotPosition
        && LookAhead == other.LookAhead;


    public override bool Equals(object? obj)
    {
        return obj is Item i && Equals(i);
    }

    public override int GetHashCode()
    {
        return ToString().GetHashCode();
    }
}