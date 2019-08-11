using System;
using System.Collections.Generic;
using System.Linq;

namespace Engage.back
{
    /// <summary>
    /// Almost code / abstract code
    /// </summary>

    public abstract class CsTop
    {
        public string Name;

        public abstract void GenerateCode(List<string> lines, int level);
    }

    public class CsClass : CsTop
    {
        public string NS;
        public string Super;
        private Dictionary<string, string> PublicFields = new Dictionary<string, string>();
        private Dictionary<string, string> PrivateFields = new Dictionary<string, string>();
        private HashSet<CsExeField> Methods = new HashSet<CsExeField>();
        private HashSet<string> Usings = new HashSet<string>();
        private List<CsTop> Inners = new List<CsTop>();

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
            lines.Add(level, $"public class {Name}" + (String.IsNullOrEmpty(Super) ? "" : $" : {Super}"));
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

    public class CsEnum : CsTop
    {
        public bool IsPublic = true;
        public List<string> Values = new List<string>();

        public override void GenerateCode(List<string> lines, int level)
        {
            lines.Add(level, $"{(IsPublic ? "public" : "private")} enum {Name}");
            lines.Open(level);
            foreach (var v in Values)
                lines.Add(level + 1, v + ",");
            lines.Close(level);
        }
    }

    public abstract class CsExeField
    {
        public bool IsPublic = true;
        protected List<Tuple<string, string>> Args = new List<Tuple<string, string>>();
        protected List<CsStmt> Code = new List<CsStmt>();

        public void AddArgument(string name, string type)
        {
            Args.Add(new Tuple<string, string>(name, type));
        }

        public void AddCode(string line)
            => AddCode(new CsSimpleStmt(line));

        public void AddCode(string cond, string line)
            => AddCode(new CsComplexStmt(cond, line));

        public void AddCode(CsStmt line)
        {
            if (line != null)
                Code.Add(line);
        }

        public abstract void GenerateCode(List<string> lines, int level, string className);
    }

    public class CsConstructor : CsExeField
    {
        public override void GenerateCode(List<string> lines, int level, string className)
        {
            string args = String.Join(", ", Args.Select(a => $"{a.Item2} _{a.Item1}"));
            lines.Add(level, $"{(IsPublic ? "public" : "private")} {className}({args})");
            lines.Open(level);
            foreach (var a in Args)
                if (a.Item2.IsCollection())
                    lines.Add(level + 1, $"{a.Item1}.AddRange(_{a.Item1});");
                else
                    lines.Add(level + 1, $"{a.Item1} = _{a.Item1};");
            foreach (var line in Code)
                line.GenerateCode(lines, level + 1);
            lines.Close(level);
        }
    }

    public class CsMethod : CsExeField
    {
        public string Name;
        public string RetType;

        public override void GenerateCode(List<string> lines, int level, string className)
        {
            string args = String.Join(", ", Args.Select(a => $"{a.Item2} _{a.Item1}"));
            lines.Add(level, $"{(IsPublic ? "public" : "private")} {RetType} {Name}({args})");
            lines.Open(level);
            foreach (var line in Code)
                line.GenerateCode(lines, level + 1);
            lines.Close(level);
        }
    }

    public abstract class CsStmt
    {
        public abstract void GenerateCode(List<string> lines, int level);
    }

    public class CsSimpleStmt : CsStmt
    {
        public string Code;

        public CsSimpleStmt()
        {
        }

        public CsSimpleStmt(string code)
        {
            Code = code;
        }

        public override void GenerateCode(List<string> lines, int level)
        {
            if (!Code.EndsWith(";"))
                Code += ";";
            lines.Add(level, Code);
        }
    }

    public class CsComplexStmt : CsStmt
    {
        public string Before, After;
        public List<CsStmt> Code = new List<CsStmt>();

        public CsComplexStmt()
        {
        }

        public CsComplexStmt(string before, CsStmt code, string after = "")
        {
            Before = before;
            Code.Add(code);
            After = after;
        }

        public CsComplexStmt(string before, string code, string after = "")
            : this(before, new CsSimpleStmt(code), after)
        {
        }

        public void AddCode(string stmt)
            => AddCode(new CsSimpleStmt(stmt));

        public void AddCode(string cond, string line)
            => AddCode(new CsComplexStmt(cond, line));

        public void AddCode(CsStmt stmt)
            => Code.Add(stmt);

        public override void GenerateCode(List<string> lines, int level)
        {
            if (!String.IsNullOrEmpty(Before))
                lines.Add(level, Before);
            lines.Open(level);
            foreach (var stmt in Code)
                stmt.GenerateCode(lines, level + 1);
            lines.Close(level);
            if (!String.IsNullOrEmpty(After))
            {
                if (!After.EndsWith(";"))
                    After += ";";
                lines.Add(level, After);
            }
        }
    }

    public class CsSwitchCase : CsStmt
    {
        public string Expression;
        public Dictionary<string, List<CsStmt>> Branches = new Dictionary<string, List<CsStmt>>();
        public List<CsStmt> DefaultBranch = new List<CsStmt>();

        public override void GenerateCode(List<string> lines, int level)
        {
            lines.Add(level, $"switch ({Expression})");
            lines.Open(level);
            foreach (var cond in Branches.Keys)
            {
                lines.Add(level + 1, $"case {cond}:");
                foreach (var line in Branches[cond])
                    line.GenerateCode(lines, level + 2);
                lines.Add(level + 2, "break;");
                lines.Empty();
            }
            if (DefaultBranch.Count > 0)
            {
                lines.Add(level + 1, "default:");
                foreach (var line in DefaultBranch)
                    line.GenerateCode(lines, level + 2);
                lines.Add(level + 2, "break;");
                lines.Empty();
            }
            lines.Close(level);
        }
    }
}