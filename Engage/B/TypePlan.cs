using System;
using System.Collections.Generic;

namespace Engage.B
{
    public class TypePlan
    {
        public string Name;
        public string Super;
        public bool IsList = false;
        private readonly List<ConstPlan> _constructors = new List<ConstPlan>();

        public TypePlan(string name)
        {
            Name = name;
        }

        public ConstPlan FirstConstructor
            => _constructors.Count > 0 ? _constructors[0] : null;

        public void InferConstructor(IEnumerable<string> args, A.HandlerDecl h, Func<string, B.TypePlan> getTypePlan)
        {
            var cp = new B.ConstPlan();
            foreach (string a in args)
                cp.AddConstructorArguments(h, a, getTypePlan);
            AddConstructor(cp);
            Console.WriteLine($"[A2B] Inferred constructor {cp.ToString(Name, Super)}");
        }

        public TypePlan Copy(bool turnIntoList = false)
        {
            var plan = new TypePlan(Name)
            {
                Super = Super,
                IsList = IsList || turnIntoList
            };
            // do not copy constructors!
            return plan;
        }

        public override string ToString()
            => IsList ? $"List<{Name}>" : Name;

        public override bool Equals(object obj)
            => this.ToString() == $"{obj}";

        public void AddConstructor(ConstPlan cp)
        {
            if (!_constructors.Contains(cp))
                _constructors.Add(cp);
        }

        internal C.CsClass GenerateClass(string ns)
        {
            var result = new C.CsClass
            {
                NS = ns,
                Name = Name,
                Super = Super
            };
            foreach (B.ConstPlan c in _constructors)
                c.AddAbstractCodeConstructor(result);
            return result;
        }
    }
}