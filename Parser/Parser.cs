//External Imports
using System.Collections.Frozen;
//Internal Imports
using HulkCompiler.Parser.Grammar;
using HulkCompiler.Parser.Ast;
using HulkCompiler.Lexer;
using System.Text.Json;

namespace HulkCompiler.Parser
{
    //Alias
    using Grammar_ = Grammar.Grammar;
    using ActionTable = Dictionary<int, Dictionary<Symbol, IParsingAction>>;
    using ParserTable = Dictionary<int, Dictionary<Symbol, int>>;


    interface IParsingAction;

    class Shift(int nextState) : IParsingAction
    {
        public int NextState { get; } = nextState;

    }

    class Reduce((NonTerminal head, AttributedSentence body) production) : IParsingAction
    {
        public (NonTerminal head, AttributedSentence body) Production { get; } = production;

    }

    class GoTo(int nextState) : IParsingAction
    {
        public int NextState { get; } = nextState;

    }

    class Accept : IParsingAction;



    public class ParserLR1
    {
        private readonly string _cachePath = "./Cache/";
        private readonly Grammar_ _grammar;
        private readonly Dictionary<TokenType, Symbol> _mapping;
        private readonly ActionTable _actionTable;

        //TODO: Improve this method using async
        private ActionTable GetGrammar()
        {
            if (File.Exists(_cachePath + "hash"))
            {
                try
                {
                    int hash = int.Parse(File.ReadAllText(_cachePath + "hash"));
                    if (hash == _grammar.GetHashCode())
                    {
                        string json = File.ReadAllText(_cachePath + "cached_grammar");
                        ActionTable? table = JsonSerializer.Deserialize<ActionTable>(json);
                        if (table is not null)
                            return table;
                    }
                }
                catch (Exception e) { }


            }

            ActionTable newGrammar = CompileGrammar();

            try
            {
                File.WriteAllText(_cachePath + "hash", _grammar.GetHashCode().ToString());
                File.WriteAllText(_cachePath + "cached_grammar", JsonSerializer.Serialize(newGrammar));
            }
            catch (IOException e) { }


            return newGrammar;
        }

        private ActionTable CompileGrammar()
        {
            var (states, goTo) = BuildStates();

            ActionTable actionTable = [];

            foreach (var (numState, stateItems) in states)
            {
                actionTable[numState] = [];
                var state = actionTable[numState];
                foreach (var item in stateItems)
                {
                    if (item.CanReduce)
                    {
                        if (item.Head == _grammar.Seed!)
                        {
                            if (!state.ContainsKey(_grammar.Eof))
                                state[_grammar.Eof] = new Accept();
                            else
                                throw new Exception("Grammar is not LR(1)");
                        }
                        else
                        {
                            if (!state.ContainsKey(item.LookAhead))
                                state[item.LookAhead] = new Reduce((item.Head, item.Body));
                            else
                                throw new Exception("");
                        }
                    }
                    else
                    {
                        if (item.NextSymbol.IsTerminal)
                        {
                            if (!state.ContainsKey(item.NextSymbol))
                                state[item.NextSymbol] = new Shift(goTo[numState][item.NextSymbol]);
                        }
                        else
                        {

                            if (!state.ContainsKey(item.NextSymbol))
                                state[item.NextSymbol] = new GoTo(goTo[numState][item.NextSymbol]);
                        }
                    }
                }
            }

            return actionTable;

        }
        private (Dictionary<int, HashSet<Item>>, ParserTable) BuildStates()
        {
            Dictionary<Symbol, HashSet<Symbol>> firsts = _grammar.GetFirst();

            HashSet<Item> initialItems = (HashSet<Item>)(from sentence in _grammar.Productions[_grammar.Seed!] select new Item(_grammar.Seed!, sentence, 0, _grammar.Eof));

            HashSet<Item> actualItems = _grammar.GetClousure(initialItems, firsts);

            int numStates = 1;

            Dictionary<int, HashSet<Item>> states = new() { { 0, actualItems } };

            Dictionary<FrozenSet<Item>, int> statesKernels = new() { { FrozenSet.ToFrozenSet<Item>(initialItems), 0 } };

            ParserTable goTo = [];

            int count = 0;

            while (count < numStates)
            {
                var actualStateItems = states[count];

                foreach (var symbol in _grammar.Symbols)
                {
                    HashSet<Item> newKernel = [];
                    foreach (var item in actualStateItems)
                    {
                        if (item.NextSymbol == symbol) newKernel.Add(item);
                    }

                    if (newKernel.Count == 0) continue;

                    var frozenNewKernel = FrozenSet.ToFrozenSet(newKernel);

                    if (statesKernels.ContainsKey(frozenNewKernel))
                    {
                        if (goTo.ContainsKey(count))
                        {
                            goTo[count][symbol] = statesKernels[frozenNewKernel];
                        }
                        else
                        {
                            goTo[count] = new() { { symbol, statesKernels[frozenNewKernel] } };
                        }
                        continue;
                    }
                    HashSet<Item> newItems = _grammar.GetClousure((HashSet<Item>)(from item in frozenNewKernel select item.MoveDot()), firsts);

                    states[numStates] = newItems;
                    if (goTo.ContainsKey(count))
                    {
                        goTo[count][symbol] = numStates;
                    }
                    else
                    {
                        goTo[count] = new() { { symbol, numStates } };
                    }

                    statesKernels[frozenNewKernel] = numStates;
                    numStates++;
                }
                count++;
            }
            return (states, goTo);
        }

        public ParserLR1(Grammar_ grammar, Dictionary<TokenType, Symbol> mapping)
        {
            _grammar = grammar;
            _mapping = mapping;
            _actionTable = GetGrammar();
        }

        public AstNode Parse(Token[] tokens)
        {
            (Symbol, Token)[] symbols = (from token in tokens select (_mapping[token.Type], token)).ToArray();

            Stack<int> controlStack = new();
            Stack<int> stateStack = new([0]);

            Stack<AstNode> astNodes = new();
            Stack<(Symbol, Token)> tokenStack = new();

            int i = 0;
            while (true)
            {
                int actualState = stateStack.Pop();

                (Symbol S, Token T) token = symbols[i];
                if (!_actionTable[actualState].TryGetValue(token.S, out IParsingAction? action))
                    throw new Exception();
                else
                {
                    if (action is Shift shiftAction)
                    {
                        tokenStack.Push(token);
                        controlStack.Push(0);
                        actualState = shiftAction!.NextState;
                        i++;
                    }
                    else if (action is Reduce reduceAction)
                    {
                        List<AstNode> _nodes = [];
                        List<Token> _tokens = [];
                        for (int j = 0; j < reduceAction!.Production.body.Length; j++)
                        {
                            if (controlStack.Pop() == 0)
                                _tokens.Add(tokenStack.Pop().Item2);
                            else
                                _nodes.Add(astNodes.Pop());

                            stateStack.Pop();
                        }
                        _nodes.Reverse();
                        _tokens.Reverse();

                        astNodes.Push(reduceAction.Production.body.Attributate([.. _tokens], [.. _nodes]));
                        controlStack.Push(1);
                        actualState = stateStack.Peek();
                    }
                    else if (action is GoTo goToAction)
                    {
                        actualState = goToAction!.NextState;
                    }
                    else if (action is Accept)
                    {
                        return astNodes.Pop();
                    }
                    else
                        throw new Exception();
                }

            }

        }
    }
}