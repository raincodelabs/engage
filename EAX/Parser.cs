using System.Collections.Generic;

namespace EAX
{
    public static class Parsers
    {
        public static EaxOpenClose.EngagedXmlDoc ParseOpenClose(string input)
            => new EaxOpenClose.Parser(input).Parse() as EaxOpenClose.EngagedXmlDoc;

        public static EaxOpenClose.EngagedXmlDoc ParseOpenCloseX(string input)
            => new EaxOpenClose.ParserOptimisedForStrings(input).Parse() as EaxOpenClose.EngagedXmlDoc;

        public static EaxOpenClose.EngagedXmlDoc ParseOpenCloseC(string input)
            => new EaxOpenClose.ParserCollapsedToMinimum(input).Parse();

        public static HashSet<string> ParseOpenCloseF(string input)
            => new EaxOpenClose.ParserCollapsedToFlatStructure(input).Parse();

        public static HashSet<string> ParseOpenCloseNon(string input)
            => new EaxOpenClose.NonParser(input).Parse();

        public static EaxFuzzy.EngagedXmlDoc ParseFuzzy(string input)
            => new EaxFuzzy.Parser(input).Parse() as EaxFuzzy.EngagedXmlDoc;
    }
}