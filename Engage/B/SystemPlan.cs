using Engage.C;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Engage.B
{
    // Dictionary<TokenPlan,HandlerCollection>
    internal class HandlerMetaCollection
    {
        private readonly Dictionary<TokenPlan, HandlerCollection>
            _data = new Dictionary<TokenPlan, HandlerCollection>();

        public void Add(HandlerPlan hp)
        {
            if (!_data.ContainsKey(hp.ReactOn))
                _data[hp.ReactOn] = new HandlerCollection();
            _data[hp.ReactOn].AddGuardAndRecipe(hp);
        }

        public IEnumerable<TokenPlan> SortedKeys()
        {
            var resortedKeys = _data.Keys.ToList();
            resortedKeys.Sort((x, y) => y.Value.Length - x.Value.Length);
            return resortedKeys;
        }

        public IReadOnlyList<string> GuardFlags(TokenPlan tp) => _data[tp].GuardFlags;
        public IReadOnlyList<List<HandleAction>> Recipes(TokenPlan tp) => _data[tp].Recipes;
    }

    // Tuple<List<string>, List<List<HandleAction>>>
    internal class HandlerCollection
    {
        private readonly List<TokenPlan> _reacts = new List<TokenPlan>();
        private readonly List<string> _triggers = new List<string>();
        private List<List<HandleAction>> Handlers = new List<List<HandleAction>>();
        public IReadOnlyList<string> GuardFlags => _triggers;
        public IReadOnlyList<List<HandleAction>> Recipes => Handlers;

        public void AddGuardAndRecipe(HandlerPlan hp)
        {
            for (int i = 0; i < _triggers.Count; i++)
                if (_triggers[i] == hp.GuardFlag && _reacts[i].Value == hp.ReactOn.Value)
                {
                    hp.AddRecipeTo(Handlers[i]);
                    return;
                }

            // if branch not found, make a new one
            _reacts.Add(hp.ReactOn);
            _triggers.Add(hp.GuardFlag);
            hp.AddRecipeTo(Handlers);
        }
    }

    public class SystemPlan
    {
        internal static readonly Dictionary<string, string> RealNames = new Dictionary<string, string>()
        {
            {"string", "System.String"},
            {"number", "System.Int32"},
        };

        public string NS;
        public string TopType;
        private readonly Dictionary<string, TypePlan> Types = new Dictionary<string, TypePlan>();
        private static Dictionary<string, string> TypeAliases = new Dictionary<string, string>();
        private readonly HashSet<string> BoolFlags = new HashSet<string>();
        private readonly HashSet<string> IntFlags = new HashSet<string>();
        private readonly Dictionary<string, List<TokenPlan>> Tokens = new Dictionary<string, List<TokenPlan>>();
        private readonly Dictionary<string, List<HandlerPlan>> Handlers = new Dictionary<string, List<HandlerPlan>>();

        public SystemPlan(string ns)
        {
            NS = ns;
        }

        public static string Dealias(string name)
            => TypeAliases.ContainsKey(name) ? RealNames[TypeAliases[name]] : name;

        public static string Unalias(string name)
            => TypeAliases.ContainsKey(name) ? TypeAliases[name] : name;

        internal void AddBoolFlag(string name)
        {
            if (!String.IsNullOrEmpty(name))
                BoolFlags.Add(name);
        }

        internal void AddIntFlag(string name)
        {
            if (!String.IsNullOrEmpty(name))
                IntFlags.Add(name);
        }

        internal void NormaliseFlags()
        {
            foreach (string f in BoolFlags.Distinct().ToArray())
                if (f.EndsWith("#"))
                {
                    IntFlags.Add(f.Substring(0, f.Length - 1));
                    BoolFlags.Remove(f);
                }
        }

        internal string AllBoolFlags()
            => String.Join(", ", BoolFlags);

        internal string AllIntFlags()
            => String.Join(", ", IntFlags);

        internal void AddHandler(HandlerPlan hp)
        {
            string type = hp.ReactOn.Value;
            foreach (var k in Tokens.Keys)
                if (Tokens[k].Contains(hp.ReactOn))
                    type = k;
            if (String.IsNullOrEmpty(type))
                Console.WriteLine($"[A2B] Cannot determine type of token '{hp.ReactOn.Value}'");
            if (!Handlers.ContainsKey(type))
                Handlers[type] = new List<B.HandlerPlan>();
            Handlers[type].Add(hp);
        }

        internal void AddToken(string type, List<B.TokenPlan> ts)
        {
            Tokens[type] = ts;
        }

        internal TypePlan GetTypePlan(string name)
        {
            if (Types.ContainsKey(name))
                return Types[name];
            if (TypeAliases.ContainsKey(name))
                return GetTypePlan(TypeAliases[name]);
            if (RealNames.ContainsKey(name))
                return new TypePlan(RealNames[name]);
            Console.WriteLine($"[ B ] Failed to get a type plan for '{name}'");
            return null;
        }

        internal bool HasType(string name)
            => Types.ContainsKey(name);

        internal void AddType(string n, string super, bool silent = false)
        {
            if (String.IsNullOrEmpty(n))
                return;
            if (Types.Count == 0)
            {
                TopType = n;
                Console.WriteLine($"[A2B] Top type is assumed to be {TopType}");
            }

            if (Types.ContainsKey(n))
            {
                if (!silent)
                    Console.WriteLine($"[A2B] Cannot add type '{n}' the second time");
                return;
            }

            var tp = new TypePlan(n) {Super = super};
            Console.WriteLine($"[A2B] Added type '{n}' to the plan");
            Types[tp.Name] = tp;
        }

        internal void InferTypeAliases()
        {
            foreach (var t in Tokens.Keys)
            {
                if (t == "mark" || t == "skip" || t == "word")
                    continue;
                foreach (var tok in Tokens[t].Where(tok => tok.Special))
                    TypeAliases[t] = tok.Value;
            }
        }

        public IEnumerable<C.CsClass> GenerateDataClasses()
            => Types.Values
                .Where(t => !t.IsList)
                .Select(t => t.GenerateClass(NS));

        public C.CsClass GenerateParser()
        {
            var p = new C.CsClass
            {
                NS = NS,
                Name = "Parser",
                Super = "BaseParser"
            };
            p.AddUsing("EngageRuntime");
            p.AddUsing("System");
            p.AddUsing("System.Collections.Generic");
            if (BoolFlags.Count > 0)
                p.AddField(String.Join(", ", BoolFlags.OrderBy(x => x)), "bool", isPublic: false);
            if (IntFlags.Count > 0)
                p.AddField(String.Join(", ", IntFlags.OrderBy(x => x)), "int", isPublic: false);
            // token types
            var tt = new C.CsEnum
            {
                IsPublic = false,
                Name = "TokenType"
            };
            tt.Add("TUndefined");
            tt.Add("TEOF");
            tt.Add(Tokens.Keys.Where(t => t != "skip").Select(t => "T" + t));
            p.AddInner(tt);
            // parser constructor
            var pc = new C.CsConstructor
            {
                InheritFromBase = true
            };
            pc.AddArgument("input", "string");
            p.AddConstructor(pc);
            // the parse function
            var pf = new C.CsMethod
            {
                Name = "Parse",
                RetType = "object"
            };
            pf.AddCode("string ERROR = \"\"");
            pf.AddCode("TokenType type");
            pf.AddCode("string lexeme");
            var loop = new List<C.CsStmt>();
            var pl = new C.WhileStmt("type != TokenType.TEOF", reversed: true);

            // main parsing loop: begin
            pl.AddCode("var _token = NextToken();");
            pl.AddCode("lexeme = _token.Item2;");
            pl.AddCode("type = _token.Item1;");

            var swType = new C.SwitchCaseStmt
            {
                Expression = "type"
            };

            var usedTokens = new HashSet<string> {"skip"};

            foreach (var hpk in Handlers.Keys)
            {
                var branchType = new List<C.CsStmt>();
                if (hpk == "EOF")
                    branchType.Add(new C.SimpleStmt("Flush()"));
                if (Handlers[hpk].Count == 1)
                {
                    Handlers[hpk][0].GenerateAbstractCode(branchType);
                }
                else
                {
                    var swLex = new C.SwitchCaseStmt();
                    // much faster to switch-case on a char than on a string
                    bool matchChar = Handlers[hpk].Select(hp => hp.ReactOn.Value).All(v => v.Length == 1);
                    swLex.Expression = "lexeme" + (matchChar ? "[0]" : "");
                    // Need this dance because there may be different actions for the same token with different guards
                    HandlerMetaCollection resortedHandlers = new HandlerMetaCollection();
                    foreach (var hp in Handlers[hpk])
                        resortedHandlers.Add(hp);
                    foreach (var key in resortedHandlers.SortedKeys())
                        GenerateLexBranch(swLex, hpk, resortedHandlers.GuardFlags(key), resortedHandlers.Recipes(key),
                            key, matchChar);
                    branchType.Add(swLex);
                }

                swType.Branches["TokenType.T" + hpk] = branchType;
                usedTokens.Add(hpk);
            }

            foreach (var t in Tokens.Keys)
            {
                if (!usedTokens.Contains(t))
                    Console.WriteLine($"[B2C] unused token {t}");
                foreach (B.TokenPlan tok in Tokens[t])
                {
                    if (!tok.Special)
                        continue;
                    var branchType = new List<C.CsStmt>();
                    string todo = tok.Value switch
                    {
                        "number" => "System.Int32.Parse(lexeme)",
                        "string" => "lexeme",
                        _ => ""
                    };
                    todo = PossiblyWrap(todo, tok.Value);
                    branchType.Add(new C.SimpleStmt($"Push({todo})"));

                    swType.Branches["TokenType.T" + t] = branchType;
                }
            }

            pl.AddCode(swType);
            const string cond = "!System.String.IsNullOrEmpty(ERROR)";
            var abend = new C.IfThenElse();
            abend.AddToBranch(cond, "Console.WriteLine(\"Parser error: \" + ERROR);");
            abend.AddToBranch(cond, "return null;");
            pl.AddCode(abend);
            // main parsing loop: end

            pf.AddCode(pl);
            pf.AddCode(new C.IfThenElse($"Main.Peek() is {TopType}", "return Main.Pop()"));
            pf.AddCode("return null"); // TODO!!!
            p.AddMethod(pf);

            // other methods
            GenerateTokeniser(p);

            return p;
        }

        private string PossiblyWrap(string v, string key)
        {
            foreach (var x in
                from t in TypeAliases.Keys
                where TypeAliases[t] == key && Handlers.ContainsKey(t)
                select Handlers[t][0].IsPushFirst())
                if (String.IsNullOrEmpty(x))
                    Console.WriteLine("[B2C] some unsupported functionality found");
                else
                    return $"new {x}({v})";
            return v;
        }

        private static void GenerateLexBranch(SwitchCaseStmt swLex, string hpk, IReadOnlyList<string> guardFlags,
            IReadOnlyList<List<HandleAction>> recipes, TokenPlan reactOn, bool matchChar)
        {
            var branchLex = new List<C.CsStmt>();
            Console.WriteLine($"[IR] in '{hpk}', handle {reactOn.Value}");
            bool onlyWraps = true;
            var ite = new C.IfThenElse();
            for (int i = 0; i < guardFlags.Count; i++)
            {
                if (!String.IsNullOrEmpty(guardFlags[i]))
                    ite.AddBranch(guardFlags[i]);
                var target
                    = String.IsNullOrEmpty(guardFlags[i])
                        ? branchLex
                        : ite.GetThenBranch(guardFlags[i]);
                foreach (var action in recipes[i])
                {
                    if (!(action is WrapOne))
                        onlyWraps = false;
                    if (action != null)
                    {
                        if (action is WrapOne)
                        {
                            var fake = new List<CsStmt>();
                            action.GenerateAbstractCode(fake);
                            if (fake.Count == 1 && fake[0] is IfThenElse ite2)
                            {
                                var newcond = guardFlags[i] + " && " + ite2.FirstThenBranchKey();
                                ite.RenameBranch(guardFlags[i], newcond);
                                foreach (CsStmt stmt in ite2.FirstThenBranchValue())
                                    ite.AddToBranch(newcond, stmt);
                            }
                            else
                                Console.WriteLine("[ERR] This particular usage of WRAP is not supported yet");
                        }
                        else
                            action.GenerateAbstractCode(target);
                    }
                    else
                        Console.WriteLine($"[B2C] Warning: no action to handle '{hpk}'/{reactOn.Value}");
                }
            }

            string flags;
            if (guardFlags.Count == 1)
                flags = "flag " + guardFlags[0] + " is not";
            else
                flags = "neither of the flags " + String.Join(", ", guardFlags) + " are";
            branchLex.Add(ite);
            if (!onlyWraps)
                if (guardFlags.Any(f => !String.IsNullOrEmpty(f)))
                    ite.AddElse($"ERROR = \"{flags} lifted when expected\"");
            if (matchChar)
                swLex.Branches["'" + reactOn.Value + "'"] = branchLex;
            else
                swLex.Branches['"' + reactOn.Value + '"'] = branchLex;
        }

        private void GenerateTokeniser(C.CsClass cls)
        {
            var skipmark = new List<string>();

            var tok = new C.CsMethod
            {
                IsPublic = false,
                Name = "NextToken",
                RetType = "Tuple<TokenType, string>"
            };

            // init phase
            tok.AddCode("TokenType t = TokenType.TUndefined;");
            tok.AddCode("string s = \"\";");
            // EOF phase
            tok.AddCode(new C.IfThenElse("Pos >= Input.Length",
                "return new Tuple<TokenType, string>(TokenType.TEOF, \"\")"));
            // skip
            if (Tokens.ContainsKey("skip"))
            {
                string cond = String.Join(" || ", Tokens["skip"].Select(t => $"Input[Pos] == '{t.Value}'"));
                tok.AddCode(new C.WhileStmt($"Pos < Input.Length && ({cond})", "Pos++"));
                Tokens["skip"].ForEach(t => skipmark.Add(t.Value));
            }
            else
                Console.WriteLine($"[IR] It is suspicious that there are no tokens of type 'skip'");

            // EOF after skip
            var megaIf = new C.IfThenElse("Pos >= Input.Length",
                "return new Tuple<TokenType, string>(TokenType.TEOF, \"\")");
            tok.AddCode(megaIf);
            // mark
            if (Tokens.ContainsKey("mark"))
            {
                Tokens["mark"].ForEach(t => skipmark.Add(t.Value));
                GenerateBranches("mark", megaIf, null);
            }
            else
                Console.WriteLine("[IR] It is suspicious that there are no tokens of type 'word'");

            // word
            if (Tokens.ContainsKey("word"))
                GenerateBranches("word", megaIf, skipmark);
            else
                Console.WriteLine("[IR] It is suspicious that there are no tokens of type 'word'");
            // number etc
            foreach (var tt in Tokens.Keys.Where(tt => tt != "skip" && tt != "word" && tt != "mark"))
                GenerateBranches(tt, megaIf, skipmark);
            tok.AddCode("return new Tuple<TokenType, string>(t, s);");

            cls.AddMethod(tok);
        }

        // Precondition: Tokens.Contains(token_name)
        private void GenerateBranches(string tokenName, C.IfThenElse ite, List<string> skipMark)
        {
            if (!Tokens.ContainsKey(tokenName))
                return;
            foreach (var tm in Tokens[tokenName])
            {
                var res
                    = tm.Special
                        ? GenerateBranchSpecialMatch(tm.Value, tokenName)
                        : GenerateBranchPreciseMatch(tm.Value, tokenName, skipMark);
                if (res != null)
                    ite.AddToBranch(res.Item1, res.Item2);
            }
        }

        private Tuple<string, IEnumerable<CsStmt>> GenerateBranchSpecialMatch(string value, string type)
        {
            switch (value)
            {
                case "number":
                    return GenerateBranchNumberMatch(type);

                case "string":
                    return GenerateBranchStringMatch(type);

                default:
                    Console.WriteLine($"[IR] Cannot generate a match for '{value}'");
                    return null;
            }
        }

        private Tuple<string, IEnumerable<CsStmt>> GenerateBranchStringMatch(string type)
        {
            string cond = "";
            if (Tokens.ContainsKey("skip"))
                foreach (var t in Tokens["skip"])
                    cond += $" && Input[Pos] != '{t.Value}'";
            if (Tokens.ContainsKey("mark"))
                foreach (var t in Tokens["mark"])
                    cond += $" && Input[Pos] != '{t.Value}'";
            var block = new List<CsStmt>();
            block.Add(new SimpleStmt($"t = TokenType.T{type}"));
            block.Add(new C.WhileStmt($"Pos < Input.Length{cond}", "s += Input[Pos++]"));
            return new Tuple<string, IEnumerable<CsStmt>>(null, block); // null condition means the ELSE branch
        }

        private Tuple<string, IEnumerable<CsStmt>> GenerateBranchNumberMatch(string type)
        {
            var block = new List<CsStmt>();
            string cond = string.Join(" || ", "0123456789".Select(c => $"Input[Pos] == '{c}'"));
            block.Add(new SimpleStmt($"t = TokenType.T{type}"));
            block.Add(new C.WhileStmt($"Pos < Input.Length && ({cond})", "s += Input[Pos++]"));
            return new Tuple<string, IEnumerable<CsStmt>>(cond, block);
        }

        private Tuple<string, IEnumerable<CsStmt>> GenerateBranchPreciseMatch(string value, string type,
            List<string> skipmark)
        {
            int len = value.Length;
            var block = new List<CsStmt>();
            var cond = len > 1 ? $"Pos + {len - 1} < Input.Length" : "";
            for (int i = 0; i < len; i++)
                cond += $" && Input[Pos + {i}] == '{value[i]}'";
            if (cond.StartsWith(" && "))
                cond = cond.Substring(4);

            // either EOF or next is skip or mark
            if (skipmark != null && skipmark.Count > 0)
                cond =
                    $"({cond}) && (Pos + {len} == Input.Length || {String.Join(" || ", skipmark.Select(c => $"Input[Pos + {len}] == '{c}'"))})";

            cond = cond.Replace(" + 0", "");
            block.Add(new SimpleStmt($"t = TokenType.T{type}"));
            block.Add(new SimpleStmt($"s = \"{value}\""));
            block.Add(new SimpleStmt(len > 1 ? $"Pos += {len}" : "Pos++"));
            return new Tuple<string, IEnumerable<CsStmt>>(cond, block);
        }
    }
}
