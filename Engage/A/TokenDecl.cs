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
                return false;
            return Type == other.Type
                   && Names.Count == other.Names.Count
                   && Names.SequenceEqual(other.Names)
                ;
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