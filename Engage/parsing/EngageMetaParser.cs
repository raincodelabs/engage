using System;
using System.Collections.Generic;
using Engage.A;
using takmelalexer;

namespace Engage.parsing
{
    public class EngageMetaParser
    {
        private int _pos;
        private List<Token> _tokens;
        private Token _lookAhead;
        private Dictionary<int, string> _tokenNames;

        public EngageMetaParser()
        {
        }

        public EngSpec ParseGrammar(string grammar)
        {
            EngageMetaLexer lexer = new EngageMetaLexer();
            _tokens = lexer.Lexise(grammar);
            _tokenNames = Utils.inverse(lexer.TokenVocab);
            if (_tokens.Count == 0)
                throw new Exception("Empty token stream when parsing grammar");
            _lookAhead = _tokens[0];
            _pos = 0;
            return EngageSpec();
        }

        private EngSpec EngageSpec()
        {
            var tds = new List<TypeDecl>();
            var lds = new List<TokenDecl>();
            var hds = new List<HandlerDecl>();

            match(EngageToken.KW_NAMESPACE);
            string ns = consumeText(EngageToken.ID);

            match(EngageToken.KW_TYPES);
            while (la_typeDecl())
                tds.Add(TypeDeclaration());

            match(EngageToken.KW_TOKENS);
            while (la_tokenDecl())
                lds.Add(TokenDeclaration());

            match(EngageToken.KW_HANDLERS);
            while (la_handlerDecl())
            {
                HandlerDecl hd = handlerDecl();
                hds.Add(hd);
            }

            return new EngSpec
            {
                NS = ns,
                Types = tds,
                Tokens = lds,
                Handlers = hds
            };
        }

        private bool la_typeDecl()
            => LookAhead(EngageToken.ID);

        private TypeDecl TypeDeclaration()
        {
            List<string> names = new List<string>();
            string super = "";

            string n = consumeText(EngageToken.ID);
            names.Add(n);

            while (LookAhead(EngageToken.COMMA))
            {
                match(EngageToken.COMMA);
                n = consumeText(EngageToken.ID);
                names.Add(n);
            }

            if (LookAhead(EngageToken.SUB_TYPE))
            {
                match(EngageToken.SUB_TYPE);
                super = consumeText(EngageToken.ID);
            }

            match(EngageToken.SEMI);

            return new TypeDecl {Names = names, Super = super};
        }

        private bool la_tokenDecl()
            => LookAhead(EngageToken.QUOTED) || LookAhead(EngageToken.KW_NUMBER) || LookAhead(EngageToken.KW_STRING);

        private TokenDecl TokenDeclaration()
        {
            var names = new List<Lexeme>();
            Lexeme n = lexeme();
            names.Add(n);
            while (LookAhead(EngageToken.COMMA))
            {
                match(EngageToken.COMMA);
                n = lexeme();
                names.Add(n);
            }

            match(EngageToken.IS_TYPE);

            return new TokenDecl {Names = names, Type = consumeText(EngageToken.ID)};
        }

        private Lexeme lexeme()
        {
            if (LookAhead(EngageToken.QUOTED))
            {
                string literal = consumeText(EngageToken.QUOTED);
                literal = preprocess(literal);
                return new LiteralLex {Literal = literal, Special = false};
            }
            else if (LookAhead(EngageToken.KW_NUMBER))
            {
                match(EngageToken.KW_NUMBER);
                return new NumberLex {Special = true};
            }
            else if (LookAhead(EngageToken.KW_STRING))
            {
                match(EngageToken.KW_STRING);
                return new StringLex {Special = true};
            }
            else
            {
                fail("Expected lexeme");
                return null;
            }
        }

        private bool la_handlerDecl()
        {
            return (LookAhead(EngageToken.QUOTED) ||
                    LookAhead(EngageToken.KW_EOF) ||
                    LookAhead(EngageToken.ID))
                   && (LookAhead2(EngageToken.KW_UPON) || LookAhead2(EngageToken.ARROW));
        }

        HandlerDecl handlerDecl()
        {
            List<Assignment> context = new List<Assignment>();

            Trigger lhs = trigger();

            match(EngageToken.ARROW);

            Reaction rhs = reaction();

            if (LookAhead(EngageToken.KW_WHERE))
            {
                match(EngageToken.KW_WHERE);

                Assignment a = assignment();
                context.Add(a);

                while (LookAhead(EngageToken.COMMA))
                {
                    match(EngageToken.COMMA);
                    a = assignment();
                    context.Add(a);
                }
            }

            HandlerDecl hd = new HandlerDecl();
            hd.LHS = lhs;
            hd.RHS = rhs;
            hd.Context = context;
            return hd;
        }

