using Engage.C;
using System.Collections.Generic;

namespace Engage.B
{
    public class TypePlan
    {
        public string Name;
        public string Super;
        public bool IsList = false;
        private List<ConstPlan> Constructors = new List<ConstPlan>();

        public TypePlan Copy(bool turnIntoList = false)
        {
            TypePlan plan = new TypePlan();
            plan.Name = Name;
            plan.Super = Super;
            plan.IsList = IsList || turnIntoList;
            // do not copy constructors!
            return plan;
        }

        public override string ToString()
            => IsList ? $"List<{Name}>" : Name;

        public override bool Equals(object obj)
            => this.ToString() == $"{obj}";

        public void AddConstructor(ConstPlan cp)
        {
            if (!Constructors.Contains(cp))
                Constructors.Add(cp);
        }

        internal CsClass GenerateClass(string ns)
        {
            var result = new CsClass();
            result.NS = ns;
            result.Name = Name;
            result.Super = Super;
            foreach (var c in Constructors)
            {
                var cc = new CsConstructor();
                foreach (var a in c.Args)
                {
                    var name = a.Item1;
                    if (name == "this")
                        name = "value";
                    var type = a.Item2.ToString();
                    if (SystemPlan.RealNames.ContainsKey(type))
                        type = SystemPlan.RealNames[type];
                    if (type == "number")
                        ;
                    result.AddField(name, type);
                    cc.AddArgument(name, type);
                }
                result.AddConstructor(cc);
            }
            return result;
        }
    }
}