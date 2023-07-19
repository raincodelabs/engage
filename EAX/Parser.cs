using System.Collections.Generic;

namespace EAX
{
    public static class Parsers
    {
        public static EaxOpenClose.EngagedXmlDoc ParseOpenClose(string input)
            => new EaxOpenClose.Parser(input).Parse() as EaxOpenClose.EngagedXmlDoc;

        public static EaxOpenClose.EngagedXmlDoc ParseOpenCloseX(string input)
            => new EaxOpenClose.AltParser(input).Parse() as EaxOpenClose.EngagedXmlDoc;

        public static EaxOpenClose.EngagedXmlDoc ParseOpenCloseC(string input)
            => new EaxOpenClose.CollapsedParser(input).Parse();

        public static HashSet<string> ParseOpenCloseF(string input)
            => new EaxOpenClose.FlatParser(input).Parse();

        public static EaxFuzzy.EngagedXmlDoc ParseFuzzy(string input)
            => new EaxFuzzy.Parser(input).Parse() as EaxFuzzy.EngagedXmlDoc;
    }
}