using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("EngageTests")]

namespace Engage.A
{
    public class EngSpec
    {
        internal string NS;
        internal List<TypeDecl> Types = new List<TypeDecl>();
        internal List<TokenDecl> Tokens = new List<TokenDecl>();
        internal List<HandlerDecl> Handlers = new List<HandlerDecl>();

        public override bool Equals(object obj)
        {
            var other = obj as EngSpec;
            if (other == null)
            {
                Console.WriteLine("[x] EngSpec compared to non-EngSpec");
                return false;
            }

            if (NS != other.NS)
            {
                Console.WriteLine("[x] EngSpec: NS mismatch");
                return false;
            }

            if (Types.Count != other.Types.Count)
            {
                Console.WriteLine("[x] EngSpec: Types length mismatch");
                return false;
            }

            if (Tokens.Count != other.Tokens.Count)
            {
                Console.WriteLine("[x] EngSpec: Tokens length mismatch");
                return false;
            }

            if (Handlers.Count != other.Handlers.Count)
            {
                Console.WriteLine("[x] EngSpec: Handlers length mismatch");
                return false;
            }

            if (!Types.SequenceEqual(other.Types))
            {
                Console.WriteLine("[x] EngSpec: Types mismatch");
                return false;
            }

            if (!Tokens.SequenceEqual(other.Tokens))
            {
                Console.WriteLine("[x] EngSpec: Tokens mismatch");
                return false;
            }

            if (!Handlers.SequenceEqual(other.Handlers))
            {
                Console.WriteLine("[x] EngSpec: Handlers mismatch");
                return false;
            }

            Console.WriteLine("[√] EngSpec == EngSpec");
            return true;
        }

        internal B.SystemPlan MakePlan()
        {
            var output = new B.SystemPlan(NS);
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
            plan.InferTypeAliases();
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

            plan.NormaliseFlags();

            Console.WriteLine($"[A2B] Inferred flags: Boolean {plan.AllBoolFlags()}; counter {plan.AllIntFlags()}");
        }
    }
}