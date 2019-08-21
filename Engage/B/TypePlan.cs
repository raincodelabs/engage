using Engage.C;
using System;
using System.Collections.Generic;

namespace Engage.B
{
    public class TypePlan
    {
        public string Name;
        public string Super;
        public bool IsList = false;
        private List<ConstPlan> Constructors = new List<ConstPlan>();

        public TypePlan(string name)
        {
            Name = name;
        }

        public ConstPlan FirstConstructor
        {
            get => Constructors.Count > 0 ? Constructors[0] : null;
        }

        public void InferConstructor(IEnumerable<string> args, A.HandlerDecl h, Func<string, B.TypePlan> getTypePlan)
        {
            B.ConstPlan cp = new B.ConstPlan();
            foreach (string a in args)
                cp.AddConstructorArguments(h, a, getTypePlan);
            AddConstructor(cp);
            Console.WriteLine($"[A2B] Inferred constructor {cp.ToString(Name, Super)}");
        }

        public TypePlan Copy(bool turnIntoList = false)
        {
            TypePlan plan = new TypePlan(Name);
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

        internal C.CsClass GenerateClass(string ns)
        {
            var result = new C.CsClass();
            result.NS = ns;
            result.Name = Name;
            result.Super = Super;
            foreach (B.ConstPlan c in Constructors)
                c.AddAbstractCodeConstructor(result);
            return result;
        }
    }
}