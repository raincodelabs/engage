using System;
using System.Collections.Generic;

namespace AB
{
    internal class TestGenerator
    {
        private readonly Random r = new Random();

        internal List<string> GenerateNestedIfs(ulong n)
        {
            List<string> lines = new List<string>();
            for (ulong i = 0; i < n; i++)
                lines.Add(new string(' ', (int)i) + $"if X{i}");
            lines.Add("RETURN");
            for (ulong i = n; i > 0; i--)
                lines.Add(new string(' ', (int)i) + "endif");
            return lines;
        }

        internal List<string> GenerateFlatVersatile(ulong n)
        {
            List<string> lines = new List<string>();
            for (ulong i = 0; i < n; i++)
                lines.Add(RandomStmt());
            return lines;
        }

        internal string RandomStmt()
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

        internal string RandomIf()
            => $"if {RandomExpr()} return endif";

        internal string RandomClear()
            => $"clear {RandomVar()}";

        internal string RandomMap()
            => $"map {RandomExpr()} to {RandomVar()}";

        internal string RandomExpr()
            => r.Next(0, 2) == 0 ? RandomVar() : RandomNum();

        internal string RandomNum()
            => r.Next(int.MaxValue).ToString();

        internal string RandomVar()
        {
            string s = "";
            for (int i = 0; i < r.Next(1, 20); i++)
                s += (char)('a' + r.Next(0, 26));
            return s;
        }
    }
}