using System.Collections.Generic;

namespace Engage.mid
{
    public partial class EngSpec
    {
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
    }

    public partial class Trigger
    {
        public string Literal = "";
        public bool EOF = false;
        public string Flag = "";
    }

    public partial class Reaction
    {
    }

    public partial class PushReaction : Reaction
    {
        public string Name;
        public List<string> Args = new List<string>();
    }

    public partial class LiftReaction : Reaction
    {
        public string Flag;
    }

    public partial class DropReaction : Reaction
    {
        public string Flag;
    }

    public partial class Assignment
    {
        public string LHS;
        public Operation RHS;
    }

    public partial class Operation
    {
    }

    public partial class PopAction : Operation
    {
        public string Name;
    }

    public partial class PopStarAction : Operation
    {
        public string Name;
    }

    public partial class AwaitAction : Operation
    {
        public string Name;
        public string TmpContext;
        public string ExtraContext;
    }

    public partial class AwaitStarAction : Operation
    {
        public string Name;
        public string TmpContext;
    }
}