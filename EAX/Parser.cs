using System.Collections.Generic;

namespace EAX
{
    public static class Parsers
    {
        public static EaxOpenClose.EngagedXmlDoc ParseOpenClose(string input)
            => new EaxOpenClose.Parser(input).Parse() as EaxOpenClose.EngagedXmlDoc;

        public static EaxOpenClose.EngagedXmlDoc ParseOpenCloseO1(string input)
            => new EaxOpenClose.ParserOptimisedForStrings(input).Parse() as EaxOpenClose.EngagedXmlDoc;

        public static EaxOpenClose.EngagedXmlDoc ParseOpenCloseO2(string input)
            => new EaxOpenClose.ParserOptimisedForTokens(input).Parse() as EaxOpenClose.EngagedXmlDoc;

        public static EaxOpenClose.EngagedXmlDoc ParseOpenCloseO3(string input)
            => new EaxOpenClose.ParserOptimisedForLevels(input).Parse() as EaxOpenClose.EngagedXmlDoc;

        public static EaxOpenClose.EngagedXmlDoc ParseOpenCloseO4(string input)
            => new EaxOpenClose.ParserCollapsedToMinimum(input).Parse();

        public static HashSet<string> ParseOpenCloseO5(string input)
            => new EaxOpenClose.ParserCollapsedToFlatStructure(input).Parse();

        public static HashSet<string> ParseOpenCloseO6(string input)
            => new EaxOpenClose.NonParser(input).Parse();

        public static EaxFuzzy.EngagedXmlDoc ParseFuzzy(string input)
            => new EaxFuzzy.Parser(input).Parse() as EaxFuzzy.EngagedXmlDoc;
    }
}