using System;
using System.Collections.Generic;
using System.Linq;

namespace Engage.A
{
    public partial class EngSpec
    {
        public string NS;
        public List<TypeDecl> Types = new List<TypeDecl>();
        public List<TokenDecl> Tokens = new List<TokenDecl>();
        public List<HandlerDecl> Handlers = new List<HandlerDecl>();

        public B.SystemPlan MakePlan()
        {
            B.SystemPlan output = new B.SystemPlan(NS);
            InferTokens(output);
            InferTypes(output);
            InferFlags(output);
            foreach (HandlerDecl hd in Handlers)
                output.AddHandler(hd.MakePlan());
            return output;
        }

        private void InferTokens(B.SystemPlan plan)
        {
            foreach (TokenDecl t in Tokens)
                plan.AddToken(t.Type, t.MakePlans());
        }

        private void InferTypes(B.SystemPlan plan)
        {
            foreach (var t in plan.Tokens.Keys)
            {
                if (t == "mark" || t == "skip" || t == "word")
                    continue;
                foreach (B.TokenPlan tok in plan.Tokens[t])
                    if (tok.Special)
                        B.SystemPlan.TypeAliases[t] = tok.Value;
            }
            foreach (var t in Types)
            {
                foreach (var n in t.Names)
                    plan.AddType(n, t.Super);
                plan.AddType(t.Super, null, silent: true);
            }
            foreach (var h in Handlers)
                if (h.RHS is A.PushReaction pr)
                {
                    if (plan.HasType(pr.Name))
                    {
                        var tp = plan.GetTypePlan(pr.Name);
                        var cp = new B.ConstPlan();
                        foreach (string a in pr.Args)
                            cp.AddConstructorArguments(h, a, plan.GetTypePlan);
                        tp.AddConstructor(cp);
                        Console.WriteLine($"[A2B] Inferred constructor {cp.ToString(pr.Name, tp.Super)}");
                    }
                }
                else if (h.RHS is A.WrapReaction wr)
                {
                    if (plan.HasType(wr.Name))
                    {
                        var tp = plan.GetTypePlan(wr.Name);
                        var cp = new B.ConstPlan();
                        foreach (var a in wr.Args)
                            cp.AddConstructorArguments(h, a, plan.GetTypePlan);
                        tp.AddConstructor(cp);
                        Console.WriteLine($"[A2B] Inferred constructor {cp.ToString(wr.Name, tp.Super)}");
                    }
                }
        }

        private void InferFlags(B.SystemPlan plan)
        {
            foreach (HandlerDecl h in Handlers)
            {
                if (h.RHS is A.LiftReaction lr)
                    plan.BoolFlags.Add(lr.Flag);
                else if (h.RHS is A.DropReaction dr)
                    plan.BoolFlags.Add(dr.Flag);

                foreach (var a in h.Context)
                    if (a.RHS is A.AwaitAction aa)
                    {
                        if (!String.IsNullOrEmpty(aa.ExtraContext))
                            plan.BoolFlags.Add(aa.ExtraContext);
                        if (!String.IsNullOrEmpty(aa.TmpContext))
                            plan.BoolFlags.Add(aa.TmpContext);
                    }
            }
            foreach (string f in plan.BoolFlags.Distinct().ToArray())
                if (f.EndsWith('#'))
                {
                    plan.IntFlags.Add(f.Substring(0, f.Length - 1));
                    plan.BoolFlags.Remove(f);
                }

            Console.WriteLine($"[A2B] Inferred flags: Boolean {String.Join(", ", plan.BoolFlags)}; counter {String.Join(", ", plan.IntFlags)}");
        }
    }
}