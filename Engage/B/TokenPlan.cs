namespace Engage.B
{
    public class TokenPlan
    {
        public bool Special = false;
        public string Value;

        internal static TokenPlan EOF()
            => new B.TokenPlan() { Special = true, Value = "EOF" };

        internal static TokenPlan FromNT(string nt)
            => new B.TokenPlan() { Special = true, Value = nt };

        internal static TokenPlan FromT(string t)
            => new B.TokenPlan() { Special = false, Value = t };

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