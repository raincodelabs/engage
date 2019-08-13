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
            "dcl",
            "enddcl",
            "endif",
            "if",
            "integer",
            "map",
            "return",
            "to",
        };

        internal List<string> GenerateStackedIfs(ulong n)
        {
            List<string> lines = new List<string>();
            RandomStmtBlock(lines, n);
            lines.Add("if COND1");
            RandomStmtBlock(lines, n);
            lines.Add("if COND2");
            RandomStmtBlock(lines, n);
            lines.Add("endif"); // cond2
            RandomStmtBlock(lines, n);
            lines.Add("endif"); // cond1
            RandomStmtBlock(lines, n);
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

        private void RandomStmtBlock(List<string> lines, ulong size)
        {
            for (ulong i = 0; i < size; i++)
                lines.Add(RandomStmt());
        }

        private string RandomStmt()
        {
            switch (r.Next(0, 5))
            {
                case 0:
                    return RandomIf();

                case 1:
                    return RandomClear();

                case 2:
                    return RandomMap();

                default:
                    return "return";
            };
        }

        private string RandomIf()
            => $"if {RandomExpr()} return endif";

        private string RandomClear()
            => $"clear {RandomVar()}";

        private string RandomMap()
            => $"map {RandomExpr()} to {RandomVar()}";

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