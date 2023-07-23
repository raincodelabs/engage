using System;
using System.Collections.Generic;
using Engage.NA;

namespace Engage.NC
{
    public abstract class Trigger
    {
        protected string Value;
        public string Flag { get; protected init; } = String.Empty;

        public IEnumerable<string> Flags => Flag.Split("_");

        public abstract NA.TokenPlan MakeTokenPlan();
    }

    public class TerminalTrigger : Trigger
    {
        public TerminalTrigger(string possiblyQuoted, string flag = null)
        {
            Value = possiblyQuoted is { Length: > 1 } && possiblyQuoted[0] == '\'' && possiblyQuoted[^1] == '\''
                ? possiblyQuoted[1..^1]
                : possiblyQuoted;
            if (!String.IsNullOrEmpty(flag))
                Flag = flag;
        }

        public override bool Equals(object obj)
        {
            var other = obj as TerminalTrigger;
            if (other == null)
            {
                Console.WriteLine("[x] Terminal trigger compared to something else");
                return false;
            }

            if (Value != other.Value)
            {
                Console.WriteLine($"[x] Trigger: terminal mismatch ({Value} vs {other.Value})");
                return false;
            }

            if (Flag != other.Flag)
            {
                Console.WriteLine($"[x] Trigger: flag mismatch ({Flag} vs {other.Flag})");
                return false;
            }

            Console.WriteLine("[√] Trigger == Trigger");
            return true;
        }

        public override string ToString()
            => $"'{Value}'";

        public override TokenPlan MakeTokenPlan()
            => NA.TokenPlan.FromT(Value);
    }

    public class NonterminalTrigger : Trigger
    {
        public NonterminalTrigger(string name, string flag = null)
        {
            Value = name;
            if (!String.IsNullOrEmpty(flag))
                Flag = flag;
        }

        public override bool Equals(object obj)
        {
            var other = obj as NonterminalTrigger;
            if (other == null)
            {
                Console.WriteLine("[x] Nonterminal trigger compared to something else");
                return false;
            }

            if (Value != other.Value)
            {
                Console.WriteLine($"[x] Trigger: nonterminal mismatch ({Value} vs {other.Value})");
                return false;
            }

            if (Flag != other.Flag)
            {
                Console.WriteLine($"[x] Trigger: flag mismatch ({Flag} vs {other.Flag})");
                return false;
            }

            Console.WriteLine("[√] Trigger == Trigger");
            return true;
        }

        public override string ToString()
            => Value;

        public override TokenPlan MakeTokenPlan()
            => NA.TokenPlan.FromNT(Value);
    }

    public class SpecialTrigger : Trigger
    {
        public bool IsBOF => Value == "BOF";
        public bool IsEOF => Value == "EOF";

        public SpecialTrigger(string name, string flag = null)
        {
            switch (name)
            {
                case "BOF":
                case "EOF":
                    Value = name;
                    break;
                default:
                    Console.WriteLine($"[ERR] Cannot create a special trigger {name}");
                    Value = "ERROR";
                    return;
            }

            if (!String.IsNullOrEmpty(flag))
                Flag = flag;
        }

        public override bool Equals(object obj)
        {
            var other = obj as SpecialTrigger;
            if (other == null)
            {
                Console.WriteLine("[x] Special trigger compared to something else");
                return false;
            }

            if (Value != other.Value)
            {
                Console.WriteLine($"[x] Trigger: special trigger mismatch ({Value} vs {other.Value})");
                return false;
            }

            if (Flag != other.Flag)
            {
                Console.WriteLine($"[x] Trigger: flag mismatch ({Flag} vs {other.Flag})");
                return false;
            }

            Console.WriteLine("[√] Trigger == Trigger");
            return true;
        }

        public override string ToString()
            => Value;

        public override TokenPlan MakeTokenPlan()
        {
            switch (Value)
            {
                case "BOF":
                    return NA.TokenPlan.BOF();
                case "EOF":
                    return NA.TokenPlan.EOF();
                default:
                    Console.WriteLine($"[NC->NA] Cannot make a token plan for a special trigger {Value}");
                    return NA.TokenPlan.FromT(Value);
            }
        }
    }
}