using System.Collections.Generic;

namespace Engage.mid
{
    public partial class EngSpec
    {
        public string NS;
        public List<TypeDecl> Types = new List<TypeDecl>();
        public List<TokenDecl> Tokens = new List<TokenDecl>();
        public List<HandlerDecl> Handlers = new List<HandlerDecl>();
    }

    public partial class TypeDecl
    {
        public List<string> Names = new List<string>();
        public string Super = "";
    }

    public partial class TokenDecl
    {
        public List<Lexeme> Names = new List<Lexeme>();
        public string Type;
    }

    public partial class Lexeme
    {
        public bool Special;
    }

    public partial class LiteralLex : Lexeme
    {
        public string Literal;
    }

    public partial class NumberLex : Lexeme
    {
    }

    public partial class StringLex : Lexeme
    {
    }

    public partial class HandlerDecl
    {
        public Trigger LHS;
        public Reaction RHS;
        public List<Assignment> Context = new List<Assignment>();

        internal Operation GetContext(string name)
        {
            foreach (var a in Context)
                if (a.LHS == name)
                    return a.RHS;
            return null;
        }
    }

    public partial class Trigger
    {
        public string Literal = "";
        public bool EOF = false;
        public string Flag = "";
    }

    public class Reaction
    {
        public virtual HandleAction ToHandleAction()
            => null;
    }

    public partial class PushReaction : Reaction
    {
        public string Name;
        public List<string> Args = new List<string>();

        public override HandleAction ToHandleAction()
            => new PushNew(Name, Args);
    }

    public partial class LiftReaction : Reaction
    {
        public string Flag;

        public override HandleAction ToHandleAction()
            => new LiftFlag() { Flag = Flag };
    }

    public partial class DropReaction : Reaction
    {
        public string Flag;

        public override HandleAction ToHandleAction()
            => new DropFlag() { Flag = Flag };
    }

    public partial class Assignment
    {
        public string LHS;
        public Operation RHS;
    }

    public partial class Operation
    {
        internal virtual HandleAction ToHandleAction(string target, HandleAction prev =null)
            => null;
    }

    public partial class PopAction : Operation
    {
        public string Name;

        internal override HandleAction ToHandleAction(string target, HandleAction prev = null)
            => new PopOne() { Name = Name, Target = target };
    }

    public partial class PopStarAction : Operation
    {
        public string Name;

        internal override HandleAction ToHandleAction(string target, HandleAction prev = null)
            => new PopAll() { Name = Name, Target = target };
    }

    public partial class AwaitAction : Operation
    {
        public string Name;
        public string TmpContext;
        public string ExtraContext;

        internal override HandleAction ToHandleAction(string target, HandleAction prev = null)
        {
            var a = new AwaitOne() { Name = Name, Target = target, Flag = TmpContext, ExtraFlag = ExtraContext };
            a.BaseAction = prev;
            return a;
        }
    }

    public partial class AwaitStarAction : Operation
    {
        public string Name;
        public string TmpContext;
    }
}