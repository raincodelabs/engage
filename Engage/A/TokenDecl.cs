using System;
using System.Collections.Generic;
using System.Linq;

namespace Engage.A
{
    public class TokenDecl
    {
        public List<Lexeme> Names = new List<Lexeme>();
        public string Type;

        public override bool Equals(object obj)
        {
            var other = obj as TokenDecl;
            if (other == null)
            {
                Console.WriteLine("[x] TokenDecl compared to non-TokenDecl");
                return false;
            }

            if (Type != other.Type)
            {
                Console.WriteLine($"[x] TokenDecl: Type mismatch");
                return false;
            }

            if (Names.Count != other.Names.Count)
            {
                Console.WriteLine("[x] TokenDecl: Names count mismatch");
                return false;
            }

            if (!Names.SequenceEqual(other.Names))
            {
                Console.WriteLine("[x] TokenDecl: Names mismatch");
                return false;
            }

            return true;
        }

        internal List<B.TokenPlan> MakePlans()
        {
            var ts = new List<B.TokenPlan>();
            foreach (var a in Names)
                if (a is A.NumberLex)
                    ts.Add(new B.TokenPlan() { Special = true, Value = "number" });
                else if (a is A.StringLex)
                    ts.Add(new B.TokenPlan() { Special = true, Value = "string" });
                else if (a is A.LiteralLex al)
                    ts.Add(new B.TokenPlan() { Special = false, Value = al.Literal });
            return ts;
        }
    }
}