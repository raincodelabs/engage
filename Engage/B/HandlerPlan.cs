using System;
using System.Collections.Generic;

namespace Engage.B
{
    public class HandlerPlan
    {
        internal TokenPlan ReactOn;
        internal string GuardFlag;
        private List<HandleAction> Recipe = new List<HandleAction>();

        internal void AddAction(HandleAction action)
            => Recipe.Add(action);

        internal void GenerateAbstractCode(List<C.CsStmt> stmts)
        {
            foreach (var action in Recipe)
            {
                if (action != null)
                    action.GenerateAbstractCode(stmts);
                else
                    Console.WriteLine($"[B2C] Warning: no action to handle '{ReactOn.Value}'");
            }
        }

        internal string IsPushFirst()
        {
            if (Recipe.Count < 1)
                return null;
            if (Recipe[0] is B.PushNew px)
                return px.Name;
            return null;
        }

        internal void AddRecipeTo(List<List<B.HandleAction>> llist)
            => llist.Add(Recipe);
    }
}