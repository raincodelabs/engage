using System;
using System.Collections.Generic;

namespace Engage.A
{
    public partial class HandlerDecl
    {
        public Trigger LHS;
        public Reaction RHS;
        public List<Assignment> Context = new List<Assignment>();

        internal Reaction GetContext(string name)
        {
            foreach (var a in Context)
                if (a.LHS == name)
                    return a.RHS;
            return null;
        }

        internal B.HandlerPlan MakePlan()
        {
            B.HandlerPlan hp = new B.HandlerPlan();
            if (LHS.EOF)
                hp.ReactOn = B.TokenPlan.EOF();
            else if (!(String.IsNullOrEmpty(LHS.NonTerminal)))
                hp.ReactOn = B.TokenPlan.FromNT(LHS.NonTerminal);
            else
                hp.ReactOn = B.TokenPlan.FromT(LHS.Terminal);
            if (!String.IsNullOrEmpty(LHS.Flag))
                hp.GuardFlag = LHS.Flag;
            ProduceActions(hp.AddAction);
            return hp;
        }

        internal void ProduceActions(Action<B.HandleAction> add)
        {
            if (Context.Count > 0 && (Context[0].RHS is A.AwaitAction || Context[0].RHS is A.AwaitStarAction || Context[0].RHS is A.PopHashAction))
            {
                int limit = Context.Count - 1;
                B.HandleAction tear = null;
                if (Context[limit].RHS is A.TearAction)
                {
                    limit--;
                    tear = Context[Context.Count - 1].RHS.ToHandleAction();
                }

                // Asynchronously: schedule parsing
                B.HandleAction act = RHS.ToHandleAction(prev: tear);
                for (int i = limit; i >= 0; i--)
                    act = Context[i].RHS.ToHandleAction(Context[i].LHS, act);
                // add *one* action!
                add(act);
            }
            else if (RHS is A.WrapReaction)
            {
                if (Context.Count > 1 || !(Context[0].RHS is A.PopAction))
                    Console.WriteLine($"[ERR] the WRAP reaction cannot handle multiple POPs at the moment. Future work!");
                // add one composite action
                add(RHS.ToHandleAction(B.SystemPlan.Dealias((Context[0].RHS as A.PopAction).Name)));
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