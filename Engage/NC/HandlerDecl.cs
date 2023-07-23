using System;
using System.Collections.Generic;
using System.Linq;

namespace Engage.NC
{
    public enum ComboEnum
    {
        None,
        Where,
        While
    }

    public class HandlerDecl
    {
        public Trigger LHS;
        public Reaction RHS;
        public ComboEnum ComboType = ComboEnum.None;
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

        internal NA.HandlerPlan MakePlan()
        {
            var hp = new NA.HandlerPlan();
            if (LHS.Special == SpecialTrigger.BOF)
                hp.ReactOn = NA.TokenPlan.BOF();
            else if (LHS.Special == SpecialTrigger.EOF)
                hp.ReactOn = NA.TokenPlan.EOF();
            else if (!String.IsNullOrEmpty(LHS.NonTerminal))
                hp.ReactOn = NA.TokenPlan.FromNT(LHS.NonTerminal);
            else
                hp.ReactOn = NA.TokenPlan.FromT(LHS.Terminal);
            if (!String.IsNullOrEmpty(LHS.Flag))
                hp.GuardFlag = LHS.Flag;
            ProduceActions(hp.AddAction);
            return hp;
        }

        private void ProduceActions(Action<NA.HandleAction> add)
        {
            if (ComboType == ComboEnum.While)
            {
                var loop = new NA.WhileStackNotEmpty();
                foreach (var assignment in Context)
                {
                    if (assignment.RHS is PopAction pop)
                    {
                        loop.Brancher.AddBranch($"Main.Peek() is {pop.Name}",
                            $"{assignment.LHS}.Add(Main.Pop() as {pop.Name})");
                        loop.AddVariable(assignment.LHS, pop.Name);
                    }
                    else if (assignment.RHS is DumpReaction dump)
                    {
                        if (dump.IsUniversal())
                            loop.Brancher.AddElse("Main.Pop()");
                        else
                            loop.Brancher.AddBranch($"Main.Peek() is {dump.Name}", "Main.Pop()");
                    }
                    else
                    {
                        Console.WriteLine($"[NC->NA] Cannot handle a While clause with {assignment.RHS.GetType().Name}");
                    }
                }

                add(loop);
                add(RHS.ToHandleAction());
                return;
            }

            if (Context.Count > 0 && (Context[0].RHS is NC.AwaitAction || Context[0].RHS is NC.AwaitStarAction))
            {
                int limit = Context.Count - 1;
                NA.HandleAction tear = null;
                if (Context[limit].RHS is NC.TearAction)
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
            else if (RHS is NC.WrapReaction)
            {
                if (Context.Count > 1 || Context[0].RHS is not NC.PopAction)
                    Console.WriteLine(
                        "[ERR] the WRAP reaction cannot handle multiple POPs at the moment. Future work!");
                // add one composite action
                add(RHS.ToHandleAction(NA.SystemPlan.Dealias((Context[0].RHS as NC.PopAction)?.Name)));
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