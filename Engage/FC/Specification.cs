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
        DistributeUntaggedTriggers();
    }

    private void DistributeUntaggedTriggers()
    {
        bool fixPoint = false;
        while (!fixPoint)
        {
            PrintThis();
            List<Formula> untagged = new();
            List<Formula> tagged = new();
            List<Formula> adequate = new();

            foreach (var formula in _formulae)
            {
                if (formula.Tagged)
                    tagged.Add(formula);
                else
                    untagged.Add(formula);
            }

            // Console.WriteLine($"// Found {untagged.Count} untagged + {tagged.Count} tagged formulae out of {_formulae.Count}");

            foreach (var untaggedFormula in untagged)
            {
                List<Formula> toReverse = new();
                foreach (var taggedFormula in tagged)
                {
                    if (taggedFormula.InputEquals(untaggedFormula))
                    {
                        adequate.Add(new Formula(taggedFormula, untaggedFormula));
                        toReverse.Add(taggedFormula);
                    }
                }

                if (toReverse.Any())
                    adequate.Add(new Formula(untaggedFormula, toReverse));
                else
                    adequate.Add(untaggedFormula);
            }

            foreach (var taggedFormula in tagged)
            {
                bool witnessedInput = false;
                foreach (var untaggedFormula in untagged)
                    if (untaggedFormula.InputEquals(taggedFormula))
                    {
                        witnessedInput = true;
                        break;
                    }

                if (!witnessedInput)
                    adequate.Add(taggedFormula);
            }

            adequate.Sort((x, y) => x.ToString().CompareTo(y.ToString()));
            fixPoint = _formulae.SequenceEqual(adequate);
            _formulae = adequate;
        }
    }

    private void MergeLeftDuplicates()
    {
        bool fixPoint = false;
        while (!fixPoint)
        {
            PrintThis();
            List<Formula> adequate = new();
            List<Formula> discarded = new();
            bool breaking = false;
            foreach (var formula1 in _formulae)
            {
                foreach (var formula2 in _formulae)
                {
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

            adequate.Sort((x, y) => x.ToString().CompareTo(y.ToString()));
            fixPoint = _formulae.SequenceEqual(adequate);
            _formulae = adequate;
        }
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

public abstract class StackAction
{
    public string Type { get; init; }
}

public class StackPop : StackAction
{
    public override string ToString()
        => $"-{Type}";

    public StackPop(string type)
    {
        Type = type;
    }
}

public class StackPopS : StackAction
{
    public override string ToString()
        => $"-{Type}*";

    public StackPopS(string type)
    {
        Type = type;
    }
}

public class StackPush : StackAction
{
    public override string ToString()
        => $"+{Type}";

    public StackPush(string type)
    {
        Type = type;
    }
}