        Trigger trigger()
        {
            Trigger trig = new Trigger();

            if (LookAhead(EngageToken.QUOTED))
            {
                string terminal = consumeText(EngageToken.QUOTED);
                terminal = preprocess(terminal);
                trig.Terminal = terminal;
            }
            else if (LookAhead(EngageToken.KW_EOF))
            {
                match(EngageToken.KW_EOF);
                trig.EOF = true;
            }
            else if (LookAhead(EngageToken.ID))
            {
                trig.NonTerminal = consumeText(EngageToken.ID);
            }
            else
            {
                fail("Expected trigger");
                return null;
            }

            if (LookAhead(EngageToken.KW_UPON))
            {
                match(EngageToken.KW_UPON);
                string flag = consumeText(EngageToken.ID);
                trig.Flag = flag;
            }

            return trig;
        }

        Reaction reaction()
        {
            if (LookAhead(EngageToken.KW_PUSH))
                return pushReaction();
            else if (LookAhead(EngageToken.KW_WRAP))
                return wrapReaction();
            else if (LookAhead(EngageToken.KW_LIFT))
                return liftReaction();
            else if (LookAhead(EngageToken.KW_DROP))
                return dropReaction();
            else if (LookAhead(EngageToken.KW_TRIM))
                return trimReaction();
            else
            {
                fail("Expected reaction");
                return null;
            }
        }

        PushReaction pushReaction()
        {
            match(EngageToken.KW_PUSH);
            string name = consumeText(EngageToken.ID);
            List<string> args = new List<string>();
            if (LookAhead(EngageToken.LPAREN))
            {
                match(EngageToken.LPAREN);
                if (LookAhead(EngageToken.ID))
                {
                    string arg = consumeText(EngageToken.ID);
                    args.Add(arg);
                    while (LookAhead(EngageToken.COMMA))
                    {
                        match(EngageToken.COMMA);
                        arg = consumeText(EngageToken.ID);
                        args.Add(arg);
                    }
                }

                match(EngageToken.RPAREN);
            }

            PushReaction pr = new PushReaction();
            pr.Name = name;
            pr.Args = args;
            return pr;
        }

        WrapReaction wrapReaction()
        {
            match(EngageToken.KW_WRAP);
            string name = consumeText(EngageToken.ID);
            List<string> args = new List<string>();
            if (LookAhead(EngageToken.LPAREN))
            {
                match(EngageToken.LPAREN);
                if (LookAhead(EngageToken.ID))
                {
                    string arg = consumeText(EngageToken.ID);
                    args.Add(arg);
                    while (LookAhead(EngageToken.COMMA))
                    {
                        match(EngageToken.COMMA);
                        arg = consumeText(EngageToken.ID);
                        args.Add(arg);
                    }
                }

                match(EngageToken.RPAREN);
            }

            WrapReaction wr = new WrapReaction();
            wr.Name = name;
            wr.Args = args;
            return wr;
        }

        LiftReaction liftReaction()
        {
            match(EngageToken.KW_LIFT);
            string flag = consumeText(EngageToken.ID);
            LiftReaction lr = new LiftReaction();
            lr.Name = "";
            lr.Flag = flag;
            return lr;
        }

        DropReaction dropReaction()
        {
            match(EngageToken.KW_DROP);
            string flag = consumeText(EngageToken.ID);
            DropReaction dr = new DropReaction();
            dr.Name = "";
            dr.Flag = flag;
            return dr;
        }

        TrimReaction trimReaction()
        {
            match(EngageToken.KW_TRIM);
            string name = consumeText(EngageToken.ID);
            bool starred = false;
            if (LookAhead(EngageToken.STAR))
            {
                match(EngageToken.STAR);
                starred = true;
            }

            TrimReaction tr = new TrimReaction();
            tr.Name = name;
            tr.Starred = starred;
            return tr;
        }

        Assignment assignment()
        {
            string lhs = consumeText(EngageToken.ID);
            match(EngageToken.ASSIGN);
            Reaction rhs = operation();
            Assignment assignment = new Assignment();
            assignment.LHS = lhs;
            assignment.RHS = rhs;
            return assignment;
        }

