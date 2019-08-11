using Engage.C;
using System.Collections.Generic;

namespace Engage.B
{
    public class TypePlan
    {
        public string Name;
        public string Super;
        public bool IsList = false;
        public List<ConstPlan> Constructors = new List<ConstPlan>();

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
                    result.AddField(a.Item1, a.Item2.ToString());
                    cc.AddArgument(a.Item1, a.Item2.ToString());
                }
                result.AddConstructor(cc);
            }
            return result;
        }
    }
}