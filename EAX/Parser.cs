namespace EAX
{
    public static class Parsers
    {
        public static EaxOpenClose.EngagedXmlDoc ParseOpenClose(string input)
            => new EaxOpenClose.Parser(input).Parse() as EaxOpenClose.EngagedXmlDoc;
    }
}