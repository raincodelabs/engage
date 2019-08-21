using System.Collections.Generic;

namespace Engage.A
{
    public partial class TokenDecl
    {
        public List<Lexeme> Names = new List<Lexeme>();
        public string Type;

        internal List<B.TokenPlan> MakePlans()
        {
            List<B.TokenPlan> ts = new List<B.TokenPlan>();
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