using System;
using System.Collections.Generic;

namespace Engage
{
    public static class GenExt
    {
        public static void Add(this List<string> lines, int level, string line)
            => lines.Add(Indent(level) + line);

        public static void Empty(this List<string> lines)
            => lines.Add("");

        public static void Comment(this List<string> lines, int level, string line)
            => lines.Add(Indent(level) + "// " + line);

        public static void Comment(this List<string> lines, string line)
            => Comment(lines, 0, line);

        public static void Open(this List<string> lines, int level = 0)
            => lines.Add(Indent(level) + "{");

        public static void Close(this List<string> lines, int level = 0)
            => lines.Add(Indent(level) + "}");

        public static bool IsCollection(this string type)
            => type.StartsWith("List<") || type.StartsWith("HashSet<") || type.StartsWith("Dictionary<") || type.StartsWith("Stack<");

        private static string Indent(int level)
            => new String(' ', level * 4);

        public static string CastAs(this string expr, string type)
            => type == "System.Int32"
            ? $"({type}){expr}"
            : $"{expr} as {type}";

        public static string DefaultValue(this string type)
            => type == "System.Int32"
            ? "0"
            : "null";
    }
}