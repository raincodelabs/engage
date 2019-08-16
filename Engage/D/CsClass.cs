using System;
using System.Collections.Generic;

namespace Engage.D
{
    public class CsClass : CsTop
    {
        public string NS;
        public string Super;
        public bool Partial = true;
        private Dictionary<string, string> PublicFields = new Dictionary<string, string>();
        private Dictionary<string, string> PrivateFields = new Dictionary<string, string>();
        private HashSet<CsExeField> Methods = new HashSet<CsExeField>();
        private HashSet<string> Usings = new HashSet<string>();
        private List<CsTop> Inners = new List<CsTop>();

        public CsClass(string name, string ns, string super,
            Dictionary<string, string> publicFields,
            Dictionary<string, string> privateFields,
            IEnumerable<D.CsExeField> methods,
            IEnumerable<string> usings,
            IEnumerable<CsTop> inners)
        {
            Name = name;
            NS = ns;
            Super = super;
            PublicFields = publicFields;
            PrivateFields = privateFields;
            Methods.UnionWith(methods);
            Usings.UnionWith(usings);
            Inners.AddRange(inners);
        }

        public void AddUsing(string name)
            => Usings.Add(name);

        public void AddInner(CsTop thing)
            => Inners.Add(thing);

        public void AddField(string name, string type, bool isPublic = true)
        {
            if (isPublic)
                PublicFields[name] = type;
            else
                PrivateFields[name] = type;
            if (type.IsCollection())
                Usings.Add("System.Collections.Generic");
        }

        public void AddConstructor(CsConstructor c)
            => Methods.Add(c);

        public void AddMethod(CsMethod c)
            => Methods.Add(c);

        public List<string> GenerateFileCode()
        {
            List<string> lines = new List<string>();
            GenerateFileCode(lines);
            return lines;
        }

        public void GenerateFileCode(List<string> lines)
        {
            lines.Comment("Engage! generated this file, please do not edit manually");
            foreach (var u in Usings)
                lines.Add($"using {u};");
            lines.Empty();
            if (String.IsNullOrEmpty(NS))
                GenerateCode(lines, 0);
            else
            {
                lines.Add($"namespace {NS}");
                lines.Open();
                GenerateCode(lines, 1);
                lines.Close();
            }
        }

        public override void GenerateCode(List<string> lines, int level)
        {
            lines.Add(level, $"public{(Partial ? " partial" : "")} class {Name}" + (String.IsNullOrEmpty(Super) ? "" : $" : {Super}"));
            lines.Open(level);
            foreach (var inner in Inners)
            {
                inner.GenerateCode(lines, level + 1);
                lines.Empty();
            }
            foreach (var fn in PublicFields.Keys)
                GenerateCodeForField(lines, level + 1, fn, PublicFields[fn]);
            foreach (var fn in PrivateFields.Keys)
                GenerateCodeForField(lines, level + 1, fn, PrivateFields[fn], isPublic: false);
            lines.Empty();
            foreach (var m in Methods)
            {
                m.GenerateCode(lines, level + 1, Name);
                lines.Empty();
            }
            //lines.Comment(level + 1, "TODO");
            lines.Close(level);
        }

        private void GenerateCodeForField(List<string> lines, int level, string name, string type, bool isPublic = true)
        {
            string mod = isPublic ? "public" : "private";
            bool col = isPublic ? PublicFields[name].IsCollection() : PrivateFields[name].IsCollection();
            if (col)
                lines.Add(level, $"{mod} {type} {name} = new {type}();");
            else
                lines.Add(level, $"{mod} {type} {name};");
        }
    }
}