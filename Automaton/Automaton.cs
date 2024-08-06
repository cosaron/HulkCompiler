namespace HulkCompiler.Automaton;

public abstract class Automaton
{
    public abstract bool Accept(string input);
    public abstract int Match(string input);
    public abstract string GetMatch(string input);
}

public class NFAState
{
    public Dictionary<char, HashSet<NFAState>> Transitions { get; private set; } = [];

    public HashSet<NFAState> EpsilonTransitions { get; private set; } = [];

    public void AddEpsilonTransition(NFAState state)
    {
        EpsilonTransitions.Add(state);
    }

    public bool IsFinal { get; private set; } = false;

    public void SetFinal() => IsFinal = true;

    public static HashSet<NFAState> EpsilonClousure(HashSet<NFAState> states)
    {
        Stack<NFAState> pendings = new(states);
        HashSet<NFAState> clousure = new(states);

        while (pendings.Count > 0)
        {
            NFAState currentState = pendings.Pop();
            foreach (NFAState state in currentState.EpsilonTransitions)
            {
                if (clousure.Add(state))
                    pendings.Push(state);
            }
        }

        return clousure;
    }

    public static HashSet<NFAState> NextStates(HashSet<NFAState> states, char symbol)
    {
        HashSet<NFAState> nextStates = [];
        foreach (NFAState state in EpsilonClousure(states))
            if (state.Transitions.TryGetValue(symbol, out HashSet<NFAState>? value))
                nextStates.UnionWith(value);

        return EpsilonClousure(nextStates);
    }

    public override string ToString()
    {
        return base.ToString() + "\nEpsilon Transitions" + string.Join(", ", EpsilonTransitions.Select(t => $"{t}")) + "\n";
    }
}

public class NFA : Automaton
{
    public NFAState InitialState { get; } = new();
    public override bool Accept(string input)
    {
        throw new NotImplementedException();
    }

    public override string GetMatch(string input)
    {
        throw new NotImplementedException();
    }

    public override int Match(string input)
    {
        throw new NotImplementedException();
    }

    public DFA ToDeterministic()
    {
        int statesCount = 0;
        HashSet<NFAState> initialClousure = NFAState.EpsilonClousure([InitialState]);

        Dictionary<int, (Dictionary<char, int> transitions, bool isFinal)> dfaTable = new() { { 0, ([], initialClousure.Any((s) => s.IsFinal)) } };

        Dictionary<HashSet<NFAState>, int> clousures = new() { { initialClousure, 0 } };
        Stack<HashSet<NFAState>> pending = new();

        while (pending.Count > 0)
        {
            HashSet<NFAState> currentClousure = pending.Pop();
            char[] symbols = currentClousure.SelectMany((s) => s.Transitions.Keys).Distinct().ToArray();
            foreach (char symbol in symbols)
            {
                HashSet<NFAState> nextClousure = NFAState.NextStates(currentClousure, symbol);
                if (!clousures.TryGetValue(nextClousure, out int nextState))
                {
                    nextState = statesCount++;
                    clousures[nextClousure] = nextState;
                    pending.Push(nextClousure);
                    if (dfaTable.TryGetValue(nextState, out var value))
                        if (!value.transitions.ContainsKey(symbol))
                            value.transitions[symbol] = nextState;
                        else
                            throw new Exception();
                    else
                        dfaTable[nextState] = (new() { { symbol, nextState } }, nextClousure.Any((s) => s.IsFinal));
                }
                else
                {
                    if (dfaTable.TryGetValue(nextState, out var value))
                        if (!value.transitions.ContainsKey(symbol))
                            value.transitions[symbol] = nextState;
                        else
                            throw new Exception();

                }

            }
        }
        return new DFA(dfaTable);

    }

    public class DFA : Automaton
    {
        private Dictionary<int, (Dictionary<char, int> Transitions, bool IsFinal)> TransitionTable { get; set; }


        public DFA() => TransitionTable = [];
        public DFA(Dictionary<int, (Dictionary<char, int>, bool)> table) => TransitionTable = table;

        public override bool Accept(string input)
        {
            int currentState = 0;
            foreach (char c in input)
            {
                if (!TransitionTable[currentState].Transitions.TryGetValue(c, out int nextState))
                    return false;
                currentState = nextState;
            }
            return TransitionTable[currentState].IsFinal;
        }

        public override string GetMatch(string input)
        {
            try
            {
                return input[..Match(input)];
            }
            catch (IndexOutOfRangeException)
            {
                return "";
            }
        }

        public override int Match(string input)
        {
            int currentState = 0;

            for (int i = 0; i < input.Length; i++)
            {
                char c = input[i];
                if (TransitionTable[currentState].Transitions.TryGetValue(c, out int nextState))
                    currentState = nextState;
                else
                    return TransitionTable[currentState].IsFinal ? i : -1;
            }

            return TransitionTable[currentState].IsFinal ? input.Length : -1;


        }

    }


}