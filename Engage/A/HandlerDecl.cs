using System;
using System.Collections.Generic;
using System.Linq;

namespace Engage.A
{
    public class HandlerDecl
    {
        public Trigger LHS;
        public Reaction RHS;
        public List<Assignment> Context = new List<Assignment>();

        public override bool Equals(object obj)
        {
            var other = obj as HandlerDecl;
            if (other == null)
            {
                Console.WriteLine("[x] HandlerDecl compared to non-HandlerDecl");
                return false;
            }

            if (!LHS.Equals(other.LHS))
            {
                Console.WriteLine("[x] HandlerDecl: LHS mismatch");
                return false;
            }

            if (!RHS.Equals(other.RHS))
            {
                Console.WriteLine("[x] HandlerDecl: RHS mismatch");
                return false;
            }

            if (Context.Count != other.Context.Count)
            {
                Console.WriteLine("[x] HandlerDecl: Context count mismatch");
                return false;
            }

            if (!Context.SequenceEqual(other.Context))
            {
                Console.WriteLine("[x] HandlerDecl: Context mismatch");
                return false;
            }

            Console.WriteLine("[√] HandlerDecl == HandlerDecl");
            return true;
        }

        internal Reaction GetContext(string name)
            => (from a in Context where a.LHS == name select a.RHS)
                .FirstOrDefault();

        internal B.HandlerPlan MakePlan()
        {
            var hp = new B.HandlerPlan();
            if (LHS.Special == SpecialTrigger.BOF)
                hp.ReactOn = B.TokenPlan.BOF();
            else if (LHS.Special == SpecialTrigger.EOF)
                hp.ReactOn = B.TokenPlan.EOF();
            else if (!String.IsNullOrEmpty(LHS.NonTerminal))
                hp.ReactOn = B.TokenPlan.FromNT(LHS.NonTerminal);
            else
                hp.ReactOn = B.TokenPlan.FromT(LHS.Terminal);
            if (!String.IsNullOrEmpty(LHS.Flag))
                hp.GuardFlag = LHS.Flag;
            ProduceActions(hp.AddAction);
            return hp;
        }

        private void ProduceActions(Action<B.HandleAction> add)
        {
            if (Context.Count > 0 && (Context[0].RHS is A.AwaitAction || Context[0].RHS is A.AwaitStarAction || Context[0].RHS is A.PopHashAction))
            {
                int limit = Context.Count - 1;
                B.HandleAction tear = null;
                if (Context[limit].RHS is A.TearAction)
                {
                    limit--;
                    tear = Context[^1].RHS.ToHandleAction();
                }

                // Asynchronously: schedule parsing
                var act = RHS.ToHandleAction(prev: tear);
                for (int i = limit; i >= 0; i--)
                    act = Context[i].RHS.ToHandleAction(Context[i].LHS, act);
                // add *one* action!
                add(act);
            }
            else if (RHS is A.WrapReaction)
            {
                if (Context.Count > 1 || !(Context[0].RHS is A.PopAction))
                    Console.WriteLine(
                        "[ERR] the WRAP reaction cannot handle multiple POPs at the moment. Future work!");
                // add one composite action
                add(RHS.ToHandleAction(B.SystemPlan.Dealias((Context[0].RHS as A.PopAction)?.Name)));
            }
            else
            {
                // Synchronously: just get it from the stack one by one
                foreach (var ass in Context)
                    add(ass.RHS.ToHandleAction(ass.LHS));
                add(RHS.ToHandleAction());
            }
        }
    }
}