using System;
using System.Collections.Generic;
using System.Linq;

namespace Engage.FC;

public class Specification
{
    private List<Formula> _formulae = new();

    public void AddFormula(Formula f)
        => _formulae.Add(f);

    public void Normalise()
    {
        Console.WriteLine("Merging duplicates...");
        MergeLeftDuplicates();
        Console.WriteLine("Distributing triggers...");
        DistributeUnflaggedTriggers();
        Console.WriteLine("Normalised specification:");
        PrintThis();
    }

    private void DistributeUnflaggedTriggers()
    {
        bool fixPoint = false;
        while (!fixPoint)
        {
            // PrintThis();
            List<Formula> unflagged = new();
            List<Formula> flagged = new();
            List<Formula> adequate = new();

            foreach (var formula in _formulae)
            {
                if (formula.Flagged)
                    flagged.Add(formula);
                else
                    unflagged.Add(formula);
            }

            foreach (var unflaggedFormula in unflagged)
            {
                List<Formula> toReverse = new();
                foreach (var flaggedFormula in flagged)
                {
                    if (flaggedFormula.InputEquals(unflaggedFormula))
                    {
                        adequate.Add(new Formula(flaggedFormula, unflaggedFormula));
                        toReverse.Add(flaggedFormula);
                    }
                }

                if (toReverse.Any())
                    adequate.Add(new Formula(unflaggedFormula, toReverse));
                else
                    adequate.Add(unflaggedFormula);
            }

            adequate.AddRange(
                flagged.Where(flaggedFormula =>
                    !unflagged.Any(unflaggedFormula => unflaggedFormula.InputEquals(flaggedFormula))));

            adequate.Sort((x, y) => String.Compare(x.ToString(), y.ToString(), StringComparison.Ordinal));
            fixPoint = _formulae.SequenceEqual(adequate);
            _formulae = adequate;
        }
    }

    private void MergeLeftDuplicates()
    {
        bool fixPoint = false;
        while (!fixPoint)
        {
            // PrintThis();
            List<Formula> adequate = new();
            List<Formula> discarded = new();
            bool breaking = false;
            foreach (var formula1 in _formulae)
            {
                foreach (var formula2 in _formulae)
                {
                    // Yes, we want referential equality here
                    if (formula1 == formula2)
                        continue;
                    if (formula1.LeftEquals(formula2))
                    {
                        adequate.Add(new Formula(formula1, formula2));
                        discarded.Add(formula1);
                        discarded.Add(formula2);
                        breaking = true;
                        break;
                    }
                }

                if (breaking)
                    break;
            }

            foreach (var formula in _formulae)
            {
                if (!discarded.Contains(formula))
                    adequate.Add(formula);
            }

            adequate.Sort((x, y) => String.Compare(x.ToString(), y.ToString(), StringComparison.Ordinal));
            fixPoint = _formulae.SequenceEqual(adequate);
            _formulae = adequate;
        }
    }

    public IEnumerable<SignedFlag> AllFlags()
    {
        HashSet<SignedFlag> flags = new();
        foreach (var formula in _formulae)
            formula.DepositFlags(flags);

        return flags;
    }

    public List<FA.Transition> FindNextSteps(FA.StateMachine machine, FA.State state)
    {
        HashSet<FC.Formula> possible = new();
        possible.UnionWith(_formulae.Where(formula => formula.IsEnabled(state.Flags)));

        foreach (var formula in possible)
        {
            var target = state.Apply(formula, machine);
            machine.CreateTransition(state, target, formula.Input);
        }

        return null;
    }

    public void PrintThis()
    {
        Console.WriteLine("---------- FORMAL ----------");
        Console.WriteLine(ToString());
        Console.WriteLine("----------INFORMAL----------");
    }

    public override string ToString()
        => String.Join(Environment.NewLine, _formulae.Select(f => f.ToString()));
}