        Reaction operation()
        {
            if (LookAhead(EngageToken.KW_POP))
            {
                match(EngageToken.KW_POP);
                string name = consumeText(EngageToken.ID);
                PopAction op = new PopAction();
                op.Name = name;
                return op;
            }
            else if (LookAhead(EngageToken.KW_POP_STAR_))
            {
                match(EngageToken.KW_POP_STAR_);
                string name = consumeText(EngageToken.ID);
                PopStarAction op = new PopStarAction();
                op.Name = name;
                return op;
            }
            else if (LookAhead(EngageToken.KW_POP_HASH_))
            {
                match(EngageToken.KW_POP_HASH_);
                string name = consumeText(EngageToken.ID);
                PopHashAction op = new PopHashAction();
                op.Name = name;
                return op;
            }
            else if (LookAhead(EngageToken.KW_AWAIT, EngageToken.LPAREN))
            {
                match(EngageToken.KW_AWAIT);
                match(EngageToken.LPAREN);
                string name = consumeText(EngageToken.ID);
                match(EngageToken.KW_UPON);
                string extraContext = consumeText(EngageToken.ID);
                match(EngageToken.RPAREN);
                string tmpContext = "";
                if (LookAhead(EngageToken.KW_WITH))
                {
                    match(EngageToken.KW_WITH);
                    tmpContext = consumeText(EngageToken.ID);
                }

                AwaitAction op = new AwaitAction();
                op.Name = name;
                op.ExtraContext = extraContext;
                op.TmpContext = tmpContext;
                return op;
            }
            else if (LookAhead(EngageToken.KW_AWAIT))
            {
                match(EngageToken.KW_AWAIT);
                string name = consumeText(EngageToken.ID);
                string tmpContext = "";
                if (LookAhead(EngageToken.KW_WITH))
                {
                    match(EngageToken.KW_WITH);
                    tmpContext = consumeText(EngageToken.ID);
                }

                AwaitAction op = new AwaitAction();
                op.Name = name;
                op.ExtraContext = "";
                op.TmpContext = tmpContext;
                return op;
            }
            else if (LookAhead(EngageToken.KW_AWAIT_STAR_))
            {
                match(EngageToken.KW_AWAIT_STAR_);
                string name = consumeText(EngageToken.ID);
                string tmpContext = "";
                if (LookAhead(EngageToken.KW_WITH))
                {
                    match(EngageToken.KW_WITH);
                    tmpContext = consumeText(EngageToken.ID);
                }

                AwaitStarAction op = new AwaitStarAction();
                op.Name = name;
                op.TmpContext = tmpContext;
                return op;
            }
            else if (LookAhead(EngageToken.KW_TEAR))
            {
                match(EngageToken.KW_TEAR);
                string name = consumeText(EngageToken.ID);
                TearAction op = new TearAction();
                op.Name = name;
                return op;
            }
            else
            {
                fail("Expected operation");
                return null;
            }
        }

        private string preprocess(string s)
        {
            return stripQuotes(s);
        }

        private string stripQuotes(string s)
        {
            string s2 = s.Substring(1, s.Length - 2);
            return s2;
        }

        private string consumeText(EngageToken tokenType)
        {
            string ret = _lookAhead.text;
            match(tokenType);
            return ret;
        }

        private string tokenName(int id) => _tokenNames[id];

        private bool eof() => _pos == _tokens.Count;

        private bool LookAhead(EngageToken tokenType)
            => !eof() && _lookAhead.id == (int) tokenType;

        private bool LookAhead(string text)
            => !eof() && _lookAhead.text == text;

        private bool LookAhead(EngageToken t1, EngageToken t2)
            => _pos + 1 < _tokens.Count && _lookAhead.id == (int) t1 && _tokens[_pos + 1].id == (int) t2;

        // Check the token *after* the current lookAhead
        private bool LookAhead2(EngageToken t)
            => _pos + 1 < _tokens.Count && _tokens[_pos + 1].id == (int) t;

        private void match(EngageToken tokenType)
        {
            if (!eof() && _lookAhead.id == (int) tokenType)
            {
                ++_pos;
                if (!eof())
                    _lookAhead = _tokens[_pos];
            }
            else
            {
                string tokType = tokenName(_lookAhead.id);
                throw new Exception($"Expected: {tokenName((int) tokenType)}, got {tokType} @line {_lookAhead.line}");
            }
        }

        private void fail(string msg)
            => throw new Exception($"Error: {msg} at token # {_pos}, line {_lookAhead.line}");
    }
}