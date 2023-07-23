namespace Engage.NA;

public class TokenPlan
{
    public bool Special;
    public string Value;

    internal static TokenPlan BOF() 
        => new() { Special = true, Value = "BOF" };

    internal static TokenPlan EOF()
        => new() { Special = true, Value = "EOF" };

    internal static TokenPlan FromNT(string nt)
        => new() { Special = true, Value = nt };

    internal static TokenPlan FromT(string t)
        => new() { Special = false, Value = t };

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