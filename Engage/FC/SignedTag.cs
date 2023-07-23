namespace Engage.FC;

public abstract class SignedTag
{
    protected string Name { get; init; }
}

public class TagUp : SignedTag
{
    public TagUp(string name)
    {
        Name = name;
    }

    public override string ToString()
        => Name;

    public override bool Equals(object obj)
        => obj is TagUp other && other.Name == Name;

    public override int GetHashCode()
        => Name.GetHashCode();
}

public class TagDown : SignedTag
{
    public TagDown(string name)
    {
        Name = name;
    }

    public override string ToString()
        => "!" + Name;

    public override bool Equals(object obj)
        => obj is TagDown other && other.Name == Name;

    public override int GetHashCode()
        => -Name.GetHashCode();
}