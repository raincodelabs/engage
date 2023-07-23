namespace Engage.C
{
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

        public override D.CsStmt Concretise()
            => new D.CsSimpleStmt(Code);
    }
}