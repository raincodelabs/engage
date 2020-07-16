using System;
using System.Collections.Generic;

namespace Engage.B
{
    public abstract class HandleAction
    {
        public abstract void GenerateAbstractCode(List<C.CsStmt> code);
    }

    public class LiftFlag : HandleAction
    {
        public string Flag;

        public override void GenerateAbstractCode(List<C.CsStmt> code)
        {
            if (Flag.EndsWith("#"))
                code.Add(new C.SimpleStmt($"{Flag.Substring(0, Flag.Length - 1)}++"));
            else
                code.Add(new C.SimpleStmt($"{Flag} = true"));
        }
    }

    public class DropFlag : HandleAction
    {
        public string Flag;

        public override void GenerateAbstractCode(List<C.CsStmt> code)
        {
            if (Flag.EndsWith("#"))
                code.Add(new C.SimpleStmt($"{Flag.Substring(0, Flag.Length - 1)}--"));
            else
                code.Add(new C.SimpleStmt($"{Flag} = false"));
        }
    }

    public class TrimStream : HandleAction
    {
        public string Type;
        public bool Starred;

        public override void GenerateAbstractCode(List<C.CsStmt> code)
        {
            code.Add(new C.SimpleStmt($"Trim(typeof({Type}))"));
        }
    }

    public class PushNew : HandleAction
    {
        public string Name;
        public List<string> Args = new List<string>();
        public string Tearing;

        public PushNew()
        {
        }

        public PushNew(string name, IEnumerable<string> args, string tearing)
        {
            Name = name;
            Args.AddRange(args);
            Tearing = tearing;
        }

        public override void GenerateAbstractCode(List<C.CsStmt> code)
        {
            if (String.IsNullOrEmpty(Tearing))
                code.Add(new C.SimpleStmt($"Push(new {Name}({String.Join(", ", Args)}))"));
            else
                code.Add(new C.SimpleStmt($"Push(new {Name}({Tearing}))"));
        }
    }

    public class PopOne : HandleAction
    {
        public string Name;
        public string Target;

        public override void GenerateAbstractCode(List<C.CsStmt> code)
        {
            code.Add(new C.SimpleStmt($"{Name} {Target}"));
            var ite = new C.IfThenElse($"Main.Peek() is {Name}", $"{Target} = {"Main.Pop()".CastAs(Name)}");
            ite.AddElse($"ERROR = \"the top of the stack is not of type {Name}\"");
            ite.AddElse($"{Target} = {Name.DefaultValue()}");
            code.Add(ite);
        }
    }

    public class WrapOne : HandleAction
    {
        public string Name;
        public string Type;
        public string Target;

        public WrapOne(string name, string type, string target)
        {
            Name = name;
            Type = type;
            Target = target;
        }

        public override void GenerateAbstractCode(List<C.CsStmt> code)
        {
            var cond = $"Main.Peek() is {Type}";
            var guard = new C.IfThenElse();
            guard.AddToBranch(cond, $"{Type} {Target} = {"Main.Pop()".CastAs(Type)}");
            guard.AddToBranch(cond, $"Push(new {Name}({Target}))");
            code.Add(guard);
        }
    }

    public class PopAll : HandleAction
    {
        public string Name;
        public string Target;

        public override void GenerateAbstractCode(List<C.CsStmt> code)
        {
            code.Add(new C.SimpleStmt($"var {Target} = new List<{Name}>()"));
            code.Add(new C.WhileStmt($"Main.Count > 0 && Main.Peek() is {Name}", $"{Target}.Add(Main.Pop() as {Name})"));
            code.Add(new C.SimpleStmt($"{Target}.Reverse()"));
        }
    }

    public class PopSeveral : HandleAction
    {
        public string Name;
        public string Target;

        public List<HandleAction> SiblingActions = new List<HandleAction>();

