using System;

namespace Engage.mid
{
    public class IntermediateFactory
    {
        public static SystemPlan Ast2ir(EngSpec input)
        {
            SystemPlan output = new SystemPlan();
            InferTypes(output, input);
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
    }
}