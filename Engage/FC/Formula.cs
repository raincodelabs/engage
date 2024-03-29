using System;
using System.Collections.Generic;
using System.Linq;

namespace Engage.FC;

public class Formula
{
    private readonly string _origin;
    private readonly List<SignedFlag> _flags = new();
    private readonly string _input;
    private readonly List<SignedFlag> _flagActions = new();
    private readonly List<StackAction> _stackActions = new();

    public bool Flagged => _flags.Any();
    public string Input => _input;

    public Formula(
        int tracebackNumber,
        IEnumerable<string> flags,
        string input,
        IEnumerable<SignedFlag> tActions,
        IEnumerable<StackAction> sActions)
    {
        _origin = $"#{tracebackNumber}";
        if (flags != null)
            foreach (var flag in flags)
                if (!String.IsNullOrWhiteSpace(flag))
                    _flags.Add(new FlagUp(flag));
        _flags.Sort((x, y) => String.Compare(x.ToString(), y.ToString(), StringComparison.Ordinal));
        _input = input;
        if (tActions != null)
            _flagActions.AddRange(tActions);
        if (sActions != null)
            _stackActions.AddRange(sActions);
    }

    /// <summary>
    ///     Creates an new formula based on the left side of the first of the given two formulae
    ///     and both right sides.
    /// </summary>
    public Formula(Formula f1, Formula f2)
    {
        _origin = $"{f1._origin} & {f2._origin}";
        _flags.AddRange(f1._flags); // assume f2.Flags are the same
        _input = f1._input; // assume f2.Input is the same
        _flagActions.AddRange(f1._flagActions);
        _flagActions.AddRange(f2._flagActions);
        _stackActions.AddRange(f1._stackActions);
        _stackActions.AddRange(f2._stackActions);
    }

    /// <summary>
    ///     Creates an new formula based on the left side of the first of the given two formulae
    ///     and both right sides.
    /// </summary>
    public Formula(Formula f1, IEnumerable<Formula> revFs)
    {
        //// The following (commented) code would have given the full traceability.
        // var x = new List<string> { f1._origin };
        // x.AddRange(revFs.Select(f => f._origin));
        // x.Sort();
        // _origin = String.Join(" & ", x);
        //// However, we only need to remember one main origin, the rest just gives it context:
        _origin = f1._origin;

        _flags.AddRange(f1._flags);
        foreach (var formula in revFs)
        foreach (var flag in formula._flags)
            _flags.Add(flag.Reversed());

        _input = f1._input; // assume Input is the same for all revFs
        _flagActions.AddRange(f1._flagActions);
        _stackActions.AddRange(f1._stackActions);
    }

    internal bool LeftEquals(Formula other)
        => _flags.SequenceEqual(other._flags) && InputEquals(other);

    internal bool InputEquals(Formula other)
        => _input.Equals(other._input);

    internal void DepositFlags(HashSet<SignedFlag> flags)
    {
        flags.UnionWith(_flags);
        // for a stupid but valid situation when a flag only occurs on the right hand side of an equation
        flags.UnionWith(_flagActions);
    }

    internal bool IsEnabled(IEnumerable<SignedFlag> flags)
        => _flags.All(flag => !flags.Contains(flag.Reversed()));

    public override string ToString()
    {
        List<string> elements = new() { $"({_origin})" };
        if (_flags is { Count: > 0 })
            elements.Add("[" + String.Join(",", _flags) + "]");
        if (!String.IsNullOrEmpty(_input))
            elements.Add(_input);
        elements.Add("-->");
        if (_flagActions is { Count: > 0 })
            elements.Add("[" + String.Join(",", _flagActions) + "]");
        if (_stackActions is { Count: > 0 })
            elements.Add("{" + String.Join(",", _stackActions) + "}");
        return String.Join(" ", elements);
    }

    public override bool Equals(object obj)
    {
        var other = obj as FC.Formula;
        if (other == null)
            return false;
        return _flags.SequenceEqual(other._flags)
               && _input.Equals(other._input)
               && _flagActions.SequenceEqual(other._flagActions)
               && _stackActions.SequenceEqual(other._stackActions);
    }

    public override int GetHashCode()
        => _flags.GetHashCode() + _input.GetHashCode() + _flagActions.GetHashCode() + _stackActions.GetHashCode();

    public HashSet<SignedFlag> ChangeFlags(HashSet<SignedFlag> initial)
    {
        HashSet<SignedFlag> result = new();
        foreach (var flag in initial)
        {
            var up = new FC.FlagUp(flag);
            var dn = new FC.FlagDown(flag);
            if (_flagActions.Contains(up))
                result.Add(up);
            else if (_flagActions.Contains(dn))
                result.Add(dn);
            else
                result.Add(flag);
        }

        return result;
    }

    public Stack<StackAction> ChangeStack(Stack<StackAction> stackState)
    {
        // looks weird but this is the true way because IEnumerable iterates by popping
        Stack<StackAction> result = new Stack<StackAction>(new Stack<StackAction>(stackState));

        foreach (var stackAction in _stackActions)
        {
            stackAction.Apply(result);
        }

        return result;
    }

    public bool StackCompatible(Stack<StackAction> stackState)
    {
        // looks weird but this is the true way because IEnumerable iterates by popping
        Stack<StackAction> result = new Stack<StackAction>(new Stack<StackAction>(stackState));

        return _stackActions.All(stackAction => stackAction.Apply(result));
    }
}