using System;
using System.Collections.Generic;

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
        private List<CsConstructor> Constructors = new List<CsConstructor>();
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
            foreach(var fn in Fields.Keys)
            {
                if (Fields[fn].IsCollection())
                    lines.Add(level + 1, $"public {Fields[fn]} {fn} = new {Fields[fn]}();");
                else
                    lines.Add(level + 1, $"public {Fields[fn]} {fn}");
            }
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
    }
}