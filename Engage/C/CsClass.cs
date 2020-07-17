using System;
using System.Collections.Generic;
using System.Linq;

namespace Engage.C
{
    public class CsClass : CsTop
    {
        public string NS;
        public string Super;
        private readonly Dictionary<string, string> _publicFields = new Dictionary<string, string>();
        private readonly Dictionary<string, string> _privateFields = new Dictionary<string, string>();
        private readonly HashSet<CsExeField> _methods = new HashSet<CsExeField>();
        private readonly HashSet<string> _usings = new HashSet<string>();
        private readonly List<CsTop> _inners = new List<CsTop>();

        public override D.CsTop Concretize()
            => new D.CsClass(Name, NS, Super, _publicFields, _privateFields, _methods.Select(m => m.Concretize()),
                _usings, _inners.Select(x => x.Concretize()));

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
    }
}