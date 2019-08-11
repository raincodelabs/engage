namespace Engage.B
{
    public class TokenPlan
    {
        public bool Special = false;
        public string Value;

        public override bool Equals(object obj)
        {
            if (obj is TokenPlan other)
                return other.Value == this.Value && other.Special == this.Special;
            return false;
        }

        public override int GetHashCode()
            => Value.GetHashCode() + Special.GetHashCode();

        public override string ToString()
            => (Special ? "!" : "") + Value;
    }
}