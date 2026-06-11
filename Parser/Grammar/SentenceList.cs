using System.Collections;

namespace HulkCompiler.Parser.Grammar;

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