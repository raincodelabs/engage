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
    // TODO: stack state

    public State(IEnumerable<FC.SignedFlag> tags)
    {
        if (tags != null)
            Flags.UnionWith(tags);
        _id = Enumerator.GetId();
        Console.WriteLine($"CREATE {ToString()}");
    }

    public override bool Equals(object obj)
        => obj is State other && Flags.SetEquals(other.Flags);

    public override int GetHashCode()
        => Flags.GetHashCode();

    public override string ToString() =>
        $"State{_id} [" + String.Join(";", Flags) + "]";

    public FA.State Apply(Formula formula, FA.StateMachine machine)
        => machine.ForgeState(formula.ChangeFlags(Flags));

    public bool TagsEqual(HashSet<SignedFlag> flags)
    {
        var t1 = String.Join(";", Flags);
        var t2 = String.Join(";", flags);
        Console.WriteLine($"IN {_id}, CMP [{t1}] to [{t2}]: {Flags.SetEquals(flags)}");
        return Flags.SetEquals(flags);
    }
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
    private readonly List<State> _states = new();
    private readonly List<Transition> _transitions = new();

    public StateMachine(FC.Specification spec)
    {
        var _start = ForgeState(AllDownFlags(spec.AllFlags()));
        Console.WriteLine($"START STATE: {_start}");
        var _first = spec.FindNextSteps(this, _start);
    }

    private HashSet<SignedFlag> AllDownFlags(IEnumerable<SignedFlag> allFlags)
    {
        HashSet<SignedFlag> result = new();
        foreach (var flag in allFlags)
        {
            if (flag is FlagDown dTag)
                result.Add(dTag);
            else
                result.Add(flag.Reversed() as FlagDown);
        }

        return result;
    }

    public FA.State ForgeState(HashSet<FC.SignedFlag> flags)
    {
        foreach (var state in _states.Where(state => state.TagsEqual(flags)))
            return state;

        var newState = new State(flags);
        _states.Add(newState);
        return newState;
    }

    public void CreateTransition(State source, State target, string token)
    {
        _transitions.Add(new FA.Transition(source, target, token));
    }
}