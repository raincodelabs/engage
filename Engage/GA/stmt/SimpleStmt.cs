namespace Engage.GA;

public class SimpleStmt : CsStmt
{
    public readonly string Code;

    public SimpleStmt()
    {
    }

    public SimpleStmt(string code)
    {
        Code = code;
    }

    public override GC.CsStmt Concretise()
        => new GC.CsSimpleStmt(Code);
}