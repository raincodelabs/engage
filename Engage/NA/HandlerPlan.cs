using System;
using System.Collections.Generic;

namespace Engage.NA
{
    public class HandlerPlan
    {
        internal NA.TokenPlan ReactOn;
        internal string GuardFlag;
        private readonly List<NA.HandleAction> _recipe = new();

        internal void AddAction(NA.HandleAction action)
            => _recipe.Add(action);

        internal void GenerateAbstractCode(List<GA.CsStmt> stmts)
        {
            foreach (var action in _recipe)
            {
                if (action != null)
                    action.GenerateAbstractCode(stmts);
                else
                    Console.WriteLine($"[NA->GA] Warning: no action to handle '{ReactOn.Value}'");
            }
        }

        internal void GenerateAbstractCode(GA.CsExeField field)
        {
            List<GA.CsStmt> stmts = new();
            GenerateAbstractCode(stmts);
            foreach (var stmt in stmts)
                field.AddCode(stmt);
        }

        internal string IsPushFirst()
        {
            if (_recipe.Count < 1)
                return null;
            if (_recipe[0] is NA.PushNew px)
                return px.Name;
            return null;
        }

        internal void AddRecipeTo(List<List<NA.HandleAction>> listOfLists)
            => listOfLists.Add(_recipe);

        internal void AddRecipeTo(List<NA.HandleAction> list)
            => list.AddRange(_recipe);
    }
}