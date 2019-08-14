namespace Engage.C
{
    public class CsSimpleStmt : CsStmt
    {
        public string Code;

        public CsSimpleStmt()
        {
        }

        public CsSimpleStmt(string code)
        {
            Code = code;
        }

        public override D.CsStmt Concretize()
            => new D.CsSimpleStmt(Code);
    }
}