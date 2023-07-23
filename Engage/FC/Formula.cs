using System;
using System.Collections.Generic;
using System.Linq;

namespace Engage.FC;

public class Formula
{
    private readonly List<SignedFlag> _flags = new();
    private readonly string _input;
    private readonly List<SignedFlag> _flagActions = new();
    private readonly List<StackAction> _stackActions = new();

    public bool Tagged => _flags.Any();
    public string Input => _input;

    public Formula(
        IEnumerable<string> tags,
        string input,
        IEnumerable<SignedFlag> tActions,
        IEnumerable<StackAction> sActions)
    {
        if (tags != null)
            foreach (var tag in tags)
                if (!String.IsNullOrWhiteSpace(tag))
                    _flags.Add(new FlagUp(tag));
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
        _flags.AddRange(f1._flags); // assume f2.Tags are the same
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
        // for a stupid but valid situation when a tag only occurs on the right hand side of an equation
        flags.UnionWith(_flagActions);
    }

    internal bool IsEnabled(IEnumerable<SignedFlag> flags)
        => _flags.All(tag => !flags.Contains(tag.Reversed()));

    public override string ToString()
    {
        List<string> elements = new();
        if (_flags != null && _flags.Count > 0)
            elements.Add("[" + String.Join(",", _flags) + "]");
        if (!String.IsNullOrEmpty(_input))
            elements.Add(_input);
        elements.Add("-->");
        if (_flagActions != null && _flagActions.Count > 0)
            elements.Add("[" + String.Join(",", _flagActions) + "]");
        if (_stackActions != null && _stackActions.Count > 0)
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
}