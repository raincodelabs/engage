// Engage! generated this file, please do not edit manually
using System.Collections.Generic;

namespace AB
{
    public partial class ABProgram
    {
        public List<Decl> data = new List<Decl>();
        public List<Stmt> code = new List<Stmt>();

        public ABProgram(List<Decl> _data, List<Stmt> _code)
        {
            data.AddRange(_data);
            code.AddRange(_code);
        }

    }
}
