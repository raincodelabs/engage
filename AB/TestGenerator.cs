using System;
using System.Collections.Generic;
using System.Linq;

namespace AB
{
    internal class TestGenerator
    {
        private readonly Random r = new Random();

        private static string[] Reserved = new[] {
            "char",
            "clear",
            "converse",
            "dcl",
            "dec",
            "enddcl",
            "endif",
            "handler",
            "if",
            "integer",
            "map",
            "overlay",
            "print",
            "return",
            "to",
        };

        internal List<string> GenerateStackedIfs(ulong n)
        {
            List<string> lines = new List<string>();
            RandomStmtBlock(lines, n, false);
            lines.Add("if COND1");
            RandomStmtBlock(lines, n, false);
            lines.Add("if COND2");
            RandomStmtBlock(lines, n, false);
            lines.Add("endif"); // cond2
            RandomStmtBlock(lines, n, false);
            lines.Add("endif"); // cond1
            RandomStmtBlock(lines, n, false);
            return lines;
        }

        internal List<string> GenerateNestedIfs(ulong n)
        {
            List<string> lines = new List<string>();
            for (ulong i = 0; i < n; i++)
                lines.Add(new string(' ', (int)i) + $"if X{i}");
            lines.Add("return");
            for (ulong i = n; i > 0; i--)
                lines.Add(new string(' ', (int)i) + "endif");
            return lines;
        }

        internal List<string> GenerateFlatVersatile(ulong n)
        {
            List<string> lines = new List<string>();
            RandomStmtBlock(lines, n);
            return lines;
        }

        private void RandomStmtBlock(List<string> lines, ulong size, bool isIfAllowed = true)
        {
            for (ulong i = 0; i < size; i++)
                lines.Add(isIfAllowed ? RandomStmt() : RandomStmtNoIf());
        }

        private string RandomStmt()
            => RandomStmtHelper(0);

        private string RandomStmtNoIf()
            => RandomStmtHelper(1);

        private string RandomStmtHelper(int start)
        {
            switch (r.Next(start, 8))
            {
                case 0:
                    return RandomIf();

                case 1:
                    return RandomClear();

                case 2:
                    return RandomConverse();

                case 3:
                    return RandomHandler();

                case 4:
                    return RandomMap();

                case 5:
                    return RandomOverlay();

                case 6:
                    return RandomPrint();

                default:
                    return "return";
            };
        }

        private string RandomIf()
            => $"if {RandomExpr()} return endif";

        private string RandomClear()
            => $"clear {RandomVar()}";

        private string RandomConverse()
            => $"converse {RandomVar()}";

        private string RandomHandler()
            => $"handler {RandomVar()} ({RandomVar()})";

        private string RandomMap()
            => $"map {RandomExpr()} to {RandomVar()}";

        private string RandomOverlay()
            => $"overlay {RandomVar()} to {RandomVar()}";

        private string RandomPrint()
            => $"print {RandomExpr()}";

        private string RandomExpr()
            => r.Next(0, 2) == 0 ? RandomVar() : RandomNum();

        private string RandomNum()
            => r.Next(int.MaxValue).ToString();

        private string RandomVar()
        {
            string s = "";
            for (int i = 0; i < r.Next(1, 20); i++)
                s += (char)('a' + r.Next(0, 26));
            if (Reserved.Contains(s))
                s += "_";
            return s;
        }
    }
}