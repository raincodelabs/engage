// Engage! generated this file, please do not edit manually
using System.Collections.Generic;

namespace AB
{
    public class IfStmt : Stmt
    {
        public Expr cond;
        public List<Stmt> code = new List<Stmt>();

        public IfStmt(Expr _cond, List<Stmt> _code)
        {
            cond = _cond;
            code.AddRange(_code);
        }

    }
}
