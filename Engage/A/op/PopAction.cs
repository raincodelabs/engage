namespace Engage.A
{
    public partial class PopAction : Reaction
    {
        public string Name;

        public override B.HandleAction ToHandleAction(string target = "", B.HandleAction prev = null)
            => new B.PopOne() { Name = B.SystemPlan.Dealias(Name), Target = target };
    }
}