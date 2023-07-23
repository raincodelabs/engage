using System;

namespace Engage.FC;

public abstract class SignedFlag
{
    protected string Name { get; init; }

    public abstract SignedFlag Reversed();
}

public class FlagUp : SignedFlag
{
    public FlagUp(string name)
    {
        Name = name;
    }
    
    public FlagUp(SignedFlag flag)
    {
        if (flag is FlagUp up1)
            Name = up1.Name;
        else if (flag.Reversed() is FlagUp up2)
            Name = up2.Name;
        else
            Console.WriteLine($"[ERROR] Cannot make a FlagUp from {flag}");
    }

    public override SignedFlag Reversed()
        => new FlagDown(Name);

    public override string ToString()
        => Name;

    public override bool Equals(object obj)
        => obj is FlagUp other && other.Name == Name;

    public override int GetHashCode()
        => Name.GetHashCode();
}

public class FlagDown : SignedFlag
{
    public FlagDown(string name)
    {
        Name = name;
    }
    
    public FlagDown(SignedFlag flag)
    {
        if (flag is FlagDown down1)
            Name = down1.Name;
        else if (flag.Reversed() is FlagDown down2)
            Name = down2.Name;
        else
            Console.WriteLine($"[ERROR] Cannot make a FlagDown from {flag}");
    }

    public override SignedFlag Reversed()
        => new FlagUp(Name);

    public override string ToString()
        => "!" + Name;

    public override bool Equals(object obj)
        => obj is FlagDown other && other.Name == Name;

    public override int GetHashCode()
        => -Name.GetHashCode();
}