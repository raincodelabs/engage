using System;
using System.Collections.Generic;
using System.Linq;

namespace Engage.NA
{
    public class ConstPlan
    {
        private readonly List<Tuple<string, NA.TypePlan>> _args = new();

        internal void AddConstructorArguments(NC.HandlerDecl h, string a, Func<string, NA.TypePlan> getTypePlan)
        {
            NC.Reaction c = h.GetContext(a);
            switch (c)
            {
                case NC.PopAction pa:
                    if (h.ComboType == NC.ComboEnum.While)
                        _args.Add(new Tuple<string, NA.TypePlan>(a, getTypePlan(pa.Name).Copy(true)));
                    else
                        _args.Add(new Tuple<string, NA.TypePlan>(a, getTypePlan(pa.Name)));
                    break;
                case NC.PopStarAction psa:
                    _args.Add(new Tuple<string, NA.TypePlan>(a, getTypePlan(psa.Name).Copy(true)));
                    break;
                case NC.AwaitAction aa:
                    _args.Add(new Tuple<string, NA.TypePlan>(a, getTypePlan(aa.Name)));
                    break;
                case NC.AwaitStarAction asa:
                    _args.Add(new Tuple<string, NA.TypePlan>(a, getTypePlan(asa.Name).Copy(true)));
                    break;
                case NC.TearAction _:
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
                    _args.Add(new Tuple<string, NA.TypePlan>(a,
                        getTypePlan(h.Context[idx].RHS.Name).FirstConstructor._args[0].Item2));
                    break;
                }
                case null when a == "this":
                    _args.Add(new Tuple<string, NA.TypePlan>(a,
                        new NA.TypePlan(NA.SystemPlan.Unalias(h.LHS.NonTerminal))));
                    break;
            }
        }

        internal void AddAbstractCodeConstructor(GA.CsClass c1ass)
        {
            var cc = new GA.CsConstructor();
            foreach (var a in _args)
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
            if (_args.Count == 0) return result;
            result += "(";
            result += String.Join(",",
                _args.Select(a => $"{a.Item1}:{(a.Item2 == null ? "object" : a.Item2.ToString())}"));
            result += ")";
            return result;
        }

        public override bool Equals(object obj)
        {
            var other = obj as ConstPlan;
            if (other == null)
                return false;
            if (this._args.Count != other._args.Count)
                return false;
            return !_args.Where((t, i) =>
                this._args[i].Item1 != other._args[i].Item1 || this._args[i].Item2 != other._args[i].Item2).Any();
        }
    }
}