namespace Engage.A
{
    public class Assignment
    {
		public string LHS;
		public Reaction RHS;

		public override bool Equals(object obj)
		{
			var other = obj as Assignment;
			if (other == null)
				return false;
			return LHS == other.LHS
			       && RHS.Equals(other.RHS)
				;
		}
    }
}