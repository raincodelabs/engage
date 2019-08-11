using Engage.C;
using System;
using System.Collections.Generic;

namespace Engage.B
{
    public abstract class HandleAction
    {
        public abstract void GenerateAbstractCode(List<CsStmt> code);
    }

    public class LiftFlag : HandleAction
    {
        public string Flag;

        public override void GenerateAbstractCode(List<CsStmt> code)
        {
            if (Flag.EndsWith('#'))
                code.Add(new CsSimpleStmt($"{Flag.Substring(0, Flag.Length - 1)}++"));
            else
                code.Add(new CsSimpleStmt($"{Flag} = true"));
        }
    }

    public class DropFlag : HandleAction
    {
        public string Flag;

        public override void GenerateAbstractCode(List<CsStmt> code)
        {
            if (Flag.EndsWith('#'))
                code.Add(new CsSimpleStmt($"{Flag.Substring(0, Flag.Length - 1)}--"));
            else
                code.Add(new CsSimpleStmt($"{Flag} = false"));
        }
    }

    public class PushNew : HandleAction
    {
        public string Name;
        public List<string> Args = new List<string>();

        public PushNew()
        {
        }

        public PushNew(string name, IEnumerable<string> args)
        {
            Name = name;
            Args.AddRange(args);
        }

        public override void GenerateAbstractCode(List<CsStmt> code)
        {
            code.Add(new CsSimpleStmt($"Push(new {Name}({String.Join(", ", Args)}))"));
        }
    }

    public class PopOne : HandleAction
    {
        public string Name;
        public string Target;

        public override void GenerateAbstractCode(List<CsStmt> code)
        {
            code.Add(new CsSimpleStmt($"{Name} {Target}"));
            var tmp = new CsComplexStmt($"if (Main.Peek() is {Name})", $"{Target} = Main.Pop() as {Name}");
            code.Add(tmp);
            tmp = new CsComplexStmt($"else", $"ERROR = \"the top of the stack is not of type {Name}\"");
            tmp.AddCode($"{Target} = null");
            code.Add(tmp);
        }
    }

    public class PopAll : HandleAction
    {
        public string Name;
        public string Target;

        public override void GenerateAbstractCode(List<CsStmt> code)
        {
            code.Add(new CsSimpleStmt($"var {Target} = new List<{Name}>()"));
            var tmp = new CsComplexStmt($"while (Main.Count > 0 && Main.Peek() is {Name})", $"{Target}.Add(Main.Pop() as {Name})");
            code.Add(tmp);
        }
    }

    public class PopSeveral : HandleAction
    {
        public string Name;
        public string Target;

        public List<HandleAction> SiblingActions = new List<HandleAction>();

        public override void GenerateAbstractCode(List<CsStmt> code)
        {
            GenerateInitialisationCode(code);
            foreach (var sa in SiblingActions)
                if (sa is PopSeveral ps)
                    ps.GenerateInitialisationCode(code);

            var loop = new CsComplexStmt();
            loop.Before = "while (Main.Count > 0)";
            loop.AddCode($"if (Main.Peek() is {Name})", $"{Target}.Add(Main.Pop() as {Name})");
            foreach (var sa in SiblingActions)
                if (sa is PopSeveral ps)
                    loop.AddCode($"else if (Main.Peek() is {ps.Name})", $"{ps.Target}.Add(Main.Pop() as {ps.Name})");
            loop.AddCode("else", "break");
            code.Add(loop);
            foreach (var sa in SiblingActions)
                if (!(sa is PopSeveral))
                    sa.GenerateAbstractCode(code);
        }

        public void GenerateInitialisationCode(List<CsStmt> code)
        {
            code.Add(new CsSimpleStmt($"var {Target} = new List<{Name}>()"));
        }
    }

    public class AwaitOne : HandleAction
    {
        public string Name;
        public string Flag;
        public string Target;
        public string ExtraFlag;

        public HandleAction BaseAction = null;

        public override void GenerateAbstractCode(List<CsStmt> code)
        {
            HandleAction tmp;
            if (!String.IsNullOrEmpty(Flag))
            {
                tmp = new LiftFlag() { Flag = Flag };
                tmp.GenerateAbstractCode(code);
            }
            // TODO: check for "extra flag"
            var lambda = new CsComplexStmt();
            lambda.Before = $"Schedule(typeof({Name}), _{Target} =>";
            lambda.After = ");";
            lambda.AddCode($"var {Target} = _{Target} as {Name};");
            if (!String.IsNullOrEmpty(Flag))
            {
                tmp = new DropFlag() { Flag = Flag };
                tmp.GenerateAbstractCode(lambda.Code);
            }
            if (!String.IsNullOrEmpty(ExtraFlag))
                lambda.AddCode($"if (!{ExtraFlag})", "return Message.Misfire");
            if (BaseAction != null)
                BaseAction.GenerateAbstractCode(lambda.Code);
            lambda.AddCode("return Message.Perfect");
            code.Add(lambda);
        }
    }
}