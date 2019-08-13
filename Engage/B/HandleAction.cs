using Engage.C;
using System;
using System.Collections.Generic;

namespace Engage.B
{
    public abstract class HandleAction
    {
        public abstract void GenerateAbstractCode(List<CsStmt> code);

        protected string CastAs(string expr, string type)
            => type == "System.Int32"
            ? $"({type}){expr}"
            : $"{expr} as {type}";

        protected string DefaultValue(string type)
            => type == "System.Int32"
            ? "0"
            : "null";
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

    public class TrimStream : HandleAction
    {
        public string Type;
        public bool Starred;

        public override void GenerateAbstractCode(List<CsStmt> code)
        {
            code.Add(new CsSimpleStmt($"Trim(typeof({Type}))"));
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
            var tmp = new CsComplexStmt($"if (Main.Peek() is {Name})", $"{Target} = {CastAs("Main.Pop()", Name)}");
            code.Add(tmp);
            tmp = new CsComplexStmt($"else", $"ERROR = \"the top of the stack is not of type {Name}\"");
            tmp.AddCode($"{Target} = {DefaultValue(Name)}");
            code.Add(tmp);
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

        public override void GenerateAbstractCode(List<CsStmt> code)
        {
            var guard = new CsComplexStmt($"if (Main.Peek() is {Type})",
                $"{Type} {Target} = {CastAs("Main.Pop()", Type)}");
            guard.AddCode($"Push(new {Name}({Target}))");
            code.Add(guard);
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
            code.Add(new CsSimpleStmt($"{Target}.Reverse()"));
        }
    }

    public class PopSeveral : HandleAction
    {
        public string Name;
        public string Target;

        public List<HandleAction> SiblingActions = new List<HandleAction>();

        public override void GenerateAbstractCode(List<CsStmt> code)
        {
            // initialise lists
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

            // reverse the order because of stack vs list differences
            code.Add(new CsSimpleStmt($"{Target}.Reverse()"));
            foreach (var sa in SiblingActions)
                if (sa is PopSeveral ps)
                    code.Add(new CsSimpleStmt($"{ps.Target}.Reverse()"));

            // produce the rest of the code (usually push new)
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
            var lambda = new CsComplexStmt();
            lambda.Before = $"Schedule(typeof({Name}), _{Target} =>";
            lambda.After = ");";
            if (Name == "System.Int32") // corner case - "as" doesn't work on ints in C#
                lambda.AddCode($"var {Target} = ({Name})_{Target};");
            else
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

    public class AwaitMany : HandleAction
    {
        public string Name;
        public string Flag;
        public string Target;

        public HandleAction BaseAction = null;

        public override void GenerateAbstractCode(List<CsStmt> code)
        {
            HandleAction tmp;
            if (!String.IsNullOrEmpty(Flag))
            {
                tmp = new LiftFlag() { Flag = Flag };
                tmp.GenerateAbstractCode(code);
            }
            code.Add(new CsSimpleStmt($"List<{Name}> {Target} = new List<{Name}>()"));
            var lambda = new CsComplexStmt();
            lambda.Before = $"Schedule(typeof({Name}), _{Target} =>";
            lambda.After = ");";
            var ifst = new CsComplexStmt();
            ifst.Before = $"if (_{Target} == null)";
            if (BaseAction != null)
                BaseAction.GenerateAbstractCode(ifst.Code);
            if (!String.IsNullOrEmpty(Flag))
            {
                tmp = new DropFlag() { Flag = Flag };
                tmp.GenerateAbstractCode(lambda.Code);
            }
            ifst.AddCode("return Message.Perfect");
            lambda.AddCode(ifst);
            lambda.AddCode($"var {Target}1 = _{Target} as {Name};");
            lambda.AddCode($"{Target}.Add({Target}1)");
            lambda.AddCode("return Message.Consume");
            code.Add(lambda);
        }
    }
}