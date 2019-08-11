using System;
using System.Collections.Generic;
using System.Linq;

namespace Engage.back
{
    /// <summary>
    /// Almost code / abstract code
    /// </summary>
    public class CsClass
    {
        public string NS; // TODO!
        public string Name;
        public string Super;
        private Dictionary<string, string> Fields = new Dictionary<string, string>();
        private HashSet<CsConstructor> Constructors = new HashSet<CsConstructor>();
        private HashSet<string> Usings = new HashSet<string>();

        public void AddField(string name, string type)
        {
            Fields[name] = type;
            if (type.IsCollection())
                Usings.Add("System.Collections.Generic");
        }

        public void AddConstructor(CsConstructor c)
        {
            Constructors.Add(c);
        }

        public List<string> GenerateCode()
        {
            List<string> lines = new List<string>();
            GenerateCode(lines);
            return lines;
        }

        public void GenerateCode(List<string> lines)
        {
            lines.Comment("Engage! generated this file, please do not edit manually");
            foreach (var u in Usings)
                lines.Add($"using {u};");
            lines.Empty();
            if (String.IsNullOrEmpty(NS))
                GenerateClassCode(lines, 0);
            else
            {
                lines.Add($"namespace {NS}");
                lines.Open();
                GenerateClassCode(lines, 1);
                lines.Close();
            }
        }

        private void GenerateClassCode(List<string> lines, int level)
        {
            lines.Add(level, $"public class {Name}" + (String.IsNullOrEmpty(Super) ? "" : $" : {Super}"));
            lines.Open(level);
            foreach (var fn in Fields.Keys)
            {
                if (Fields[fn].IsCollection())
                    lines.Add(level + 1, $"public {Fields[fn]} {fn} = new {Fields[fn]}();");
                else
                    lines.Add(level + 1, $"public {Fields[fn]} {fn};");
            }
            lines.Empty();
            foreach (var c in Constructors)
                c.GenerateClassCode(lines, level + 1, Name);
            lines.Comment(level + 1, "TODO");
            lines.Close(level);
        }
    }

    public class CsConstructor
    {
        private List<Tuple<string, string>> Args = new List<Tuple<string, string>>();

        public void AddArgument(string name, string type)
        {
            Args.Add(new Tuple<string, string>(name, type));
        }

        public void GenerateClassCode(List<string> lines, int level, string className)
        {
            string args = String.Join(", ", Args.Select(a => $"{a.Item2} _{a.Item1}"));
            lines.Add(level, $"public {className}({args})");
            lines.Open(level);
            foreach (var a in Args)
                if (a.Item2.IsCollection())
                    lines.Add(level + 1, $"{a.Item1}.AddRange(_{a.Item1});");
                else
                    lines.Add(level + 1, $"{a.Item1} = _{a.Item1};");
            lines.Close(level);
        }
    }
}