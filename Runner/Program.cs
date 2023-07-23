using System.Collections.Generic;
using System.IO;
using Engage;

namespace Runner;

public static class Program
{
    private static string FourUp { get; } = Path.Combine("..", "..", "..", "..");

    public static readonly Dictionary<string, string> CompilationList = new Dictionary<string, string>
    {
        [Path.Combine(FourUp, "AB", "spec", "appbuilder.eng")] = Path.Combine(FourUp, "AB"),
        [Path.Combine(FourUp, "EAX", "specs", "OpenClose.eng")] = Path.Combine(FourUp, "EAX", "OpenClose"),
        [Path.Combine(FourUp, "EAX", "specs", "Fuzzy.eng")] = Path.Combine(FourUp, "EAX", "Fuzzy"),
    };

    private static void Main(string[] args)
    {
        foreach (var spec in CompilationList.Keys)
            FrontEnd.FullPipeline(spec, CompilationList[spec]);
    }
}