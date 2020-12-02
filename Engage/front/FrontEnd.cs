using System.IO;
using Engage.A;

namespace Engage.front
{
	public static class Parser
	{
		public static EngSpec EngSpecFromText(string code)
		{
			EngageMetaParser parser = new EngageMetaParser();
			return parser.ParseGrammar(code);
		}

		public static EngSpec EngSpecFromFile(string filename)
			=> EngSpecFromText(File.ReadAllText(filename));
	}
}