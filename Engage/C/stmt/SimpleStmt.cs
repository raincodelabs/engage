namespace Engage.C
{
    public class SimpleStmt : CsStmt
    {
        public string Code;

        public SimpleStmt()
        {
        }

        public SimpleStmt(string code)
        {
            Code = code;
        }

        public override D.CsStmt Concretize()
            => new D.CsSimpleStmt(Code);
    }
}