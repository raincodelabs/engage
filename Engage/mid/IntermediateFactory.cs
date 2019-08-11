using System;
using System.Collections.Generic;
using System.Linq;

namespace Engage.mid
{
    public class IntermediateFactory
    {
        public static SystemPlan Ast2ir(EngSpec input)
        {
            SystemPlan output = new SystemPlan(input.NS);
            InferTypes(output, input);
            InferFlags(output, input);
            InferTokens(output, input);
            foreach (var hd in input.Handlers)
            {
                var hp = ConvertHandler(hd);
                string type = hp.ReactOn.Value;
                foreach (var k in output.Tokens.Keys)
                    if (output.Tokens[k].Contains(hp.ReactOn))
                        type = k;
                if (String.IsNullOrEmpty(type))
                    Console.WriteLine($"[IF] Cannot determine type of token '{hp.ReactOn.Value}'");
                if (!output.Handlers.ContainsKey(type))
                    output.Handlers[type] = new List<HandlerPlan>();
                output.Handlers[type].Add(hp);
            }
            return output;
        }

        private static HandlerPlan ConvertHandler(HandlerDecl hd)
        {
            HandlerPlan hp = new HandlerPlan();
            if (hd.LHS.EOF)
                hp.ReactOn = new TokenPlan() { Special = true, Value = "EOF" };
            else
                hp.ReactOn = new TokenPlan() { Special = false, Value = hd.LHS.Literal };
            if (!String.IsNullOrEmpty(hd.LHS.Flag))
                hp.GuardFlag = hd.LHS.Flag;
            if (hd.Context.All(ass => ass.RHS is AwaitAction || ass.RHS is AwaitStarAction))
            {
                // Asynchronously: schedule parsing
                // in reverse order, provide last result to the new one
                HandleAction act = hd.RHS.ToHandleAction();
                for (int i = hd.Context.Count - 1; i >= 0; i--)
                    act = hd.Context[i].RHS.ToHandleAction(hd.Context[i].LHS, act);
                // add *one* action!
                hp.Recipe.Add(act);
            }
            else
            {
                // Synchronously: just get it from the stack one by one
                foreach (var ass in hd.Context)
                    hp.Recipe.Add(ass.RHS.ToHandleAction(ass.LHS));
                hp.Recipe.Add(hd.RHS.ToHandleAction());
            }
            return hp;
        }

        private static void InferTypes(SystemPlan plan, EngSpec spec)
        {
            foreach (var t in spec.Types)
            {
                foreach (var n in t.Names)
                    plan.AddType(n, t.Super);
                plan.AddType(t.Super, null, silent: true);
            }
            foreach (var h in spec.Handlers)
                if (h.RHS is PushReaction pr)
                {
                    if (plan.Types.ContainsKey(pr.Name))
                    {
                        var tp = plan.Types[pr.Name];
                        var cp = new ConstPlan();
                        foreach (var a in pr.Args)
                        {
                            Operation c = h.GetContext(a);
                            if (c is PopAction pa)
                                cp.Args.Add(new Tuple<string, TypePlan>(a, plan.Types[pa.Name]));
                            else if (c is PopStarAction psa)
                                cp.Args.Add(new Tuple<string, TypePlan>(a, plan.Types[psa.Name].Copy(true)));
                            else if (c is AwaitAction aa)
                                cp.Args.Add(new Tuple<string, TypePlan>(a, plan.Types[aa.Name]));
                            else if (c is AwaitStarAction asa)
                                cp.Args.Add(new Tuple<string, TypePlan>(a, plan.Types[asa.Name].Copy(true)));
                        }
                        tp.Constructors.Add(cp);
                        Console.WriteLine($"[IR] Inferred constructor {cp.ToString(pr.Name, tp.Super)}");
                    }
                }
        }

        private static void InferFlags(SystemPlan plan, EngSpec spec)
        {
            foreach (var h in spec.Handlers)
            {
                if (h.RHS is LiftReaction lr)
                    plan.BoolFlags.Add(lr.Flag);
                else if (h.RHS is DropReaction dr)
                    plan.BoolFlags.Add(dr.Flag);

                foreach (var a in h.Context)
                    if (a.RHS is AwaitAction aa)
                    {
                        if (!String.IsNullOrEmpty(aa.ExtraContext))
                            plan.BoolFlags.Add(aa.ExtraContext);
                        if (!String.IsNullOrEmpty(aa.TmpContext))
                            plan.BoolFlags.Add(aa.TmpContext);
                    }
            }
            foreach (var f in plan.BoolFlags.Distinct().ToArray())
                if (f.EndsWith('#'))
                {
                    plan.IntFlags.Add(f.Substring(0, f.Length - 1));
                    plan.BoolFlags.Remove(f);
                }

            Console.WriteLine($"[IR] Inferred flags: Boolean {String.Join(", ", plan.BoolFlags)}; counter {String.Join(", ", plan.IntFlags)}");
        }

        private static void InferTokens(SystemPlan plan, EngSpec spec)
        {
            foreach (var t in spec.Tokens)
            {
                List<TokenPlan> ts = new List<TokenPlan>();
                foreach (var a in t.Names)
                    if (a is NumberLex)
                        ts.Add(new TokenPlan() { Special = true, Value = "number" });
                    else if (a is StringLex)
                        ts.Add(new TokenPlan() { Special = true, Value = "string" });
                    else if (a is LiteralLex al)
                        ts.Add(new TokenPlan() { Special = false, Value = al.Literal });
                plan.Tokens[t.Type] = ts;
            }
        }
    }
}