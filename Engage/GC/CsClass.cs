using System;
using System.Collections.Generic;
using System.Linq;

namespace Engage.GC;

public class CsClass : CsTop
{
    public readonly string NS;
    public readonly string Super;
    public readonly bool Partial = true;
    private readonly Dictionary<string, string> _publicFields;
    private readonly Dictionary<string, string> _privateFields;
    private readonly HashSet<CsExeField> _methods = new();
    private readonly HashSet<string> _usings = new();
    private readonly List<CsTop> _inners = new();

    public CsClass(string name, string ns, string super,
        Dictionary<string, string> publicFields,
        Dictionary<string, string> privateFields,
        IEnumerable<GC.CsExeField> methods,
        IEnumerable<string> usings,
        IEnumerable<GC.CsTop> inners)
    {
        Name = name;
        NS = ns;
        Super = super;
        _publicFields = publicFields;
        _privateFields = privateFields;
        _methods.UnionWith(methods);
        _usings.UnionWith(usings);
        _inners.AddRange(inners);
    }

    public void AddUsing(string name)
        => _usings.Add(name);

    public void AddInner(CsTop thing)
        => _inners.Add(thing);

    public void AddField(string name, string type, bool isPublic = true)
    {
        if (isPublic)
            _publicFields[name] = type;
        else
            _privateFields[name] = type;
        if (type.IsCollection())
            _usings.Add("System.Collections.Generic");
    }

    public void AddConstructor(CsConstructor c)
        => _methods.Add(c);

    public void AddMethod(CsMethod c)
        => _methods.Add(c);

    public IEnumerable<string> GenerateFileCode()
    {
        var lines = new List<string>();
        GenerateFileCode(lines);
        return lines;
    }

    private void GenerateFileCode(List<string> lines)
    {
        lines.Comment("Engage! generated this file, please do not edit manually");
        lines.AddRange(_usings.Select(u => $"using {u};"));
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
        lines.Add(level,
            $"public{(Partial ? " partial" : "")} class {Name}" +
            (String.IsNullOrEmpty(Super) ? "" : $" : {Super}"));
        lines.Open(level);
        foreach (var inner in _inners)
        {
            inner.GenerateCode(lines, level + 1);
            lines.Empty();
        }

        foreach (var fn in _publicFields.Keys)
            GenerateCodeForField(lines, level + 1, fn, _publicFields[fn]);
        foreach (var fn in _privateFields.Keys)
            GenerateCodeForField(lines, level + 1, fn, _privateFields[fn], isPublic: false);
        lines.Empty();
        foreach (var m in _methods)
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
        bool col = isPublic ? _publicFields[name].IsCollection() : _privateFields[name].IsCollection();
        lines.Add(level, col
            ? $"{mod} {type} {name} = new {type}();"
            : $"{mod} {type} {name};");
    }
}