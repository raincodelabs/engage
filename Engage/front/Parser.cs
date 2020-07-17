using Engage.A;

namespace Engage.front
{
    public class Parser
    {
		public static EngSpec ParseEngSpec(string code)
		{
			EngageMetaParser parser = new EngageMetaParser();
			return parser.ParseGrammar (code);
		}
    }
}
