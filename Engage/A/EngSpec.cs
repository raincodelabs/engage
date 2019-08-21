using System;
using System.Collections.Generic;
using System.Linq;

namespace Engage.A
{
    public partial class EngSpec
    {
        internal string NS;
        internal List<TypeDecl> Types = new List<TypeDecl>();
        internal List<TokenDecl> Tokens = new List<TokenDecl>();
        internal List<HandlerDecl> Handlers = new List<HandlerDecl>();

        internal B.SystemPlan MakePlan()
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
            foreach (A.TypeDecl t in Types)
            {
                foreach (var n in t.Names)
                    plan.AddType(n, t.Super);
                plan.AddType(t.Super, null, silent: true);
            }
            foreach (A.HandlerDecl h in Handlers)
                if (h.RHS is A.PushReaction pr)
                {
                    if (plan.HasType(pr.Name))
                        plan.GetTypePlan(pr.Name).InferConstructor(pr.Args, h, plan.GetTypePlan);
                    else
                        Console.WriteLine($"[A2B] Unknown pushed type '{pr.Name}'");
                }
                else if (h.RHS is A.WrapReaction wr)
                {
                    if (plan.HasType(wr.Name))
                        plan.GetTypePlan(wr.Name).InferConstructor(wr.Args, h, plan.GetTypePlan);
                    else
                        Console.WriteLine($"[A2B] Unknown wrapped type '{wr.Name}'");
                }
        }

        private void InferFlags(B.SystemPlan plan)
        {
            foreach (A.HandlerDecl h in Handlers)
            {
                foreach (var flag in h.RHS.GetFlags())
                    plan.AddBoolFlag(flag);
                foreach (var a in h.Context)
                    foreach (var flag in a.RHS.GetFlags())
                        plan.AddBoolFlag(flag);
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