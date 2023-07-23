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

    public override bool Equals(object obj)
        => obj is State other && Flags.SetEquals(other.Flags);

    public override int GetHashCode()
        => Flags.GetHashCode();

    public override string ToString() =>
        $"State{_id} [" + String.Join(";", Flags) + "] {" + String.Join(";", StackState) + "}";

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

    public override string ToString()
        => $"{_from} =={_token}==> {_to}";
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
        if (token == "EOF")
            _final.Add(target);
        _transitions.Add(transition);
    }
}