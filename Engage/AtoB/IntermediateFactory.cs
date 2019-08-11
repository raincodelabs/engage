using System;
using System.Collections.Generic;
using System.Linq;

namespace Engage
{
    public class IntermediateFactory
    {
        public static B.SystemPlan Ast2ir(A.EngSpec input)
        {
            B.SystemPlan output = new B.SystemPlan(input.NS);
            InferTokens(output, input);
            InferTypes(output, input);
            InferFlags(output, input);
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
                    output.Handlers[type] = new List<B.HandlerPlan>();
                output.Handlers[type].Add(hp);
            }
            return output;
        }

        private static B.HandlerPlan ConvertHandler(A.HandlerDecl hd)
        {
            B.HandlerPlan hp = new B.HandlerPlan();
            if (hd.LHS.EOF)
                hp.ReactOn = new B.TokenPlan() { Special = true, Value = "EOF" };
            else
                hp.ReactOn = new B.TokenPlan() { Special = false, Value = hd.LHS.Terminal };
            if (!String.IsNullOrEmpty(hd.LHS.Flag))
                hp.GuardFlag = hd.LHS.Flag;
            if (hd.Context.All(ass => ass.RHS is A.AwaitAction || ass.RHS is A.AwaitStarAction || ass.RHS is A.PopHashAction))
            {
                // Asynchronously: schedule parsing
                B.HandleAction act = hd.RHS.ToHandleAction();
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

        private static void InferTypes(B.SystemPlan plan, A.EngSpec spec)
        {
            foreach (var t in spec.Types)
            {
                foreach (var n in t.Names)
                    plan.AddType(n, t.Super);
                plan.AddType(t.Super, null, silent: true);
            }
            foreach (var h in spec.Handlers)
                if (h.RHS is A.PushReaction pr)
                {
                    if (plan.Types.ContainsKey(pr.Name))
                    {
                        var tp = plan.Types[pr.Name];
                        var cp = new B.ConstPlan();
                        foreach (var a in pr.Args)
                        {
                            A.Operation c = h.GetContext(a);
                            if (c is A.PopAction pa)
                                cp.Args.Add(new Tuple<string, B.TypePlan>(a, plan.Types[pa.Name]));
                            else if (c is A.PopStarAction psa)
                                cp.Args.Add(new Tuple<string, B.TypePlan>(a, plan.Types[psa.Name].Copy(true)));
                            else if (c is A.PopHashAction pha)
                                cp.Args.Add(new Tuple<string, B.TypePlan>(a, plan.Types[pha.Name].Copy(true)));
                            else if (c is A.AwaitAction aa)
                                cp.Args.Add(new Tuple<string, B.TypePlan>(a, plan.Types[aa.Name]));
                            else if (c is A.AwaitStarAction asa)
                                cp.Args.Add(new Tuple<string, B.TypePlan>(a, plan.Types[asa.Name].Copy(true)));
                        }
                        tp.Constructors.Add(cp);
                        Console.WriteLine($"[IR] Inferred constructor {cp.ToString(pr.Name, tp.Super)}");
                    }
                }
            foreach (var t in plan.Tokens.Keys)
            {
                if (t == "skip" || t == "reserved")
                    continue;
                if (!plan.Types.ContainsKey(t))
                    plan.AddType(t, null);
                B.TypePlan tp = plan.Types[t];
                foreach (B.TokenPlan tok in plan.Tokens[t])
                {
                    if (!tok.Special)
                        continue;
                    B.ConstPlan cp = new B.ConstPlan();
                    switch (tok.Value)
                    {
                        case "number":
                            cp.Args.Add(new Tuple<string, B.TypePlan>("n", new B.TypePlan() { Name = "System.Int32" }));
                            break;

                        case "string":
                            cp.Args.Add(new Tuple<string, B.TypePlan>("s", new B.TypePlan() { Name = "System.String" }));
                            break;

                        default:
                            cp = null;
                            break;
                    }
                    if (cp != null)
                    {
                        tp.Constructors.Add(cp);
                        Console.WriteLine($"[IR] Inferred trivial constructor {cp.ToString(tp.Name, null)}");
                    }
                }

                Console.WriteLine($"[DEBUG] token {String.Join("; ", plan.Tokens[t])} - {tp}");
            }
        }

        private static void InferFlags(B.SystemPlan plan, A.EngSpec spec)
        {
            foreach (var h in spec.Handlers)
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
            foreach (var f in plan.BoolFlags.Distinct().ToArray())
                if (f.EndsWith('#'))
                {
                    plan.IntFlags.Add(f.Substring(0, f.Length - 1));
                    plan.BoolFlags.Remove(f);
                }

            Console.WriteLine($"[IR] Inferred flags: Boolean {String.Join(", ", plan.BoolFlags)}; counter {String.Join(", ", plan.IntFlags)}");
        }

        private static void InferTokens(B.SystemPlan plan, A.EngSpec spec)
        {
            foreach (var t in spec.Tokens)
            {
                List<B.TokenPlan> ts = new List<B.TokenPlan>();
                foreach (var a in t.Names)
                    if (a is A.NumberLex)
                        ts.Add(new B.TokenPlan() { Special = true, Value = "number" });
                    else if (a is A.StringLex)
                        ts.Add(new B.TokenPlan() { Special = true, Value = "string" });
                    else if (a is A.LiteralLex al)
                        ts.Add(new B.TokenPlan() { Special = false, Value = al.Literal });
                plan.Tokens[t.Type] = ts;
            }
        }
    }
}