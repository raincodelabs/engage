using System;
using System.Collections.Generic;
using System.Linq;
using Engage.FC;

namespace Engage.FA;

public static class Enumerator
{
    private static int _max = 1;

    public static int GetId()
        => _max++;
}

public class State
{
    private readonly int _id;

    public string Name => $"State{_id}";

    public HashSet<FC.SignedFlag> Flags { get; } = new();
    public Stack<FC.StackAction> StackState { get; } = new();

    public State(IEnumerable<FC.SignedFlag> flags, IEnumerable<FC.StackAction> stack)
    {
        if (flags != null)
            Flags.UnionWith(flags);
        if (stack != null)
            foreach (var action in stack.Reverse()) //NB: reversing is important
                StackState.Push(action);
        _id = Enumerator.GetId();
        Console.WriteLine($"CREATE {ToString()}");
    }

    public FA.State Apply(Formula formula, FA.StateMachine machine)
        => machine.ForgeState(formula.ChangeFlags(Flags), formula.ChangeStack(StackState));

    public bool FlagsEqual(HashSet<SignedFlag> flags)
    {
        var t1 = String.Join(";", Flags);
        var t2 = String.Join(";", flags);
        //Console.WriteLine($"IN {_id}, CMP [{t1}] to [{t2}]: {Flags.SetEquals(flags)}");
        return Flags.SetEquals(flags);
    }

    public bool StackExpectationsEqual(Stack<StackAction> stack)
        => StackState.SequenceEqual(stack);

    public string ToLabel()
        => "[" + String.Join(";", Flags) + "] {" + String.Join(";", StackState) + "}";

    public string ToDot()
        => $"{Name} [label=\"{ToLabel()}\"];";

    public override bool Equals(object obj)
        => obj is State other && Flags.SetEquals(other.Flags) && StackState.SequenceEqual(other.StackState);

    public override int GetHashCode()
        => Flags.GetHashCode() + StackState.GetHashCode();

    public override string ToString()
        => $"{Name} {ToLabel()}";
}

public class Transition
{
    private readonly State _from;
    private readonly State _to;
    private string _token;

    public Transition(State from, State to, string token)
    {
        _from = from;
        _to = to;
        _token = token;
        Console.WriteLine($"NEW LINK {ToString()}");
    }

    public string ToDot()
        => $"{_from.Name} -> {_to.Name} [label=\"{_token}\"];";

    public override string ToString()
        => $"{_from} =={_token}==> {_to}";

    public override bool Equals(object obj)
    {
        var other = obj as Transition;
        return other != null
               && _from.Equals(other._from)
               && _to.Equals(other._to)
               && _token.Equals(other._token);
    }

    public override int GetHashCode()
        => _from.GetHashCode() + _to.GetHashCode() + _token.GetHashCode();
}

public class StateMachine
{
    private readonly State _start;
    private readonly List<State> _final = new();
    private readonly List<State> _states = new();
    private readonly List<Transition> _transitions = new();

    public StateMachine(FC.Specification spec)
    {
        _start = ForgeState(AllDownFlags(spec.AllFlags()), null);
        Console.WriteLine($"START STATE: {_start}");
        var _first = spec.FindNextSteps(this, _start);
        // take more steps
        ComputeOneStep(spec);
        ComputeOneStep(spec);
        ComputeOneStep(spec);
        ComputeOneStep(spec);
    }

    private void ComputeOneStep(Specification spec)
    {
        List<FA.State> _savedStates = new();
        _savedStates.AddRange(_states);
        foreach (var state in _savedStates)
            if (!_final.Contains(state))
                spec.FindNextSteps(this, state);
    }

    private HashSet<SignedFlag> AllDownFlags(IEnumerable<SignedFlag> allFlags)
    {
        HashSet<SignedFlag> result = new();
        foreach (var flag in allFlags)
        {
            if (flag is FlagDown down)
                result.Add(down);
            else
                result.Add(flag.Reversed() as FlagDown);
        }

        return result;
    }

    public FA.State ForgeState(HashSet<FC.SignedFlag> flags, Stack<FC.StackAction> stack)
    {
        stack ??= new Stack<StackAction>();
        foreach (var state in _states)
            if (state.FlagsEqual(flags) && state.StackExpectationsEqual(stack))
                return state;

        var newState = new State(flags, stack);
        _states.Add(newState);
        return newState;
    }

    public void CreateTransition(State source, State target, string token)
    {
        var transition = new FA.Transition(source, target, token);
        if (_transitions.Contains(transition))
            return;
        _transitions.Add(transition);
        if (token == "EOF")
            _final.Add(target);
    }

    public string ToDot()
    {
        List<string> result = new();
        result.Add("digraph sm {");
        result.Add(_start.Name + " [shape=invhouse];");
        result.AddRange(_final.Select(state => state.Name + " [shape=house];"));
        result.Add(String.Empty);
        result.AddRange(_states.Select(state => state.ToDot()));
        result.Add(String.Empty);
        result.AddRange(_transitions.Select(transition => transition.ToDot()));
        result.Add("}");
        return String.Join("\n\t", result);
    }
}