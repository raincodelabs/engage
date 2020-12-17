using System;
using System.Collections.Generic;

namespace Engage.B
{
    public class HandlerPlan
    {
        internal TokenPlan ReactOn;
        internal string GuardFlag;
        private readonly List<HandleAction> _recipe = new List<HandleAction>();

        internal void AddAction(HandleAction action)
            => _recipe.Add(action);

        internal void GenerateAbstractCode(List<C.CsStmt> stmts)
        {
            foreach (var action in _recipe)
            {
                if (action != null)
                    action.GenerateAbstractCode(stmts);
                else
                    Console.WriteLine($"[B2C] Warning: no action to handle '{ReactOn.Value}'");
            }
        }

        internal string IsPushFirst()
        {
            if (_recipe.Count < 1)
                return null;
            if (_recipe[0] is B.PushNew px)
                return px.Name;
            return null;
        }

        internal void AddRecipeTo(List<List<B.HandleAction>> listOfLists)
            => listOfLists.Add(_recipe);
        
        internal void AddRecipeTo(List<B.HandleAction> list)
            => list.AddRange(_recipe);
    }
}