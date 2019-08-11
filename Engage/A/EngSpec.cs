using System.Collections.Generic;

namespace Engage.A
{
    public partial class EngSpec
    {
        public string NS;
        public List<TypeDecl> Types = new List<TypeDecl>();
        public List<TokenDecl> Tokens = new List<TokenDecl>();
        public List<HandlerDecl> Handlers = new List<HandlerDecl>();
    }
}