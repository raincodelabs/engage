using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Engage.FC;
using Engage.NA;

[assembly: InternalsVisibleTo("EngageTests")]

namespace Engage.NC
{
    public class EngSpec
    {
        internal string NS;
        internal readonly List<TypeDecl> Types = new();
        internal readonly List<TokenDecl> Tokens = new();
        internal readonly List<HandlerDecl> Handlers = new();

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

        internal NA.SystemPlan MakePlan()
        {
            var output = new NA.SystemPlan(NS);
            InferTokens(output);
            InferTypes(output);
            InferFlags(output);
            foreach (HandlerDecl hd in Handlers)
                output.AddHandler(hd.MakePlan());
            return output;
        }

        private void InferTokens(NA.SystemPlan plan)
        {
            foreach (TokenDecl t in Tokens)
                plan.AddToken(t.Type, t.MakePlans());
        }

        private void InferTypes(NA.SystemPlan plan)
        {
            plan.InferTypeAliases();
            foreach (NC.TypeDecl t in Types)
            {
                foreach (var n in t.Names)
                    plan.AddType(n, t.Super);
                plan.AddType(t.Super, null, silent: true);
            }

            foreach (NC.HandlerDecl h in Handlers)
                if (h.RHS is NC.PushReaction pr)
                {
                    if (plan.HasType(pr.Name))
                        plan.GetTypePlan(pr.Name).InferConstructor(pr.Args, h, plan.GetTypePlan);
                    else
                        Console.WriteLine($"[NC->NA] Unknown pushed type '{pr.Name}'");
                }
                else if (h.RHS is NC.WrapReaction wr)
                {
                    if (plan.HasType(wr.Name))
                        plan.GetTypePlan(wr.Name).InferConstructor(wr.Args, h, plan.GetTypePlan);
                    else
                        Console.WriteLine($"[NC->NA] Unknown wrapped type '{wr.Name}'");
                }
        }

        private void InferFlags(NA.SystemPlan plan)
        {
            foreach (NC.HandlerDecl h in Handlers)
            {
                foreach (var flag in h.RHS.GetFlags())
                    plan.AddBoolFlag(flag);
                foreach (var a in h.Context)
                foreach (var flag in a.RHS.GetFlags())
                    plan.AddBoolFlag(flag);
            }

            plan.NormaliseFlags();

            Console.WriteLine($"[NC->NA] Inferred flags: Boolean {plan.AllBoolFlags()}; counter {plan.AllIntFlags()}");
        }

        internal FC.Specification Formalise()
        {
            var result = new FC.Specification();
            foreach (var handler in Handlers)
            {
                result.AddFormula(handler.MakeFormula());
            }

            return result;
        }
    }
}