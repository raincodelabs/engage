namespace Engage.A
{
    public class PopHashAction : Reaction
    {
        public override bool Equals(object obj)
        {
            var other = obj as PopHashAction;
            if (other == null)
                return false;
            return Name == other.Name;
        }

        public override B.HandleAction ToHandleAction(string target = "", B.HandleAction prev = null)
        {
            var a = new B.PopSeveral {Name = B.SystemPlan.Dealias(Name), Target = target};
            a.SiblingActions.Add(prev);
            if (prev is B.PopSeveral ps)
                a.SiblingActions.AddRange(ps.SiblingActions);
            return a;
        }
    }
}