using System.Collections.Generic;
using System.IO;
using Engage;

namespace Runner
{
    internal static class Program
    {
        private static string FourUp { get; } = Path.Combine("..", "..", "..", "..");

        private static void Main(string[] args)
        {
            Dictionary<string, string> compilationList = new Dictionary<string, string>
            {
                [Path.Combine(FourUp, "example", "appbuilder.eng")] = Path.Combine(FourUp, "AB"),
                [Path.Combine(FourUp, "example", "xml.eng")] = Path.Combine(FourUp, "EAX")
            };

            foreach (var spec in compilationList.Keys)
                FrontEnd.FullPipeline(spec, compilationList[spec]);
        }
    }
}