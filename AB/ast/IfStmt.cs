// Engage! generated this file, please do not edit manually
using System.Collections.Generic;

namespace AB
{
    public partial class IfStmt : Stmt
    {
        public Expr cond;
        public List<Stmt> branch = new List<Stmt>();

        public IfStmt(Expr _cond, List<Stmt> _branch)
        {
            cond = _cond;
            branch.AddRange(_branch);
        }

    }
}