        public override void GenerateAbstractCode(List<C.CsStmt> code)
        {
            // initialise lists
            GenerateInitialisationCode(code);
            foreach (var sa in SiblingActions)
                if (sa is PopSeveral ps)
                    ps.GenerateInitialisationCode(code);

            var loop = new C.WhileStmt();
            loop.Condition = "Main.Count > 0";
            var ite = new C.IfThenElse($"Main.Peek() is {Name}", $"{Target}.Add(Main.Pop() as {Name})");
            foreach (var sa in SiblingActions)
                if (sa is PopSeveral ps)
                    ite.AddBranch($"Main.Peek() is {ps.Name}", $"{ps.Target}.Add(Main.Pop() as {ps.Name})");
            ite.ElseBranch.Add(new C.SimpleStmt("break"));
            loop.Code.Add(ite);
            code.Add(loop);

            // reverse the order because of stack vs list differences
            code.Add(new C.SimpleStmt($"{Target}.Reverse()"));
            foreach (var sa in SiblingActions)
                if (sa is PopSeveral ps)
                    code.Add(new C.SimpleStmt($"{ps.Target}.Reverse()"));

            // produce the rest of the code (usually push new)
            foreach (var sa in SiblingActions)
                if (!(sa is PopSeveral))
                    sa.GenerateAbstractCode(code);
        }

        public void GenerateInitialisationCode(List<C.CsStmt> code)
        {
            code.Add(new C.SimpleStmt($"var {Target} = new List<{Name}>()"));
        }
    }

    public class AwaitOne : HandleAction
    {
        public string Name;
        public string Flag;
        public string Target;
        public string ExtraFlag;

        public HandleAction BaseAction = null;

        public override void GenerateAbstractCode(List<C.CsStmt> code)
        {
            HandleAction tmp;
            if (!String.IsNullOrEmpty(Flag))
            {
                tmp = new LiftFlag() { Flag = Flag };
                tmp.GenerateAbstractCode(code);
            }
            var lambda = new C.ScheduleStmt(Name, "_" + Target);
            lambda.AddCode($"var {Target} = {$"_{Target}".CastAs(Name)};");
            if (!String.IsNullOrEmpty(Flag))
            {
                tmp = new DropFlag() { Flag = Flag };
                tmp.GenerateAbstractCode(lambda.Code);
            }
            if (!String.IsNullOrEmpty(ExtraFlag))
                lambda.AddCode(new C.IfThenElse($"!{ExtraFlag}", "return Message.Misfire"));
            if (BaseAction != null)
                BaseAction.GenerateAbstractCode(lambda.Code);
            lambda.AddCode("return Message.Perfect");
            code.Add(lambda);
        }
    }

    public class AwaitMany : HandleAction
    {
        public string Name;
        public string Flag;
        public string Target;

        public HandleAction BaseAction = null;

        public override void GenerateAbstractCode(List<C.CsStmt> code)
        {
            HandleAction tmp;
            if (!String.IsNullOrEmpty(Flag))
            {
                tmp = new LiftFlag() { Flag = Flag };
                tmp.GenerateAbstractCode(code);
            }
            code.Add(new C.SimpleStmt($"List<{Name}> {Target} = new List<{Name}>()"));
            var lambda = new C.ScheduleStmt(Name, "_" + Target);
            var ite = new C.IfThenElse();
            var cond = $"_{Target} == null";
            ite.AddBranch(cond);
            if (BaseAction != null)
                BaseAction.GenerateAbstractCode(ite.ThenBranches[cond]);
            if (!String.IsNullOrEmpty(Flag))
            {
                tmp = new DropFlag() { Flag = Flag };
                tmp.GenerateAbstractCode(lambda.Code);
            }
            ite.AddToBranch(cond, "return Message.Perfect");
            lambda.AddCode(ite);
            lambda.AddCode($"var {Target}1 = _{Target} as {Name};");
            lambda.AddCode($"{Target}.Add({Target}1)");
            lambda.AddCode("return Message.Consume");
            code.Add(lambda);
        }
    }

    public class TearOne : HandleAction
    {
        public string Name;
        public string Target;

        public override void GenerateAbstractCode(List<C.CsStmt> code)
        {
            code.Add(new C.SimpleStmt($"{Name} almost{Target}"));
            var cond = $"Main.Peek() is {Name}";
            var ite = new C.IfThenElse(cond, $"almost{Target} = {"Main.Pop()".CastAs(Name)}");
            // TODO: not always ".value", must get the type right
            ite.AddToBranch(cond, $"var {Target} = almost{Target}.value");
            ite.AddElse($"ERROR = \"the top of the stack is not of type {Name}\"");
            ite.AddElse($"{Target} = {Name.DefaultValue()}");
            code.Add(ite);
        }
    }
}