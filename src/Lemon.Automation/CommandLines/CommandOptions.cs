using CommandLine;

namespace Lemon.Automation.CommandLines
{
    public class CommandOptions
    {
        [Option('a', "app", Required = false, HelpText = "Apps to start.")]
        public required IEnumerable<string> Apps { get; set; }

        [Option('b', "isboot", Default = false, Required = false)]
        public bool? IsBootstrapper { get; set; }

        [Option('c', "con", Default = "Lemon.Automation-9527", Required = false)]
        public string? ConnectionKey { get; set; }

        public override string ToString()
        {
            return $"IsBootstrapper:{IsBootstrapper};ConnectionKey:{ConnectionKey};Apps:{string.Join(",", Apps)}";
        }
    }
}
