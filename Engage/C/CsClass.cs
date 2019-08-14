using System;
using System.Collections.Generic;
using System.Linq;

namespace Engage.C
{
    public class CsClass : CsTop
    {
        public string NS;
        public string Super;
        private Dictionary<string, string> PublicFields = new Dictionary<string, string>();
        private Dictionary<string, string> PrivateFields = new Dictionary<string, string>();
        private HashSet<CsExeField> Methods = new HashSet<CsExeField>();
        private HashSet<string> Usings = new HashSet<string>();
        private List<CsTop> Inners = new List<CsTop>();

        public override D.CsTop Concretize()
            => new D.CsClass(Name, NS, Super, PublicFields, PrivateFields, Methods.Select(m => m.Concretize()), Usings, Inners.Select(x => x.Concretize()));

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

        

    }
}