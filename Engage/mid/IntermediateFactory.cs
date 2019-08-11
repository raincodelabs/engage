using System;
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
            return output;
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
    }
}