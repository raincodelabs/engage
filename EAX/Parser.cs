namespace EAX
{
    public static class Parsers
    {
        public static EaxOpenClose.EngagedXmlDoc ParseOpenClose(string input)
            => new EaxOpenClose.Parser(input).Parse() as EaxOpenClose.EngagedXmlDoc;

        public static EaxOpenClose.EngagedXmlDoc ParseOpenCloseX(string input)
            => new EaxOpenClose.AltParser(input).Parse() as EaxOpenClose.EngagedXmlDoc;

        public static EaxFuzzy.EngagedXmlDoc ParseFuzzy(string input)
            => new EaxFuzzy.Parser(input).Parse() as EaxFuzzy.EngagedXmlDoc;
    }
}