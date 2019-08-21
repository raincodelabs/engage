using System;
using System.Collections.Generic;
using System.Linq;

namespace Engage.B
{
    public class ConstPlan
    {
        private List<Tuple<string, TypePlan>> Args = new List<Tuple<string, TypePlan>>();

        internal void AddConstructorArguments(A.HandlerDecl h, string a, Func<string, B.TypePlan> getTypePlan)
        {
            A.Reaction c = h.GetContext(a);
            if (c is A.PopAction pa)
                Args.Add(new Tuple<string, B.TypePlan>(a, getTypePlan(pa.Name)));
            else if (c is A.PopStarAction psa)
                Args.Add(new Tuple<string, B.TypePlan>(a, getTypePlan(psa.Name).Copy(true)));
            else if (c is A.PopHashAction pha)
                Args.Add(new Tuple<string, B.TypePlan>(a, getTypePlan(pha.Name).Copy(true)));
            else if (c is A.AwaitAction aa)
                Args.Add(new Tuple<string, B.TypePlan>(a, getTypePlan(aa.Name)));
            else if (c is A.AwaitStarAction asa)
                Args.Add(new Tuple<string, B.TypePlan>(a, getTypePlan(asa.Name).Copy(true)));
            else if (c is A.TearAction ta)
            {
                int idx = -1;
                for (int i = 0; i < h.Context.Count; i++)
                    if (h.Context[i].LHS == a)
                    {
                        idx = i;
                        break;
                    }
                idx--; // previous
                if (idx < 0)
                    Console.WriteLine($"the TEAR action must not be the first one");
                Args.Add(new Tuple<string, B.TypePlan>(a, getTypePlan(h.Context[idx].RHS.Name).FirstConstructor.Args[0].Item2));
            }
            else if (c == null && a == "this")
                Args.Add(new Tuple<string, B.TypePlan>(a, new B.TypePlan(B.SystemPlan.Unalias(h.LHS.NonTerminal))));
        }

        internal void AddAbstractCodeConstructor(C.CsClass c1ass)
        {
            var cc = new C.CsConstructor();
            foreach (var a in Args)
            {
                var name = a.Item1;
                if (name == "this")
                    name = "value";
                var type = a.Item2.ToString();
                if (SystemPlan.RealNames.ContainsKey(type))
                    type = SystemPlan.RealNames[type];
                if (type == "number")
                    ;
                c1ass.AddField(name, type);
                cc.AddArgument(name, type);
            }
            c1ass.AddConstructor(cc);
        }

        public string ToString(string name, string super)
        {
            string result = name;
            if (!String.IsNullOrEmpty(super))
                result += ":" + super;
            if (Args.Count > 0)
            {
                result += "(";
                result += String.Join(",", Args.Select(a => $"{a.Item1}:{(a.Item2 == null ? "object" : a.Item2.ToString())}"));
                result += ")";
            }
            return result;
        }

        public override bool Equals(object obj)
        {
            var other = obj as ConstPlan;
            if (other == null)
                return false;
            if (this.Args.Count != other.Args.Count)
                return false;
            for (int i = 0; i < Args.Count; i++)
                if (this.Args[i].Item1 != other.Args[i].Item1 || this.Args[i].Item2 != this.Args[i].Item2)
                    return false;
            return true;
        }
    }
}